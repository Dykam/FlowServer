using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Flow
{
	class StreamWrapper : Stream
	{
		long offset;
		Stream source;
		public StreamWrapper(Stream source)
		{
			offset = source.Position;
		}

		public override bool CanRead
		{
			get { return source.CanRead; }
		}

		public override bool CanSeek
		{
			get { return source.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return source.CanWrite; }
		}

		public override void Flush()
		{
			source.Flush();
		}

		public override long Length
		{
			get { return source.Length - offset; }
		}

		public override long Position
		{
			get
			{
				return source.Position - offset;
			}
			set
			{
				source.Position = value + offset;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return source.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return source.Seek(offset + offset, origin);
		}

		public override void SetLength(long value)
		{
			source.SetLength(value + offset);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			source.Write(buffer, offset, count);
		}
	}
}
