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
		TcpClient client;
		NetworkStream stream;
		ReadOnlyNetworkStream body;
		TextReader reader;
		LinkedList<string> lines;
		bool disposed;
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

		public IEnumerable<string> Lines { get; private set; }
		public IEnumerable<string> HeaderLines { get; private set; }
		public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

		public bool HeadersCompleted { get { return body != null; } }

		public Request(TcpClient newClient, int port, Func<int, string> statusMessageFetcher)
		{
			GetStatusMessage = statusMessageFetcher;

			client = newClient;
			stream = client.GetStream();
			reader = (TextReader)new StreamReader(stream);
			Port = port;

			lines = new LinkedList<string>();

			Lines = lines;
			HeaderLines = lines.TakeWhile(line => line != "\n");
			Headers =
				HeaderLines
				.Select(line =>
				{
					var parts = line.Split(new[] { ':' }, 2);
					if (parts.Length == 2) {
						return new KeyValuePair<string, string>(parts[0], parts[1]);
					}
					return default(KeyValuePair<string, string>);
				});

			var firstLine = FirstLineParser.Match(reader.ReadLine());
			Method = firstLine.Groups["Method"].ToString();
			Path = firstLine.Groups["Path"].ToString();
			Version = firstLine.Groups["Version"].ToString();
		}

		public Request(TcpClient newClient, int port)
			: this(newClient, port, status => (String)Properties.Settings.Default["Status" + status.ToString()])
		{
		}

		public HeaderBuilder Respond(string version, int status, string statusMessage, long contentLength)
		{
			if (response != null)
				throw new Exception("You cannot instantiate a response twice or more times.");
			var writer = (TextWriter)new StreamWriter(stream);
			writer.WriteLine("{0} {1} {2}", version, status, statusMessage);
			writer.Flush();
			response = new HeaderBuilder(new WriteOnlyStreamWrapper(stream));
			response.ContentLength = contentLength;
			return response;
		}
		public HeaderBuilder Respond(string version, int status, long contentLength)
		{
			return Respond(version, status, GetStatusMessage(status), contentLength);
		}

		public void CompleteHeaders()
		{
			while (FetchHeader()) ;
			body = new ReadOnlyNetworkStream(stream);
		}

		public bool FetchHeader()
		{
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
				if (body == null) CompleteHeaders();
				return body;
			}
		}

		public void Dispose()
		{
			if (!disposed) {
				if (stream != null) stream.Dispose();
				if (client != null) client.Close();
				if(response != null) response.Dispose();
				disposed = true;
			}
		}

		public static Func<int, string> GetStatusMessage { get; set; }
	}
}
