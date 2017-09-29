using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace MonolithicExtensions.Portable
{
    public static class TypeExtensions
    {
        //public static string GetRealTypeName(this Type t)
        //{
        //    if (!t.IsGenericType)
        //        return t.Name;

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
        //    sb.Append('<');
        //    bool appendComma = false;
        //    foreach (Type arg in t.GetGenericArguments())
        //    {
        //        if (appendComma) sb.Append(',');
        //        sb.Append(GetRealTypeName(arg));
        //        appendComma = true;
        //    }
        //    sb.Append('>');
        //    return sb.ToString();
        //}
    }

    public static class IntegerExtensions
    {
        /// <summary>
        /// Convert integer into a guid where the rightmost characters in the string representation
        /// are the integer and the rest of the characters (to the left) are zeros. These guids are
        /// terrible, but are useful for testing.
        /// </summary>
        /// <param name="Integer"></param>
        /// <returns></returns>
        public static Guid ToGuid(this int Integer)
        {
            return new Guid(Integer.ToString().PadLeft(32, '0'));
        }
    }

    public static class ByteExtensions
    {
        private const byte One = 1;

        #region Unnecessary
        public static byte[] BitReverseTable = new byte[] {
        0x0, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
        0x8, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
        0x4, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
        0xC, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
        0x2, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2, 0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
        0xA, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
        0x6, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
        0xE, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
        0x1, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
        0x9, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
        0x5, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
        0xD, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
        0x3, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
        0xB, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB, 0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
        0x7, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
        0xF, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F, 0xFF
    };
        #endregion

        /// <summary>
        /// Reverse the bits in a byte using a loop rather than a lookup table. This is slow.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static byte ReverseBits(this byte original)
        {
            byte result = 0;
            for (byte i = 0; i < 8; i++)
                result = (byte)((result << 1) | ((original >> i) & One));
            return result;
        }

        /// <summary>
        /// Retrieve LSB ordered bytes from the given 8 byte value. Optionally retrieve only the specified
        /// number of bytes (will pad if larger than 8)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static byte[] GetLSBArrayFromValue(Int64 value, int amount = 8)
        {
            var result = new byte[amount];
            for (int i = 0; i < amount; i++)
                result[i] = (byte)((value >> (i * 8)) & 0xFF);
            return result;
        }

        /// <summary>
        /// Retrieve the 8 byte single value from the array. Alternatively, retrieve only up to the given amount of bytes
        /// starting at the given position
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Int64 GetValueFromLSBArray(byte[] data, int offset = 0, int length = 8)
        {
            Int64 result = 0;
            for (int i = 0; i < Math.Min(length, data.Length - offset); i++)
                result += data[offset + i] * (Int64)(Math.Pow(256, i));
            return result;
        }

    }

    public static class StringExtensions
    {
        /// <summary>
        /// Convert a CamelCasedString to an equivalent Spaced String by inserting spaces at the boundaries
        /// between lowercase and uppercase letters
        /// </summary>
        /// <param name="Original"></param>
        /// <returns></returns>
        public static string CamelCaseToSpaced(this string Original)
        {
            return Regex.Replace(Original, "([a-z])([A-Z])", "$1 $2");
        }

        /// <summary>
        /// Given a non plural word, this will pluralize it IF the given count is plural
        /// </summary>
        /// <param name="base"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Pluralize(this string baseString, double count)
        {
            if (count == 1)
                return baseString;
            else
                return baseString + "s";
        }

        /// <summary>
        /// Searches for various meanings of the string to try to parse a boolean out of it. Accepts
        /// true, false, t, f, yes, no, y, n, and numbers
        /// </summary>
        /// <param name="s"></param>
        /// <param name="cstyle"></param>
        /// <returns></returns>
        public static bool ToBoolean(this string s, bool cstyle = false)
        {
            s = s.Trim();
            bool bresult;
            if (Boolean.TryParse(s, out bresult)) return bresult;
            long lresult;
            if (Int64.TryParse(s, out lresult))
            {
                if (cstyle)
                    return lresult != 0;
                else
                    return lresult > 0;
            }
            string sl = s.ToLower();
            if (sl == "t" || sl == "y" || sl == "yes") return true;
            if (sl == "f" || sl == "n" || sl == "no") return false;
            throw new InvalidOperationException("Could Not cast string to boolean");
        }
    }

    public static class HumanReadableExtensions
    {
        public enum ByteUnit
        {
            B = 0,
            KiB,
            MiB,
            GiB,
            TiB,
            PiB,
            EiB,
            ZiB,
            YiB,
        }

        /// <summary>
        /// Convert a given amount of bytes into human readable bytes with an appropriate unit. For instance,
        /// 74567 becomes 72.8KiB.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static string ToByteUnits(this long size, int decimals = 1)
        {
            var raw = ToByteUnitsRaw(size);
            if (raw.Item2 == ByteUnit.B) decimals = 0;
            return String.Format("{0:F" + decimals + "}{1}", raw.Item1, raw.Item2);
        }

        /// <summary>
        /// Convert given amount of bytes to the amount of best-fit ByteUnit
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Tuple<double, ByteUnit> ToByteUnitsRaw(long size)
        {
            var orderOfMagnitude = 0;
            double finalSize = size;
            var maxMagnitude = Enum.GetValues(typeof(ByteUnit)).Length - 1;
            var magnitudeScale = 1024;

            while (finalSize > magnitudeScale && orderOfMagnitude < maxMagnitude)
            {
                finalSize /= magnitudeScale;
                orderOfMagnitude += 1;
            }

            return Tuple.Create(finalSize, (ByteUnit)orderOfMagnitude);
        }

        /// <summary>
        /// Converts a timespan into an english phrase representing the most logical unit. For instance,
        /// it might produce the phrase "5 hours" or "1 year"
        /// </summary>
        /// <param name="span"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static string ToSimplePhrase(this TimeSpan span, int decimalPlaces = 0)
        {
            string unit = "";
            double value = 0;

            //The MOST we'll do is years
            if (span.TotalDays > 365)
            {
                unit = "year";
                value = span.TotalDays / 365.0;
            }
            else if (span.TotalDays >= 1)
            {
                unit = "day";
                value = span.TotalDays;
            }
            else if (span.TotalHours >= 1)
            {
                unit = "hour";
                value = span.TotalHours;
            }
            else if (span.TotalMinutes >= 1)
            {
                unit = "minute";
                value = span.TotalMinutes;
            }
            else if (span.TotalSeconds >= 1)
            {
                unit = "second";
                value = span.TotalSeconds;
            }
            else if (span.TotalMilliseconds >= 1)
            {
                unit = "millisecond";
                value = span.TotalMilliseconds;
            }

            if (decimalPlaces == 0)
            {
                var intValue = (int)Math.Round(value);
                return String.Format("{0} {1}", intValue, unit.Pluralize(intValue));
            }
            else
            {
                return String.Format("{0:F" + decimalPlaces + "} {1}", value, unit.Pluralize(value));
            }
        }
    }

    /// <summary>
    /// Used as a return type in environments which cannot pass exceptions (such as WCF). The exception is
    /// captured and stored, then thrown when accessed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CapturedExceptionResult<T>
    {
        public CapturedExceptionResult() { /*Just because it's needed?*/  }
        public CapturedExceptionResult(Exception ex) { this.ThrownException = ex; }
        public CapturedExceptionResult(T result, Exception ex = null) : this(ex) { this.Result = result; }

        public T Result { get; } = default(T);
        public Exception ThrownException { get; } = null;

        public bool HasException
        {
            get { return ThrownException != null; }
        }

        /// <summary>
        /// Returns result, but throws stored exception if there is one.
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            if (HasException)
                throw ThrownException;
            else
                return Result;
        }
    }
}