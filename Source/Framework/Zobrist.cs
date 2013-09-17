using System;

namespace AbsoluteZero {

    /// <summary>
    /// Provides methods for Zobrist hashing. 
    /// </summary>
    static class Zobrist {

        /// <summary>
        /// The table giving the hash value for a given piece on a given square. 
        /// </summary>
        public static readonly UInt64[][] PiecePosition = new UInt64[Piece.Max + 1][];

        /// <summary>
        /// The table giving the hash value for ability to castle on the king side 
        /// for a given colour. 
        /// </summary>
        public static readonly UInt64[] CastleKingside = new UInt64[2];

        /// <summary>
        /// The table giving the hash value for ability to castle on the queen side 
        /// for a given colour. 
        /// </summary>
        public static readonly UInt64[] CastleQueenside = new UInt64[2];

        /// <summary>
        /// The table giving the hash value for ability to perform en passant on a 
        /// given square. 
        /// </summary>
        public static readonly UInt64[] EnPassant = new UInt64[64];

        /// <summary>
        /// The hash value for black side to move. 
        /// </summary>
        public static UInt64 Colour;

        /// <summary>
        /// The seed for the pseudorandom number generator used to generate the hash 
        /// values. 
        /// </summary>
        private static UInt64 _seed = 11830773696567897325UL;

        /// <summary>
        /// Initializes hash values. 
        /// </summary>
        static Zobrist() {
            for (Int32 piece = Piece.Min; piece <= Piece.Max; piece++) {
                PiecePosition[piece] = new UInt64[64];
                for (Int32 square = 0; square < 64; square++)
                    PiecePosition[piece][square] = NextUInt64();
            }
            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++) {
                CastleKingside[colour] = NextUInt64();
                CastleQueenside[colour] = NextUInt64();
            }
            for (Int32 file = 0; file < 8; file++) {
                UInt64 hashValue = NextUInt64();
                for (Int32 rank = 0; rank < 8; rank++)
                    EnPassant[file + rank * 8] = hashValue;
            }
            Colour = NextUInt64();
        }

        /// <summary>
        /// Returns a pseudorandom 64-bit integer.
        /// </summary>
        /// <returns></returns>
        private static UInt64 NextUInt64() {
            _seed ^= _seed << 13;
            _seed ^= _seed >> 7;
            _seed ^= _seed << 17;
            return _seed;
        }
    }
}
