using System;

namespace AbsoluteZero {
    
    /// <summary>
    /// Encapsulates the transposition table entry component of the Absolute Zero 
    /// chess engine. 
    /// </summary>
    partial class Zero {

        /// <summary>
        /// Represents an entry in the transposition hash table. 
        /// </summary>
        private struct HashEntry {

            /// <summary>
            /// Specifies the hash entry is invalid. 
            /// </summary>
            public const Int32 Invalid = 0;

            /// <summary>
            /// Specifies the value associated with the hash entry gives an exact value.
            /// </summary>
            public const Int32 Exact = 1;

            /// <summary>
            /// Specifies the value associated with the hash entry gives a lower bound 
            /// value.
            /// </summary>
            public const Int32 Alpha = 2;

            /// <summary>
            /// Specifies the value associated with the hash entry gives an upper bound 
            /// value.
            /// </summary>
            public const Int32 Beta = 3;

            /// <summary>
            /// The size of a hash entry in bytes. 
            /// </summary>
            public const Int32 Size = 16;

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
            /// The value for normalizing the depth in the miscellaneous field. Adding 
            /// this factor will guarantee a positive depth. 
            /// </summary>
            private const Int32 DepthNormal = 1 << (DepthBits - 1);

            /// <summary>
            /// The value for normalizing the value in the miscellaneous field. Adding 
            /// this factor will guarantee a position value. 
            /// </summary>
            private const Int32 ValueNormal = Int16.MaxValue;

            /// <summary>
            /// The mask for extracting the unshifted type from the miscellaneous field.
            /// </summary>
            private const Int32 TypeMask = (1 << TypeBits) - 1;

            /// <summary>
            /// The mask for extracting the unshifted depth from the miscellaneous field.
            /// </summary>
            private const Int32 DepthMask = (1 << DepthBits) - 1;

            /// <summary>
            /// The Zobrist key of the position associated with the hash entry. 
            /// </summary>
            public readonly UInt64 Key;

            /// <summary>
            /// The best move for the position associated with the hash entry. 
            /// </summary>
            public readonly Int32 Move;

            /// <summary>
            /// Contains the entry type, search depth, and search value associated with 
            /// the hash entry. The properties are rolled into a single value for space
            /// efficiency. 
            /// </summary>
            public readonly Int32 Misc;

            /// <summary>
            /// The type of the value associated with the hash entry.
            /// </summary>
            public Int32 Type {
                get {
                    return Misc & TypeMask;
                }
            }

            /// <summary>
            /// The search depth associated with the hash entry. 
            /// </summary>
            public Int32 Depth {
                get {
                    return ((Misc >> DepthShift) - DepthNormal) & DepthMask;
                }
            }

            /// <summary>
            /// Constructs a hash entry.
            /// </summary>
            /// <param name="position">The position to associate with the hash entry.</param>
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
            /// Returns the value associated with the hash entry. The search ply is 
            /// required to determine correct checkmate values. 
            /// </summary>
            /// <param name="ply">The ply of the search routine that is requesting the value.</param>
            /// <returns>The value associated with the hash entry.</returns>
            public Int32 GetValue(Int32 ply) {
                Int32 value = (Misc >> ValueShift) - ValueNormal;
                if (Math.Abs(value) > NearCheckmateValue)
                    return value - Math.Sign(value) * ply;
                return value;
            }
        }
    }
}
