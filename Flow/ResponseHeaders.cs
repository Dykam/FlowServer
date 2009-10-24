using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow
{
	public class ResponseHeaders
	{
		Stream target;
		TextWriter writer;

		public bool Finished { get; private set; }

		internal ResponseHeaders(Stream target)
		{
			this.target = target;
			writer = new StreamWriter(target);
		}
		public ResponseHeaders Add(string key, string value)
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			writer.WriteLine("{0}: {2}", key, value);
			return this;
		}

		public ResponseHeaders Add(KeyValuePair<string, string> header)
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			return Add(header.Key, header.Value);
		}

		public Stream Finish()
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");
			Finished = true;
			return target;
		}
	}
}
