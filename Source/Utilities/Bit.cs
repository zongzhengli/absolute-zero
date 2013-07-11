using System;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Provides useful bitwise functions. 
    /// </summary>
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

        public static Int32 Pop(ref UInt64 bitboard) {
            UInt64 isolatedBit = bitboard & (0UL - bitboard);
            bitboard &= bitboard - 1;
            return BitIndex[(isolatedBit * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        public static Int32 Read(UInt64 bitboard) {
            return BitIndex[(bitboard * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        public static Int32 Scan(UInt64 bitboard) {
            return BitIndex[((bitboard & (UInt64)(-(Int64)bitboard)) * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        public static Int32 ScanReverse(UInt64 bitboard) {
            Int32 result = 0;
            if (bitboard > 0xFFFFFFFF) {
                bitboard >>= 32;
                result = 32;
            }
            if (bitboard > 0xFFFF) {
                bitboard >>= 16;
                result += 16;
            }
            if (bitboard > 0xFF) {
                bitboard >>= 8;
                result += 8;
            }
            if (bitboard > 0xF) {
                bitboard >>= 4;
                result += 4;
            }
            if (bitboard > 0x3) {
                bitboard >>= 2;
                result += 2;
            }
            if (bitboard > 0x1)
                result++;
            return result;
        }

        public static Int32 Count(UInt64 bitboard) {
            bitboard -= (bitboard >> 1) & 0x5555555555555555UL;
            bitboard = (bitboard & 0x3333333333333333UL) + ((bitboard >> 2) & 0x3333333333333333UL);
            return (Int32)(((bitboard + (bitboard >> 4) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        public static Int32 CountSparse(UInt64 bitboard) {
            Int32 count = 0;
            while (bitboard != 0) {
                count++;
                bitboard &= bitboard - 1;
            }
            return count;
        }

        public static UInt64 FloodFill(Int32 index, Int32 distance) {
            if (distance < 0 || index < 0 || index > 63)
                return 0;
            UInt64 bitboard = 1UL << index;
            bitboard |= FloodFill(index + 8, distance - 1);
            bitboard |= FloodFill(index - 8, distance - 1);
            if (Math.Floor(index / 8F) == Math.Floor((index + 1) / 8F))
                bitboard |= FloodFill(index + 1, distance - 1);
            if (Math.Floor(index / 8F) == Math.Floor((index - 1) / 8F))
                bitboard |= FloodFill(index - 1, distance - 1);
            return bitboard;
        }

        public static UInt64 DirectionFill(Int32 index, Int32 dx, Int32 dy) {
            if (index < 0 || index > 63)
                return 0;
            UInt64 bitboard = 1UL << index;
            if (Math.Floor(index / 8F) == Math.Floor((index + dx) / 8F))
                bitboard |= DirectionFill(index + dx + dy * 8, dx, dy);
            return bitboard;

        }

        public static String ToString(Int32 bitboard) {
            Char[] sequence = new Char[32];
            for (Int32 i = sequence.Length - 1; i >= 0; i--) {
                sequence[i] = (Char)((bitboard & 1) + 48);
                bitboard >>= 1;
            }
            return new String(sequence);
        }

        public static String ToString(UInt64 bitboard) {
            Char[] sequence = new Char[64];
            for (Int32 i = sequence.Length - 1; i >= 0; i--) {
                sequence[i] = (Char)((bitboard & 1) + 48);
                bitboard >>= 1;
            }
            return new String(sequence);
        }

        public static String ToMatrix(UInt64 bitboard) {
            StringBuilder sequence = new StringBuilder(78);
            Int32 file = 0;
            for (Int32 i = 0; i < 71; i++)
                if (++file > 8) {
                    sequence.Append(Environment.NewLine);
                    file = 0;
                } else {
                    sequence.Append(bitboard & 1);
                    bitboard >>= 1;
                }
            return sequence.ToString();

        }
    }
}
