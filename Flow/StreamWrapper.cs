/*   Copyright 2009 Dykam (kramieb@gmail.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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

	/// <remarks>
	/// Makes the base stream appear readonly.
	/// </remarks>
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

	/// <remarks>
	/// Makes the base stream appear writeonly.
	/// </remarks>
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
