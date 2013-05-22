using System;
using System.Text;

namespace AbsoluteZero {
    static class Format {
        public static String Pad(Int32 length, Char character = ' ') {
            Char[] pad = new Char[length];
            for (Int32 i = 0; i < pad.Length; i++)
                pad[i] = character;
            return new String(pad);
        }

        public static String PadRight(String value, Int32 width, Char character = ' ') {
            Int32 length = Math.Max(0, width - value.Length);
            return value + Pad(length);
        }

        public static String PadRight(Object value, Int32 width, Char character = ' ') {
            return PadRight(value.ToString(), width, character);
        }

        public static String PadRightAll(Int32 width, params Object[] values) {
            StringBuilder sequence = new StringBuilder(width * values.Length);
            for (Int32 i = 0; i < values.Length - 1; i++)
                sequence.Append(PadRight(values[i], width));
            sequence.Append(values[values.Length - 1]);
            return sequence.ToString();
        }

        public static String PadLeft(String value, Int32 width, Char character = ' ') {
            Int32 length = Math.Max(0, width - value.Length);
            return Pad(length) + value;
        }

        public static String PadLeft(Object value, Int32 width, Char character = ' ') {
            return PadLeft(value.ToString(), width, character);
        }

        public static String Precision(Double value, Int32 digits = 0) {
            Double result = Math.Round(value, digits, MidpointRounding.AwayFromZero);
            if (digits > 0)
                return result.ToString("N" + digits).Replace(",", String.Empty);
            return result.ToString();
        }

        public static String PrecisionAndSign(Double value, Int32 digits = 0) {
            String result = Precision(value, digits);
            return result[0] == '-' ? result : '+' + result;
        }

        public static String Sign(Int64 value) {
            return value < 0 ? value.ToString() : "+" + value;
        }
    }
}
