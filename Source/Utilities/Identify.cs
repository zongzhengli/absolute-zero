using System;
using System.Collections.Generic;
using System.Text;

using PieceClass = AbsoluteZero.Piece;
using MoveClass = AbsoluteZero.Move;

namespace AbsoluteZero {

    /// <summary>
    /// Specifies whether to use proper identification. 
    /// </summary>
    public enum IdentificationOptions { None, Proper };

    /// <summary>
    /// Provides methods that identify, or give text representations of, various 
    /// chess data types. 
    /// </summary>
    static class Identify {

        /// <summary>
        /// Returns the text representation of the file for the given square. 
        /// </summary>
        /// <param name="square">The square to identify.</param>
        /// <returns>The name of the file for the given square.</returns>
        public static String File(Int32 square) {
            return ((Char)(Position.File(square) + 'a')).ToString();
        }

        /// <summary>
        /// Returns the text representation of the rank for the given square. 
        /// </summary>
        /// <param name="square">The square to identify.</param>
        /// <returns>The name of the rank for the given square.</returns>
        public static String Rank(Int32 square) {
            return (8 - Position.Rank(square)).ToString();
        }

        /// <summary>
        /// Returns the text representation of the given square in coordinate 
        /// notation.
        /// </summary>
        /// <param name="square">The square to identify.</param>
        /// <returns>The name of the given square.</returns>
        public static String Square(Int32 square) {
            return File(square) + Rank(square);
        }

        /// <summary>
        /// Returns the text representation of the given colour.  
        /// </summary>
        /// <param name="colour">The colour to identify.</param>
        /// <returns>The text representation of the given colour</returns>
        public static String Colour(Int32 colour) {
            return (colour & PieceClass.Colour) == PieceClass.White ? "White" : "Black";
        }

        /// <summary>
        /// Returns the text representation of the given piece. 
        /// </summary>
        /// <param name="piece">The piece to identify.</param>
        /// <returns>The text representation of the given piece.</returns>
        public static String Piece(Int32 piece) {
            switch (piece & PieceClass.Type) {
                case PieceClass.King:
                    return "King";
                case PieceClass.Queen:
                    return "Queen";
                case PieceClass.Rook:
                    return "Rook";
                case PieceClass.Bishop:
                    return "Bishop";
                case PieceClass.Knight:
                    return "Knight";
                case PieceClass.Pawn:
                    return "Pawn";
            }
            return "-";
        }

        /// <summary>
        /// Returns the text representation of the given piece as an initial. 
        /// </summary>
        /// <param name="piece">The piece to identify.</param>
        /// <returns>The text representation of the given piece as an initial.</returns>
        public static String PieceInitial(Int32 piece) {
            if ((piece & PieceClass.Type) == PieceClass.Knight)
                return "N";
            return Piece(piece)[0].ToString();
        }

