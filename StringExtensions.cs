using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Loxifi
{
    /// <summary>
    ///
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns the portion of the source string after the last instance of the delimiter
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="fromText">The delimiter</param>
        /// <returns>The substring found after the last instance of the delimiter</returns>
        public static string? FromLast(this string s, string fromText)
        {
            if (s is null)
            {
                return null;
            }

            if (fromText is null)
            {
                throw new ArgumentNullException(nameof(fromText));
            }

            int i = s.LastIndexOf(fromText, StringComparison.OrdinalIgnoreCase);

            return i >= 0 ? s[(i + fromText.Length)..] : s;
        }

        /// <summary>
        /// Returns the portion of the source string after the last instance of the delimiter
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="fromText">The delimiter</param>
        /// <returns>The substring found after the last instance of the delimiter</returns>
        public static string? FromLast(this string s, char fromText) => s?[(s.LastIndexOf(fromText) + 1)..];

        private const string EMPTY_STRING_MESSAGE = "The string to find may not be empty";

        /// <summary>
        /// Returns the portion of the source string after the first instance of the delimiter
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="fromText">The delimiter</param>
        /// <param name="inclusive">A bool indicating whether or not the delimiter should be returned with the result</param>
        /// <param name="comparison">The string comparison to use when searching for a match</param>
        /// <returns>The substring found after the first instance of the delimiter</returns>
        public static string? From(this string s, string fromText, bool inclusive = false, StringComparison comparison = StringComparison.Ordinal)
        {
            if (s is null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(fromText))
            {
                throw new ArgumentException(EMPTY_STRING_MESSAGE, nameof(fromText));
            }

            int i = s.IndexOf(fromText, comparison);

            return i >= 0 ? inclusive ? s[i..] : s[(i + fromText.Length)..] : s;
        }

        /// <summary>
        /// Returns the rightmost portion of a string of a specified length
        /// </summary>
        /// <param name="str">The source string</param>
        /// <param name="count">The number of characters to return</param>
        /// <returns>The rightmost portion of the source string of a specified length</returns>
        public static string? Right(this string str, int count) => str?[^count..];

        /// <summary>
        /// Returns the leftmost portion of a string of a specified length
        /// </summary>
        /// <param name="str">The source string</param>
        /// <param name="count">The number of characters to return</param>
        /// <returns>A substring of the specified length from the source string</returns>
        public static string? Left(this string str, int count) => str?[..count];

        /// <summary>
        /// Splits a string on a delimiter
        /// </summary>
        /// <param name="input">The source string</param>
        /// <param name="spliton">The delimiter to split on</param>
        /// <param name="preserveSplit">A bool indicating whether or not to append the delimiter back to the results</param>
        /// <param name="options">String split options for the initial split</param>
        /// <returns>An array of strings</returns>
        public static string[]? Split(this string input, string spliton, bool preserveSplit = false, StringSplitOptions options = StringSplitOptions.None)
        {
            if (input is null)
            {
                return null;
            }

            string[] output = input.Split(new string[] { spliton }, options);

            if (preserveSplit)
            {
                output = output.Select(s => spliton + s).ToArray();
            }

            return output;
        }

        /// <summary>
        /// Splits a CamelCase string on each uppercase letter by adding a space in front of each
        /// </summary>
        /// <param name="str">The string to split</param>
        /// <returns>A string where each uppercase letter is preceded by a space</returns>
        public static string? SplitCamelCase(this string str)
        {
            if (str is null)
            {
                return null;
            }

            int uppers = 0;

            foreach (char c in str.Skip(1))
            {
                if (char.IsUpper(c))
                {
                    uppers++;
                }
            }

            char[] toReturn = new char[str.Length + uppers];

            toReturn[0] = str[0];

            int index = 1;
            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]))
                {
                    toReturn[index++] = ' ';
                }

                toReturn[index++] = str[i];
            }

            return new string(toReturn);
        }

        /// <summary>
        /// Returns the portion of a string up until the first instance of a delimiter
        /// </summary>
        /// <param name="s">The source</param>
        /// <param name="toText">The delimeter</param>
        /// <param name="inclusive">A bool indicating whether or not the delimiter should be included in the return</param>
        /// <param name="comparison">String comparison options</param>
        /// <returns>The portion of a string up until the first instance of a delimiter</returns>
        public static string? To(this string s, string toText, bool inclusive = false, StringComparison comparison = StringComparison.Ordinal)
        {
            if (s is null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(toText))
            {
                throw new ArgumentException(EMPTY_STRING_MESSAGE, nameof(toText));
            }

            int i = s.IndexOf(toText, comparison);

            return i >= 0 ? inclusive ? s[..(i + toText.Length)] : s[..i] : s;
        }

