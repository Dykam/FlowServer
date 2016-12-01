using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Flow
{
    /// <remarks>
    ///     A class managing incoming traffic and dispatching them to the appropiate responders.
    /// </remarks>
    public partial class Router : IDisposable
    {
        private const int TimeoutTime = 10000;
        private bool _disposed;
        private readonly EventWaitHandle _handle;

        protected List<Listener> Listeners;
        protected List<Responder> Processors;

        /// <summary>
        ///     Constructs a new router to listen to the <paramref name="ports" />.
        /// </summary>
        /// <param name="port">
        ///     A <see cref="IEnumerable" /> denoting the ports to listen to.
        /// </param>
        /// <param name="errorHandler">
        ///     A <see cref="Func" /> to handle errors with.
        ///     Should return true if the execution of the router has to be continued.
        /// </param>
        public Router(IEnumerable<int> ports, Func<Exception, Request, bool> errorHandler)
        {
            Processors = new List<Responder>();
            Ports = new PortList(ports);
            Ports.PortAdded += PortAdded;
            Ports.PortRemoved += PortRemoved;

            ErrorHandler = errorHandler;

            _handle = new EventWaitHandle(false, EventResetMode.ManualReset);

            Listeners =
                ports
                    .Select(port => new Listener(this, port, _handle, Processors))
                    .ToList();
        }

        /// <summary>
        ///     Constructs a new router to listen to the <paramref name="ports" />.
        /// </summary>
        /// <param name="port">
        ///     A <see cref="IEnumerable" /> denoting the ports to listen to.
        /// </param>
        public Router(IEnumerable<int> ports)
            : this(ports, (ex, req) => false)
        {
        }

        /// <summary>
        ///     Constructs a new router to listen to <paramref name="port" />.
        /// </summary>
        /// <param name="port">
        ///     A <see cref="System.Int32" /> denoting the port to listen to.
        /// </param>
        /// <param name="errorHandler">
        ///     A <see cref="Func" /> to handle errors with.
        ///     Should return true if the execution of the router has to be continued.
        /// </param>
        public Router(int port, Func<Exception, Request, bool> errorHandler)
            : this(new[] {port}, errorHandler)
        {
        }

        /// <summary>
        ///     Constructs a new router to listen to <paramref name="port" />.
        /// </summary>
        /// <param name="port">
        ///     A <see cref="System.Int32" /> denoting the port to listen to.
        /// </param>
        public Router(int port)
            : this(new[] {port})
        {
        }

        /// <summary>
        ///     Constructs a new router to listen to port 80.
        /// </summary>
        /// <param name="errorHandler">
        ///     A <see cref="Func" /> to handle errors with.
        ///     Should return true if the execution of the router has to be continued.
        /// </param>
        public Router(Func<Exception, Request, bool> errorHandler)
            : this(80, errorHandler)
        {
        }

        /// <summary>
        ///     Constructs a new router to listen to port 80.
        /// </summary>
        public Router()
            : this(80)
        {
        }

        /// <value>
        ///     The ports to listen to.
        /// </value>
        public PortList Ports { get; }

        /// <value>
        ///     True if the router is listening for incoming requests.
        /// </value>
        public bool Running { get; private set; }

        /// <value>
        ///     This function is the general error handler for this router instance.
        ///     It should return true if the execution of the router has to be canceled.
        /// </value>
        public Func<Exception, Request, bool> ErrorHandler { get; set; }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Pause();
                foreach (var listener in Listeners)
                {
                    listener.Dispose();
                }
            }
        }

        private void PortRemoved(object sender, PortList.PortListEventArgs e)
        {
            Listeners.RemoveAll(listener => listener.Port == e.Port);
        }

        private void PortAdded(object sender, PortList.PortListEventArgs e)
        {
            var listener = new Listener(this, e.Port, _handle, Processors);
            Listeners.Add(listener);
        }

        /// <summary>
        ///     Starts accepting requests.
        /// </summary>
        public void Start()
        {
            foreach (var listener in Listeners)
            {
                listener.TcpListener.Start();
            }
            _handle.Set();
            Running = true;
        }

        /// <summary>
        ///     Pauses accepting requests.
        /// </summary>
        public void Pause()
        {
            foreach (var listener in Listeners)
            {
                listener.TcpListener.Stop();
            }
            _handle.Reset();
            Running = false;
        }

        /// <summary>
        ///     Adds a responder to the router, to be called when the condition returns false.
        /// </summary>
        /// <param name="condition">
        ///     A <see cref="Predicate" /> which determines if the responder should respond to this request.
        /// </param>
        /// <returns>
        ///     A <see cref="ResponderAdder" /> to add the responder or another condition.
        /// </returns>
        public ResponderAdder If(Predicate<Request> condition)
        {
            return new ResponderAdder(this, condition);
        }
    }
}