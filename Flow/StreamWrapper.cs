using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace Flow
{
	public class StreamWrapper<T> : Stream
		where T : Stream
	{
		long offset;
		protected T Source;
		internal StreamWrapper(T source)
		{
			this.Source = source;
			try {
				offset = source.Position;
			}
			catch (NotSupportedException) {
				// Not implemented
			}
		}

		public override bool CanRead
		{
			get { return Source.CanRead; }
		}

		public override bool CanSeek
		{
			get { return Source.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return Source.CanWrite; }
		}

		public override void Flush()
		{
			Source.Flush();
		}

		public override long Length
		{
			get { return Source.Length - offset; }
		}

		public override long Position
		{
			get
			{
				return Source.Position - offset;
			}
			set
			{
				Source.Position = value + offset;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return Source.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return Source.Seek(offset + offset, origin);
		}

		public override void SetLength(long value)
		{
			Source.SetLength(value + offset);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Source.Write(buffer, offset, count);
		}
	}

	public class ReadOnlyNetworkStream : StreamWrapper<NetworkStream>
	{
		internal ReadOnlyNetworkStream(NetworkStream source)
			: base(source)
		{
		}
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("This stream is read only.");
		}
		public override void WriteByte(byte value)
		{
			throw new NotSupportedException("This stream is read only.");
		}

		public bool DataAvailable
		{
			get
			{
				return Source.DataAvailable;
			}
		}
	}

	class WriteOnlyStreamWrapper : StreamWrapper<Stream>
	{
		public WriteOnlyStreamWrapper(Stream source)
			: base(source)
		{
		}
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("This stream is write only.");
		}
		public override int ReadByte()
		{
			throw new NotSupportedException("This stream is write only.");
		}
	}
}
