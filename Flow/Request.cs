using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Flow
{
	public class Request : IDisposable
	{
		const string defaultHtppVersion = "HTTP/1.1";

		NetworkStream stream;
		ReadOnlyNetworkStream body;
		TextReader reader;
		LinkedList<string> lines;
		bool disposed;
		String httpVersion;
		HeaderBuilder response;

		static Regex firstLineParser;
		internal static Regex FirstLineParser
		{
			get
			{
				return firstLineParser == null ? firstLineParser = new Regex("^(?<Method>[A-Za-z]+) (?<Path>.*) (?<Version>[^ ]+)$", RegexOptions.Compiled) : firstLineParser;
			}
		}

		public string Method { get; private set; }
		public string Path { get; private set; }
		public string Version { get; private set; }

		public int Port { get; private set; }
		public TcpClient Client { get; private set; }

		public Func<int, string> GetStatusMessage { get; set; }

		public bool Accepted { get; private set; }

		public IEnumerable<string> Lines { get; private set; }
		public IEnumerable<string> HeaderLines { get; private set; }
		public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

		public bool HeadersCompleted { get { return body != null; } }

		public Request(TcpClient newClient, int port, Func<int, string> statusMessageFetcher, string httpVersion)
		{
			GetStatusMessage = statusMessageFetcher;
			this.httpVersion = httpVersion;

			Client = newClient;
			stream = Client.GetStream();
			reader = (TextReader)new StreamReader(stream);
			Port = port;

			lines = new LinkedList<string>();

			Lines = lines;
			HeaderLines = lines.TakeWhile(line => line != "\n");
			Headers =
				from line in HeaderLines
				let parts = line.Split(new[] { ':' }, 2)
				where parts.Length == 2
				select new KeyValuePair<string, string>(parts[0], parts[1]);

			string firstLine = reader.ReadLine();
			if (!string.IsNullOrEmpty(firstLine)) {
				var firstLineMatch = FirstLineParser.Match(firstLine);
				Method = firstLineMatch.Groups["Method"].ToString();
				Path = firstLineMatch.Groups["Path"].ToString();
				Version = firstLineMatch.Groups["Version"].ToString();
			}
		}
		public Request(TcpClient newClient, int port, Func<int, string> statusMessageFetcher)
			: this(newClient, port, statusMessageFetcher, defaultHtppVersion)
		{
		}
		public Request(TcpClient newClient, int port, string httpVersion)
			: this(newClient, port, status => (String)Properties.Settings.Default["Status" + status.ToString()], httpVersion)
		{
		}

		public Request(TcpClient newClient, int port)
			: this(newClient, port, defaultHtppVersion)
		{
		}

		public Request Accept()
		{
			Accepted = true;
			return this;
		}

		public HeaderBuilder Respond(string version, int status, string statusMessage)
		{
			if (!Accepted)
				throw new Exception("You need to accept the request first.");
			if (response != null)
				throw new Exception("You cannot instantiate a response twice or more times.");
			var writer = (TextWriter)new StreamWriter(stream);
			writer.WriteLine("{0} {1} {2}", version, status, statusMessage);
			writer.Flush();
			response = new HeaderBuilder(new WriteOnlyStreamWrapper(stream));
			return response;
		}
		public HeaderBuilder Respond(string version, int status)
		{
			if (!Accepted)
				throw new Exception("You need to accept the request first.");
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

		public Request CompleteHeaders()
		{
			if (!Accepted)
				throw new Exception("You need to accept the request first.");
			while (FetchHeader()) ;
			body = new ReadOnlyNetworkStream(stream);
			return this;
		}

		public bool FetchHeader()
		{
			if (!Accepted)
				throw new Exception("You need to accept the request first.");
			if (!HeadersCompleted) {
				var line = reader.ReadLine();
				if (String.IsNullOrEmpty(line)) {
					body = new ReadOnlyNetworkStream(stream);
				} else {
					lines.AddLast(line);
				}
			}
			return !HeadersCompleted;
		}

		public ReadOnlyNetworkStream Body
		{
			get
			{
				if (!Accepted)
					throw new Exception("You need to accept the request first.");
				if (body == null) CompleteHeaders();
				return body;
			}
		}

		public void Dispose()
		{
			if (!Accepted)
				throw new Exception("You need to accept the request first.");
			if (!disposed) {
				if (stream != null) stream.Dispose();
				if (Client != null) Client.Close();
				if(response != null) response.Dispose();
				disposed = true;
			}
		}
	}
}
