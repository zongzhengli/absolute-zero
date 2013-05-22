using System;

namespace AbsoluteZero {
    static class Move {
        public const Int32 Invalid = 0;

        private const Int32 ToShift = 6;
        private const Int32 PieceShift = ToShift + 6;
        private const Int32 CaptureShift = PieceShift + 4;
        private const Int32 SpecialShift = CaptureShift + 4;

        private const Int32 SquareMask = 63;
        private const Int32 PieceMask = 15;

        private const Int32 TypeCaptureShifted = Piece.Type << CaptureShift;
        private const Int32 EmptyCaptureShifted = Piece.Empty << CaptureShift;
        private const Int32 TypeSpecialShifted = Piece.Type << SpecialShift;
        private const Int32 KingSpecialShifted = Piece.King << SpecialShift;
        private const Int32 PawnSpecialShifted = Piece.Pawn << SpecialShift;
        private const Int32 QueenSpecialShifted = Piece.Queen << SpecialShift;
        private const Int32 TypePieceShifted = Piece.Type << PieceShift;
        private const Int32 PawnPieceShifted = Piece.Pawn << PieceShift;

        public static UInt64 Pawn(Int32 square, Int32 colour) {
            return 1UL << (square + 16 * colour - 8);
        }

        public static Int32 Create(Position position, Int32 from, Int32 to, Int32 special = Piece.Empty) {
            return from | (to << ToShift) | (position.Square[from] << PieceShift) | (position.Square[to] << CaptureShift) | (special << SpecialShift);
        }

        public static Int32 GetFrom(Int32 move) {
            return move & SquareMask;
        }

        public static Int32 GetTo(Int32 move) {
            return (move >> ToShift) & SquareMask;
        }

        public static Int32 GetPiece(Int32 move) {
            return (move >> PieceShift) & PieceMask;
        }

        public static Int32 GetCapture(Int32 move) {
            return (move >> CaptureShift) & PieceMask;
        }

        public static Int32 GetSpecial(Int32 move) {
            return move >> SpecialShift;
        }

        public static Boolean IsCapture(Int32 move) {
            return (move & TypeCaptureShifted) != EmptyCaptureShifted;
        }

        public static Boolean IsCastle(Int32 move) {
            return (move & TypeSpecialShifted) == KingSpecialShifted;
        }

        public static Boolean IsPromotion(Int32 move) {
            if ((move & TypePieceShifted) != PawnPieceShifted)
                return false;
            Int32 to = (move >> ToShift) & SquareMask;
            return (to - 8) * (to - 55) > 0;
        }

        public static Boolean IsEnPassant(Int32 move) {
            return (move & TypeSpecialShifted) == PawnSpecialShifted;
        }

        public static Boolean IsPawnAdvance(Int32 move) {
            return (move & TypePieceShifted) == PawnPieceShifted;
        }

        public static Boolean IsQueenPromotion(Int32 move) {
            return (move & TypeSpecialShifted) == QueenSpecialShifted;
        }

        public static Int32 Create(Position position, String name) {
            foreach (Int32 move in position.LegalMoves())
                if (name == Identify.Move(move))
                    return move;
            return Invalid;
        }
    }
}
