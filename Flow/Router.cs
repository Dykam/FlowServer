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
		protected List<Listener> Listeners;
		protected List<Predicate<Request>> Processors;
		public PortList Ports { get; private set; }
		protected EventWaitHandle Handle;
		public bool Running { get; private set; }

		public Router(IEnumerable<int> ports)
		{
			Processors = new List<Predicate<Request>>();
			Ports = new PortList(ports);
			Ports.PortAdded += new EventHandler<PortList.PortListEventArgs>(PortAdded);
			Ports.PortRemoved += new EventHandler<PortList.PortListEventArgs>(PortRemoved);

			Handle = new EventWaitHandle(false, EventResetMode.ManualReset);

			Listeners =
				ports
				.Select(port => new Listener(port, Handle, Processors))
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
			Listeners.RemoveAll(listener => listener.Port == e.Port);
		}

		void PortAdded(object sender, PortList.PortListEventArgs e)
		{
			var listener = new Listener(e.Port, Handle, Processors);
			Listeners.Add(listener);
		}

		public void Start()
		{
			foreach (var listener in Listeners) {
				listener.TcpListener.Start();
			}
			Handle.Set();
			Running = true;
		}

		public void Stop()
		{
			foreach (var listener in Listeners) {
				listener.TcpListener.Stop();
			}
			Handle.Reset();
			Running = false;
		}

		public IDisposable Add(Predicate<Request> processor)
		{
			Processors.Add(processor);
			return new ProcessorDisposer(Remove, processor);
		}

		public void Remove(Predicate<Request> processor)
		{
			if (Processors.Contains(processor))
				Processors.Remove(processor);
		}

		public void Dispose()
		{
			Stop();
			foreach (var listener in Listeners) {
				listener.Dispose();
			}
		}
	}
}