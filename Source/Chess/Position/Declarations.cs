using System;

namespace AbsoluteZero {

    /// <summary>
    /// Declares the constants and fields used to represent the chess position.
    /// </summary>
    public sealed partial class Position : IEquatable<Position> {

        /// <summary>
        /// The FEN string of the starting chess position. 
        /// </summary>
        public const String StartingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// The maximum number of plies the position is to support. 
        /// </summary>
        public const Int32 HalfMovesLimit = 1024;

        /// <summary>
        /// The value representing an invalid square. 
        /// </summary>
        public const Int32 InvalidSquare = -1;

        /// <summary>
        /// The collection of pieces on the squares on the chessboard. Square[n] 
        /// gives the piece at the nth square in the chess position where 0 is A8 and 
        /// 63 is H1. 
        /// </summary>
        public Int32[] Square = new Int32[64];

        /// <summary>
        /// The collection of bitboards in representing the sets of pieces. 
        /// Bitboard[p] gives the bitboard for the piece represented by p. 
        /// </summary>
        public UInt64[] Bitboard = new UInt64[14];

        /// <summary>
        /// The bitboard of all pieces in play. 
        /// </summary>
        public UInt64 OccupiedBitboard = 0;

        /// <summary>
        /// The total material values for each colour. Material[c] gives the total 
        /// material possessed by the colour c of the appropriate sign. 
        /// </summary>
        public Int32[] Material = new Int32[2];

        /// <summary>
        /// The colour that is to make the next move. 
        /// </summary>
        public Int32 SideToMove = 0;

        /// <summary>
        /// The total number of plies the position has advanced from its initial 
        /// state. 
        /// </summary>
        public Int32 HalfMoves = 0;

        /// <summary>
        /// The values indicating whether kingside castling is permitted for each 
        /// colour. CastleKingside[c] is positive if and only if c can castle 
        /// kingside. 
        /// </summary>
        public Int32[] CastleKingside = new Int32[2];

        /// <summary>
        /// The values indicating whether queenside castling is permitted for each 
        /// colour. CastleQueenside[c] is positive if and only if c can castle 
        /// queenside. 
        /// </summary>
        public Int32[] CastleQueenside = new Int32[2];

        /// <summary>
        /// The square indicating en passant is permitted and giving where a pawn 
        /// performing enpassant would move to. 
        /// </summary>
        public Int32 EnPassantSquare = InvalidSquare;

        /// <summary>
        /// The EnPassantSquare values for every ply up to and including the current 
        /// ply. 
        /// </summary>
        public Int32[] EnPassantHistory = new Int32[HalfMovesLimit];

        /// <summary>
        /// The value used to track and enforce the whether fifty-move rule. 
        /// </summary>
        public Int32 FiftyMovesClock = 0;

        /// <summary>
        /// The FiftyMovesClock values for every ply up to and including the current 
        /// ply. 
        /// </summary>
        public Int32[] FiftyMovesHistory = new Int32[HalfMovesLimit];

        /// <summary>
        /// The Zobrist hash value of the position. 
        /// </summary>
        public UInt64 ZobristKey;

        /// <summary>
        /// The ZobristKey values for every ply up to and including the current ply. 
        /// </summary>
        public UInt64[] ZobristKeyHistory = new UInt64[HalfMovesLimit];
    }
}
