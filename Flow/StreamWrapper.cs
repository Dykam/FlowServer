using System;
using System.IO;
using System.Net.Sockets;

namespace Flow
{
    public class StreamWrapper<T> : Stream
        where T : Stream
    {
        private readonly long _offset;
        protected T Source;

        internal StreamWrapper(T source)
        {
            Source = source;
            try
            {
                _offset = source.Position;
            }
            catch (NotSupportedException)
            {
                // Not implemented
            }
        }

        public override bool CanRead => Source.CanRead;

        public override bool CanSeek => Source.CanSeek;

        public override bool CanWrite => Source.CanWrite;

        public override long Length => Source.Length - _offset;

        public override long Position
        {
            get { return Source.Position - _offset; }
            set { Source.Position = value + _offset; }
        }

        public override void Flush()
        {
            Source.Flush();
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
            Source.SetLength(value + _offset);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Source.Write(buffer, offset, count);
        }
    }

    /// <remarks>
    ///     Makes the base stream appear readonly.
    /// </remarks>
    public class ReadOnlyNetworkStream : StreamWrapper<NetworkStream>
    {
        internal ReadOnlyNetworkStream(NetworkStream source)
            : base(source)
        {
        }

        public override bool CanWrite => false;

        public bool DataAvailable => Source.DataAvailable;

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This stream is read only.");
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException("This stream is read only.");
        }
    }

    /// <remarks>
    ///     Makes the base stream appear writeonly.
    /// </remarks>
    internal class WriteOnlyStreamWrapper : StreamWrapper<Stream>
    {
        public WriteOnlyStreamWrapper(Stream source)
            : base(source)
        {
        }

        public override bool CanRead => false;

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