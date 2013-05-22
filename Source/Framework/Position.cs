using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace AbsoluteZero {
    class Position : IEquatable<Position> {
        public const String StartingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const Int32 HalfMovesLimit = 1024;
        public const Int32 InvalidSquare = -1;

        public Int32[] Square = new Int32[64];
        public UInt64[] Bitboard = new UInt64[16];
        public UInt64 OccupiedBitboard = 0;
        public Int32[] Material = new Int32[2];
        public Int32 SideToMove = 0;
        public Int32 HalfMoves = 0;
        public Int32[] CastleKingside = new Int32[2];
        public Int32[] CastleQueenside = new Int32[2];
        public Int32 EnPassantSquare = InvalidSquare;
        public Int32[] EnPassantHistory = new Int32[HalfMovesLimit];
        public Int32 FiftyMovesClock = 0;
        public Int32[] FiftyMovesHistory = new Int32[HalfMovesLimit];
        public UInt64 ZobristKey;
        public UInt64[] ZobristKeyHistory = new UInt64[HalfMovesLimit];

        public Position() { }

        public Position(String fen) {
            try {
                ParseFen(fen);
            } catch {
                ParseFen(StartingFEN);
            }
        }

        private void ParseFen(String fen) {
            Array.Clear(Square, 0, Square.Length);
            String[] terms = fen.Trim().Split(' ');
            Int32 file = 0;
            Int32 rank = 0;
            foreach (Char c in terms[0]) {
                Char upperChar = Char.ToUpperInvariant(c);
                Int32 colour = c == upperChar ? Piece.White : Piece.Black;
                switch (upperChar) {
                    default:
                        file += upperChar - 48;
                        break;
                    case '/':
                        file = 0;
                        rank++;
                        break;
                    case 'K':
                        Square[file++ + rank * 8] = colour | Piece.King;
                        break;
                    case 'Q':
                        Square[file++ + rank * 8] = colour | Piece.Queen;
                        break;
                    case 'R':
                        Square[file++ + rank * 8] = colour | Piece.Rook;
                        break;
                    case 'B':
                        Square[file++ + rank * 8] = colour | Piece.Bishop;
                        break;
                    case 'N':
                        Square[file++ + rank * 8] = colour | Piece.Knight;
                        break;
                    case 'P':
                        Square[file++ + rank * 8] = colour | Piece.Pawn;
                        break;
                }
            }
            SideToMove = terms[1] == "w" ? Piece.White : Piece.Black;
            if (terms.Length > 2) {
                if (terms[2].Contains("Q"))
                    CastleQueenside[0] = 1;
                if (terms[2].Contains("K"))
                    CastleKingside[0] = 1;
                if (terms[2].Contains("q"))
                    CastleQueenside[1] = 1;
                if (terms[2].Contains("k"))
                    CastleKingside[1] = 1;
            }
            if (terms.Length > 3)
                if (terms[3] != "-")
                    EnPassantSquare = SquareAt(terms[3]);
            if (terms.Length > 5) {
                FiftyMovesClock = Int32.Parse(terms[4]);
                Int32 moveNumber = Int32.Parse(terms[5]);
                HalfMoves = 2 * (moveNumber - 1) + SideToMove;
                HalfMoves = Math.Max(0, HalfMoves);
                FiftyMovesClock = Math.Min(FiftyMovesClock, HalfMoves);
            }

            for (Int32 i = 0; i < EnPassantHistory.Length; i++)
                EnPassantHistory[i] = InvalidSquare;
            EnPassantHistory[HalfMoves] = EnPassantSquare;
            FiftyMovesHistory[HalfMoves] = FiftyMovesClock;
            for (Int32 square = 0; square < Square.Length; square++)
                if (Square[square] != Piece.Empty) {
                    Int32 colour = Square[square] & Piece.Colour;
                    Bitboard[Square[square]] |= 1UL << square;
                    Bitboard[colour | Piece.All] |= 1UL << square;
                    OccupiedBitboard |= 1UL << square;
                    if ((Square[square] & Piece.Type) != Piece.King)
                        Material[colour] += Zero.PieceValue[Square[square]];
                }
            ZobristKey = GenerateZobristKey();
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        public Int32 LegalMoves(Int32[] moves) {
            UInt64 bishopQueenBitboard = Bitboard[(1 - SideToMove) | Piece.Bishop] | Bitboard[(1 - SideToMove) | Piece.Queen];
            UInt64 rookQueenBitboard = Bitboard[(1 - SideToMove) | Piece.Rook] | Bitboard[(1 - SideToMove) | Piece.Queen];
            UInt64 checkBitboard = 0;
            UInt64 pinBitboard = 0;
            Int32 kingSquare = Bit.Read(Bitboard[SideToMove | Piece.King]);

            checkBitboard |= Bitboard[(1 - SideToMove) | Piece.Knight] & Attack.Knight(kingSquare);
            checkBitboard |= Bitboard[(1 - SideToMove) | Piece.Pawn] & Attack.Pawn(kingSquare, SideToMove);
            if ((bishopQueenBitboard & Attack.Diagonals[kingSquare]) > 0) {
                checkBitboard |= bishopQueenBitboard & Attack.Bishop(kingSquare, OccupiedBitboard);

                UInt64 occupiedBitboardCopy = OccupiedBitboard;
                UInt64 pinnedBitboard = Attack.RayNE[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.ScanReverse(pinnedBitboard);
                pinnedBitboard = Attack.RayNW[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.ScanReverse(pinnedBitboard);
                pinnedBitboard = Attack.RaySE[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.Scan(pinnedBitboard);
                pinnedBitboard = Attack.RaySW[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.Scan(pinnedBitboard);

                pinBitboard |= bishopQueenBitboard & Attack.Bishop(kingSquare, OccupiedBitboard);
                OccupiedBitboard = occupiedBitboardCopy;
            }
            if ((rookQueenBitboard & Attack.Axes[kingSquare]) > 0) {
                checkBitboard |= rookQueenBitboard & Attack.Rook(kingSquare, OccupiedBitboard);

                UInt64 occupiedBitboardCopy = OccupiedBitboard;
                UInt64 pinnedBitboard = Attack.RayN[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.ScanReverse(pinnedBitboard);
                pinnedBitboard = Attack.RayE[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.Scan(pinnedBitboard);
                pinnedBitboard = Attack.RayS[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.Scan(pinnedBitboard);
                pinnedBitboard = Attack.RayW[kingSquare] & Bitboard[SideToMove | Piece.All];
                if (pinnedBitboard > 0)
                    OccupiedBitboard ^= 1UL << Bit.ScanReverse(pinnedBitboard);

                pinBitboard |= rookQueenBitboard & Attack.Rook(kingSquare, OccupiedBitboard);
                OccupiedBitboard = occupiedBitboardCopy;
            }

            Int32 index = 0;

            // Castling is always fully tested for legality. 
            if (checkBitboard <= 0) {
                Int32 rank = -56 * SideToMove + 56;
                if (CastleQueenside[SideToMove] > 0 && (Square[1 + rank] | Square[2 + rank] | Square[3 + rank]) == Piece.Empty)
                    if (!IsAttacked(SideToMove, 3 + rank) && !IsAttacked(SideToMove, 2 + rank))
                        moves[index++] = Move.Create(this, kingSquare, 2 + rank, SideToMove | Piece.King);
                if (CastleKingside[SideToMove] > 0 && (Square[5 + rank] | Square[6 + rank]) == Piece.Empty)
                    if (!IsAttacked(SideToMove, 5 + rank) && !IsAttacked(SideToMove, 6 + rank))
                        moves[index++] = Move.Create(this, kingSquare, 6 + rank, SideToMove | Piece.King);
            }

            UInt64 opponentBitboard = Bitboard[(1 - SideToMove) | Piece.All];
            UInt64 targetBitboard = ~Bitboard[SideToMove | Piece.All];
            UInt64 enPassantBitboard = 0;
            UInt64 enPassantPawnBitboard = 0;
            if (EnPassantSquare != InvalidSquare) {
                enPassantBitboard = 1UL << EnPassantSquare;
                enPassantPawnBitboard = Move.Pawn(EnPassantSquare, 1 - SideToMove);
            }

            // Case 1. If we are not in check and there are no pinned pieces, we don't need to test normal moves for legality. 
            if (checkBitboard <= 0 & pinBitboard <= 0) {
                UInt64 pieceBitboard = Bitboard[SideToMove | Piece.Pawn];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    Int32 to = from + 16 * SideToMove - 8;
                    UInt64 moveBitboard = ~OccupiedBitboard & (1UL << to);
                    if (moveBitboard > 0 && (from - 16) * (from - 47) > 0 && (to - 8) * (to - 55) < 0)
                        moveBitboard |= ~OccupiedBitboard & (1UL << (from + 32 * SideToMove - 16));
                    UInt64 attackBitboard = Attack.Pawn(from, SideToMove);
                    moveBitboard |= opponentBitboard & attackBitboard;
                    while (moveBitboard > 0) {
                        to = Bit.Pop(ref moveBitboard);
                        if ((to - 8) * (to - 55) > 0) {
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Queen);
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Knight);
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Rook);
                            moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Bishop);
                        } else
                            moves[index++] = Move.Create(this, from, to);
                    }

                    // En passant is always fully tested for legality. 
                    if ((enPassantBitboard & attackBitboard) > 0) {
                        Bitboard[(1 - SideToMove) | Piece.Pawn] ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= (1UL << from) | enPassantBitboard;
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, EnPassantSquare, (1 - SideToMove) | Piece.Pawn);
                        Bitboard[(1 - SideToMove) | Piece.Pawn] ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= (1UL << from) | enPassantBitboard;
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Knight];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Knight(from);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Bishop];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Bishop(from, OccupiedBitboard);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Queen];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Queen(from, OccupiedBitboard);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Rook];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Rook(from, OccupiedBitboard);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }
            }

            // Case 2. There are pinned pieces or a single check, so all moves are tested for legality. 
            else if ((checkBitboard & (checkBitboard - 1)) <= 0) {
                UInt64 pieceBitboard = Bitboard[SideToMove | Piece.Pawn];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    Int32 to = from + 16 * SideToMove - 8;
                    UInt64 moveBitboard = ~OccupiedBitboard & (1UL << to);
                    if (moveBitboard > 0 && (from - 16) * (from - 47) > 0 && (to - 8) * (to - 55) < 0)
                        moveBitboard |= ~OccupiedBitboard & (1UL << (from + 32 * SideToMove - 16));
                    UInt64 attackBitboard = Attack.Pawn(from, SideToMove);
                    moveBitboard |= opponentBitboard & attackBitboard;
                    while (moveBitboard > 0) {
                        to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;
                        if (!IsAttacked(SideToMove, kingSquare))
                            if ((to - 8) * (to - 55) > 0) {
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Queen);
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Knight);
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Rook);
                                moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Bishop);
                            } else
                                moves[index++] = Move.Create(this, from, to);
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }

                    // Generate en passant moves. 
                    if ((enPassantBitboard & attackBitboard) > 0) {
                        Bitboard[(1 - SideToMove) | Piece.Pawn] ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= (1UL << from) | enPassantBitboard;
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, EnPassantSquare, (1 - SideToMove) | Piece.Pawn);
                        Bitboard[(1 - SideToMove) | Piece.Pawn] ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= enPassantPawnBitboard;
                        OccupiedBitboard ^= (1UL << from) | enPassantBitboard;
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Knight];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Knight(from);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Bishop];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Bishop(from, OccupiedBitboard);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Queen];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Queen(from, OccupiedBitboard);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }

                pieceBitboard = Bitboard[SideToMove | Piece.Rook];
                while (pieceBitboard > 0) {
                    Int32 from = Bit.Pop(ref pieceBitboard);
                    UInt64 moveBitboard = targetBitboard & Attack.Rook(from, OccupiedBitboard);
                    while (moveBitboard > 0) {
                        Int32 to = Bit.Pop(ref moveBitboard);
                        UInt64 occupiedBitboardCopy = OccupiedBitboard;
                        Int32 capture = Square[to];
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard ^= 1UL << from;
                        OccupiedBitboard |= 1UL << to;
                        if (!IsAttacked(SideToMove, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        Bitboard[capture] ^= 1UL << to;
                        OccupiedBitboard = occupiedBitboardCopy;
                    }
                }
            }

            // King moves are always tested for legality. 
            {
                Int32 from = kingSquare;
                UInt64 moveBitboard = targetBitboard & Attack.King(from);
                while (moveBitboard > 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    UInt64 occupiedBitboardCopy = OccupiedBitboard;
                    Int32 capture = Square[to];
                    Bitboard[capture] ^= 1UL << to;
                    OccupiedBitboard ^= 1UL << from;
                    OccupiedBitboard |= 1UL << to;
                    if (!IsAttacked(SideToMove, to))
                        moves[index++] = Move.Create(this, from, to);
                    Bitboard[capture] ^= 1UL << to;
                    OccupiedBitboard = occupiedBitboardCopy;
                }
            }
            return index;
        }

        public Int32 PseudoQuiescenceMoves(Int32[] moves) {
            Int32 index = 0;
            UInt64 targetBitboard = Bitboard[(1 - SideToMove) | Piece.All];

            UInt64 pieceBitboard = Bitboard[SideToMove | Piece.King];
            Int32 from = Bit.Read(pieceBitboard);
            UInt64 moveBitboard = targetBitboard & Attack.King(from);
            while (moveBitboard > 0) {
                Int32 to = Bit.Pop(ref moveBitboard);
                moves[index++] = Move.Create(this, from, to);
            }

            pieceBitboard = Bitboard[SideToMove | Piece.Queen];
            while (pieceBitboard > 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Queen(from, OccupiedBitboard);
                while (moveBitboard > 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceBitboard = Bitboard[SideToMove | Piece.Rook];
            while (pieceBitboard > 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Rook(from, OccupiedBitboard);
                while (moveBitboard > 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceBitboard = Bitboard[SideToMove | Piece.Knight];
            while (pieceBitboard > 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Knight(from);
                while (moveBitboard > 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceBitboard = Bitboard[SideToMove | Piece.Bishop];
            while (pieceBitboard > 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Bishop(from, OccupiedBitboard);
                while (moveBitboard > 0) {
                    Int32 to = Bit.Pop(ref moveBitboard);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceBitboard = Bitboard[SideToMove | Piece.Pawn];
            while (pieceBitboard > 0) {
                from = Bit.Pop(ref pieceBitboard);
                moveBitboard = targetBitboard & Attack.Pawn(from, SideToMove);
                Int32 to = from + 16 * SideToMove - 8;
                Boolean promotion = (to - 8) * (to - 55) > 0;
                if (promotion)
                    moveBitboard |= ~OccupiedBitboard & (1UL << to);
                while (moveBitboard > 0) {
                    to = Bit.Pop(ref moveBitboard);
                    if (promotion)
                        moves[index++] = Move.Create(this, from, to, SideToMove | Piece.Queen);
                    else
                        moves[index++] = Move.Create(this, from, to);
                }
            }
            return index;
        }

        public List<Int32> LegalMoves() {
            Int32[] moves = new Int32[256];
            Int32 movesCount = LegalMoves(moves);
            List<Int32> list = new List<Int32>(45);
            for (Int32 i = 0; i < movesCount; i++)
                list.Add(moves[i]);
            return list;
        }

        public void Make(Int32 move) {
            Int32 from = Move.GetFrom(move);
            Int32 to = Move.GetTo(move);
            Int32 piece = Move.GetPiece(move);
            Int32 capture = Move.GetCapture(move);
            Int32 special = Move.GetSpecial(move);

            Square[to] = piece;
            Square[from] = Piece.Empty;
            Bitboard[piece] ^= (1UL << from) | (1UL << to);
            Bitboard[SideToMove | Piece.All] ^= (1UL << from) | (1UL << to);
            OccupiedBitboard ^= (1UL << from) | (1UL << to);
            ZobristKey ^= Zobrist.PiecePosition[piece][from] ^ Zobrist.PiecePosition[piece][to];
            ZobristKey ^= Zobrist.Colour;

            if (EnPassantSquare != InvalidSquare) {
                ZobristKey ^= Zobrist.EnPassant[EnPassantSquare];
                EnPassantSquare = InvalidSquare;
            }
            FiftyMovesClock++;
            HalfMoves++;
            switch (capture & Piece.Type) {
                case Piece.Empty:
                    break;
                case Piece.Rook:
                    if ((SideToMove == Piece.White && to == 0) || (SideToMove == Piece.Black && to == 56)) {
                        if (CastleQueenside[1 - SideToMove]-- > 0)
                            ZobristKey ^= Zobrist.CastleQueenside[1 - SideToMove];
                    } else if ((SideToMove == Piece.White && to == 7) || (SideToMove == Piece.Black && to == 63))
                        if (CastleKingside[1 - SideToMove]-- > 0)
                            ZobristKey ^= Zobrist.CastleKingside[1 - SideToMove];
                    goto default;
                default:
                    Bitboard[capture] ^= 1UL << to;
                    Bitboard[(1 - SideToMove) | Piece.All] ^= 1UL << to;
                    OccupiedBitboard |= 1UL << to;
                    ZobristKey ^= Zobrist.PiecePosition[capture][to];
                    Material[1 - SideToMove] -= Zero.PieceValue[capture];
                    FiftyMovesClock = 0;
                    break;
            }
            switch (special & Piece.Type) {
                case Piece.Empty:
                    switch (piece & Piece.Type) {
                        case Piece.Pawn:
                            FiftyMovesClock = 0;
                            if ((from - to) * (from - to) == 256) {
                                ZobristKey ^= Zobrist.EnPassant[from];
                                EnPassantHistory[HalfMoves] = EnPassantSquare = (from + to) / 2;
                            }
                            break;
                        case Piece.Rook:
                            if ((SideToMove == Piece.White && from == 56) || (SideToMove == Piece.Black && from == 0)) {
                                if (CastleQueenside[SideToMove]-- > 0)
                                    ZobristKey ^= Zobrist.CastleQueenside[SideToMove];
                            } else if ((SideToMove == Piece.White && from == 63) || (SideToMove == Piece.Black && from == 7))
                                if (CastleKingside[SideToMove]-- > 0)
                                    ZobristKey ^= Zobrist.CastleKingside[SideToMove];
                            break;
                        case Piece.King:
                            if (CastleQueenside[SideToMove]-- > 0)
                                ZobristKey ^= Zobrist.CastleQueenside[SideToMove];
                            if (CastleKingside[SideToMove]-- > 0)
                                ZobristKey ^= Zobrist.CastleKingside[SideToMove];
                            break;
                    }
                    break;
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
                    Bitboard[SideToMove | Piece.All] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedBitboard ^= (1UL << rookFrom) | (1UL << rookTo);
                    ZobristKey ^= Zobrist.PiecePosition[SideToMove | Piece.Rook][rookFrom];
                    ZobristKey ^= Zobrist.PiecePosition[SideToMove | Piece.Rook][rookTo];
                    Square[rookFrom] = Piece.Empty;
                    Square[rookTo] = SideToMove | Piece.Rook;
                    break;
                case Piece.Pawn:
                    Square[File(to) + Rank(from) * 8] = Piece.Empty;
                    Bitboard[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    Bitboard[(1 - SideToMove) | Piece.All] ^= 1UL << (File(to) + Rank(from) * 8);
                    OccupiedBitboard ^= 1UL << (File(to) + Rank(from) * 8);
                    ZobristKey ^= Zobrist.PiecePosition[special][File(to) + Rank(from) * 8];
                    Material[1 - SideToMove] -= Zero.PieceValue[special];
                    break;
                default:
                    Bitboard[piece] ^= 1UL << to;
                    Bitboard[special] ^= 1UL << to;
                    ZobristKey ^= Zobrist.PiecePosition[piece][to];
                    ZobristKey ^= Zobrist.PiecePosition[special][to];
                    Material[SideToMove] += Zero.PieceValue[special] - Zero.PieceValue[piece];
                    Square[to] = special;
                    break;
            }
            SideToMove = 1 - SideToMove;
            FiftyMovesHistory[HalfMoves] = FiftyMovesClock;
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        public void Unmake(Int32 move) {
            Int32 from = Move.GetFrom(move);
            Int32 to = Move.GetTo(move);
            Int32 piece = Move.GetPiece(move);
            Int32 capture = Move.GetCapture(move);
            Int32 special = Move.GetSpecial(move);

            SideToMove = 1 - SideToMove;
            Square[from] = piece;
            Square[to] = capture;
            Bitboard[piece] ^= (1UL << from) | (1UL << to);
            Bitboard[SideToMove | Piece.All] ^= (1UL << from) | (1UL << to);
            OccupiedBitboard ^= (1UL << from) | (1UL << to);
            ZobristKey = ZobristKeyHistory[HalfMoves - 1];

            EnPassantHistory[HalfMoves] = InvalidSquare;
            EnPassantSquare = EnPassantHistory[HalfMoves - 1];
            FiftyMovesClock = FiftyMovesHistory[HalfMoves - 1];
            HalfMoves--;
            switch (capture & Piece.Type) {
                case Piece.Empty:
                    break;
                case Piece.Rook:
                    if ((SideToMove == Piece.White && to == 0) || (SideToMove == Piece.Black && to == 56)) {
                        CastleQueenside[1 - SideToMove]++;
                    } else if ((SideToMove == Piece.White && to == 7) || (SideToMove == Piece.Black && to == 63))
                        CastleKingside[1 - SideToMove]++;
                    goto default;
                default:
                    Bitboard[capture] ^= 1UL << to;
                    Bitboard[(1 - SideToMove) | Piece.All] ^= 1UL << to;
                    OccupiedBitboard |= 1UL << to;
                    Material[1 - SideToMove] += Zero.PieceValue[capture];
                    break;
            }
            switch (special & Piece.Type) {
                case Piece.Empty:
                    switch (piece & Piece.Type) {
                        case Piece.Rook:
                            if ((SideToMove == Piece.White && from == 56) || (SideToMove == Piece.Black && from == 0)) {
                                CastleQueenside[SideToMove]++;
                            } else if ((SideToMove == Piece.White && from == 63) || (SideToMove == Piece.Black && from == 7))
                                CastleKingside[SideToMove]++;
                            break;
                        case Piece.King:
                            CastleQueenside[SideToMove]++;
                            CastleKingside[SideToMove]++;
                            break;
                    }
                    break;
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
                    Bitboard[SideToMove | Piece.All] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedBitboard ^= (1UL << rookFrom) | (1UL << rookTo);
                    Square[rookFrom] = SideToMove | Piece.Rook;
                    Square[rookTo] = Piece.Empty;
                    break;
                case Piece.Pawn:
                    Square[File(to) + Rank(from) * 8] = special;
                    Bitboard[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    Bitboard[(1 - SideToMove) | Piece.All] ^= 1UL << (File(to) + Rank(from) * 8);
                    OccupiedBitboard ^= 1UL << (File(to) + Rank(from) * 8);
                    Material[1 - SideToMove] += Zero.PieceValue[special];
                    break;
                default:
                    Bitboard[piece] ^= 1UL << to;
                    Bitboard[special] ^= 1UL << to;
                    Material[SideToMove] -= Zero.PieceValue[special] - Zero.PieceValue[piece];
                    break;
            }
        }

        public void MakeNull() {
            ZobristKey ^= Zobrist.Colour;
            if (EnPassantSquare != InvalidSquare) {
                ZobristKey ^= Zobrist.EnPassant[EnPassantSquare];
                EnPassantSquare = InvalidSquare;
            }
            SideToMove = 1 - SideToMove;
            FiftyMovesClock++;
            HalfMoves++;
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        public void UnmakeNull() {
            ZobristKey = ZobristKeyHistory[HalfMoves - 1];
            EnPassantSquare = EnPassantHistory[HalfMoves - 1];
            SideToMove = 1 - SideToMove;
            FiftyMovesClock--;
            HalfMoves--;
        }

        public UInt64 GenerateZobristKey() {
            UInt64 key = 0;
            for (Int32 square = 0; square < Square.Length; square++)
                if (Square[square] != Piece.Empty)
                    key ^= Zobrist.PiecePosition[Square[square]][square];
            if (EnPassantSquare != InvalidSquare)
                key ^= Zobrist.EnPassant[EnPassantSquare];
            if (SideToMove != Piece.White)
                key ^= Zobrist.Colour;
            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++) {
                if (CastleQueenside[colour] > 0)
                    key ^= Zobrist.CastleQueenside[colour];
                if (CastleKingside[colour] > 0)
                    key ^= Zobrist.CastleKingside[colour];
            }
            return key;
        }

        public Boolean InCheck(Int32 colour) {
            return IsAttacked(colour, Bit.Read(Bitboard[colour | Piece.King]));
        }

        public Boolean IsAttacked(Int32 colour, Int32 square) {
            if ((Bitboard[(1 - colour) | Piece.Knight] & Attack.Knight(square)) > 0)
                return true;
            if ((Bitboard[(1 - colour) | Piece.Pawn] & Attack.Pawn(square, colour)) > 0)
                return true;
            if ((Bitboard[(1 - colour) | Piece.King] & Attack.King(square)) > 0)
                return true;
            UInt64 bishopQueenBitboard = Bitboard[(1 - colour) | Piece.Bishop] | Bitboard[(1 - colour) | Piece.Queen];
            if ((bishopQueenBitboard & Attack.Diagonals[square]) > 0)
                if ((bishopQueenBitboard & Attack.Bishop(square, OccupiedBitboard)) > 0)
                    return true;
            UInt64 rookQueenBitboard = Bitboard[(1 - colour) | Piece.Rook] | Bitboard[(1 - colour) | Piece.Queen];
            if ((rookQueenBitboard & Attack.Axes[square]) > 0)
                if ((rookQueenBitboard & Attack.Rook(square, OccupiedBitboard)) > 0)
                    return true;
            return false;
        }

        public Boolean CausesCheck(Int32 move) {
            UInt64 fromBitboard = 1UL << Move.GetFrom(move);
            UInt64 toBitboard = 1UL << Move.GetTo(move);
            Int32 piece = Move.GetPiece(move);
            Int32 special = Move.GetSpecial(move);
            UInt64 occupiedBitboardCopy = OccupiedBitboard;

            Boolean value = false;
            switch (special & Piece.Type) {
                case Piece.Empty:
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard ^= fromBitboard;
                    OccupiedBitboard |= toBitboard;
                    value = InCheck(1 - SideToMove);
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard = occupiedBitboardCopy;
                    break;
                case Piece.King:
                    Int32 rookToBitboard = (toBitboard < fromBitboard ? 3 : 5) + Rank(Move.GetTo(move)) * 8;
                    Bitboard[SideToMove | Piece.Rook] ^= 1UL << rookToBitboard;
                    value = InCheck(1 - SideToMove);
                    Bitboard[SideToMove | Piece.Rook] ^= 1UL << rookToBitboard;
                    break;
                case Piece.Pawn:
                    UInt64 enPassantPawnBitboard = Move.Pawn(EnPassantSquare, 1 - SideToMove);
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard ^= fromBitboard | toBitboard | enPassantPawnBitboard;
                    value = InCheck(1 - SideToMove);
                    Bitboard[piece] ^= fromBitboard | toBitboard;
                    OccupiedBitboard = occupiedBitboardCopy;
                    break;
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

        public Boolean InsufficientMaterial() {
            Int32 pieces = Bit.Count(OccupiedBitboard);
            if (pieces > 4)
                return false;
            if (pieces <= 2)
                return true;
            if (pieces <= 3)
                for (Int32 colour = Piece.White; colour <= Piece.Black; colour++)
                    if ((Bitboard[colour | Piece.Knight] | Bitboard[colour | Piece.Bishop]) > 0)
                        return true;
            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++)
                if (Bit.CountSparse(Bitboard[colour | Piece.Knight]) >= 2)
                    return true;
            if (Bitboard[Piece.White | Piece.Bishop] > 0 && Bitboard[Piece.Black | Piece.Bishop] > 0)
                return (Bitboard[Piece.White | Piece.Bishop] & Bit.LightSquares) > 0 == (Bitboard[Piece.Black | Piece.Bishop] & Bit.LightSquares) > 0;
            return false;
        }

        public Boolean HasRepeated(Int32 times) {
            Int32 repetitions = 1;
            for (Int32 i = HalfMoves - 4; i >= HalfMoves - FiftyMovesClock; i -= 2)
                if (ZobristKeyHistory[i] == ZobristKey)
                    if (++repetitions >= times)
                        return true;
            return false;
        }

        public Boolean Equals(Position other) {
            if (ZobristKey != other.ZobristKey)
                return false;
            if (HalfMoves != other.HalfMoves)
                return false;
            if (FiftyMovesClock != other.FiftyMovesClock)
                return false;
            if (Material != other.Material)
                return false;
            if (SideToMove != other.SideToMove)
                return false;
            if (EnPassantSquare != other.EnPassantSquare)
                return false;
            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++) {
                if (CastleKingside[colour] != other.CastleKingside[colour])
                    return false;
                if (CastleQueenside[colour] != other.CastleQueenside[colour])
                    return false;
            }
            for (Int32 piece = 0; piece < Bitboard.Length; piece++)
                if (Bitboard[piece] != other.Bitboard[piece])
                    return false;
            for (Int32 square = 0; square < Square.Length; square++)
                if (Square[square] != other.Square[square])
                    return false;
            return true;
        }

        public Position Clone() {
            return new Position() {
                Square = this.Square.Clone() as Int32[],
                Bitboard = this.Bitboard.Clone() as UInt64[],
                OccupiedBitboard = this.OccupiedBitboard,
                CastleKingside = this.CastleKingside.Clone() as Int32[],
                CastleQueenside = this.CastleQueenside.Clone() as Int32[],
                EnPassantHistory = this.EnPassantHistory.Clone() as Int32[],
                EnPassantSquare = this.EnPassantSquare,
                Material = this.Material,
                SideToMove = this.SideToMove,
                HalfMoves = this.HalfMoves,
                FiftyMovesHistory = this.FiftyMovesHistory.Clone() as Int32[],
                FiftyMovesClock = this.FiftyMovesClock,
                ZobristKey = this.ZobristKey,
                ZobristKeyHistory = this.ZobristKeyHistory.Clone() as UInt64[]
            };
        }

        public String GetFEN() {
            StringBuilder fen = new StringBuilder();
            for (Int32 rank = 0; rank < 8; rank++) {
                Int32 space = 0;
                for (Int32 file = 0; file < 8; file++) {
                    Int32 square = file + rank * 8;
                    if (Square[square] == Piece.Empty)
                        space++;
                    else {
                        if (space > 0) {
                            fen.Append(space);
                            space = 0;
                        }
                        String piece = Identify.PieceInitial(Square[square]);
                        if ((Square[square] & Piece.Colour) == Piece.Black)
                            piece = piece.ToLowerInvariant();
                        fen.Append(piece);
                    }
                }
                if (space > 0)
                    fen.Append(space);
                if (rank < 7)
                    fen.Append('/');
            }
            fen.Append(' ');
            fen.Append(SideToMove == Piece.White ? 'w' : 'b');
            fen.Append(' ');
            if (CastleKingside[Piece.White] > 0)
                fen.Append('K');
            if (CastleQueenside[Piece.White] > 0)
                fen.Append('Q');
            if (CastleKingside[Piece.Black] > 0)
                fen.Append('k');
            if (CastleQueenside[Piece.Black] > 0)
                fen.Append('q');
            if (fen[fen.Length - 1] == ' ')
                fen.Append('-');
            fen.Append(' ');
            if (EnPassantSquare != InvalidSquare)
                fen.Append(Identify.Square(EnPassantSquare));
            else
                fen.Append('-');
            fen.Append(' ');
            fen.Append(FiftyMovesClock);
            fen.Append(' ');
            fen.Append(HalfMoves / 2 + 1);
            return fen.ToString();
        }

        public override String ToString() {
            return ToStringAppend();
        }

        public String ToStringAppend(params String[] comments) {
            StringBuilder result = new StringBuilder("   +------------------------+ ", 400);
            Int32 index = 0;
            if (index < comments.Length)
                result.Append(comments[index++]);

            for (Int32 rank = 0; rank < 8; rank++) {
                result.Append(Environment.NewLine);
                result.Append(' ');
                result.Append(8 - rank);
                result.Append(" |");
                for (Int32 file = 0; file < 8; file++) {
                    Int32 piece = Square[file + rank * 8];
                    if (piece != Piece.Empty) {
                        result.Append((piece & Piece.Colour) == Piece.White ? '<' : '[');
                        result.Append(Identify.PieceInitial(piece));
                        result.Append((piece & Piece.Colour) == Piece.White ? '>' : ']');
                    } else
                        result.Append((file + rank) % 2 > 0 ? ":::" : "   ");
                }
                result.Append("| ");
                if (index < comments.Length)
                    result.Append(comments[index++]);
            }
            result.Append(Environment.NewLine);
            result.Append("   +------------------------+ ");
            if (index < comments.Length)
                result.Append(comments[index++]);

            result.Append(Environment.NewLine);
            result.Append("     a  b  c  d  e  f  g  h   ");
            if (index < comments.Length)
                result.Append(comments[index++]);
            return result.ToString();
        }

        public static Int32 File(Int32 square) {
            return square & 7;
        }

        public static Int32 Rank(Int32 square) {
            return square >> 3;
        }

        public static Int32 SquareAt(Point e) {
            Int32 file = e.X / VisualPosition.SquareWidth;
            Int32 rank = (e.Y - Window.MenuHeight) / VisualPosition.SquareWidth;
            if (VisualPosition.Rotated)
                return 7 - file + (7 - rank) * 8;
            return file + rank * 8;
        }

        public static Int32 SquareAt(String name) {
            return (Int32)(name[0] - 97 + (56 - name[1]) * 8);
        }
    }
}
