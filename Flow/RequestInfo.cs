using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using Flow.Handlers;

namespace Flow
{
	public class RequestInfo : IDisposable
	{
		protected const string defaultHtppVersion = "HTTP/1.1";

		protected NetworkStream stream;
		protected ReadOnlyNetworkStream body;
		protected TextReader reader;
		protected bool disposed;
		protected String httpVersion;
		protected HeaderBuilder response;
		protected SmartDictionary<string, string> headers;

		protected static readonly Regex firstLineParser = new Regex("^(?<Method>[A-Za-z]+) (?<Path>.*) (?<Version>[^ ]+)$", RegexOptions.Compiled);

		RequestMethods method;
		public RequestMethods Method { get { return method; } }
		public string Path { get; private set; }
		public string Version { get; private set; }
		public ReadOnlySmartDictionary<String, String> Headers { get; private set; }

		public int Port { get; private set; }
		public TcpClient Client { get; private set; }
		public Router @Router { get; private set; }

		public Func<int, string> GetStatusMessage { get; set; }

		internal RequestInfo(Router router, TcpClient newClient, int port, Func<int, string> statusMessageFetcher, string httpVersion)
		{
			GetStatusMessage = statusMessageFetcher;
			this.httpVersion = httpVersion;
			@Router = router;
			body = new ReadOnlyNetworkStream(newClient.GetStream());

			Client = newClient;
			stream = Client.GetStream();
			reader = (TextReader)new StreamReader(stream);
			Port = port;

			headers = new SmartDictionary<string, string>();

			Headers = new ReadOnlySmartDictionary<string, string>(headers);

			if (!fetchRequestLine()) {
				throw new Exception("Error during fetching of the request line.");
			}
			if (!fetchHeaders()) {
				throw new Exception("Error during fetching of the headers.");
			}
		}
		internal RequestInfo(Router router, TcpClient newClient, int port, Func<int, string> statusMessageFetcher)
			: this(router, newClient, port, statusMessageFetcher, defaultHtppVersion)
		{
		}
		internal RequestInfo(Router router, TcpClient newClient, int port, string httpVersion)
			: this(router, newClient, port, status => (String)Properties.Settings.Default["Status" + status.ToString()], httpVersion)
		{
		}
		internal RequestInfo(Router router, TcpClient newClient, int port)
			: this(router, newClient, port, defaultHtppVersion)
		{
		}

		bool fetchHeaders()
		{
			string line = string.Empty;
			string previous = string.Empty;
			while (true) {
				line = reader.ReadLine();
				if (string.IsNullOrEmpty(line) || line == "\n")
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

			headers[parts[0].Trim()] = parts[1].Trim();
			return true;
		}

		bool fetchRequestLine()
		{
			string firstLine;
			do {
				if (!body.DataAvailable)
					return false;
				firstLine = reader.ReadLine();
				if (string.IsNullOrEmpty(firstLine))
					return false;
			} while (firstLine == "\n" || firstLine == "\r\n" || firstLine == "\r");
			if (!string.IsNullOrEmpty(firstLine)) {
				var firstLineMatch = firstLineParser.Match(firstLine);
				var method = firstLineMatch.Groups["Method"].ToString();
				if (enumTryParse(method, out this.method)) {
					this.method = RequestMethods.None;
				}
				Path = firstLineMatch.Groups["Path"].ToString();
				Path = Uri.UnescapeDataString(Path);
				Version = firstLineMatch.Groups["Version"].ToString();
			}
			return true;
		}

		public void Dispose()
		{
			if (!disposed) {
				if (stream != null) stream.Dispose();
				if (Client != null) Client.Close();
				if (response != null) response.Dispose();
				disposed = true;
			}
		}

		public static bool enumTryParse<T>(string strType, out T result)
		{
			string strTypeFixed = strType.Replace(' ', '_');
			if (Enum.IsDefined(typeof(T), strTypeFixed)) {
				result = (T)Enum.Parse(typeof(T), strTypeFixed, true);
				return true;
			} else {
				foreach (string value in Enum.GetNames(typeof(T))) {
					if (value.Equals(strTypeFixed, StringComparison.OrdinalIgnoreCase)) {
						result = (T)Enum.Parse(typeof(T), value);
						return true;
					}
				}
				result = default(T);
				return false;
			}
		}
	}
}
