using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Flow
{
	public class Request : IDisposable
	{
		TcpClient client;
		NetworkStream stream;
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
		}

		public void StreamToEnd()
		{

		}

		public void StreamNext()
		{

		}

		public bool StreamDataAvailable
		{
			get
			{
				return stream.DataAvailable;
			}
		}

		protected enum StreamingState
		{
			Headers,
			Body
		}

		public void Dispose()
		{
			
		}
	}
}
