using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates general chess logic routines for the chess position. 
    /// </summary>
    public sealed partial class Position : IEquatable<Position> {

        /// <summary>
        /// Returns whether the given side is in check. 
        /// </summary>
        /// <param name="colour">The side to test for check.</param>
        /// <returns>Whether the given side is in check.</returns>
        public Boolean InCheck(Int32 colour) {
            return IsAttacked(colour, Bit.Read(Bitboard[colour | Piece.King]));
        }

        /// <summary>
        /// Returns whether the given side is attacked on the given square. 
        /// </summary>
        /// <param name="colour">The side to test for being attacked.</param>
        /// <param name="square">The square to test for attacks.</param>
        /// <returns>Whether the given side is attacked on the given square</returns>
        public Boolean IsAttacked(Int32 colour, Int32 square) {
            Int32 enemy = 1 - colour;

            if ((Bitboard[enemy | Piece.Knight] & Attack.Knight(square)) != 0
             || (Bitboard[enemy | Piece.Pawn] & Attack.Pawn(square, colour)) != 0
             || (Bitboard[enemy | Piece.King] & Attack.King(square)) != 0)
                return true;

            UInt64 bishopQueenBitboard = Bitboard[enemy | Piece.Bishop] | Bitboard[enemy | Piece.Queen];
            if ((bishopQueenBitboard & Bit.Diagonals[square]) != 0
             && (bishopQueenBitboard & Attack.Bishop(square, OccupiedBitboard)) != 0)
                return true;

            UInt64 rookQueenBitboard = Bitboard[enemy | Piece.Rook] | Bitboard[enemy | Piece.Queen];
            if ((rookQueenBitboard & Bit.Axes[square]) != 0
             && (rookQueenBitboard & Attack.Rook(square, OccupiedBitboard)) != 0)
                return true;

            return false;
        }

        /// <summary>
        /// Returns whether the given move puts the opponent in check.  
        /// </summary>
        /// <param name="move">The move to test for check.</param>
        /// <returns>Whether the given move puts the opponent in check.</returns>
        public Boolean CausesCheck(Int32 move) {
            UInt64 fromBitboard = 1UL << Move.From(move);
            UInt64 toBitboard = 1UL << Move.To(move);
            Int32 piece = Move.Piece(move);
            Int32 special = Move.Special(move);
            UInt64 occupiedBitboardCopy = OccupiedBitboard;

            Boolean value = false;
            switch (special & Piece.Mask) {

                // Consider normal move. 
                case Piece.Empty:
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard ^= fromBitboard;
                    OccupiedBitboard |= toBitboard;
                    value = InCheck(1 - SideToMove);
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard = occupiedBitboardCopy;
                    break;

                // Consider castling. 
                case Piece.King:
                    UInt64 rookToBitboard = 1UL << ((toBitboard < fromBitboard ? 3 : 5) + Rank(Move.To(move)) * 8);
                    Bitboard[SideToMove | Piece.Rook] ^= rookToBitboard;
                    OccupiedBitboard ^= fromBitboard;
                    value = InCheck(1 - SideToMove);
                    Bitboard[SideToMove | Piece.Rook] ^= rookToBitboard;
                    OccupiedBitboard = occupiedBitboardCopy;
                    break;

                // Consider en passant. 
                case Piece.Pawn:
                    UInt64 enPassantPawnBitboard = Move.Pawn(EnPassantSquare, 1 - SideToMove);
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard ^= fromBitboard | toBitboard | enPassantPawnBitboard;
                    value = InCheck(1 - SideToMove);
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard = occupiedBitboardCopy;
                    break;

                // Consider pawn promotion. 
                default:
                    Bitboard[SideToMove | Piece.Pawn] ^= fromBitboard;
                    Bitboard[special] ^= toBitboard;
                    OccupiedBitboard ^= fromBitboard;
                    OccupiedBitboard |= toBitboard;
                    value = InCheck(1 - SideToMove);
                    Bitboard[SideToMove | Piece.Pawn] ^= fromBitboard;
                    Bitboard[special] ^= toBitboard;
                    OccupiedBitboard = occupiedBitboardCopy;
                    break;
            }
            return value;
        }

        /// <summary>
        /// Returns whether the position represents a draw by insufficient material. 
        /// </summary>
        /// <returns>Whether the position represents a draw by insufficient material.</returns>
        public Boolean InsufficientMaterial() {
            Int32 pieces = Bit.Count(OccupiedBitboard);
            if (pieces > 4)
                return false;
            if (pieces <= 2)
                return true;
            if (pieces <= 3)
                return Bitboard[Colour.White | Piece.Knight] != 0
                    || Bitboard[Colour.White | Piece.Bishop] != 0
                    || Bitboard[Colour.Black | Piece.Knight] != 0
                    || Bitboard[Colour.Black | Piece.Bishop] != 0;
            if (Bit.CountSparse(Bitboard[Colour.White | Piece.Knight]) == 2
             || Bit.CountSparse(Bitboard[Colour.Black | Piece.Knight]) == 2)
                return true;
            if (Bitboard[Colour.White | Piece.Bishop] != 0
             && Bitboard[Colour.Black | Piece.Bishop] != 0 
             && ((Bitboard[Colour.White | Piece.Bishop] & Bit.LightSquares) != 0)
             == ((Bitboard[Colour.Black | Piece.Bishop] & Bit.LightSquares) != 0))
                return true;
            return false;
        }

        /// <summary>
        /// Returns whether the position has repeated the given number of times. 
        /// </summary>
        /// <param name="times">The number of repetitions to test for.</param>
        /// <returns>Whether the position has repeated the given number of times.</returns>
        public Boolean HasRepeated(Int32 times) {
            Int32 repetitions = 1;
            for (Int32 i = HalfMoves - 4; i >= HalfMoves - FiftyMovesClock; i -= 2)
                if (ZobristKeyHistory[i] == ZobristKey)
                    if (++repetitions >= times)
                        return true;
            return false;
        }

        /// <summary>
        /// Returns the file of the given square.
        /// </summary>
        /// <param name="square">The square to determine the file of.</param>
        /// <returns>The file of the given square.</returns>
        public static Int32 File(Int32 square) {
            return square & 7;
        }

        /// <summary>
        /// Returns the rank of the given square.
        /// </summary>
        /// <param name="square">The square to determine the rank of.</param>
        /// <returns>The rank of the given square.</returns>
        public static Int32 Rank(Int32 square) {
            return square >> 3;
        }

        /// <summary>
        /// Returns the square with the given name.
        /// </summary>
        /// <param name="name">The name of the square.</param>
        /// <returns>The square with the given name</returns>
        public static Int32 SquareAt(String name) {
            return (Int32)(name[0] - 'a' + ('8' - name[1]) * 8);
        }
    }
}
