using System;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Provides useful bitwise functions. 
    /// </summary>
    static class Bit {

        /// <summary>
        /// The table for getting the bitboard file for a given square. 
        /// </summary>
        public static readonly UInt64[] File = new UInt64[64];

        /// <summary>
        /// The table for getting the bitboard rank for a given square.
        /// </summary>
        public static readonly UInt64[] Rank = new UInt64[64];

        /// <summary>
        /// The bitboard of all light squares. 
        /// </summary>
        public const UInt64 LightSquares = 12273903644374837845UL;

        /// <summary>
        /// The table for getting the index of a single bit. 
        /// </summary>
        private static readonly Int32[] BitIndex = new Int32[64];

        /// <summary>
        /// Initializes lookup tables. 
        /// </summary>
        static Bit() {

            // Initialize bit index table. 
            for (Int32 i = 0; i < 64; i++)
                BitIndex[((1UL << i) * 0x07EDD5E59A4E28C2UL) >> 58] = i;

            // Initialize file and rank bitboard tables. 
            for (Int32 square = 0; square < 64; square++) {
                File[square] = LineFill(Position.File(square), 0, 1);
                Rank[square] = LineFill(Position.Rank(square) * 8, 1, 0);
            }
        }

        /// <summary>
        /// Removes and returns the index of the least significant set bit in the 
        /// given bitboard.  
        /// </summary>
        /// <param name="bitboard">The bitboard to pop.</param>
        /// <returns>The index of the least significant set bit.</returns>
        public static Int32 Pop(ref UInt64 bitboard) {
            UInt64 isolatedBit = bitboard & (0UL - bitboard);
            bitboard &= bitboard - 1;
            return BitIndex[(isolatedBit * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        /// <summary>
        /// Returns the index of the bit in a bitboard with a single set bit. 
        /// </summary>
        /// <param name="bitboard">The bitboard to read.</param>
        /// <returns>The index of the single set bit.</returns>
        public static Int32 Read(UInt64 bitboard) {
            return BitIndex[(bitboard * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        /// <summary>
        /// Returns the index of the least significant set bit in the given 
        /// bitboard.
        /// </summary>
        /// <param name="bitboard">The bitboard to scan.</param>
        /// <returns>The index of the least significant set bit.</returns>
        public static Int32 Scan(UInt64 bitboard) {
            return BitIndex[((bitboard & (UInt64)(-(Int64)bitboard)) * 0x07EDD5E59A4E28C2UL) >> 58];
        }

        /// <summary>
        /// Returns the index of the most significant set bit in the given bitboard.
        /// </summary>
        /// <param name="bitboard">The bitboard to scan.</param>
        /// <returns>The index of the most significant set bit.</returns>
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

        /// <summary>
        /// Returns the number of set bits in the given bitboard.
        /// </summary>
        /// <param name="bitboard">The bitboard to count.</param>
        /// <returns>The number of set bits.</returns>
        public static Int32 Count(UInt64 bitboard) {
            bitboard -= (bitboard >> 1) & 0x5555555555555555UL;
            bitboard = (bitboard & 0x3333333333333333UL) + ((bitboard >> 2) & 0x3333333333333333UL);
            return (Int32)(((bitboard + (bitboard >> 4) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        /// <summary>
        /// Returns the number of set bits in the given bitboard. For a bitboard 
        /// with very few set bits this may be faster than Bit.Count(). 
        /// </summary>
        /// <param name="bitboard">The bitboard to count.</param>
        /// <returns>The number of set bits.</returns>
        public static Int32 CountSparse(UInt64 bitboard) {
            Int32 count = 0;
            while (bitboard != 0) {
                count++;
                bitboard &= bitboard - 1;
            }
            return count;
        }

        /// <summary>
        /// Returns a bitboard that gives the result of performing a floodfill from 
        /// a given index for a given distance. 
        /// </summary>
        /// <param name="index">The index to floodfill from.</param>
        /// <param name="distance">The distance to floodfill.</param>
        /// <returns>A bitboard that is the result of the floodfill.</returns>
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

        /// <summary>
        /// Returns a bitboard that has set bits along a given line.  
        /// </summary>
        /// <param name="index">A point on the line.</param>
        /// <param name="dx">The x component of the line's direction vector.</param>
        /// <param name="dy">The y component of the line's direction vector.</param>
        /// <returns>The bitboard that is the result of the line fill.</returns>
        public static UInt64 LineFill(Int32 index, Int32 dx, Int32 dy) {
            if (index < 0 || index > 63)
                return 0;
            UInt64 bitboard = 1UL << index;
            if (Math.Floor(index / 8F) == Math.Floor((index + dx) / 8F))
                bitboard |= LineFill(index + dx + dy * 8, dx, dy);
            return bitboard;

        }
        
        /// <summary>
        /// Returns a string giving the binary representation of the move.
        /// </summary>
        /// <param name="x">The move to convert.</param>
        /// <returns>The binary representation of the move.</returns>
        public static String ToString(Int32 move) {
            Char[] sequence = new Char[32];
            for (Int32 i = sequence.Length - 1; i >= 0; i--) {
                sequence[i] = (Char)((move & 1) + 48);
                move >>= 1;
            }
            return new String(sequence);
        }

        /// <summary>
        /// Returns a string giving the binary representation of the bitboard.
        /// </summary>
        /// <param name="bitboard">The bitboard to convert.</param>
        /// <returns>The binary representation of the bitboard.</returns>
        public static String ToString(UInt64 bitboard) {
            Char[] sequence = new Char[64];
            for (Int32 i = sequence.Length - 1; i >= 0; i--) {
                sequence[i] = (Char)((bitboard & 1) + 48);
                bitboard >>= 1;
            }
            return new String(sequence);
        }

        /// <summary>
        /// Returns a string giving the binary representation of the bitboard with 
        /// appropriate line terminating characters. The result is a 8 by 8 matrix. 
        /// </summary>
        /// <param name="bitboard">The bitboard to convert</param>
        /// <returns>The binary representation of the bitboard in a matrix format.</returns>
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
