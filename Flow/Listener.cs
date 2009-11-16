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
			public Router @Router;

			IEnumerable<Responder> Processors;

			bool disposed;
			public Listener(Router router, int port, EventWaitHandle handle, IEnumerable<Responder> processors)
			{
				Port = port;
				@Router = router;
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
				Processors = processors;
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
							var request = new Request(@Router, client, Port);
							bool accepted = false;
							foreach (var processor in Processors) {
								accepted = processor.Respond(request);
								if (accepted)
									break;
							}

							if (!accepted) {
								request
									.Respond(404)
									.Finish()
									.Dispose();
								request.Dispose();
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
