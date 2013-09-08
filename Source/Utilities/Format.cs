using System;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Provides string formatting methods. 
    /// </summary>
    static class Format {
        
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
