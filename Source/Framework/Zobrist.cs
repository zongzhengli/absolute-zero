using System;

namespace AbsoluteZero {
    static class Zobrist {
        public static readonly UInt64[][] PiecePosition = new UInt64[14][];
        public static readonly UInt64[] CastleKingside = new UInt64[2];
        public static readonly UInt64[] CastleQueenside = new UInt64[2];
        public static readonly UInt64[] EnPassant = new UInt64[64];
        public static UInt64 Colour;

        private static UInt64 seed = 11830773696567897325UL;

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

        private static UInt64 NextUInt64() {
            seed ^= seed << 13;
            seed ^= seed >> 7;
            seed ^= seed << 17;
            return seed;
        }
    }
}
