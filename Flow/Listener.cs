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

			IEnumerable<Action<Request>> Processors;
			bool disposed;
			public Listener(int port, EventWaitHandle handle, IEnumerable<Action<Request>> Processors)
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
				disposed = false;
				this.Processors = Processors;
				Thread.Start();
				HandleRequestThread.Start();
			}
			void listen()
			{
				while (!disposed) {
					if (Handle.WaitOne(timeoutTime)) {
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
				while (!disposed) {
					if (HandleRequestHandle.WaitOne(timeoutTime)) {
						while (true) {
							TcpClient client;
							lock (Clients) {
								if (Clients.Count == 0) break;
								client = Clients.Dequeue();
							}
							try {
								var request = new Request(client, Port);
								bool accepted = false;
								foreach (var processor in Processors) {
									processor(request);
									if (request.Accepted)
										break;
								}
								if (!request.Accepted) {
									request
										.Accept(500)
										.Finish()
										.Dispose();
									request.Dispose();
								}
							}
							catch {
								continue;
							}
						}
					}
				}
			}

			public void Dispose()
			{
				disposed = true;
			}
		}
	}
}
