using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow
{

	public class CustomHeaderBuilder
	{

		public HeaderBuilder Required { get; private set; }

		internal CustomHeaderBuilder(HeaderBuilder parent)
		{
			Required = parent;
		}
		public CustomHeaderBuilder Add(string key, string value)
		{
			if (Required.Finished)
				throw new Exception("Headers cannot be changed after ending.");
			Required.Writer.WriteLine("{0}: {1}", key, value);
			Required.Writer.Flush();
			return this;
		}

		public CustomHeaderBuilder Add(IEnumerable<KeyValuePair<string, string>> headers)
		{
			if (Required.Finished)
				throw new Exception("Headers cannot be changed after ending.");
			foreach (var header in headers) {
				Add(headers);
			}
			return this;
		}

		public CustomHeaderBuilder Add(KeyValuePair<string, string> header)
		{
			if (Required.Finished)
				throw new Exception("Headers cannot be changed after ending.");
			return Add(header.Key, header.Value);
		}

		public CustomHeaderBuilder Flush()
		{
			if (Required.Finished)
				throw new Exception("Headers cannot be changed after ending.");
			Required.Writer.Flush();
			return this;
		}

		public Stream Finish()
		{
			if (Required.Finished)
				throw new Exception("Headers cannot be changed after ending.");
			return Required.Finish();
		}
	}
}
