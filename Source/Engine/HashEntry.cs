using System;

namespace AbsoluteZero {

    /// <summary>
    /// The transposition table entry component of the Absolute Zero chess 
    /// engine. 
    /// </summary>
    partial class Zero {

       /// <summary>
       /// Defines an entry in the transposition hash table. 
       /// </summary>
        private struct HashEntry {

            /// <summary>
            /// Specifies the HashEntry is invalid. 
            /// </summary>
            public const Int32 Invalid = 0;

            /// <summary>
            /// Specifies the value associated with the HashEntry gives an exact value.
            /// </summary>
            public const Int32 Exact = 1;

            /// <summary>
            /// Specifies the value associated with the HashEntry gives a lower bound 
            /// value.
            /// </summary>
            public const Int32 Alpha = 2;

            /// <summary>
            /// Specifies the value associated with the HashEntry gives an upper bound 
            /// value.
            /// </summary>
            public const Int32 Beta = 3;

            /// <summary>
            /// The number of bits used for encoding the type in the miscellaneous field. 
            /// </summary>
            private const Int32 TypeBits = 2;

            /// <summary>
            /// The number of bits used for encoding the depth in the miscellaneous field. 
            /// </summary>
            private const Int32 DepthBits = 8;

            /// <summary>
            /// The amount the type is shifted in the miscellaneous field. 
            /// </summary>
            private const Int32 DepthShift = TypeBits;

            /// <summary>
            /// The amount the value is shifted in the miscellaneous field. 
            /// </summary>
            private const Int32 ValueShift = DepthShift + DepthBits;

            /// <summary>
            /// The value used to normalize the depth in the miscellaneous field. Adding 
            /// this factor will make the depth positive. 
            /// </summary>
            private const Int32 DepthNormal = 1 << (DepthBits - 1);

            /// <summary>
            /// The value used to normalize the value in the miscellaneous field. Adding 
            /// this factor will make the value positive. 
            /// </summary>
            private const Int32 ValueNormal = Int16.MaxValue;

            /// <summary>
            /// The mask used to extract the unshifted type from the miscellaneous
            /// field.
            /// </summary>
            private const Int32 TypeMask = (1 << TypeBits) - 1;

            /// <summary>
            /// The mask used to extract the unshifted depth from the miscellaneous
            /// field.
            /// </summary>
            private const Int32 DepthMask = (1 << DepthBits) - 1;

            /// <summary>
            /// The size of a HashEntry in bytes. 
            /// </summary>
            public const Int32 Size = 16;

            /// <summary>
            /// The zobrist key of the position associated with the HashEntry. 
            /// </summary>
            public readonly UInt64 Key;

            /// <summary>
            /// The best move for the position associated with the HashEntry. 
            /// </summary>
            public readonly Int32 Move;

            /// <summary>
            /// Contains the entry type, search depth, and search value associated with 
            /// the HashEntry. The properties are rolled into a single value to save 
            /// space. 
            /// </summary>
            public readonly Int32 Misc;

            /// <summary>
            /// Constructs a HashEntry.
            /// </summary>
            /// <param name="position">The position to associate with the HashEntry.</param>
            /// <param name="depth">The depth of the search.</param>
            /// <param name="ply">The ply of the search.</param>
            /// <param name="move">The best move for the position.</param>
            /// <param name="value">The value of the search.</param>
            /// <param name="type">The type of the value.</param>
            public HashEntry(Position position, Int32 depth, Int32 ply, Int32 move, Int32 value, Int32 type) {
                Key = position.ZobristKey;
                Move = move;
                if (Math.Abs(value) > NearCheckmateValue)
                    value += Math.Sign(value) * ply;
                Misc = type | ((depth + DepthNormal) << DepthShift) | (value + ValueNormal) << ValueShift;
            }

            /// <summary>
            /// Returns the type of the value associated with the HashEntry.
            /// </summary>
            /// <returns></returns>
            public new Int32 GetType() {
                return Misc & TypeMask;
            }

            /// <summary>
            /// Returns the search depth associated with the HashEntry. 
            /// </summary>
            /// <returns></returns>
            public Int32 GetDepth() {
                return ((Misc >> DepthShift) - DepthNormal) & DepthMask;
            }

            /// <summary>
            /// Returns the value associated with the HashEntry. 
            /// </summary>
            /// <param name="ply"></param>
            /// <returns></returns>
            public Int32 GetValue(Int32 ply) {
                Int32 value = (Misc >> ValueShift) - ValueNormal;
                if (Math.Abs(value) > NearCheckmateValue)
                    return value - Math.Sign(value) * ply;
                return value;
            }
        }
    }
}
