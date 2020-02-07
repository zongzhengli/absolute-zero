using System;
using System.Runtime.CompilerServices;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the move making component of the chess position. 
    /// </summary>
    public sealed partial class Position : IEquatable<Position> {

        /// <summary>
        /// Makes the given move on the position.
        /// </summary>
        /// <param name="move">The move to make.</param>
        public void Make(Int32 move) {
            Int32 from = Move.From(move);
            Int32 to = Move.To(move);
            Int32 piece = Move.Piece(move);
            Int32 capture = Move.Capture(move);
            Int32 special = Move.Special(move);

            // Update core board state.
            Value[SideToMove] -= GetIncrementalValue(from, piece);
            Square[to] = piece;
            Square[from] = Piece.Empty;
            Bitboard[piece] ^= (1UL << from) | (1UL << to);
            Bitboard[SideToMove] ^= (1UL << from) | (1UL << to);
            OccupiedBitboard ^= (1UL << from) | (1UL << to);

            // Update metainformation.
            ZobristKey ^= Zobrist.PiecePosition[piece][from] ^ Zobrist.PiecePosition[piece][to];
            ZobristKey ^= Zobrist.Colour;
            if (EnPassantSquare != InvalidSquare) {
                ZobristKey ^= Zobrist.EnPassant[EnPassantSquare];
                EnPassantSquare = InvalidSquare;
            }
            FiftyMovesClock++;
            HalfMoves++;

            // Handle capture if applicable.
            switch (capture & Piece.Mask) {
                case Piece.Empty:
                    break;
                case Piece.Rook:
                    if ((SideToMove == Colour.White && to == 0) || (SideToMove == Colour.Black && to == 56)) {
                        if (CastleQueenside[1 - SideToMove]-- > 0)
                            ZobristKey ^= Zobrist.CastleQueenside[1 - SideToMove];
                    } else if ((SideToMove == Colour.White && to == 7) || (SideToMove == Colour.Black && to == 63))
                        if (CastleKingside[1 - SideToMove]-- > 0)
                            ZobristKey ^= Zobrist.CastleKingside[1 - SideToMove];
                    goto default;
                default:
                    Bitboard[capture] ^= 1UL << to;
                    Bitboard[1 - SideToMove] ^= 1UL << to;
                    OccupiedBitboard |= 1UL << to;
                    ZobristKey ^= Zobrist.PiecePosition[capture][to];
                    Value[1 - SideToMove] -= Zero.PieceValue[capture];
                    FiftyMovesClock = 0;
                    break;
            }

            switch (special & Piece.Mask) {
                // Handle regular move (not en passant, castling, or pawn promotion).
                case Piece.Empty:
                    switch (piece & Piece.Mask) {
                        // For pawn move, update fifty moves clock and en passant state.
                        case Piece.Pawn:
                            FiftyMovesClock = 0;
                            if ((from - to) * (from - to) == 256) {
                                ZobristKey ^= Zobrist.EnPassant[from];
                                EnPassantHistory[HalfMoves] = EnPassantSquare = (from + to) / 2;
                            }
                            break;
                        // For rook move, disable castling on one side.
                        case Piece.Rook:
                            if ((SideToMove == Colour.White && from == 56) || (SideToMove == Colour.Black && from == 0)) {
                                if (CastleQueenside[SideToMove]-- > 0)
                                    ZobristKey ^= Zobrist.CastleQueenside[SideToMove];
                            } else if ((SideToMove == Colour.White && from == 63) || (SideToMove == Colour.Black && from == 7))
                                if (CastleKingside[SideToMove]-- > 0)
                                    ZobristKey ^= Zobrist.CastleKingside[SideToMove];
                            break;
                        // For king move, disable castling on both sides.
                        case Piece.King:
                            if (CastleQueenside[SideToMove]-- > 0)
                                ZobristKey ^= Zobrist.CastleQueenside[SideToMove];
                            if (CastleKingside[SideToMove]-- > 0)
                                ZobristKey ^= Zobrist.CastleKingside[SideToMove];
                            break;
                    }
                    break;
                // Handle castling.
                case Piece.King:
                    if (CastleQueenside[SideToMove]-- > 0)
                        ZobristKey ^= Zobrist.CastleQueenside[SideToMove];
                    if (CastleKingside[SideToMove]-- > 0)
                        ZobristKey ^= Zobrist.CastleKingside[SideToMove];
                    Int32 rookFrom;
                    Int32 rookTo;
                    if (to < from) {
                        rookFrom = Rank(to) * 8;
                        rookTo = 3 + Rank(to) * 8;
                    } else {
                        rookFrom = 7 + Rank(to) * 8;
                        rookTo = 5 + Rank(to) * 8;
                    }
                    Bitboard[SideToMove | Piece.Rook] ^= (1UL << rookFrom) | (1UL << rookTo);
                    Bitboard[SideToMove] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedBitboard ^= (1UL << rookFrom) | (1UL << rookTo);
                    ZobristKey ^= Zobrist.PiecePosition[SideToMove | Piece.Rook][rookFrom];
                    ZobristKey ^= Zobrist.PiecePosition[SideToMove | Piece.Rook][rookTo];
                    Square[rookFrom] = Piece.Empty;
                    Square[rookTo] = SideToMove | Piece.Rook;
                    break;
                // Handle en passant.
                case Piece.Pawn:
                    Square[File(to) + Rank(from) * 8] = Piece.Empty;
                    Bitboard[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    Bitboard[1 - SideToMove] ^= 1UL << (File(to) + Rank(from) * 8);
                    OccupiedBitboard ^= 1UL << (File(to) + Rank(from) * 8);
                    ZobristKey ^= Zobrist.PiecePosition[special][File(to) + Rank(from) * 8];
                    Value[1 - SideToMove] -= Zero.PieceValue[special];
                    break;
                // Handle pawn promotion.
                default:
                    Bitboard[piece] ^= 1UL << to;
                    Bitboard[special] ^= 1UL << to;
                    ZobristKey ^= Zobrist.PiecePosition[piece][to];
                    ZobristKey ^= Zobrist.PiecePosition[special][to];
                    Value[SideToMove] += Zero.PieceValue[special] - Zero.PieceValue[piece];
                    Square[to] = special;
                    break;
            }

            Value[SideToMove] += GetIncrementalValue(to, piece);
            SideToMove = 1 - SideToMove;
            FiftyMovesHistory[HalfMoves] = FiftyMovesClock;
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        /// <summary>
        /// Unmakes the given move from the position.
        /// </summary>
        /// <param name="move">The move to unmake.</param>
        public void Unmake(Int32 move) {
            Int32 from = Move.From(move);
            Int32 to = Move.To(move);
            Int32 piece = Move.Piece(move);
            Int32 capture = Move.Capture(move);
            Int32 special = Move.Special(move);

            // Rewind core board state.
            SideToMove = 1 - SideToMove;
            Value[SideToMove] -= GetIncrementalValue(to, piece);
            Square[from] = piece;
            Square[to] = capture;
            Bitboard[piece] ^= (1UL << from) | (1UL << to);
            Bitboard[SideToMove] ^= (1UL << from) | (1UL << to);
            OccupiedBitboard ^= (1UL << from) | (1UL << to);

            // Rewind metainformation.
            ZobristKey = ZobristKeyHistory[HalfMoves - 1];
            EnPassantHistory[HalfMoves] = InvalidSquare;
            EnPassantSquare = EnPassantHistory[HalfMoves - 1];
            FiftyMovesClock = FiftyMovesHistory[HalfMoves - 1];
            HalfMoves--;

            // Rewind capture if applicable.
            switch (capture & Piece.Mask) {
                case Piece.Empty:
                    break;
                case Piece.Rook:
                    if ((SideToMove == Colour.White && to == 0) || (SideToMove == Colour.Black && to == 56)) {
                        CastleQueenside[1 - SideToMove]++;
                    } else if ((SideToMove == Colour.White && to == 7) || (SideToMove == Colour.Black && to == 63))
                        CastleKingside[1 - SideToMove]++;
                    goto default;
                default:
                    Bitboard[capture] ^= 1UL << to;
                    Bitboard[1 - SideToMove] ^= 1UL << to;
                    OccupiedBitboard |= 1UL << to;
                    Value[1 - SideToMove] += Zero.PieceValue[capture];
                    break;
            }

            switch (special & Piece.Mask) {
                // Rewind regular move.
                case Piece.Empty:
                    switch (piece & Piece.Mask) {
                        // For rook move, restore castling on one side if applicable.
                        case Piece.Rook:
                            if ((SideToMove == Colour.White && from == 56) || (SideToMove == Colour.Black && from == 0)) {
                                CastleQueenside[SideToMove]++;
                            } else if ((SideToMove == Colour.White && from == 63) || (SideToMove == Colour.Black && from == 7))
                                CastleKingside[SideToMove]++;
                            break;
                        // For king move, restore castling on both sides if applicable.
                        case Piece.King:
                            CastleQueenside[SideToMove]++;
                            CastleKingside[SideToMove]++;
                            break;
                    }
                    break;
                // Rewind castling.
                case Piece.King:
                    CastleQueenside[SideToMove]++;
                    CastleKingside[SideToMove]++;
                    Int32 rookFrom;
                    Int32 rookTo;
                    if (to < from) {
                        rookFrom = Rank(to) * 8;
                        rookTo = 3 + Rank(to) * 8;
                    } else {
                        rookFrom = 7 + Rank(to) * 8;
                        rookTo = 5 + Rank(to) * 8;
                    }
                    Bitboard[SideToMove | Piece.Rook] ^= (1UL << rookFrom) | (1UL << rookTo);
                    Bitboard[SideToMove] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedBitboard ^= (1UL << rookFrom) | (1UL << rookTo);
                    Square[rookFrom] = SideToMove | Piece.Rook;
                    Square[rookTo] = Piece.Empty;
                    break;
                // Rewind en passant.
                case Piece.Pawn:
                    Square[File(to) + Rank(from) * 8] = special;
                    Bitboard[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    Bitboard[1 - SideToMove] ^= 1UL << (File(to) + Rank(from) * 8);
                    OccupiedBitboard ^= 1UL << (File(to) + Rank(from) * 8);
                    Value[1 - SideToMove] += Zero.PieceValue[special];
                    break;
                // Rewind pawn promotion.
                default:
                    Bitboard[piece] ^= 1UL << to;
                    Bitboard[special] ^= 1UL << to;
                    Value[SideToMove] -= Zero.PieceValue[special] - Zero.PieceValue[piece];
                    break;
            }
            Value[SideToMove] += GetIncrementalValue(from, piece);
        }

        /// <summary>
        /// Makes the null move on the position.
        /// </summary>
        public void MakeNull() {
            ZobristKey ^= Zobrist.Colour;
            if (EnPassantSquare != InvalidSquare) {
                ZobristKey ^= Zobrist.EnPassant[EnPassantSquare];
                EnPassantSquare = InvalidSquare;
            }
            SideToMove = 1 - SideToMove;
            FiftyMovesClock++;
            HalfMoves++;
            FiftyMovesHistory[HalfMoves] = FiftyMovesClock;
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        /// <summary>
        /// Unmakes the null move on the position.
        /// </summary>
        public void UnmakeNull() {
            FiftyMovesClock = FiftyMovesHistory[HalfMoves - 1];
            ZobristKey = ZobristKeyHistory[HalfMoves - 1];
            EnPassantSquare = EnPassantHistory[HalfMoves - 1];
            SideToMove = 1 - SideToMove;
            HalfMoves--;
        }

        private Int32 GetIncrementalValue(Int32 square, Int32 piece) {
            Int32 colour = piece & Colour.Mask;
            Single value = 0;
            switch (piece & Piece.Mask) {
                case Piece.Pawn: {
                    value += Zero.PawnPositionValue[colour][square];
                    break;
                }
                case Piece.Bishop: {
                    value += Zero.BishopPositionValue[colour][square];
                    break;
                }
                case Piece.Knight: {
                    Single opening = GetPhase();
                    value += opening * Zero.KnightOpeningPositionValue[colour][square];
                    break;
                }
                case Piece.Rook: {
                    value += Zero.RookPositionValue[colour][square];
                    break;
                }
                case Piece.Queen: {
                    Single opening = GetPhase();
                    value += opening * Zero.QueenOpeningPositionValue[colour][square];
                    break;
                }
                case Piece.King: {
                    Single opening = GetPhase();
                    Single endgame = 1 - opening;
                    value += opening * Zero.KingOpeningPositionValue[colour][square];
                    value += endgame * Zero.KingEndgamePositionValue[colour][square];
                    break;
                }
            }
            return (Int32)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Single GetPhase() {
            return (Bit.Count(OccupiedBitboard) - 2) / 30F;
        }
    }
}