#if !NETSTANDARD2_0
        //https://medium.com/@SergioPedri/optimizing-string-count-all-the-way-from-linq-to-hardware-accelerated-vectorized-instructions-186816010ad9
        internal static int VectorizedCount(this string source, char c)
        {
            /* Get a reference to the first string character.
            * Strings are supposed to be immutable in .NET, so
            * in order to do this we first get a ReadOnlySpan<char>
            * from our string, and then use the MemoryMarshal.GetReference
            * API, which returns a mutable reference to the first
            * element of the input span. As a result, we now have
            * a mutable char reference to the first character in the string. */
            ReadOnlySpan<char> span = source.AsSpan();
            ref char r0 = ref MemoryMarshal.GetReference(span);
            int length = span.Length;
            int i = 0, result;

            /* As before, only execute the SIMD-enabled branch if the Vector<T> APIs
             * are hardware accelerated. Note that we're using ushort instead of char
             * in the Vector<T> type, because the char type is not supported.
             * But that is fine: ushort and char have the same byte size and the same
             * numerical range, and they behave exactly the same in this context. */
            if (Vector.IsHardwareAccelerated)
            {
                int end = length - Vector<ushort>.Count;

                // SIMD register all set to 0, to store partial results
                Vector<ushort> partials = Vector<ushort>.Zero;

                // SIMD register with the target character c copied in every position
                Vector<ushort> vc = new(c);

                for (; i <= end; i += Vector<ushort>.Count)
                {
                    // Get the reference to the current characters chunk
                    ref char ri = ref Unsafe.Add(ref r0, i);

                    /* Read a Vector<ushort> value from that offset, by
                     * reinterpreting the char reference as a ref Vector<ushort>.
                     * As with the previous example, doing this allows us to read
                     * the series of consecutive character starting from the current
                     * offset, and to load them in a single SIMD register. */

                    // vi = { text[i], ..., text[i + Vector<char>.Count - 1] }
                    Vector<ushort> vi = Unsafe.As<char, Vector<ushort>>(ref ri);

                    /* The Vector.Equals method sets each T item in a Vector<T> to
                     * either all 1s if the two elements match (as if we had used
                     * the == operator), or to all 0s if a pair doesn't match. */
                    Vector<ushort> ve = Vector.Equals(vi, vc);

                    /* First we load Vector<ushort>.One, which is a Vector<ushort> with
                     * just 1 in each position. Then we do a bitwise and with the
                     * previous result. Since matching values were all 1s, and non
                     * matching values were all 0s, we will have 1 in the position
                     * of pairs of values that were the same, or 0 otherwise. */
                    Vector<ushort> va = Vector.BitwiseAnd(ve, Vector<ushort>.One);

                    // Accumulate the partial results in each position
                    partials += va;
                }

                /* The dot product of a vector with a vector with 1 in each
                 * position results in the horizontal sum of all the values
                 * in the first vector, because:
                 *
                 * { a, b, c } DOT { 1, 1, 1 } = a * 1 + b * 1 + c * 1.
                 *
                 * So result will hold all the matching characters up to this point. */
                result = Vector.Dot(partials, Vector<ushort>.One);
            }
            else
            {
                result = 0;
            }

            // Iterate over the remaining characters and count those that match
            for (; i < length; i++)
            {
                bool equals = Unsafe.Add(ref r0, i) == c;
                result += Unsafe.As<bool, byte>(ref equals);
            }

            return result;
        }
#else
        static class Vector
        {
            public static bool IsHardwareAccelerated => false;
		}

        public static int VectorizedCount(this string source, char c) => throw new NotImplementedException();
#endif

		/// <summary>
		/// Splits a string into a dictionary as denoted by the provided K/V separator and KVP delimeter characters
		/// </summary>
		/// <param name="source">The source string to split</param>
		/// <param name="delimeter">The character that separates the key value pairs</param>
		/// <param name="separator">The character that separates the key and value within the pair</param>
		/// <returns>A dictionary representing the values</returns>
		public static Dictionary<string, string> ToDictionary(this string source, char delimeter = ';', char separator = '=')
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.Contains(separator))
            {
                return new Dictionary<string, string>();
            }

            int eq;
            int sc;

            if (Vector.IsHardwareAccelerated)
            {
                eq = source.VectorizedCount(separator);

                sc = source.VectorizedCount(delimeter);
            }
            else
            {
                eq = source.Count(c => c == separator);

                sc = source.Count(c => c == delimeter);
            }

            if (sc != eq - 1 && sc != eq)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> toReturn = new(eq);

            foreach (string skvp in source.Trim(delimeter).Split(delimeter))
            {
                string[] vs = skvp.Split(separator);

                toReturn.Add(vs[0], vs[1]);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a portion of a string up to the last instance of a specified delimiter
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="toText">The delimiter</param>
        /// <param name="inclusive">Whether or not to return the delimiter as part of result</param>
        /// <param name="comparison">The string comparison to use when searching</param>
        /// <returns>A portion of a string up to the last instance of a specified delimiter</returns>
        public static string? ToLast(this string s, string toText, bool inclusive = false, StringComparison comparison = StringComparison.Ordinal)
        {
            if (s is null)
            {
                return null;
            }

            if (toText is null)
            {
                throw new ArgumentNullException(nameof(toText));
            }

            int i = s.LastIndexOf(toText, comparison);

            return i >= 0 ? inclusive ? s[..(i + toText.Length)] : s[..i] : s;
        }

        /// <summary>
        /// Returns a portion of a string up to the last instance of a specified delimiter
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="toText">The delimiter</param>
        /// <param name="inclusive">Whether or not to return the delimiter as part of result</param>
        /// <returns>A portion of a string up to the last instance of a specified delimiter</returns>
        public static string? ToLast(this string s, char toText, bool inclusive = false) => s?[..(s.LastIndexOf(toText) + (inclusive ? 1 : 0))];
    }
}