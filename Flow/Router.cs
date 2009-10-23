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
	public class Router
	{

		protected List<Listener> Listeners;
		protected List<Predicate<Request>> Processors;
		public IEnumerable<Thread> Threads { get; private set; }
		public PortList Ports { get; private set; }
		protected EventWaitHandle Handle;
		public bool Running { get; private set; }

		public Router(IEnumerable<int> ports)
		{
			Processors = new List<Predicate<Request>>();
			Ports = new PortList(ports);
			Ports.PortAdded += new EventHandler<PortList.PortListEventArgs>(Ports_PortAdded);
			Ports.PortRemoved += new EventHandler<PortList.PortListEventArgs>(Ports_PortRemoved);

			Handle = new EventWaitHandle(false, EventResetMode.ManualReset);

			Listeners =
				ports
				.Select(port => new Listener(port, Handle, Processors))
				.ToList();

			// As it is an enumerable, it updates live.
			Threads = Listeners.Select(listener => listener.Thread);
		}

		void Ports_PortRemoved(object sender, PortList.PortListEventArgs e)
		{
			Listeners.RemoveAll(listener => listener.Port == e.Port);
		}

		void Ports_PortAdded(object sender, PortList.PortListEventArgs e)
		{
			var listener = new Listener(e.Port, Handle, Processors);
			Listeners.Add(listener);
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
			Handle.Set();
			Running = true;
		}

		public void Stop()
		{
			Handle.Reset();
			Running = false;
		}

		public IDisposable Subscribe(Predicate<Request> processor)
		{
			Processors.Add(processor);
			return new ProcessorDisposer(Unsubscribe, processor);
		}

		public void Unsubscribe(Predicate<Request> processor)
		{
			if (Processors.Contains(processor))
				Processors.Remove(processor);
		}

		protected class Listener : IDisposable
		{
			const int timeoutTime = 10000;

			public Thread @Thread;
			public Thread HandleRequestThread;
			public TcpListener TcpListener;
			public int Port;
			public EventWaitHandle Handle;
			public EventWaitHandle HandleRequestHandle;
			public Queue<TcpClient> Clients;
			public object ClientsLock;

			IEnumerable<Predicate<Request>> Processors;
			bool dispose;
			public Listener(int port, EventWaitHandle handle, IEnumerable<Predicate<Request>> Processors)
			{
				Port = port;
				TcpListener = new TcpListener(IPAddress.Any, port);
				Thread = new Thread(listen);
				HandleRequestThread = new Thread(clientsHandler);

				Clients = new Queue<TcpClient>();

				Handle = handle;
				HandleRequestHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
				dispose = false;
				this.Processors = Processors;
				Thread.Start();
				HandleRequestThread.Start();
			}
			void listen()
			{
				while (true) {
					var signalled = Handle.WaitOne(timeoutTime);
					if (dispose) {
						return;
					}
					if (signalled) {
						var client = TcpListener.AcceptTcpClient();
						lock (ClientsLock) {
							Clients.Enqueue(client);
						}
						HandleRequestHandle.Set();
					}
				}
			}

			void clientsHandler()
			{
				while (true) {
					var signalled = HandleRequestHandle.WaitOne(timeoutTime);
					if (dispose) {
						return;
					}
					if (signalled) {
						while (true) {
							TcpClient client;
							lock (Clients) {
								if (Clients.Count == 0) break;
								client = Clients.Dequeue();
							}
							var request = new Request(client);
							foreach (var processor in Processors) {
								if (processor(request)) break;
							}
						}
					}
				}
			}

			public void Dispose()
			{
				dispose = true;
			}
		}
	}

	

	public class PortList : IEnumerable<int>
	{
		LinkedList<int> ports;
		public PortList(IEnumerable<int> ports)
		{
			this.ports = new LinkedList<int>(ports);
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

	internal class ProcessorDisposer : IDisposable
	{
		Action<Predicate<Request>> detach;
		Predicate<Request> processor;
		public ProcessorDisposer(Action<Predicate<Request>> detach, Predicate<Request> processor)
		{
			this.detach = detach;
			this.processor = processor;
		}

		public void Dispose()
		{
			detach(processor);
		}
	}

	public class Request {
		public Request(TcpClient newClient)
		{

		}
	}
	public class Response { }
}