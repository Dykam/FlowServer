using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Flow
{
    public partial class Router
    {
        protected class Listener : IDisposable
        {
            private const int TimeoutTime = 10000;
            public Queue<TcpClient> Clients;
            public object ClientsLock;

            private bool _disposed;
            public EventWaitHandle Handle;
            public EventWaitHandle HandleRequestHandle;
            public Thread HandleRequestThread;
            public int Port;

            private readonly IEnumerable<Responder> _processors;
            public Router @Router;
            public TcpListener TcpListener;

            public Thread @Thread;

            public Listener(Router router, int port, EventWaitHandle handle, IEnumerable<Responder> processors)
            {
                Port = port;
                @Router = router;
                TcpListener = new TcpListener(IPAddress.Any, port);
                Thread = new Thread(Listen);
                Thread.Name = "ListenerThread Port " + Port;
                HandleRequestThread = new Thread(clientsHandler);
                HandleRequestThread.Name = "HandleRequestThread Port " + Port;
                ClientsLock = new object();

                Clients = new Queue<TcpClient>();

                Handle = handle;
                HandleRequestHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                _disposed = false;
                _processors = processors;
                Thread.Start();
                HandleRequestThread.Start();
            }

            public void Dispose()
            {
                _disposed = true;
            }

            private void Listen()
            {
                while (!_disposed)
                {
                    if (Handle.WaitOne(TimeoutTime))
                    {
                        var client = TcpListener.AcceptTcpClient();
                        lock (ClientsLock)
                        {
                            Clients.Enqueue(client);
                        }
                        HandleRequestHandle.Set();
                    }
                }
            }

            private void clientsHandler()
            {
                while (!_disposed)
                {
                    if (HandleRequestHandle.WaitOne(TimeoutTime))
                    {
                        while (true)
                        {
                            TcpClient client;
                            lock (Clients)
                            {
                                if (Clients.Count == 0) break;
                                client = Clients.Dequeue();
                            }
                            var request = new Request(@Router, client, Port);
                            var accepted = false;
                            try
                            {
                                foreach (var processor in _processors)
                                {
                                    accepted = processor.Respond(request);
                                    if (accepted)
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                accepted = true;
                                if (Router.ErrorHandler(ex, request))
                                    throw new Exception("An internal error caused abortion of this router.", ex);
                            }

                            if (!accepted)
                            {
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
        }
    }
}