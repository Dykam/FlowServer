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
	public class Router : IObservable<Request>
	{
		protected bool run;
		protected List<TcpListener> tcpListeners;
		protected List<IObserver<Request>> observers;

		public Thread @Thread { get; private set; }
		public PortList Ports { get; private set; }

		public Router(IEnumerable<int> ports)
		{
			Func<int, int> foo;
			@Thread = new Thread(new ThreadStart(listener));
			observers = new List<IObserver<Request>>();
			Ports = new PortList(ports);
			var listeners = new List<TcpListener>();
			for(var port in Ports)
				listeners.Add(new TcpListener(IPAddress.Any, port));
		}
		public Router(int port)
			: this(new[] { port })
		{
		}
		public Router()
			: this(80)
		{
		}

		public void Start()
		{
			run = true;
		}

		public void Stop()
		{
			run = false;
		}

		void listener()
		{
			while (run) {

			}
		}

		public IDisposable Subscribe(IObserver<Request> observer)
		{
			observers.Add(observer);
			return new ObserverDisposer(Unsubscribe, observer);
		}

		public void Unsubscribe(IObserver<Request> observer)
		{
			if (observers.Contains(observer))
				observers.Remove(observer);
		}
	}

	public class PortList : IEnumerable<int>
	{
		LinkedList<int> ports;
		public PortList(IEnumerable<int> ports)
		{
			ports = new LinkedList<int>(ports);
		}

		public event EventHandler<PortListEventArgs> PortAdded;
		protected void OnPortAdded(int port)
		{
			var e = PortAdded;
			if (e != null) {
				e(this, new PortListEventArgs(port));
			}
		}
		public event EventHandler<PortListEventArgs> PortRemoved;
		protected void OnPortRemoved(int port)
		{
			var e = PortRemoved;
			if (e != null) {
				e(this, new PortListEventArgs(port));
			}
		}

		public bool this[int index]
		{
			get
			{
				return ports.Contains(index);
			}
			set
			{
				if (ports.Contains(index) == value) return;
				if (value) {
					ports.AddLast(index);
					OnPortAdded(index);
				} else {
					ports.Remove(index);
					OnPortRemoved(index);
				}
			}
		}

		public class PortListEventArgs : EventArgs
		{
			public int Port { get; private set; }
			public PortListEventArgs(int port)
			{
				Port = port;
			}
		}

		public IEnumerator<int> GetEnumerator()
		{
			return ports.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ports.GetEnumerator();
		}
	}

	internal class ObserverDisposer : IDisposable
	{
		Action<IObserver<Request>> detach;
		IObserver<Request> observer;
		public ObserverDisposer(Action<IObserver<Request>> detach, IObserver<Request> observer)
		{
			this.detach = detach;
			this.observer = observer;
		}

		public void Dispose()
		{
			detach(observer);
		}
	}

	public class Request { }
	public class Response { }
}