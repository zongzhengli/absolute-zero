using System;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Provides string formatting methods. 
    /// </summary>
    static class Format {

        /// <summary>
        /// Returns a string of the given length consisting entirely of the given 
        /// character. If a character is not specified the space character is used. 
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <param name="character">The character that is repeated to produce the string.</param>
        /// <returns>A string of the given length consisting entirely of the given character.</returns>
        public static String Pad(Int32 length, Char character = ' ') {
            Char[] pad = new Char[length];
            for (Int32 i = 0; i < pad.Length; i++)
                pad[i] = character;
            return new String(pad);
        }

        /// <summary>
        /// Pads the right side of the given string with the given character so that 
        /// the resultant string is of the given length. If a character is not 
        /// specified the space character is used. 
        /// </summary>
        /// <param name="value">The value to pad.</param>
        /// <param name="length">The length of the padded string.</param>
        /// <param name="character">The character that is repeated to pad the string.</param>
        /// <returns>The given value padded to the given length with the given character.</returns>
        public static String PadRight(String value, Int32 length, Char character = ' ') {
            Int32 len = Math.Max(0, length - value.Length);
            return value + Pad(len);
        }

        /// <summary>
        /// Pads the right side of the text representation of the given object with 
        /// the given character so that the resultant string is of the given length. 
        /// If a character is not specified the space character is used. 
        /// </summary>
        /// <param name="value">The value to pad.</param>
        /// <param name="length">The length of the padded string.</param>
        /// <param name="character">The character that is repeated to pad the string.</param>
        /// <returns>The given value padded to the given length with the given character.</returns>
        public static String PadRight(Object value, Int32 length, Char character = ' ') {
            return PadRight(value.ToString(), length, character);
        }

        /// <summary>
        /// Pads the right side of all of the text representations of the given 
        /// objects with spaces until they are each of the given length and then 
        /// concatenates them together. 
        /// </summary>
        /// <param name="length">The length of each of the padded strings.</param>
        /// <param name="values">The values to pad.</param>
        /// <returns>The concatenation of all the given values padded to the right with spaces.</returns>
        public static String PadRightAll(Int32 length, params Object[] values) {
            StringBuilder sequence = new StringBuilder(length * values.Length);
            for (Int32 i = 0; i < values.Length - 1; i++)
                sequence.Append(PadRight(values[i], length));
            sequence.Append(values[values.Length - 1]);
            return sequence.ToString();
        }
        
        /// <summary>
        /// Pads the left side of the given string with the given character so that 
        /// the resultant string is of the given length. If a character is not 
        /// specified the space character is used. 
        /// </summary>
        /// <param name="value">The value to pad.</param>
        /// <param name="length">The length of the padded string.</param>
        /// <param name="character">The character that is repeated to pad the string.</param>
        /// <returns>The given value padded to the given length with the given character.</returns>
        public static String PadLeft(String value, Int32 length, Char character = ' ') {
            Int32 len = Math.Max(0, length - value.Length);
            return Pad(len) + value;
        }

        /// <summary>
        /// Pads the left side of the text representation of the given object with 
        /// the given character so that the resultant string is of the given length. 
        /// If a character is not specified the space character is used. 
        /// </summary>
        /// <param name="value">The value to pad.</param>
        /// <param name="length">The length of the padded string.</param>
        /// <param name="character">The character that is repeated to pad the string.</param>
        /// <returns>The given value padded to the given length with the given character.</returns>
        public static String PadLeft(Object value, Int32 length, Char character = ' ') {
            return PadLeft(value.ToString(), length, character);
        }
        
        /// <summary>
        /// Returns a string that is the given floating point value rounded to the 
        /// specified number of decimal digits. 
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="digits">The number of decimal digits to round to.</param>
        /// <returns>A string that is the given value rounded to the specified number of decimal digits.</returns>
        public static String Precision(Double value, Int32 digits = 0) {
            Double result = Math.Round(value, digits, MidpointRounding.AwayFromZero);
            if (digits > 0)
                return result.ToString("N" + digits).Replace(",", String.Empty);
            return result.ToString();
        }

        /// <summary>
        /// Returns a string that is the given floating point value rounded to the 
        /// specified number of decimal digits with its sign explicitedly stated. 
        /// This method gives zero a positive sign. 
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="digits">The number of decimal digits to format to.</param>
        /// <returns>A string that is the given value rounded to the specified number of decimal digits with an explicit sign.</returns>
        public static String PrecisionAndSign(Double value, Int32 digits = 0) {
            String result = Precision(value, digits);
            return (result[0] == '-') ? result : '+' + result;
        }

        /// <summary>
        /// Returns a string that is the given integer with its sign explicitedly 
        /// stated. This method gives zero a positive sign. 
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string that is the given integer with an explicit sign.</returns>
        public static String Sign(Int64 value) {
            return (value < 0) ? value.ToString() : "+" + value;
        }
    }
}