        /// <summary>
        /// Returns the text representation of the given move in coordinate notation. 
        /// </summary>
        /// <param name="move">The move to identify.</param>
        /// <returns>The text representation of the given move in coordinate notation.</returns>
        public static String Move(Int32 move) {
            String coordinates = Identify.Square(MoveClass.From(move)) + Identify.Square(MoveClass.To(move));
            Int32 special = MoveClass.Special(move);

            switch (special & PieceClass.Type) {
                default:
                    return coordinates;
                case PieceClass.Queen:
                case PieceClass.Rook:
                case PieceClass.Bishop:
                case PieceClass.Knight:
                    return coordinates + PieceInitial(special).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Returns the text representation of the given sequence of moves in 
        /// coordinate notation. 
        /// </summary>
        /// <param name="moves">The sequence of moves to identify.</param>
        /// <returns>The text representation of the given sequence of moves in coordinate notation.</returns>
        public static String Moves(List<Int32> moves) {
            if (moves.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder(6 * moves.Count);
            foreach (Int32 move in moves) {
                sb.Append(Move(move));
                sb.Append(' ');
            }
            return sb.ToString(0, sb.Length - 1);
        }

        /// <summary>
        /// Returns the text representation of the given move in algebraic notation. 
        /// </summary>
        /// <param name="position">The position on which the move is to be played.</param>
        /// <param name="move">The move to identify.</param>
        /// <returns>The text representation of the given move in algebraic notation</returns>
        public static String MoveAlgebraically(Position position, Int32 move) {
            if (MoveClass.IsCastle(move))
                return MoveClass.To(move) < MoveClass.From(move) ? "O-O-O" : "O-O";

            // Determine the piece associated with the move. Pawns are not explicitly 
            // identified. 
            String piece = (MoveClass.Piece(move) & PieceClass.Type) == PieceClass.Pawn ? "" : PieceInitial(MoveClass.Piece(move));

            // Determine the necessary disambiguation property for the move. If two or 
            // more pieces of the same type are moving to the same square, disambiguate 
            // with the square that it is moving from's file, rank, or both, in that 
            // order. 
            String disambiguation = "";
            List<Int32> alternatives = new List<Int32>();
            foreach (Int32 alt in position.LegalMoves())
                if (alt != move && MoveClass.Piece(alt) == MoveClass.Piece(move) && MoveClass.To(alt) == MoveClass.To(move))
                    alternatives.Add(alt);

            if (alternatives.Count > 0) {
                Boolean uniqueFile = true;
                Boolean uniqueRank = true;
                foreach (Int32 alt in alternatives) {
                    if (Position.File(MoveClass.From(alt)) == Position.File(MoveClass.From(move)))
                        uniqueFile = false;
                    if (Position.Rank(MoveClass.From(alt)) == Position.Rank(MoveClass.From(move)))
                        uniqueRank = false;
                }
                if (uniqueFile)
                    disambiguation = File(MoveClass.From(move));
                else if (uniqueRank)
                    disambiguation = Rank(MoveClass.From(move));
                else
                    disambiguation = Square(MoveClass.From(move));
            }

            // Determine if the capture flag is necessary for the move. If the capturing 
            // piece is a pawn, it is identified by the file it is moving from. 
            Boolean isCapture = MoveClass.IsCapture(move) || MoveClass.IsEnPassant(move);
            String capture = isCapture ? "x" : "";
            if ((MoveClass.Piece(move) & PieceClass.Type) == PieceClass.Pawn && isCapture)
                if (disambiguation == "")
                    disambiguation = File(MoveClass.From(move));

            // Determine the square property for the move. 
            String square = Square(MoveClass.To(move));

            // Determine the necessary promotion property for the move. 
            String promotion = MoveClass.IsPromotion(move) ? "=" + PieceInitial(MoveClass.Special(move)) : "";

            // Determine the necessary check property for the move. 
            String check = "";
            position.Make(move);
            if (position.InCheck(position.SideToMove))
                check = position.LegalMoves().Count > 0 ? "+" : "#";
            position.Unmake(move);

            return piece + disambiguation + capture + square + promotion + check;
        }

        /// <summary>
        /// Returns the text representation of the given sequence of moves in 
        /// algebraic notation. 
        /// </summary>
        /// <param name="position">The position on which the sequence of moves are to be played.</param>
        /// <param name="moves">The sequence of moves to identify.</param>
        /// <param name="options">The identification option specifying whether to be absolutely proper.</param>
        /// <returns>The text representation of the given sequence of moves in algebraic notation</returns>
        public static String MovesAlgebraically(Position position, List<Int32> moves, IdentificationOptions options = IdentificationOptions.None) {
            if (moves.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder(5 * moves.Count);
            Int32 halfMoves = 0;

            if (options == IdentificationOptions.Proper) {
                halfMoves = position.HalfMoves;
                if (position.SideToMove == PieceClass.Black) {
                    sb.Append(halfMoves / 2 + 1);
                    sb.Append("... ");
                }
            }

            foreach (Int32 move in moves) {
                if ((halfMoves++ % 2) == 0) {
                    sb.Append(halfMoves / 2 + 1);
                    sb.Append('.');
                    if (options == IdentificationOptions.Proper)
                        sb.Append(' ');
                }
                sb.Append(MoveAlgebraically(position, move));
                sb.Append(' ');
                position.Make(move);
            }
            for (Int32 i = moves.Count - 1; i >= 0; i--)
                position.Unmake(moves[i]);

            return sb.ToString(0, sb.Length - 1);
        }
    }
}
