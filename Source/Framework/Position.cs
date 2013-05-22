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

        public Int32[] Element = new Int32[64];
        public UInt64[] BitField = new UInt64[16];
        public UInt64 OccupiedField = 0;
        public Int32[] Material = new Int32[2];
        public Int32 Colour = 0;
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
            Array.Clear(Element, 0, Element.Length);
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
                        Element[file++ + rank * 8] = colour | Piece.King;
                        break;
                    case 'Q':
                        Element[file++ + rank * 8] = colour | Piece.Queen;
                        break;
                    case 'R':
                        Element[file++ + rank * 8] = colour | Piece.Rook;
                        break;
                    case 'B':
                        Element[file++ + rank * 8] = colour | Piece.Bishop;
                        break;
                    case 'N':
                        Element[file++ + rank * 8] = colour | Piece.Knight;
                        break;
                    case 'P':
                        Element[file++ + rank * 8] = colour | Piece.Pawn;
                        break;
                }
            }
            Colour = terms[1] == "w" ? Piece.White : Piece.Black;
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
                    EnPassantSquare = Square(terms[3]);
            if (terms.Length > 5) {
                FiftyMovesClock = Int32.Parse(terms[4]);
                Int32 moveNumber = Int32.Parse(terms[5]);
                HalfMoves = 2 * (moveNumber - 1) + Colour;
                HalfMoves = Math.Max(0, HalfMoves);
                FiftyMovesClock = Math.Min(FiftyMovesClock, HalfMoves);
            }

            for (Int32 i = 0; i < EnPassantHistory.Length; i++)
                EnPassantHistory[i] = InvalidSquare;
            EnPassantHistory[HalfMoves] = EnPassantSquare;
            FiftyMovesHistory[HalfMoves] = FiftyMovesClock;
            for (Int32 square = 0; square < Element.Length; square++)
                if (Element[square] != Piece.Empty) {
                    Int32 colour = Element[square] & Piece.Colour;
                    BitField[Element[square]] |= 1UL << square;
                    BitField[colour | Piece.All] |= 1UL << square;
                    OccupiedField |= 1UL << square;
                    if ((Element[square] & Piece.Type) != Piece.King)
                        Material[colour] += Zero.PieceValue[Element[square]];
                }
            ZobristKey = GenerateZobristKey();
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        public Int32 LegalMoves(Int32[] moves) {
            UInt64 bishopQueenField = BitField[(1 - Colour) | Piece.Bishop] | BitField[(1 - Colour) | Piece.Queen];
            UInt64 rookQueenField = BitField[(1 - Colour) | Piece.Rook] | BitField[(1 - Colour) | Piece.Queen];
            UInt64 checkField = 0;
            UInt64 pinField = 0;
            Int32 kingSquare = Bit.Read(BitField[Colour | Piece.King]);

            checkField |= BitField[(1 - Colour) | Piece.Knight] & Attack.Knight(kingSquare);
            checkField |= BitField[(1 - Colour) | Piece.Pawn] & Attack.Pawn(kingSquare, Colour);
            if ((bishopQueenField & Attack.Diagonals[kingSquare]) > 0) {
                checkField |= bishopQueenField & Attack.Bishop(kingSquare, OccupiedField);

                UInt64 occupiedFieldCopy = OccupiedField;
                UInt64 pinnedField = Attack.RayNE[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.ScanReverse(pinnedField);
                pinnedField = Attack.RayNW[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.ScanReverse(pinnedField);
                pinnedField = Attack.RaySE[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.Scan(pinnedField);
                pinnedField = Attack.RaySW[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.Scan(pinnedField);

                pinField |= bishopQueenField & Attack.Bishop(kingSquare, OccupiedField);
                OccupiedField = occupiedFieldCopy;
            }
            if ((rookQueenField & Attack.Axes[kingSquare]) > 0) {
                checkField |= rookQueenField & Attack.Rook(kingSquare, OccupiedField);

                UInt64 occupiedFieldCopy = OccupiedField;
                UInt64 pinnedField = Attack.RayN[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.ScanReverse(pinnedField);
                pinnedField = Attack.RayE[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.Scan(pinnedField);
                pinnedField = Attack.RayS[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.Scan(pinnedField);
                pinnedField = Attack.RayW[kingSquare] & BitField[Colour | Piece.All];
                if (pinnedField > 0)
                    OccupiedField ^= 1UL << Bit.ScanReverse(pinnedField);

                pinField |= rookQueenField & Attack.Rook(kingSquare, OccupiedField);
                OccupiedField = occupiedFieldCopy;
            }

            Int32 index = 0;

            // castling is always fully checked
            if (checkField <= 0) {
                Int32 rank = -56 * Colour + 56;
                if (CastleQueenside[Colour] > 0 && (Element[1 + rank] | Element[2 + rank] | Element[3 + rank]) == Piece.Empty)
                    if (!IsAttacked(Colour, 3 + rank) && !IsAttacked(Colour, 2 + rank))
                        moves[index++] = Move.Create(this, kingSquare, 2 + rank, Colour | Piece.King);
                if (CastleKingside[Colour] > 0 && (Element[5 + rank] | Element[6 + rank]) == Piece.Empty)
                    if (!IsAttacked(Colour, 5 + rank) && !IsAttacked(Colour, 6 + rank))
                        moves[index++] = Move.Create(this, kingSquare, 6 + rank, Colour | Piece.King);
            }

            UInt64 opponentField = BitField[(1 - Colour) | Piece.All];
            UInt64 targetField = ~BitField[Colour | Piece.All];
            UInt64 enPassantField = 0;
            UInt64 enPassantPawnField = 0;
            if (EnPassantSquare != InvalidSquare) {
                enPassantField = 1UL << EnPassantSquare;
                enPassantPawnField = Move.Pawn(EnPassantSquare, 1 - Colour);
            }

            // not in check and no pinned pieces; almost all moves are legal
            if (checkField <= 0 & pinField <= 0) {
                UInt64 pieceField = BitField[Colour | Piece.Pawn];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    Int32 to = from + 16 * Colour - 8;
                    UInt64 moveField = ~OccupiedField & (1UL << to);
                    if (moveField > 0 && (from - 16) * (from - 47) > 0 && (to - 8) * (to - 55) < 0)
                        moveField |= ~OccupiedField & (1UL << (from + 32 * Colour - 16));
                    UInt64 attackField = Attack.Pawn(from, Colour);
                    moveField |= opponentField & attackField;
                    while (moveField > 0) {
                        to = Bit.Pop(ref moveField);
                        if ((to - 8) * (to - 55) > 0) {
                            moves[index++] = Move.Create(this, from, to, Colour | Piece.Queen);
                            moves[index++] = Move.Create(this, from, to, Colour | Piece.Knight);
                            moves[index++] = Move.Create(this, from, to, Colour | Piece.Rook);
                            moves[index++] = Move.Create(this, from, to, Colour | Piece.Bishop);
                        } else
                            moves[index++] = Move.Create(this, from, to);
                    }

                    // en passant is always fully checked
                    if ((enPassantField & attackField) > 0) {
                        BitField[(1 - Colour) | Piece.Pawn] ^= enPassantPawnField;
                        OccupiedField ^= enPassantPawnField;
                        OccupiedField ^= (1UL << from) | enPassantField;
                        if (!IsAttacked(Colour, kingSquare))
                            moves[index++] = Move.Create(this, from, EnPassantSquare, (1 - Colour) | Piece.Pawn);
                        BitField[(1 - Colour) | Piece.Pawn] ^= enPassantPawnField;
                        OccupiedField ^= enPassantPawnField;
                        OccupiedField ^= (1UL << from) | enPassantField;
                    }
                }

                pieceField = BitField[Colour | Piece.Knight];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Knight(from);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                pieceField = BitField[Colour | Piece.Bishop];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Bishop(from, OccupiedField);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                pieceField = BitField[Colour | Piece.Queen];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Queen(from, OccupiedField);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }

                pieceField = BitField[Colour | Piece.Rook];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Rook(from, OccupiedField);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        moves[index++] = Move.Create(this, from, to);
                    }
                }
            }

            // single check or pieces are pinned; all moves are fully checked
            else if ((checkField & (checkField - 1)) <= 0) {
                UInt64 pieceField = BitField[Colour | Piece.Pawn];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    Int32 to = from + 16 * Colour - 8;
                    UInt64 moveField = ~OccupiedField & (1UL << to);
                    if (moveField > 0 && (from - 16) * (from - 47) > 0 && (to - 8) * (to - 55) < 0)
                        moveField |= ~OccupiedField & (1UL << (from + 32 * Colour - 16));
                    UInt64 attackField = Attack.Pawn(from, Colour);
                    moveField |= opponentField & attackField;
                    while (moveField > 0) {
                        to = Bit.Pop(ref moveField);
                        UInt64 occupiedFieldCopy = OccupiedField;
                        Int32 capture = Element[to];
                        BitField[capture] ^= 1UL << to;
                        OccupiedField ^= 1UL << from;
                        OccupiedField |= 1UL << to;
                        if (!IsAttacked(Colour, kingSquare))
                            if ((to - 8) * (to - 55) > 0) {
                                moves[index++] = Move.Create(this, from, to, Colour | Piece.Queen);
                                moves[index++] = Move.Create(this, from, to, Colour | Piece.Knight);
                                moves[index++] = Move.Create(this, from, to, Colour | Piece.Rook);
                                moves[index++] = Move.Create(this, from, to, Colour | Piece.Bishop);
                            } else
                                moves[index++] = Move.Create(this, from, to);
                        BitField[capture] ^= 1UL << to;
                        OccupiedField = occupiedFieldCopy;
                    }
                    if ((enPassantField & attackField) > 0) {
                        BitField[(1 - Colour) | Piece.Pawn] ^= enPassantPawnField;
                        OccupiedField ^= enPassantPawnField;
                        OccupiedField ^= (1UL << from) | enPassantField;
                        if (!IsAttacked(Colour, kingSquare))
                            moves[index++] = Move.Create(this, from, EnPassantSquare, (1 - Colour) | Piece.Pawn);
                        BitField[(1 - Colour) | Piece.Pawn] ^= enPassantPawnField;
                        OccupiedField ^= enPassantPawnField;
                        OccupiedField ^= (1UL << from) | enPassantField;
                    }
                }

                // en passant is always fully checked
                pieceField = BitField[Colour | Piece.Knight];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Knight(from);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        UInt64 occupiedFieldCopy = OccupiedField;
                        Int32 capture = Element[to];
                        BitField[capture] ^= 1UL << to;
                        OccupiedField ^= 1UL << from;
                        OccupiedField |= 1UL << to;
                        if (!IsAttacked(Colour, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        BitField[capture] ^= 1UL << to;
                        OccupiedField = occupiedFieldCopy;
                    }
                }

                pieceField = BitField[Colour | Piece.Bishop];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Bishop(from, OccupiedField);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        UInt64 occupiedFieldCopy = OccupiedField;
                        Int32 capture = Element[to];
                        BitField[capture] ^= 1UL << to;
                        OccupiedField ^= 1UL << from;
                        OccupiedField |= 1UL << to;
                        if (!IsAttacked(Colour, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        BitField[capture] ^= 1UL << to;
                        OccupiedField = occupiedFieldCopy;
                    }
                }

                pieceField = BitField[Colour | Piece.Queen];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Queen(from, OccupiedField);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        UInt64 occupiedFieldCopy = OccupiedField;
                        Int32 capture = Element[to];
                        BitField[capture] ^= 1UL << to;
                        OccupiedField ^= 1UL << from;
                        OccupiedField |= 1UL << to;
                        if (!IsAttacked(Colour, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        BitField[capture] ^= 1UL << to;
                        OccupiedField = occupiedFieldCopy;
                    }
                }

                pieceField = BitField[Colour | Piece.Rook];
                while (pieceField > 0) {
                    Int32 from = Bit.Pop(ref pieceField);
                    UInt64 moveField = targetField & Attack.Rook(from, OccupiedField);
                    while (moveField > 0) {
                        Int32 to = Bit.Pop(ref moveField);
                        UInt64 occupiedFieldCopy = OccupiedField;
                        Int32 capture = Element[to];
                        BitField[capture] ^= 1UL << to;
                        OccupiedField ^= 1UL << from;
                        OccupiedField |= 1UL << to;
                        if (!IsAttacked(Colour, kingSquare))
                            moves[index++] = Move.Create(this, from, to);
                        BitField[capture] ^= 1UL << to;
                        OccupiedField = occupiedFieldCopy;
                    }
                }
            }

            // king moves are always fully checked
            {
                Int32 from = kingSquare;
                UInt64 moveField = targetField & Attack.King(from);
                while (moveField > 0) {
                    Int32 to = Bit.Pop(ref moveField);
                    UInt64 occupiedFieldCopy = OccupiedField;
                    Int32 capture = Element[to];
                    BitField[capture] ^= 1UL << to;
                    OccupiedField ^= 1UL << from;
                    OccupiedField |= 1UL << to;
                    if (!IsAttacked(Colour, to))
                        moves[index++] = Move.Create(this, from, to);
                    BitField[capture] ^= 1UL << to;
                    OccupiedField = occupiedFieldCopy;
                }
            }
            return index;
        }

        public Int32 PseudoQuiescenceMoves(Int32[] moves) {
            Int32 index = 0;
            UInt64 targetField = BitField[(1 - Colour) | Piece.All];

            UInt64 pieceField = BitField[Colour | Piece.King];
            Int32 from = Bit.Read(pieceField);
            UInt64 moveField = targetField & Attack.King(from);
            while (moveField > 0) {
                Int32 to = Bit.Pop(ref moveField);
                moves[index++] = Move.Create(this, from, to);
            }

            pieceField = BitField[Colour | Piece.Queen];
            while (pieceField > 0) {
                from = Bit.Pop(ref pieceField);
                moveField = targetField & Attack.Queen(from, OccupiedField);
                while (moveField > 0) {
                    Int32 to = Bit.Pop(ref moveField);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceField = BitField[Colour | Piece.Rook];
            while (pieceField > 0) {
                from = Bit.Pop(ref pieceField);
                moveField = targetField & Attack.Rook(from, OccupiedField);
                while (moveField > 0) {
                    Int32 to = Bit.Pop(ref moveField);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceField = BitField[Colour | Piece.Knight];
            while (pieceField > 0) {
                from = Bit.Pop(ref pieceField);
                moveField = targetField & Attack.Knight(from);
                while (moveField > 0) {
                    Int32 to = Bit.Pop(ref moveField);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceField = BitField[Colour | Piece.Bishop];
            while (pieceField > 0) {
                from = Bit.Pop(ref pieceField);
                moveField = targetField & Attack.Bishop(from, OccupiedField);
                while (moveField > 0) {
                    Int32 to = Bit.Pop(ref moveField);
                    moves[index++] = Move.Create(this, from, to);
                }
            }

            pieceField = BitField[Colour | Piece.Pawn];
            while (pieceField > 0) {
                from = Bit.Pop(ref pieceField);
                moveField = targetField & Attack.Pawn(from, Colour);
                Int32 to = from + 16 * Colour - 8;
                Boolean promotion = (to - 8) * (to - 55) > 0;
                if (promotion)
                    moveField |= ~OccupiedField & (1UL << to);
                while (moveField > 0) {
                    to = Bit.Pop(ref moveField);
                    if (promotion)
                        moves[index++] = Move.Create(this, from, to, Colour | Piece.Queen);
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

            Element[to] = piece;
            Element[from] = Piece.Empty;
            BitField[piece] ^= (1UL << from) | (1UL << to);
            BitField[Colour | Piece.All] ^= (1UL << from) | (1UL << to);
            OccupiedField ^= (1UL << from) | (1UL << to);
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
                    if ((Colour == Piece.White && to == 0) || (Colour == Piece.Black && to == 56)) {
                        if (CastleQueenside[1 - Colour]-- > 0)
                            ZobristKey ^= Zobrist.CastleQueenside[1 - Colour];
                    } else if ((Colour == Piece.White && to == 7) || (Colour == Piece.Black && to == 63))
                        if (CastleKingside[1 - Colour]-- > 0)
                            ZobristKey ^= Zobrist.CastleKingside[1 - Colour];
                    goto default;
                default:
                    BitField[capture] ^= 1UL << to;
                    BitField[(1 - Colour) | Piece.All] ^= 1UL << to;
                    OccupiedField |= 1UL << to;
                    ZobristKey ^= Zobrist.PiecePosition[capture][to];
                    Material[1 - Colour] -= Zero.PieceValue[capture];
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
                            if ((Colour == Piece.White && from == 56) || (Colour == Piece.Black && from == 0)) {
                                if (CastleQueenside[Colour]-- > 0)
                                    ZobristKey ^= Zobrist.CastleQueenside[Colour];
                            } else if ((Colour == Piece.White && from == 63) || (Colour == Piece.Black && from == 7))
                                if (CastleKingside[Colour]-- > 0)
                                    ZobristKey ^= Zobrist.CastleKingside[Colour];
                            break;
                        case Piece.King:
                            if (CastleQueenside[Colour]-- > 0)
                                ZobristKey ^= Zobrist.CastleQueenside[Colour];
                            if (CastleKingside[Colour]-- > 0)
                                ZobristKey ^= Zobrist.CastleKingside[Colour];
                            break;
                    }
                    break;
                case Piece.King:
                    if (CastleQueenside[Colour]-- > 0)
                        ZobristKey ^= Zobrist.CastleQueenside[Colour];
                    if (CastleKingside[Colour]-- > 0)
                        ZobristKey ^= Zobrist.CastleKingside[Colour];
                    Int32 rookFrom;
                    Int32 rookTo;
                    if (to < from) {
                        rookFrom = Rank(to) * 8;
                        rookTo = 3 + Rank(to) * 8;
                    } else {
                        rookFrom = 7 + Rank(to) * 8;
                        rookTo = 5 + Rank(to) * 8;
                    }
                    BitField[Colour | Piece.Rook] ^= (1UL << rookFrom) | (1UL << rookTo);
                    BitField[Colour | Piece.All] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedField ^= (1UL << rookFrom) | (1UL << rookTo);
                    ZobristKey ^= Zobrist.PiecePosition[Colour | Piece.Rook][rookFrom];
                    ZobristKey ^= Zobrist.PiecePosition[Colour | Piece.Rook][rookTo];
                    Element[rookFrom] = Piece.Empty;
                    Element[rookTo] = Colour | Piece.Rook;
                    break;
                case Piece.Pawn:
                    Element[File(to) + Rank(from) * 8] = Piece.Empty;
                    BitField[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    BitField[(1 - Colour) | Piece.All] ^= 1UL << (File(to) + Rank(from) * 8);
                    OccupiedField ^= 1UL << (File(to) + Rank(from) * 8);
                    ZobristKey ^= Zobrist.PiecePosition[special][File(to) + Rank(from) * 8];
                    Material[1 - Colour] -= Zero.PieceValue[special];
                    break;
                default:
                    BitField[piece] ^= 1UL << to;
                    BitField[special] ^= 1UL << to;
                    ZobristKey ^= Zobrist.PiecePosition[piece][to];
                    ZobristKey ^= Zobrist.PiecePosition[special][to];
                    Material[Colour] += Zero.PieceValue[special] - Zero.PieceValue[piece];
                    Element[to] = special;
                    break;
            }
            Colour = 1 - Colour;
            FiftyMovesHistory[HalfMoves] = FiftyMovesClock;
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        public void Unmake(Int32 move) {
            Int32 from = Move.GetFrom(move);
            Int32 to = Move.GetTo(move);
            Int32 piece = Move.GetPiece(move);
            Int32 capture = Move.GetCapture(move);
            Int32 special = Move.GetSpecial(move);

            Colour = 1 - Colour;
            Element[from] = piece;
            Element[to] = capture;
            BitField[piece] ^= (1UL << from) | (1UL << to);
            BitField[Colour | Piece.All] ^= (1UL << from) | (1UL << to);
            OccupiedField ^= (1UL << from) | (1UL << to);
            ZobristKey = ZobristKeyHistory[HalfMoves - 1];

            EnPassantHistory[HalfMoves] = InvalidSquare;
            EnPassantSquare = EnPassantHistory[HalfMoves - 1];
            FiftyMovesClock = FiftyMovesHistory[HalfMoves - 1];
            HalfMoves--;
            switch (capture & Piece.Type) {
                case Piece.Empty:
                    break;
                case Piece.Rook:
                    if ((Colour == Piece.White && to == 0) || (Colour == Piece.Black && to == 56)) {
                        CastleQueenside[1 - Colour]++;
                    } else if ((Colour == Piece.White && to == 7) || (Colour == Piece.Black && to == 63))
                        CastleKingside[1 - Colour]++;
                    goto default;
                default:
                    BitField[capture] ^= 1UL << to;
                    BitField[(1 - Colour) | Piece.All] ^= 1UL << to;
                    OccupiedField |= 1UL << to;
                    Material[1 - Colour] += Zero.PieceValue[capture];
                    break;
            }
            switch (special & Piece.Type) {
                case Piece.Empty:
                    switch (piece & Piece.Type) {
                        case Piece.Rook:
                            if ((Colour == Piece.White && from == 56) || (Colour == Piece.Black && from == 0)) {
                                CastleQueenside[Colour]++;
                            } else if ((Colour == Piece.White && from == 63) || (Colour == Piece.Black && from == 7))
                                CastleKingside[Colour]++;
                            break;
                        case Piece.King:
                            CastleQueenside[Colour]++;
                            CastleKingside[Colour]++;
                            break;
                    }
                    break;
                case Piece.King:
                    CastleQueenside[Colour]++;
                    CastleKingside[Colour]++;
                    Int32 rookFrom;
                    Int32 rookTo;
                    if (to < from) {
                        rookFrom = Rank(to) * 8;
                        rookTo = 3 + Rank(to) * 8;
                    } else {
                        rookFrom = 7 + Rank(to) * 8;
                        rookTo = 5 + Rank(to) * 8;
                    }
                    BitField[Colour | Piece.Rook] ^= (1UL << rookFrom) | (1UL << rookTo);
                    BitField[Colour | Piece.All] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedField ^= (1UL << rookFrom) | (1UL << rookTo);
                    Element[rookFrom] = Colour | Piece.Rook;
                    Element[rookTo] = Piece.Empty;
                    break;
                case Piece.Pawn:
                    Element[File(to) + Rank(from) * 8] = special;
                    BitField[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    BitField[(1 - Colour) | Piece.All] ^= 1UL << (File(to) + Rank(from) * 8);
                    OccupiedField ^= 1UL << (File(to) + Rank(from) * 8);
                    Material[1 - Colour] += Zero.PieceValue[special];
                    break;
                default:
                    BitField[piece] ^= 1UL << to;
                    BitField[special] ^= 1UL << to;
                    Material[Colour] -= Zero.PieceValue[special] - Zero.PieceValue[piece];
                    break;
            }
        }

        public void MakeNull() {
            ZobristKey ^= Zobrist.Colour;
            if (EnPassantSquare != InvalidSquare) {
                ZobristKey ^= Zobrist.EnPassant[EnPassantSquare];
                EnPassantSquare = InvalidSquare;
            }
            Colour = 1 - Colour;
            FiftyMovesClock++;
            HalfMoves++;
            ZobristKeyHistory[HalfMoves] = ZobristKey;
        }

        public void UnmakeNull() {
            ZobristKey = ZobristKeyHistory[HalfMoves - 1];
            EnPassantSquare = EnPassantHistory[HalfMoves - 1];
            Colour = 1 - Colour;
            FiftyMovesClock--;
            HalfMoves--;
        }

        public UInt64 GenerateZobristKey() {
            UInt64 key = 0;
            for (Int32 square = 0; square < Element.Length; square++)
                if (Element[square] != Piece.Empty)
                    key ^= Zobrist.PiecePosition[Element[square]][square];
            if (EnPassantSquare != InvalidSquare)
                key ^= Zobrist.EnPassant[EnPassantSquare];
            if (Colour != Piece.White)
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
            return IsAttacked(colour, Bit.Read(BitField[colour | Piece.King]));
        }

        public Boolean IsAttacked(Int32 colour, Int32 square) {
            if ((BitField[(1 - colour) | Piece.Knight] & Attack.Knight(square)) > 0)
                return true;
            if ((BitField[(1 - colour) | Piece.Pawn] & Attack.Pawn(square, colour)) > 0)
                return true;
            if ((BitField[(1 - colour) | Piece.King] & Attack.King(square)) > 0)
                return true;
            UInt64 bishopQueenField = BitField[(1 - colour) | Piece.Bishop] | BitField[(1 - colour) | Piece.Queen];
            if ((bishopQueenField & Attack.Diagonals[square]) > 0)
                if ((bishopQueenField & Attack.Bishop(square, OccupiedField)) > 0)
                    return true;
            UInt64 rookQueenField = BitField[(1 - colour) | Piece.Rook] | BitField[(1 - colour) | Piece.Queen];
            if ((rookQueenField & Attack.Axes[square]) > 0)
                if ((rookQueenField & Attack.Rook(square, OccupiedField)) > 0)
                    return true;
            return false;
        }

        public Boolean CausesCheck(Int32 move) {
            UInt64 fromField = 1UL << Move.GetFrom(move);
            UInt64 toField = 1UL << Move.GetTo(move);
            Int32 piece = Move.GetPiece(move);
            Int32 special = Move.GetSpecial(move);
            UInt64 occupiedFieldCopy = OccupiedField;

            Boolean value = false;
            switch (special & Piece.Type) {
                case Piece.Empty:
                    BitField[piece] ^= fromField | toField;
                    OccupiedField ^= fromField;
                    OccupiedField |= toField;
                    value = InCheck(1 - Colour);
                    BitField[piece] ^= fromField | toField;
                    OccupiedField = occupiedFieldCopy;
                    break;
                case Piece.King:
                    Int32 rookToField = (toField < fromField ? 3 : 5) + Rank(Move.GetTo(move)) * 8;
                    BitField[Colour | Piece.Rook] ^= 1UL << rookToField;
                    value = InCheck(1 - Colour);
                    BitField[Colour | Piece.Rook] ^= 1UL << rookToField;
                    break;
                case Piece.Pawn:
                    UInt64 enPassantPawnField = Move.Pawn(EnPassantSquare, 1 - Colour);
                    BitField[piece] ^= fromField | toField;
                    OccupiedField ^= fromField | toField | enPassantPawnField;
                    value = InCheck(1 - Colour);
                    BitField[piece] ^= fromField | toField;
                    OccupiedField = occupiedFieldCopy;
                    break;
                default:
                    BitField[Colour | Piece.Pawn] ^= fromField;
                    BitField[special] ^= toField;
                    OccupiedField ^= fromField;
                    OccupiedField |= toField;
                    value = InCheck(1 - Colour);
                    BitField[Colour | Piece.Pawn] ^= fromField;
                    BitField[special] ^= toField;
                    OccupiedField = occupiedFieldCopy;
                    break;
            }
            return value;
        }

        public Boolean InsufficientMaterial() {
            Int32 pieces = Bit.Count(OccupiedField);
            if (pieces > 4)
                return false;
            if (pieces <= 2)
                return true;
            if (pieces <= 3)
                for (Int32 colour = Piece.White; colour <= Piece.Black; colour++)
                    if ((BitField[colour | Piece.Knight] | BitField[colour | Piece.Bishop]) > 0)
                        return true;
            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++)
                if (Bit.CountSparse(BitField[colour | Piece.Knight]) >= 2)
                    return true;
            if (BitField[Piece.White | Piece.Bishop] > 0 && BitField[Piece.Black | Piece.Bishop] > 0)
                return (BitField[Piece.White | Piece.Bishop] & Bit.LightSquares) > 0 == (BitField[Piece.Black | Piece.Bishop] & Bit.LightSquares) > 0;
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
            if (Colour != other.Colour)
                return false;
            if (EnPassantSquare != other.EnPassantSquare)
                return false;
            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++) {
                if (CastleKingside[colour] != other.CastleKingside[colour])
                    return false;
                if (CastleQueenside[colour] != other.CastleQueenside[colour])
                    return false;
            }
            for (Int32 piece = 0; piece < BitField.Length; piece++)
                if (BitField[piece] != other.BitField[piece])
                    return false;
            for (Int32 square = 0; square < Element.Length; square++)
                if (Element[square] != other.Element[square])
                    return false;
            return true;
        }

        public Position Clone() {
            return new Position() {
                Element = this.Element.Clone() as Int32[],
                BitField = this.BitField.Clone() as UInt64[],
                OccupiedField = this.OccupiedField,
                CastleKingside = this.CastleKingside.Clone() as Int32[],
                CastleQueenside = this.CastleQueenside.Clone() as Int32[],
                EnPassantHistory = this.EnPassantHistory.Clone() as Int32[],
                EnPassantSquare = this.EnPassantSquare,
                Material = this.Material,
                Colour = this.Colour,
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
                    if (Element[square] == Piece.Empty)
                        space++;
                    else {
                        if (space > 0) {
                            fen.Append(space);
                            space = 0;
                        }
                        String piece = Identify.PieceInitial(Element[square]);
                        if ((Element[square] & Piece.Colour) == Piece.Black)
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
            fen.Append(Colour == Piece.White ? 'w' : 'b');
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
                    Int32 piece = Element[file + rank * 8];
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

        public static Int32 Square(Point e) {
            Int32 file = e.X / VisualPosition.SquareWidth;
            Int32 rank = (e.Y - Window.MenuHeight) / VisualPosition.SquareWidth;
            if (VisualPosition.Rotated)
                return 7 - file + (7 - rank) * 8;
            return file + rank * 8;
        }

        public static Int32 Square(String name) {
            return (Int32)(name[0] - 97 + (56 - name[1]) * 8);
        }
    }
}
