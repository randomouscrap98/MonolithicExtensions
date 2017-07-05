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
    }
}
