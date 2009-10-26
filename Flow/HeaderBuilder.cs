using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow
{
	public class HeaderBuilder : IDisposable
	{
		internal Stream Target;
		internal TextWriter Writer;

		public bool Finished { get; private set; }

		internal HeaderBuilder(Stream target)
		{
			this.Target = target;
			Writer = (TextWriter)new StreamWriter(target);
		}

		public HeaderBuilder Add(string key, string value)
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			Writer.WriteLine("{0}: {1}", key, value);
			Writer.Flush();
			return this;
		}

		public HeaderBuilder Add(IEnumerable<KeyValuePair<string, string>> headers)
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			foreach (var header in headers) {
				Add(headers);
			}
			return this;
		}

		public HeaderBuilder Add(KeyValuePair<string, string> header)
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			return Add(header.Key, header.Value);
		}

		public HeaderBuilder Flush()
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			Writer.Flush();
			return this;
		}

		public Stream Finish()
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			Writer.WriteLine();
			Flush();
			Finished = true;
			return Target;
		}

		public void Dispose()
		{
			if (!Finished) Finish();
		}
	}
}
