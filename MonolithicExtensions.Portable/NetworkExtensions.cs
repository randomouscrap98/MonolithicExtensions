
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace MonolithicExtensions.Portable
{
    public static class NetworkServices
    {
        public const uint ITUV41Polynomial = 0x11021;

        public static uint ReverseBits(uint dword)
        {
            dword = ((dword & 0x55555555u) << 1) | ((dword >> 1) & 0x55555555u);
            dword = ((dword & 0x33333333u) << 2) | ((dword >> 2) & 0x33333333u);
            dword = ((dword & 0xf0f0f0fu) << 4) | ((dword >> 4) & 0xf0f0f0fu);
            dword = (dword << 24) | ((dword & 0xff00u) << 8) | ((dword >> 8) & 0xff00u) | (dword >> 24);
            return dword;
        }

        /// <summary>
        /// Produces CRC16 for ITU V.41 recommendation. Polynomial is 0x11021 and initial
        /// value is 0, but bits are read in reverse order (least significant first)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] CRC16_KERMIT(IList<byte> data)
        {
            return CRC16_ReverseGeneral(data, ITUV41Polynomial, 0);
        }

        /// <summary>
        /// Produces XMODEM CRC16 checksums which have a polynomial
        /// of 0x11021 and an initial value of 0x0000
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] CRC16_XMODEM(IList<byte> data)
        {
            return CRC16_ForwardGeneral(data, ITUV41Polynomial, 0);
        }

        /// <summary>
        /// The "false" CCITT algorithm which uses &HFFFF as the initial value. (MSB)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] CRC16_CCITTFalse(IList<byte> data)
        {
            return CRC16_ForwardGeneral(data, ITUV41Polynomial, 0xffff);
        }

        /// <summary>
        /// Produces CRC16 checksums with the given polynomial and initial value. No reversals,
        /// final xors, or anything of that sort is performed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="polynomial"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        public static byte[] CRC16_ForwardGeneral(IList<byte> data, uint polynomial, uint init)
        {
            uint result = init;
            dynamic bitCount = data.Count * 8;
            const uint one = 1;
            for (int i = 0; i <= bitCount + 15; i++)
            {
                dynamic bit = bitCount - i - 1;
                if (bit >= 0)
                {
                    result = (uint)((result << 1) | ((data[i >> 3] >> (bit & 0x7)) & one));
                }
                else
                {
                    result <<= 1;
                }
                if ((result & 0x10000) > 0)
                    result = result ^ polynomial;
            }
            return BitConverter.GetBytes(result).Take(2).ToArray();
        }

        /// <summary>
        /// Produces CRC16 checksums with the given polynomial and initial value. The
        /// bits are read in LSB first rather than the norm
        /// </summary>
        /// <param name="data"></param>
        /// <param name="polynomial"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        public static byte[] CRC16_ReverseGeneral(IList<byte> data, uint polynomial, uint init)
        {
            uint result = init;
            dynamic bitCount = data.Count * 8;
            const uint one = 1;
            for (int i = 0; i <= bitCount + 15; i++)
            {
                if (i < bitCount)
                {
                    result = (uint)((result << 1) | ((data[i >> 3] >> (i & 0x7)) & one));
                }
                else
                {
                    result <<= 1;
                }
                if ((result & 0x10000) > 0)
                    result = result ^ polynomial;
            }
            return BitConverter.GetBytes(result).Take(2).Select(x => x.ReverseBits()).ToArray();
        }

        /// <summary>
        /// Produce a REGULAR CRC32 (the one used for Ethernet, GZip, PNG, etc.), or supply your own polynomial and initial value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="polynomial"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        public static byte[] CRC32(IList<byte> data, uint polynomial = 0x4c11db7, uint init = 0xffffffffu)
        {
            uint crc = init;
            foreach (uint dataByte in data)
            {
                uint currentByte = ReverseBits(dataByte);
                for (int i = 0; i <= 7; i++)
                {
                    if (((crc ^ currentByte) & 0x80000000u) > 0)
                    {
                        crc = (crc << 1) ^ polynomial;
                    }
                    else
                    {
                        crc = crc << 1;
                    }
                    currentByte <<= 1;
                }
            }
            return BitConverter.GetBytes(ReverseBits(~crc)).ToArray();
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
