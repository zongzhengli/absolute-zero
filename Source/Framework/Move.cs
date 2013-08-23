using System;
using PieceClass = AbsoluteZero.Piece;

namespace AbsoluteZero {

    /// <summary>
    /// Provides move methods. 
    /// </summary>
    static class Move {

        /// <summary>
        /// The invalid move.
        /// </summary>
        public const Int32 Invalid = 0;

        /// <summary>
        /// The amount the to square is shifted in the move. 
        /// </summary>
        private const Int32 ToShift = 6;

        /// <summary>
        /// The amount the piece is shifted in the move.
        /// </summary>
        private const Int32 PieceShift = ToShift + 6;

        /// <summary>
        /// The amount the captured piece is shifted in the move.
        /// </summary>
        private const Int32 CaptureShift = PieceShift + 4;

        /// <summary>
        /// The amount the special piece is shifted in the move.
        /// </summary>
        private const Int32 SpecialShift = CaptureShift + 4;

        /// <summary>
        /// The mask used to extract the unshifted square from the miscellaneous
        /// field.
        /// </summary>
        private const Int32 SquareMask = (1 << 6) - 1;

        /// <summary>
        /// The mask used to extract the unshifted square from the miscellaneous 
        /// field.
        /// </summary>
        private const Int32 PieceMask = (1 << 4) - 1;

        private const Int32 TypeCaptureShifted = PieceClass.Type << CaptureShift;
        private const Int32 EmptyCaptureShifted = PieceClass.Empty << CaptureShift;
        private const Int32 TypeSpecialShifted = PieceClass.Type << SpecialShift;
        private const Int32 KingSpecialShifted = PieceClass.King << SpecialShift;
        private const Int32 PawnSpecialShifted = PieceClass.Pawn << SpecialShift;
        private const Int32 QueenSpecialShifted = PieceClass.Queen << SpecialShift;
        private const Int32 TypePieceShifted = PieceClass.Type << PieceShift;
        private const Int32 PawnPieceShifted = PieceClass.Pawn << PieceShift;

        public static UInt64 Pawn(Int32 square, Int32 colour) {
            return 1UL << (square + 16 * colour - 8);
        }

        public static Int32 Create(Position position, Int32 from, Int32 to, Int32 special = PieceClass.Empty) {
            return from | (to << ToShift) | (position.Square[from] << PieceShift) | (position.Square[to] << CaptureShift) | (special << SpecialShift);
        }

        public static Int32 Create(Position position, String name) {
            foreach (Int32 move in position.LegalMoves())
                if (name == Identify.Move(move))
                    return move;
            return Invalid;
        }

        public static Int32 From(Int32 move) {
            return move & SquareMask;
        }

        public static Int32 To(Int32 move) {
            return (move >> ToShift) & SquareMask;
        }

        public static Int32 Piece(Int32 move) {
            return (move >> PieceShift) & PieceMask;
        }

        public static Int32 Capture(Int32 move) {
            return (move >> CaptureShift) & PieceMask;
        }

        public static Int32 Special(Int32 move) {
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
    }
}
