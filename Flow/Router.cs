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
	/// <remarks>
	/// A class managing incoming traffic and dispatching them to the appropiate responders.
	/// </remarks>
	public partial class Router : IDisposable
	{
		const int timeoutTime = 10000;

		protected List<Listener> listeners;
		protected List<Responder> processors;

		/// <value>
		/// The ports to listen to.
		/// </value>
		public PortList Ports { get; private set; }
		EventWaitHandle handle;
		bool disposed;

		/// <value>
		/// True if the router is listening for incoming requests.
		/// </value>
		public bool Running { get; private set; }

		/// <summary>
		/// Constructs a new router to listen to the <paramref name="ports"/>.
		/// </summary>
		/// <param name="port">
		/// A <see cref="IEnumerable"/> denoting the ports to listen to.
		/// </param>
		public Router(IEnumerable<int> ports)
		{
			processors = new List<Responder>();
			Ports = new PortList(ports);
			Ports.PortAdded += new EventHandler<PortList.PortListEventArgs>(PortAdded);
			Ports.PortRemoved += new EventHandler<PortList.PortListEventArgs>(PortRemoved);

			handle = new EventWaitHandle(false, EventResetMode.ManualReset);

			listeners =
				ports
				.Select(port => new Listener(this, port, handle, processors))
				.ToList();
		}

		/// <summary>
		/// Constructs a new router to listen to <paramref name="port"/>.
		/// </summary>
		/// <param name="port">
		/// A <see cref="System.Int32"/> denoting the port to listen to.
		/// </param>
		public Router(int port)
			: this(new[] { port })
		{
		}

		/// <summary>
		/// Constructs a new router to listen to port 80.
		/// </summary>
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
		
		/// <summary>
		/// Starts accepting requests.
		/// </summary>
		public void Start()
		{
			foreach (var listener in listeners) {
				listener.TcpListener.Start();
			}
			handle.Set();
			Running = true;
		}

		/// <summary>
		/// Pauses accepting requests.
		/// </summary>
		public void Pause()
		{
			foreach (var listener in listeners) {
				listener.TcpListener.Stop();
			}
			handle.Reset();
			Running = false;
		}
		
		/// <summary>
		/// Adds a responder to the router, to be called when the condition returns false.
		/// </summary>
		/// <param name="condition">
		/// A <see cref="Predicate"/> which determines if the responder should respond to this request.
		/// </param>
		/// <returns>
		/// A <see cref="ResponderAdder"/> to add the responder or another condition.
		/// </returns>
		public ResponderAdder If(Predicate<Request> condition)
		{
			return new ResponderAdder(this, condition);
		}

		public void Dispose()
		{
			if(!disposed) {
				disposed = true;
				Pause();
				foreach (var listener in listeners) {
					listener.Dispose();
				}
			}
		}
	}
}