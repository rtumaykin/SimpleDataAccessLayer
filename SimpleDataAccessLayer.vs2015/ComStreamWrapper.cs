using System;
using System.IO;
using Microsoft.VisualStudio.OLE.Interop;

namespace SimpleDataAccessLayer.vs2015
{
    /// <summary>
    /// Wrapper around an IStream object.  Stream should really implement this, as it's pretty cookie-cutter.  But it doesn't.
    /// </summary>
    public class StreamEater : Stream
    {
        private readonly IStream _iStream;

        public StreamEater(IStream streamFood)
        {
            _iStream = streamFood;
        }

        public override bool CanRead => _iStream != null;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        {
            _iStream.Commit(0);
        }

        public override long Length
        {
            get
            {
                STATSTG[] stat = new STATSTG[1];
                _iStream.Stat(stat, (uint)STATFLAG.STATFLAG_DEFAULT);

                return (long)stat[0].cbSize.QuadPart;
            }
        }

        public override long Position
        {
            get
            {
                return Seek(0, SeekOrigin.Current);
            }

            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            uint byteCounter;
            byte[] b = buffer;

            if (offset != 0)
            {
                b = new byte[buffer.Length - offset];
                buffer.CopyTo(b, 0);
            }

            _iStream.Read(b, (uint)count, out byteCounter);

            if (offset != 0)
            {
                b.CopyTo(buffer, offset);
            }

            return (int)byteCounter;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var l = new LARGE_INTEGER();
            ULARGE_INTEGER[] ul = { new ULARGE_INTEGER() };
            l.QuadPart = offset;
            _iStream.Seek(l, (uint)origin, ul);
            return (long)ul[0].QuadPart;
        }

        public override void SetLength(long value)
        {
            if (!CanWrite)
                throw new InvalidOperationException();

            var ul = new ULARGE_INTEGER {QuadPart = (ulong) value};
            _iStream.SetSize(ul);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            else if (!CanWrite)
                throw new InvalidOperationException();

            if (count > 0)
            {

                byte[] b = buffer;

                if (offset != 0)
                {
                    b = new byte[buffer.Length - offset];
                    buffer.CopyTo(b, 0);
                }

                uint byteCounter;
                _iStream.Write(b, (uint)count, out byteCounter);
                if (byteCounter != count)
                    throw new IOException("Failed to write the total number of bytes to IStream!");

                if (offset != 0)
                {
                    b.CopyTo(buffer, offset);
                }
            }
        }
    }
}
