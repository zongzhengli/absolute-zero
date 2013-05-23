using System;
using System.Collections.Generic;
using System.Text;

using PieceClass = AbsoluteZero.Piece;
using MoveClass = AbsoluteZero.Move;

namespace AbsoluteZero {
    public enum IdentificationOptions { None, Proper };

    class Identify {
        public static String File(Int32 square) {
            return ((Char)(Position.File(square) + 97)).ToString();
        }

        public static String Rank(Int32 square) {
            return (8 - Position.Rank(square)).ToString();
        }

        public static String Square(Int32 square) {
            return File(square) + Rank(square);
        }

        public static String Colour(Int32 colour) {
            return (colour & PieceClass.Colour) == PieceClass.White ? "White" : "Black";
        }

        public static String PieceInitial(Int32 piece) {
            switch (piece & PieceClass.Type) {
                case PieceClass.King:
                    return "K";
                case PieceClass.Queen:
                    return "Q";
                case PieceClass.Rook:
                    return "R";
                case PieceClass.Bishop:
                    return "B";
                case PieceClass.Knight:
                    return "N";
                case PieceClass.Pawn:
                    return "P";
            }
            return "-";
        }

        public static String Move(Int32 move) {
            String coordinates = Identify.Square(MoveClass.GetFrom(move)) + Identify.Square(MoveClass.GetTo(move));
            switch (MoveClass.GetSpecial(move) & PieceClass.Type) {
                default:
                    return coordinates;
                case PieceClass.Queen:
                    return coordinates + "q";
                case PieceClass.Rook:
                    return coordinates + "r";
                case PieceClass.Bishop:
                    return coordinates + "b";
                case PieceClass.Knight:
                    return coordinates + "n";
            }
        }

        public static String Moves(List<Int32> moves) {
            if (moves.Count == 0)
                return String.Empty;
            StringBuilder sequence = new StringBuilder(6 * moves.Count);
            foreach (Int32 move in moves) {
                sequence.Append(Move(move));
                sequence.Append(' ');
            }
            return sequence.ToString(0, sequence.Length - 1);
        }

        public static String MoveAlgebraically(Position position, Int32 move) {
            if (MoveClass.IsCastle(move))
                return MoveClass.GetTo(move) < MoveClass.GetFrom(move) ? "O-O-O" : "O-O";

            String piece = PieceInitial(MoveClass.GetPiece(move));
            if (piece == "P")
                piece = String.Empty;

            String disambiguation = String.Empty;
            List<Int32> alternatives = new List<Int32>();
            foreach (Int32 m in position.LegalMoves())
                if (MoveClass.GetFrom(m) != MoveClass.GetFrom(move) && MoveClass.GetPiece(m) == MoveClass.GetPiece(move) && MoveClass.GetTo(m) == MoveClass.GetTo(move))
                    alternatives.Add(m);
            if (alternatives.Count > 0) {
                Boolean uniqueFile = true;
                Boolean uniqueRank = true;
                foreach (Int32 m in alternatives) {
                    if (Position.File(MoveClass.GetFrom(m)) == Position.File(MoveClass.GetFrom(move)))
                        uniqueFile = false;
                    if (Position.Rank(MoveClass.GetFrom(m)) == Position.Rank(MoveClass.GetFrom(move)))
                        uniqueRank = false;
                }
                if (uniqueFile)
                    disambiguation = File(MoveClass.GetFrom(move));
                else if (uniqueRank)
                    disambiguation = Rank(MoveClass.GetFrom(move));
                else
                    disambiguation = Square(MoveClass.GetFrom(move));
            }

            Boolean isCapture = MoveClass.IsCapture(move) || MoveClass.IsEnPassant(move);
            String capture = isCapture ? "x" : String.Empty;
            if ((MoveClass.GetPiece(move) & Piece.Type) == Piece.Pawn && isCapture)
                if (disambiguation == String.Empty)
                    disambiguation = File(MoveClass.GetFrom(move));

            String square = Square(MoveClass.GetTo(move));
            String promotion = MoveClass.IsPromotion(move) ? "=" + PieceInitial(MoveClass.GetSpecial(move)) : String.Empty;

            String check = String.Empty;
            position.Make(move);
            if (position.InCheck(position.SideToMove))
                check = position.LegalMoves().Count > 0 ? "+" : "#";
            position.Unmake(move);

            return piece + disambiguation + capture + square + promotion + check;
        }

        public static String MovesAlgebraically(Position position, List<Int32> moves, IdentificationOptions options = IdentificationOptions.None) {
            if (moves.Count == 0)
                return String.Empty;
            StringBuilder sequence = new StringBuilder(5 * moves.Count);
            Int32 halfMoves = 0;
            if (options == IdentificationOptions.Proper) {
                halfMoves = position.HalfMoves;
                if (position.SideToMove == Piece.Black) {
                    sequence.Append(halfMoves / 2 + 1);
                    sequence.Append("... ");
                }
            }

            foreach (Int32 move in moves) {
                if ((halfMoves++ % 2) == 0) {
                    sequence.Append(halfMoves / 2 + 1);
                    sequence.Append('.');
                    if (options == IdentificationOptions.Proper)
                        sequence.Append(' ');
                }
                sequence.Append(MoveAlgebraically(position, move));
                sequence.Append(' ');
                position.Make(move);
            }
            for (Int32 i = moves.Count - 1; i >= 0; i--)
                position.Unmake(moves[i]);
            return sequence.ToString(0, sequence.Length - 1);
        }
    }
}
