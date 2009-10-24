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
		LinkedList<KeyValuePair<String, String>> headers;
		internal ResponseHeaders(Stream target)
		{
			this.target = target;
		}
		public ResponseHeaders Add(string key, string value)
		{
			return this;
		}

		public ResponseHeaders Add(KeyValuePair<string, string> header)
		{
			return this;
		}

		public Stream End()
		{
			return target;
		}
	}
}
