using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Flow
{
	public class Request : RequestInfo
	{
		public bool Used { get; private set; }

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
		
		public HeaderBuilder Respond(string version, int status, string statusMessage, IEnumerable<KeyValuePair<string, string>> headers)
		{
			checkIfUsed();

			Used = true;
			var writer = (TextWriter)new StreamWriter(stream);
			writer.WriteLine("{0} {1} {2}", version, status, statusMessage);
			writer.Flush();
			response = new HeaderBuilder(new WriteOnlyStreamWrapper(stream));
			foreach (var header in headers)
				response.Add(headers);
			return response;
		}
		public HeaderBuilder Respond(string version, int status, IEnumerable<KeyValuePair<string, string>> headers)
		{
			return Respond(version, status, GetStatusMessage(status), headers);
		}
		public HeaderBuilder Respond(int status, IEnumerable<KeyValuePair<string, string>> headers)
		{
			return Respond(httpVersion, status, headers);
		}
		public HeaderBuilder Respond(int status, string statusMessage, IEnumerable<KeyValuePair<string, string>> headers)
		{
			return Respond(httpVersion, status, statusMessage, headers);
		}

		public HeaderBuilder Respond(string version, int status, string statusMessage)
		{
			checkIfUsed();

			var writer = (TextWriter)new StreamWriter(stream);
			writer.WriteLine("{0} {1} {2}", version, status, statusMessage);
			writer.Flush();
			response = new HeaderBuilder(new WriteOnlyStreamWrapper(stream));
			return response;
		}
		public HeaderBuilder Respond(string version, int status)
		{
			return Respond(version, status, GetStatusMessage(status));
		}
		public HeaderBuilder Respond(int status)
		{
			return Respond(httpVersion, status);
		}
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
			return string.Format("Request from {0}.", Client.Client.RemoteEndPoint);
		}
	}
}
