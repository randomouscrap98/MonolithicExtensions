using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.Portable
{
    public static class StreamExtensions
    {
        /// <summary>
        /// A (probably useless) way to easily read a single byte asynchronously from the given stream.
        /// The user MUST provide a buffer of at least size 1 to use as a temporary storage, 
        /// otherwise the performance drops significantly
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="tempBuffer"></param>
        /// <returns></returns>
        public static async Task<byte> ReadByteAsync(Stream dataStream, byte[] tempBuffer)
        {
            while (await dataStream.ReadAsync(tempBuffer, 0, 1) < 1) { }
            return tempBuffer[0];
        }

        /// <summary>
        /// Perform a synchronous read of a single byte with the given timeout. Restores the ReadTimeout property afterwards.
        /// Negative timeouts will immediately throw a timeout exception.
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="timeout"></param>
        /// <param name="reusableBuffer"></param>
        /// <returns></returns>
        public static byte ReadByteWithTimeout(this Stream dataStream, TimeSpan timeout, byte[] reusableBuffer)
        {
            if (!dataStream.CanTimeout)
                return dataStream.ReadByteWithTimeoutManual(timeout);
            if (timeout.TotalMilliseconds <= 0)
                throw new TimeoutException("ReadByteWithTimeout timed out immediately due to a non-positive timeout!");

            var originalTimeout = dataStream.ReadTimeout;

            try
            {
                dataStream.ReadTimeout = Convert.ToInt32(timeout.TotalMilliseconds);
                var result = dataStream.Read(reusableBuffer, 0, 1);
                if (result <= 0)
                    throw new EndOfStreamException("The stream ended while reading a byte with timeout! There is no more data!");
                else
                    return reusableBuffer[0];
            }
            finally
            {
                dataStream.ReadTimeout = originalTimeout;
            }
        }

        public static byte ReadByteWithTimeout(this Stream dataStream, TimeSpan timeout)
        {
            return dataStream.ReadByteWithTimeout(timeout, new byte[1]);
        }

        /// <summary>
        /// Manually waits to read a single byte from the given stream without using built-in timeouts (useful
        /// if the stream doesn't support timeouts). Not guaranteed to return in the given timeout!
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static byte ReadByteWithTimeoutManual(this Stream dataStream, TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds < 0)
                throw new TimeoutException("ReadByteTimeoutManual timed out immediately due to a non-positive timeout!");

            DateTime start = DateTime.Now;
            int result = dataStream.ReadByte();
            if ((DateTime.Now - start) > timeout)
                throw new TimeoutException($"Read a byte but it took longer than the manual timeout of {timeout}");
            else if(result < 0)
                throw new EndOfStreamException("The stream ended while reading a byte with timeout! There is no more data!");
            else
                return (byte)result;
        }
    }

    /// <summary>
    /// Provides a single stream from input and output streams. Some systems give separate streams
    /// for input and output, but it may be easier to work with a single stream.
    /// </summary>
    public class DualStream : Stream
    {
        /// <summary>
        /// The stream that you READ data from
        /// </summary>
        public Stream InputStream;

        /// <summary>
        /// The stream that you WRITE data to
        /// </summary>
        public Stream OutputStream;

        public override bool CanRead
        {
            get { return InputStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return InputStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return OutputStream.CanWrite; }
        }

        public override bool CanTimeout
        {
            get { return InputStream.CanTimeout && OutputStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return InputStream.ReadTimeout; }
            set { InputStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return OutputStream.WriteTimeout; }
            set { OutputStream.WriteTimeout = value; }
        }

        public override long Length
        {
            get { return InputStream.Length; }
        }

        public override long Position
        {
            get { return InputStream.Position; }
            set { InputStream.Position = value; }
        }

        public override void Flush()
        {
            InputStream.Flush();
            OutputStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return InputStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InputStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InputStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            OutputStream.Write(buffer, offset, count);
        }
    }
}
