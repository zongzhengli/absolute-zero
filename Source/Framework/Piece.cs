using System;

namespace AbsoluteZero {

    /// <summary>
    /// Defines piece values. 
    /// </summary>
    static class Piece {

        public const Int32 Type = 14;                   // 1110
        public const Int32 Colour = Black;              // 0001

        public const Int32 White = 0;                   // 0000
        public const Int32 Black = 1;                   // 0001

        public const Int32 Min = White | Pawn;
        public const Int32 Max = Black | King;

        public const Int32 Empty = 0;                   // 0000
        public const Int32 Pawn = 2;                    // 0010
        public const Int32 Knight = 4;                  // 0100
        public const Int32 Bishop = 6;                  // 0110
        public const Int32 Rook = 8;                    // 1000
        public const Int32 Queen = 10;                  // 1010
        public const Int32 King = 12;                   // 1100
        public const Int32 All = 14;                    // 1110
    }
}
