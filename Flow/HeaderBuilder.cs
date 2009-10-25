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

		public long ContentLength { get; set; }

		public CustomHeaderBuilder Custom { get; private set; }

		internal HeaderBuilder(Stream target)
		{
			this.Target = target;
			Writer = (TextWriter)new StreamWriter(target);
			Custom = new CustomHeaderBuilder(this);
		}

		public Stream Finish()
		{
			if (Finished)
				throw new Exception("Headers cannot be changed after ending.");

			Custom.Flush();

			Writer.WriteLine("Content-Length: " + ContentLength.ToString());
			Writer.WriteLine();
			Writer.Flush();
			Finished = true;
			return Target;
		}

		public void Dispose()
		{
			if (!Finished) Finish();
		}
	}
}
