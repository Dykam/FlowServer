using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

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
				Thread.Name = "ListenerThread Port " + Port.ToString();
				HandleRequestThread = new Thread(clientsHandler);
				HandleRequestThread.Name = "HandleRequestThread Port " + Port.ToString();
				ClientsLock = new object();

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
							var request = new Request(client, Port);
							bool accepted = false;
							foreach (var processor in Processors) {
								if (processor(request)) {
									accepted = true;
									break;
								}
							}
							if (!accepted) {
								request.Respond("HTTP/1.1", 500, 0).Finish().Close();
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
