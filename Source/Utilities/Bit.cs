using System;
using System.Text;

namespace AbsoluteZero {
    static class Bit {
        public static readonly UInt64[] File = new UInt64[64];
        public static readonly UInt64[] Rank = new UInt64[64];
        public const UInt64 LightSquares = 12273903644374837845UL;

        private static readonly Int32[] BitIndex = new Int32[64];

        static Bit() {
            for (Int32 i = 0; i < 64; i++)
                BitIndex[((1UL << i) * 0x07EDD5E59A4E28C2UL) >> 58] = i;

            for (Int32 square = 0; square < 64; square++) {
                File[square] = DirectionFill(Position.File(square), 0, 1);
                Rank[square] = DirectionFill(Position.Rank(square) * 8, 1, 0);
            }
        }

        public static Int32 Pop(ref UInt64 field) {
            UInt64 isolatedBit = field & (0UL - field);
            field &= field - 1;
            return BitIndex[(isolatedBit * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        public static Int32 Read(UInt64 field) {
            return BitIndex[(field * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        public static Int32 Scan(UInt64 field) {
            return BitIndex[((field & (UInt64)(-(Int64)field)) * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        public static Int32 ScanReverse(UInt64 field) {
            Int32 result = 0;
            if (field > 0xFFFFFFFF) {
                field >>= 32;
                result = 32;
            }
            if (field > 0xFFFF) {
                field >>= 16;
                result += 16;
            }
            if (field > 0xFF) {
                field >>= 8;
                result += 8;
            }
            if (field > 0xF) {
                field >>= 4;
                result += 4;
            }
            if (field > 0x3) {
                field >>= 2;
                result += 2;
            }
            if (field > 0x1)
                result++;
            return result;
        }

        public static Int32 Count(UInt64 field) {
            field -= (field >> 1) & 0x5555555555555555UL;
            field = (field & 0x3333333333333333UL) + ((field >> 2) & 0x3333333333333333UL);
            return (Int32)(((field + (field >> 4) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        public static Int32 CountSparse(UInt64 field) {
            Int32 count = 0;
            while (field > 0) {
                count++;
                field &= field - 1;
            }
            return count;
        }

        public static UInt64 FloodFill(Int32 index, Int32 distance) {
            if (distance < 0 || index < 0 || index > 63)
                return 0;
            UInt64 field = 1UL << index;
            field |= FloodFill(index + 8, distance - 1);
            field |= FloodFill(index - 8, distance - 1);
            if (Math.Floor(index / 8F) == Math.Floor((index + 1) / 8F))
                field |= FloodFill(index + 1, distance - 1);
            if (Math.Floor(index / 8F) == Math.Floor((index - 1) / 8F))
                field |= FloodFill(index - 1, distance - 1);
            return field;
        }

        public static UInt64 DirectionFill(Int32 index, Int32 dx, Int32 dy) {
            if (index < 0 || index > 63)
                return 0;
            UInt64 field = 1UL << index;
            if (Math.Floor(index / 8F) == Math.Floor((index + dx) / 8F))
                field |= DirectionFill(index + dx + dy * 8, dx, dy);
            return field;

        }

        public static String ToString(Int32 field) {
            Char[] sequence = new Char[32];
            for (Int32 i = sequence.Length - 1; i >= 0; i--) {
                sequence[i] = (Char)((field & 1) + 48);
                field >>= 1;
            }
            return new String(sequence);
        }

        public static String ToString(UInt64 field) {
            Char[] sequence = new Char[64];
            for (Int32 i = sequence.Length - 1; i >= 0; i--) {
                sequence[i] = (Char)((field & 1) + 48);
                field >>= 1;
            }
            return new String(sequence);
        }

        public static String ToMatrix(UInt64 field) {
            StringBuilder sequence = new StringBuilder(78);
            Int32 file = 0;
            for (Int32 i = 0; i < 71; i++)
                if (++file > 8) {
                    sequence.Append(Environment.NewLine);
                    file = 0;
                } else {
                    sequence.Append(field & 1);
                    field >>= 1;
                }
            return sequence.ToString();

        }
    }
}
