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
		TextReader reader;
		StreamingState state;
		LinkedList<string> lines;

		public string Method { get; private set; }
		public string Version { get; private set; }
		public string Path { get; private set; }

		public IEnumerable<string> Lines { get; private set; }
		public IEnumerable<string> HeaderLines { get; private set; }
		public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

		public Boolean StreamedToEnd { get; private set; }

		public Request(TcpClient newClient)
		{
			client = newClient;
			stream = client.GetStream();
			reader = (TextReader)stream;

			state = StreamingState.Headers;

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
			builder = new StringBuilder();
		}

		public void StreamToEnd()
		{
			while (StreamLine()) ;
		}

		public bool StreamLine()
		{
			if (!StreamedToEnd) {
				var line = reader.ReadLine();
				if (String.IsNullOrEmpty(line))
					StreamedToEnd = true;
				else
					lines.AddLast(line);
			}
			return !StreamedToEnd;
		}

		protected enum StreamingState
		{
			Headers,
			Body
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
