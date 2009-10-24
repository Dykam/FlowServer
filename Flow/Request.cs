using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Flow
{
	public class Request : IDisposable
	{
		TcpClient client;
		NetworkStream stream;
		Stream body;
		TextReader reader;
		LinkedList<string> lines;
		bool disposed;

		public string Method { get; private set; }
		public string Version { get; private set; }
		public string Path { get; private set; }

		public IEnumerable<string> Lines { get; private set; }
		public IEnumerable<string> HeaderLines { get; private set; }
		public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

		public bool HeadersCompleted { get { return body == null; } }

		public Request(TcpClient newClient)
		{
			client = newClient;
			stream = client.GetStream();
			reader = (TextReader)new StreamReader(stream);

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
		}

		public ResponseHeaders Respond(string version, int status, string statusMessage)
		{
			return new ResponseHeaders(new WriteOnlyStreamWrapper(stream));
		}

		public void CompleteHeaders()
		{
			while (StreamHeaderLine()) ;
			body = new StreamWrapper(stream);
		}

		public bool StreamHeaderLine()
		{
			if (!HeadersCompleted) {
				var line = reader.ReadLine();
				if (String.IsNullOrEmpty(line)) {
					body = new ReadOnlyStreamWrapper(stream);
				} else {
					lines.AddLast(line);
				}
			}
			return !HeadersCompleted;
		}

		public Stream Body
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
				stream.Dispose();
				client.Close();
				disposed = true;
			}
		}
	}
}
