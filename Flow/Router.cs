using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;

namespace Flow
{
	public partial class Router : IDisposable
	{
		const int timeoutTime = 10000;

		protected List<Listener> listeners;
		protected List<Action<Request>> processors;
		public PortList Ports { get; private set; }
		EventWaitHandle handle;
		bool disposed;

		public bool Running { get; private set; }

		public Router(IEnumerable<int> ports)
		{
			processors = new List<Action<Request>>();
			Ports = new PortList(ports);
			Ports.PortAdded += new EventHandler<PortList.PortListEventArgs>(PortAdded);
			Ports.PortRemoved += new EventHandler<PortList.PortListEventArgs>(PortRemoved);

			handle = new EventWaitHandle(false, EventResetMode.ManualReset);

			listeners =
				ports
				.Select(port => new Listener(this, port, handle, processors))
				.ToList();
		}

		public Router(int port)
			: this(new[] { port })
		{
		}

		public Router()
			: this(80)
		{
		}

		void PortRemoved(object sender, PortList.PortListEventArgs e)
		{
			listeners.RemoveAll(listener => listener.Port == e.Port);
		}

		void PortAdded(object sender, PortList.PortListEventArgs e)
		{
			var listener = new Listener(this, e.Port, handle, processors);
			listeners.Add(listener);
		}

		public void Start()
		{
			foreach (var listener in listeners) {
				listener.TcpListener.Start();
			}
			handle.Set();
			Running = true;
		}

		public void Stop()
		{
			foreach (var listener in listeners) {
				listener.TcpListener.Stop();
			}
			handle.Reset();
			Running = false;
		}

		public IDisposable Add(Action<Request> processor)
		{
			processors.Add(processor);
			return new ProcessorDisposer(Remove, processor);
		}

		public void Remove(Action<Request> processor)
		{
			if (processors.Contains(processor))
				processors.Remove(processor);
		}

		public void Dispose()
		{
			Stop();
			foreach (var listener in listeners) {
				listener.Dispose();
			}
		}
	}
}