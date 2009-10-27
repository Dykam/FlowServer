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
		bool disposed;
		String httpVersion;
		HeaderBuilder response;
		Dictionary<string, string> headers;

		static readonly Regex firstLineParser = new Regex("^(?<Method>[A-Za-z]+) (?<Path>.*) (?<Version>[^ ]+)$", RegexOptions.Compiled);

		public string Method { get; private set; }
		public string Path { get; private set; }
		public string Version { get; private set; }

		public int Port { get; private set; }
		public TcpClient Client { get; private set; }

		public Func<int, string> GetStatusMessage { get; set; }

		public bool Accepted { get; private set; }


		public ReadOnlyDictionary<String, String> Headers { get; private set; }

		public bool HeadersCompleted { get { return body != null; } }

		public Request(TcpClient newClient, int port, Func<int, string> statusMessageFetcher, string httpVersion)
		{
			GetStatusMessage = statusMessageFetcher;
			this.httpVersion = httpVersion;

			Client = newClient;
			stream = Client.GetStream();
			reader = (TextReader)new StreamReader(stream);
			Port = port;

			headers = new Dictionary<string, string>();

			Headers = new ReadOnlyDictionary<string, string>(headers);

			if (!fetchRequestLine()) {
				throw new Exception("Error during fetching of the request line.");
			}
			if (!fetchHeaders()) {
				throw new Exception("Error during fetching of the headers.");
			}
		}

		bool fetchHeaders()
		{
			string line = string.Empty;
			string previous = string.Empty ;
			while (true) {
				line = reader.ReadLine();
				if (line == "\n")
					return addLine(previous);
				else if (line.StartsWith(" ") || line.StartsWith("\t"))
					previous += line.Trim();
				else {
					addLine(previous);
					previous = line;
				}
			}
		}

		static readonly char[] seperator = new[] { ':' };

		bool addLine(string line)
		{
			if (string.IsNullOrEmpty(line))
				return false;

			var parts = line.Split(seperator, 2);
			if (parts.Length != 2)
				return false;

			headers.Add(parts[0].Trim(), parts[1].Trim());
			return true;
		}

		bool fetchRequestLine()
		{
			string firstLine;
			do {
				firstLine = reader.ReadLine();
				if (string.IsNullOrEmpty(firstLine))
					return false;
			} while (firstLine == "\n" || firstLine == "\r\n" || firstLine == "\r");
			if (!string.IsNullOrEmpty(firstLine)) {
				var firstLineMatch = firstLineParser.Match(firstLine);
				Method = firstLineMatch.Groups["Method"].ToString();
				Path = firstLineMatch.Groups["Path"].ToString();
				Version = firstLineMatch.Groups["Version"].ToString();
			}
			return true;
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

		public HeaderBuilder Accept(string version, int status, string statusMessage)
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
		public HeaderBuilder Accept(string version, int status)
		{
			if (!Accepted)
				throw new Exception("You need to accept the request first.");
			return Accept(version, status, GetStatusMessage(status));
		}
		public HeaderBuilder Accept(int status)
		{
			return Accept(httpVersion, status);
		}
		public HeaderBuilder Accept(int status, string statusMessage)
		{
			return Accept(httpVersion, status, statusMessage);
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
