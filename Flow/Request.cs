using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Flow
{
	/// <remarks>
	/// Contains the incoming request.
	/// </remarks>
	public class Request : RequestInfo
	{
		/// <value>
		/// Returns true if there is already responded to the request.
		/// </value>
		public bool Used { get; private set; }

		/// <value>
		/// The body of the request.
		/// </value>
		public ReadOnlyNetworkStream Body { get { return body; } }

		internal Request(Router router, TcpClient newClient, int port, Func<int, string> statusMessageFetcher, string httpVersion)
			: base(router, newClient, port, statusMessageFetcher, httpVersion)
		{
		}
		internal Request(Router router, TcpClient newClient, int port, Func<int, string> statusMessageFetcher)
			: this(router, newClient, port, statusMessageFetcher, defaultHtppVersion)
		{
		}
		internal Request(Router router, TcpClient newClient, int port, string httpVersion)
			: this(router, newClient, port, status => (String)Properties.Settings.Default["Status" + status.ToString()], httpVersion)
		{
		}
		internal Request(Router router, TcpClient newClient, int port)
			: this(router, newClient, port, defaultHtppVersion)
		{
		}
		
		/// <summary>
		/// Starts the response to the request.
		/// </summary>
		/// <param name="version">
		/// A <see cref="System.String"/> denoting the HTTP version of the response.
		/// </param>
		/// <param name="status">
		/// A <see cref="System.Int32"/> denoting the HTTP status code.
		/// </param>
		/// <param name="statusMessage">
		/// A <see cref="System.String"/>  denoting the human readable form of the status code.
		/// </param>
		/// <returns>
		/// A <see cref="HeaderBuilder"/> to add the headers to return to the client.
		/// </returns>
		public HeaderBuilder Respond(string version, int status, string statusMessage)
		{
			checkIfUsed();
			Used = true;
			
			var writer = (TextWriter)new StreamWriter(stream);
			writer.WriteLine("{0} {1} {2}", version, status, statusMessage);
			writer.Flush();
			response = new HeaderBuilder(new WriteOnlyStreamWrapper(stream));
			return response;
		}
		
		/// <summary>
		/// Starts the response to the request.
		/// </summary>
		/// <param name="version">
		/// A <see cref="System.String"/> denoting the HTTP version of the response.
		/// </param>
		/// <param name="status">
		/// A <see cref="System.Int32"/> denoting the HTTP status code.
		/// </param>
		/// <returns>
		/// A <see cref="HeaderBuilder"/> to add the headers to return to the client.
		/// </returns>
		public HeaderBuilder Respond(string version, int status)
		{
			return Respond(version, status, GetStatusMessage(status));
		}
		
		/// <summary>
		/// Starts the response to the request.
		/// </summary>
		/// <param name="status">
		/// A <see cref="System.Int32"/> denoting the HTTP status code.
		/// </param>
		/// <returns>
		/// A <see cref="HeaderBuilder"/> to add the headers to return to the client.
		/// </returns>		
		public HeaderBuilder Respond(int status)
		{
			return Respond(httpVersion, status);
		}
		
		/// <summary>
		/// Starts the response to the request.
		/// </summary>
		/// <param name="status">
		/// A <see cref="System.Int32"/> denoting the HTTP status code.
		/// </param>
		/// <param name="statusMessage">
		/// A <see cref="System.String"/>  denoting the human readable form of the status code.
		/// </param>
		/// <returns>
		/// A <see cref="HeaderBuilder"/> to add the headers to return to the client.
		/// </returns>
		public HeaderBuilder Respond(int status, string statusMessage)
		{
			return Respond(httpVersion, status, statusMessage);
		}

		void checkIfUsed()
		{
			if (Used)
				throw new InvalidOperationException("You can not use a request twice.");
		}

		public override string ToString()
		{
			return string.Format("{0}: {1} {2} {3}.", Client.Client.RemoteEndPoint, Method, Path, Version);
		}
	}
}
