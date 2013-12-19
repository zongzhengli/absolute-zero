using System;

namespace AbsoluteZero {

    /// <summary>
    /// Defines pieces and masks. 
    /// </summary>
    static class Piece {
        
        /// <summary>
        /// The mask for extracting the type of a piece. 
        /// </summary>
        public const Int32 Type = 14;                                                // 1110

        /// <summary>
        /// The mask for extracting the colour of a piece. 
        /// </summary>
        public const Int32 Colour = Black;                                           // 0001

        /// <summary>
        /// The colour white. 
        /// </summary>
        public const Int32 White = 0;                                                // 0000

        /// <summary>
        /// The colour black. 
        /// </summary>
        public const Int32 Black = 1;                                                // 0001

        /// <summary>
        /// The smallest value used to represent a piece. 
        /// </summary>
        public const Int32 Min = White | Pawn;

        /// <summary>
        /// The largest value used to represent a piece. 
        /// </summary>
        public const Int32 Max = Black | King;

        /// <summary>
        /// The empty, or non-existent, piece. 
        /// </summary>
        public const Int32 Empty = 0;                                                // 0000

        /// <summary>
        /// The pawn piece. 
        /// </summary>
        public const Int32 Pawn = 2;                                                 // 0010

        /// <summary>
        /// The knight piece. 
        /// </summary>
        public const Int32 Knight = 4;                                               // 0100

        /// <summary>
        /// The bishop piece. 
        /// </summary>
        public const Int32 Bishop = 6;                                               // 0110

        /// <summary>
        /// The rook piece. 
        /// </summary>
        public const Int32 Rook = 8;                                                 // 1000

        /// <summary>
        /// The queen piece. 
        /// </summary>
        public const Int32 Queen = 10;                                               // 1010

        /// <summary>
        /// The king piece. 
        /// </summary>
        public const Int32 King = 12;                                                // 1100
    }
}
