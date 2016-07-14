using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the move generation component of the chess position. 
    /// </summary>
    public sealed partial class Position : IEquatable<Position> {
        
        /// <summary>
        /// Returns the list of legal moves for the position. Don't use this overload
        /// when performance is important.
        /// </summary>
        /// <returns>The list of legal moves for the position.</returns>
        public List<Int32> LegalMoves() {
            Int32[] moves = new Int32[256];
            Int32 movesCount = LegalMoves(moves);
            List<Int32> list = new List<Int32>();
            for (Int32 i = 0; i < movesCount; i++)
                list.Add(moves[i]);
            return list;
        }

        /// <summary>
        /// Populates the given array with the legal moves for the position and 
        /// returns the number of legal moves. 
        /// </summary>
        /// <param name="moves">The array to populate with the legal moves.</param>
        /// <returns>The number of legal moves for the position.</returns>
        public Int32 LegalMoves(Int32[] moves) {

            // Initialize bitboards and squares that describe the position. 
            Int32 enemy = 1 - SideToMove;
            Int32 kingSquare = Bit.Read(Bitboard[SideToMove | Piece.King]);

            UInt64 selfBitboard = Bitboard[SideToMove];
            UInt64 enemyBitboard = Bitboard[enemy];
            UInt64 targetBitboard = ~selfBitboard;

            UInt64 enemyBishopQueenBitboard = Bitboard[enemy | Piece.Bishop] | Bitboard[enemy | Piece.Queen];
            UInt64 enemyRookQueenBitboard = Bitboard[enemy | Piece.Rook] | Bitboard[enemy | Piece.Queen];

            // Initialize variables for move generation. 
            UInt64 checkingBitboard = 0;
            UInt64 pinningBitboard = 0;
            Int32 index = 0;

            // Consider knight and pawn checks. 
            checkingBitboard |= Bitboard[enemy | Piece.Knight] & Attack.Knight(kingSquare);
            checkingBitboard |= Bitboard[enemy | Piece.Pawn] & Attack.Pawn(kingSquare, SideToMove);

            // Consider bishop and queen checks and pins. 
            if ((enemyBishopQueenBitboard & Bit.Diagonals[kingSquare]) != 0) {
                checkingBitboard |= enemyBishopQueenBitboard & Attack.Bishop(kingSquare, OccupiedBitboard);

                // Determine pinning pieces by removing the first line of defence around the
                // the king, then seeing which pieces are able to attack.
                UInt64 defenceRemovedBitboard = OccupiedBitboard;
                UInt64 defenceBitboard = Bit.RayNE[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);
                defenceBitboard = Bit.RayNW[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);
                defenceBitboard = Bit.RaySE[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.Scan(defenceBitboard);
                defenceBitboard = Bit.RaySW[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.Scan(defenceBitboard);

                if (defenceRemovedBitboard != OccupiedBitboard)
                    pinningBitboard |= enemyBishopQueenBitboard & Attack.Bishop(kingSquare, defenceRemovedBitboard);
            }

            // Consider rook and queen checks and pins. 
            if ((enemyRookQueenBitboard & Bit.Axes[kingSquare]) != 0) {
                checkingBitboard |= enemyRookQueenBitboard & Attack.Rook(kingSquare, OccupiedBitboard);

                // Determine pinning pieces by removing the first line of defence around the
                // the king, then seeing which pieces are able to attack.
                UInt64 defenceRemovedBitboard = OccupiedBitboard;
                UInt64 defenceBitboard = Bit.RayN[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);
                defenceBitboard = Bit.RayE[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.Scan(defenceBitboard);
                defenceBitboard = Bit.RayS[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.Scan(defenceBitboard);
                defenceBitboard = Bit.RayW[kingSquare] & selfBitboard;
                if (defenceBitboard != 0)
                    defenceRemovedBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);

                if (defenceRemovedBitboard != OccupiedBitboard)
                    pinningBitboard |= enemyRookQueenBitboard & Attack.Rook(kingSquare, defenceRemovedBitboard);
            }

            // Consider castling. This is always fully tested for legality. 
            if (checkingBitboard == 0) {
                Int32 rank = -56 * SideToMove + 56;

                if (CastleQueenside[SideToMove] > 0 && (Square[1 + rank] | Square[2 + rank] | Square[3 + rank]) == Piece.Empty)
                    if (!IsAttacked(SideToMove, 3 + rank) && !IsAttacked(SideToMove, 2 + rank))
                        moves[index++] = Move.Create(this, kingSquare, 2 + rank, SideToMove | Piece.King);

                if (CastleKingside[SideToMove] > 0 && (Square[5 + rank] | Square[6 + rank]) == Piece.Empty)
                    if (!IsAttacked(SideToMove, 5 + rank) && !IsAttacked(SideToMove, 6 + rank))
                        moves[index++] = Move.Create(this, kingSquare, 6 + rank, SideToMove | Piece.King);
            }

            // Consider en passant. This is always fully tested for legality. 
            if (EnPassantSquare != InvalidSquare) {

                UInt64 enPassantPawnBitboard = Bitboard[SideToMove | Piece.Pawn] & Attack.Pawn(EnPassantSquare, enemy);
                UInt64 enPassantVictimBitboard = Move.Pawn(EnPassantSquare, enemy);
                while (enPassantPawnBitboard != 0) {

                    // Perform minimal state changes to mimick en passant and check for 
                    // legality. 
                    Int32 from = Bit.Pop(ref enPassantPawnBitboard);
                    Bitboard[enemy | Piece.Pawn] ^= enPassantVictimBitboard;
                    OccupiedBitboard ^= enPassantVictimBitboard;
                    OccupiedBitboard ^= (1UL << from) | (1UL << EnPassantSquare);

                    // Check for legality and add move. 
                    if (!IsAttacked(SideToMove, kingSquare))
                        moves[index++] = Move.Create(this, from, EnPassantSquare, enemy | Piece.Pawn);

                    // Revert state changes. 
                    Bitboard[enemy | Piece.Pawn] ^= enPassantVictimBitboard;
                    OccupiedBitboard ^= enPassantVictimBitboard;
                    OccupiedBitboard ^= (1UL << from) | (1UL << EnPassantSquare);
                }
            }

            // Consider king moves. This is always fully tested for legality. 
            {
                Int32 from = kingSquare;
                UInt64 moveBitboard = targetBitboard & Attack.King(from);
                while (moveBitboard != 0) {

                    // Perform minimal state changes to mimick real move and check for legality. 
                    Int32 to = Bit.Pop(ref moveBitboard);
                    UInt64 occupiedBitboardCopy = OccupiedBitboard;
                    Int32 capture = Square[to];
                    Bitboard[capture] ^= 1UL << to;
                    OccupiedBitboard ^= 1UL << from;
                    OccupiedBitboard |= 1UL << to;

                    // Check for legality and add move. 
                    if (!IsAttacked(SideToMove, to))
                        moves[index++] = Move.Create(this, from, to);

                    // Revert state changes. 
                    Bitboard[capture] ^= 1UL << to;
                    OccupiedBitboard = occupiedBitboardCopy;
                }
            }

            // Case 1. If we are not in check and there are no pinned pieces, we don't 
            //         need to test normal moves for legality. 
            if (checkingBitboard == 0 & pinningBitboard == 0) {

                // Consider normal pawn moves. 
                UInt64 pieceBitboard = Bitboard[SideToMove | Piece.Pawn];
                while (pieceBitboard != 0) {

                    // Consider single square advance. 
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    Int32 to = from + 16 * SideToMove - 8;
                    UInt64 moveBitboard = ~OccupiedBitboard & (1UL << to);

                    // Consider two square advance. 
                    if (moveBitboard != 0 && (from - 16) * (from - 47) > 0 && (to - 8) * (to - 55) < 0)
                        moveBitboard |= ~OccupiedBitboard & (1UL << (from + 32 * SideToMove - 16));

                    // Consider captures. 
                    UInt64 attackBitboard = Attack.Pawn(from, SideToMove);
                    moveBitboard |= enemyBitboard & attackBitboard;

                    // Populate pawn moves. 
                    while (moveBitboard != 0) {
                        to = Bit.Pop(ref moveBitboard);
                        if ((to - 8) * (to - 55) > 0) {
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Queen);
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Knight);
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Rook);
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Bishop);
                        } else
                            moves[index++] = Move.Create(this, from, to);
                    }
                }

                // Consider knight moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Knight];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Knight(from);
                    while (moveBitboard != 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                // Consider bishop moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Bishop];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Bishop(from, OccupiedBitboard);
                    while (moveBitboard != 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                // Consider queen moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Queen];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Queen(from, OccupiedBitboard);
                    while (moveBitboard != 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                // Consider rook moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Rook];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Rook(from, OccupiedBitboard);
                    while (moveBitboard != 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }
            }

            // Case 2. There are pinned pieces or a single check. We can still move but 
            //         all moves are tested for legality. 
            else if ((checkingBitboard & (checkingBitboard - 1)) == 0) {

                // Consider pawn moves. 
                UInt64 pieceBitboard = Bitboard[SideToMove | Piece.Pawn];
                while (pieceBitboard != 0) {

                    // Consider single square advance. 
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    Int32 to = from + 16 * SideToMove - 8;
                    UInt64 moveBitboard = ~OccupiedBitboard & (1UL << to);

                    // Consider two square advance. 
                    if (moveBitboard != 0 && (from - 16) * (from - 47) > 0 && (to - 8) * (to - 55) < 0)
                        moveBitboard |= ~OccupiedBitboard & (1UL << (from + 32 * SideToMove - 16));

                    // Consider captures. 
                    UInt64 attackBitboard = Attack.Pawn(from, SideToMove);
                    moveBitboard |= enemyBitboard & attackBitboard;

                    // Populate pawn moves. 
                    while (moveBitboard != 0) {

                        // Perform minimal state changes to mimick real move and check for legality. 
                        to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;

                        // Check for legality and add moves. 
                        if (!IsAttacked(SideToMove, kingSquare))
                            if ((to - 8) * (to - 55) > 0) {
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Queen);
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Knight);
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Rook);
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Bishop);
                            } else
                                moves[index++] = Move.Create(this, from, to);

                        // Revert state changes. 
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }

                // Consider knight moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Knight];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Knight(from);
                    while (moveBitboard != 0) {

                        // Perform minimal state changes to mimick real move and check for legality. 
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;

                        // Check for legality and add move. 
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);

                        // Revert state changes. 
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }

                // Consider bishop moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Bishop];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Bishop(from, OccupiedBitboard);
                    while (moveBitboard != 0) {

                        // Perform minimal state changes to mimick real move and check for legality. 
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;

                        // Check for legality and add move. 
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);

                        // Revert state changes. 
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }

                // Consider queen moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Queen];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Queen(from, OccupiedBitboard);
                    while (moveBitboard != 0) {

                        // Perform minimal state changes to mimick real move and check for legality. 
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;

                        // Check for legality and add move. 
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);

                        // Revert state changes. 
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }

                // Consider rook moves. 
                pieceBitboard = Bitboard[SideToMove | Piece.Rook];
                while (pieceBitboard != 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Rook(from, OccupiedBitboard);
                    while (moveBitboard != 0) {

                        // Perform minimal state changes to mimick real move and check for legality. 
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;

                        // Check for legality and add move. 
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);

                        // Revert state changes. 
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Populates the given array with the pseudo-legal capturing and queen 
        /// promotion moves for the position and returns the number of moves. 
        /// </summary>
        /// <param name="moves">The array to populate with the pseudo-legal moves.</param>
        /// <returns>The number of moves generated for the position.</returns>
        public Int32 PseudoQuiescenceMoves(Int32[] moves) {
            UInt64 targetBitboard = Bitboard[(1 - SideToMove)];
            Int32 index = 0;

            // Consider king moves. 
            UInt64 pieceBitboard = Bitboard[SideToMove | Piece.King];
            Int32 from = Bit.Read(pieceBitboard);
            UInt64 moveBitboard = targetBitboard & Attack.King(from);
            while (moveBitboard != 0) {
                Int32 to = Bit.Pop(ref moveBitboard);
                moves[index++] = Move.Create(this, from, to);
            }


            // Consider queen moves. 
            pieceBitboard = Bitboard[SideToMove | Piece.Queen];
            while (pieceBitboard != 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Queen(from, OccupiedBitboard);
                while (moveBitboard != 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            // Consider rook moves. 
            pieceBitboard = Bitboard[SideToMove | Piece.Rook];
            while (pieceBitboard != 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Rook(from, OccupiedBitboard);
                while (moveBitboard != 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            // Consider knight moves. 
            pieceBitboard = Bitboard[SideToMove | Piece.Knight];
            while (pieceBitboard != 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Knight(from);
                while (moveBitboard != 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            // Consider bishop moves. 
            pieceBitboard = Bitboard[SideToMove | Piece.Bishop];
            while (pieceBitboard != 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Bishop(from, OccupiedBitboard);
                while (moveBitboard != 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            // Consider pawn moves. 
            pieceBitboard = Bitboard[SideToMove | Piece.Pawn];
            while (pieceBitboard != 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Pawn(from, SideToMove);
                Int32 to = from + 16 * SideToMove - 8;
                Boolean promotion = (to - 8) * (to - 55) > 0;
                if (promotion)
                    moveBitboard |= ~OccupiedBitboard & (1UL << to);
                while (moveBitboard != 0) {
                    to = Bit.Pop(ref moveBitboard);
                    if (promotion)
                        moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Queen);
                    else
                        moves[index++] = Move.Create(this, from, to);
                }
            }
            return index;
        }
    }
}
