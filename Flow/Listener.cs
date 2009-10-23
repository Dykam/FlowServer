using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	public partial class Router
	{
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
}
