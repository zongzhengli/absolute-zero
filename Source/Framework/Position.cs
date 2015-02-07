using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Represents a complete chess position. 
    /// </summary>
    class Position : IEquatable<Position> {

        /// <summary>
        /// The FEN string of the starting chess position. 
        /// </summary>
        public const String StartingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// The maximum number of plies the position is to support. 
        /// </summary>
        public const Int32 HalfMovesLimit = 1024;

        /// <summary>
        /// The value representing an invalid square. 
        /// </summary>
        public const Int32 InvalidSquare = -1;

        /// <summary>
        /// The collection of pieces on the squares on the chessboard. Square[n] 
        /// gives the piece at the nth square in the chess position where 0 is A8 and 
        /// 63 is H1. 
        /// </summary>
        public Int32[] Square = new Int32[64];

        /// <summary>
        /// The collection of bitboards in representing the sets of pieces. 
        /// Bitboard[p] gives the bitboard for the piece represented by p. 
        /// </summary>
        public UInt64[] Bitboard = new UInt64[16];

        /// <summary>
        /// The bitboard of all pieces in play. 
        /// </summary>
        public UInt64 OccupiedBitboard = 0;

        /// <summary>
        /// The total material values for each colour. Material[c] gives the total 
        /// material possessed by the colour c of the appropriate sign. 
        /// </summary>
        public Int32[] Material = new Int32[2];

        /// <summary>
        /// The colour that is to make the next move. 
        /// </summary>
        public Int32 SideToMove = 0;

        /// <summary>
        /// The total number of plies the position has advanced from its initial 
        /// state. 
        /// </summary>
        public Int32 HalfMoves = 0;

        /// <summary>
        /// The values indicating whether kingside castling is permitted for each 
        /// colour. CastleKingside[c] is positive if and only if c can castle 
        /// kingside. 
        /// </summary>
        public Int32[] CastleKingside = new Int32[2];

        /// <summary>
        /// The values indicating whether queenside castling is permitted for each 
        /// colour. CastleQueenside[c] is positive if and only if c can castle 
        /// queenside. 
        /// </summary>
        public Int32[] CastleQueenside = new Int32[2];

        /// <summary>
        /// The square indicating en passant is permitted and giving where a pawn 
        /// performing enpassant would move to. 
        /// </summary>
        public Int32 EnPassantSquare = InvalidSquare;

        /// <summary>
        /// The EnPassantSquare values for every ply up to and including the current 
        /// ply. 
        /// </summary>
        public Int32[] EnPassantHistory = new Int32[HalfMovesLimit];

        /// <summary>
        /// The value used to track and enforce the whether fifty-move rule. 
        /// </summary>
        public Int32 FiftyMovesClock = 0;

        /// <summary>
        /// The FiftyMovesClock values for every ply up to and including the current 
        /// ply. 
        /// </summary>
        public Int32[] FiftyMovesHistory = new Int32[HalfMovesLimit];

        /// <summary>
        /// The Zobrist hash value of the position. 
        /// </summary>
        public UInt64 ZobristKey;

        /// <summary>
        /// The ZobristKey values for every ply up to and including the current ply. 
        /// </summary>
        public UInt64[] ZobristKeyHistory = new UInt64[HalfMovesLimit];

        /// <summary>
        /// Constructs a position with the given FEN string. If the FEN string is 
        /// invalid the start position is used. 
        /// </summary>
        /// <param name="fen">The FEN of the position.</param>
        public Position(String fen) {
            try {
                ParseFen(fen);
            } catch {
                ParseFen(StartingFEN);
            }
        }

        /// <summary>
        /// Constructs a position with an invalid state. This constructor is used for 
        /// cloning. 
        /// </summary>
        private Position() { }

        /// <summary>
        /// Parses the given FEN string and changes the position's state to represent 
        /// that of the FEN string. 
        /// </summary>
        /// <param name="fen">The FEN string to parse.</param>
        private void ParseFen(String fen) {

            // Clear squares. 
            Array.Clear(Square, 0, Square.Length);

            // Split FEN into terms based on whitespace. 
            String[] terms = fen.Split(new Char[0], StringSplitOptions.RemoveEmptyEntries);

            Int32 file = 0;
            Int32 rank = 0;

            // Determine piece locations and populate squares. 
            foreach (Char c in terms[0]) {
                Char uc = Char.ToUpperInvariant(c);
                Int32 colour = (c == uc) ? Colour.White : Colour.Black;

                switch (uc) {

                    // Parse number denoting blank squares. 
                    default:
                        file += uc - '0';
                        break;

                    // Parse separator denoting new rank. 
                    case '/':
                        file = 0;
                        rank++;
                        break;

                    // Parse piece abbreviations. 
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

            // Determine side to move. 
            SideToMove = (terms[1] == "w") ? Colour.White : Colour.Black;

            // Determine castling rights. 
            if (terms.Length > 2) {
                if (terms[2].Contains("Q"))
                    CastleQueenside[Colour.White] = 1;
                if (terms[2].Contains("K"))
                    CastleKingside[Colour.White] = 1;
                if (terms[2].Contains("q"))
                    CastleQueenside[Colour.Black] = 1;
                if (terms[2].Contains("k"))
                    CastleKingside[Colour.Black] = 1;
            }

            // Determine en passant square. 
            if (terms.Length > 3)
                if (terms[3] != "-")
                    EnPassantSquare = SquareAt(terms[3]);

            // Determine fifty-moves clock and plies advanced. 
            if (terms.Length > 5) {
                FiftyMovesClock = Int32.Parse(terms[4]);
                Int32 moveNumber = Int32.Parse(terms[5]);
                HalfMoves = 2 * (moveNumber - 1) + SideToMove;
                HalfMoves = Math.Max(0, HalfMoves);
                FiftyMovesClock = Math.Min(FiftyMovesClock, HalfMoves);
            }

            // Initialize history information. 
            for (Int32 i = 0; i < EnPassantHistory.Length; i++)
                EnPassantHistory[i] = InvalidSquare;
            EnPassantHistory[HalfMoves] = EnPassantSquare;
            FiftyMovesHistory[HalfMoves] = FiftyMovesClock;

            // Initialize bitboards and material information. 
            for (Int32 square = 0; square < Square.Length; square++)
                if (Square[square] != Piece.Empty) {
                    Int32 colour = Square[square] & Colour.Mask;
                    Bitboard[Square[square]] |= 1UL << square;
                    Bitboard[colour] |= 1UL << square;
                    OccupiedBitboard |= 1UL << square;
                    if ((Square[square] & Piece.Mask) != Piece.King)
                        Material[colour] += Zero.PieceValue[Square[square]];
                }

            // Initialize Zobrist key and history. 
            ZobristKey = GenerateZobristKey();
            ZobristKeyHistory[HalfMoves] = ZobristKey;
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

            UInt64 friendlyBitboard = Bitboard[SideToMove];
            UInt64 enemyBitboard = Bitboard[enemy];
            UInt64 targetBitboard = ~friendlyBitboard;

            UInt64 enemyBishopQueenBitboard = Bitboard[enemy | Piece.Bishop] | Bitboard[enemy | Piece.Queen];
            UInt64 enemyRookQueenBitboard = Bitboard[enemy | Piece.Rook] | Bitboard[enemy | Piece.Queen];

            // Initialize variables for move generation. 
            UInt64 checkBitboard = 0;
            UInt64 pinBitboard = 0;
            Int32 index = 0;

            // Consider knight and pawn checks. 
            checkBitboard |= Bitboard[enemy | Piece.Knight] & Attack.Knight(kingSquare);
            checkBitboard |= Bitboard[enemy | Piece.Pawn] & Attack.Pawn(kingSquare, SideToMove);

            // Consider bishop and queen checks and pins. 
            if ((enemyBishopQueenBitboard & Bit.Diagonals[kingSquare]) != 0) {
                checkBitboard |= enemyBishopQueenBitboard & Attack.Bishop(kingSquare, OccupiedBitboard);

                UInt64 defencelessBitboard = OccupiedBitboard;
                UInt64 defenceBitboard = Bit.RayNE[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);
                defenceBitboard = Bit.RayNW[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);
                defenceBitboard = Bit.RaySE[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.Scan(defenceBitboard);
                defenceBitboard = Bit.RaySW[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.Scan(defenceBitboard);

                if (defencelessBitboard != OccupiedBitboard)
                    pinBitboard |= enemyBishopQueenBitboard & Attack.Bishop(kingSquare, defencelessBitboard);
            }

            // Consider rook and queen checks and pins. 
            if ((enemyRookQueenBitboard & Bit.Axes[kingSquare]) != 0) {
                checkBitboard |= enemyRookQueenBitboard & Attack.Rook(kingSquare, OccupiedBitboard);

                UInt64 defencelessBitboard = OccupiedBitboard;
                UInt64 defenceBitboard = Bit.RayN[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);
                defenceBitboard = Bit.RayE[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.Scan(defenceBitboard);
                defenceBitboard = Bit.RayS[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.Scan(defenceBitboard);
                defenceBitboard = Bit.RayW[kingSquare] & friendlyBitboard;
                if (defenceBitboard != 0)
                    defencelessBitboard ^= 1UL << Bit.ScanReverse(defenceBitboard);

                if (defencelessBitboard != OccupiedBitboard)
                    pinBitboard |= enemyRookQueenBitboard & Attack.Rook(kingSquare, defencelessBitboard);
            }

            // Consider castling. This is always fully tested for legality. 
            if (checkBitboard == 0) {
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

            // Case 1. If we are not in check and there are no pinned pieces. We don't 
            //         need to test normal moves for legality. 
            if (checkBitboard == 0 & pinBitboard == 0) {

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
                //     all moves are tested for legality. 
            else if ((checkBitboard & (checkBitboard - 1)) == 0) {

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

        /// <summary>
        /// Returns the list of legal moves for the position. 
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
        /// Makes the given move on the position.
        /// </summary>
        /// <param name="move">The move to make.</param>
        public void Make(Int32 move) {
            Int32 from = Move.From(move);
            Int32 to = Move.To(move);
            Int32 piece = Move.Piece(move);
            Int32 capture = Move.Capture(move);
            Int32 special = Move.Special(move);

            Square[to] = piece;
            Square[from] = Piece.Empty;
            Bitboard[piece] ^= (1UL << from) | (1UL << to);
            Bitboard[SideToMove] ^= (1UL << from) | (1UL << to);
            OccupiedBitboard ^= (1UL << from) | (1UL << to);

            ZobristKey ^= Zobrist.PiecePosition[piece][from] ^ Zobrist.PiecePosition[piece][to];
            ZobristKey ^= Zobrist.Colour;
            if (EnPassantSquare != InvalidSquare) {
                ZobristKey ^= Zobrist.EnPassant[EnPassantSquare];
                EnPassantSquare = InvalidSquare;
            }
            FiftyMovesClock++;
            HalfMoves++;

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
                    Bitboard[(1 - SideToMove)] ^= 1UL << to;
                    OccupiedBitboard |= 1UL << to;
                    ZobristKey ^= Zobrist.PiecePosition[capture][to];
                    Material[1 - SideToMove] -= Zero.PieceValue[capture];
                    FiftyMovesClock = 0;
                    break;
            }

            switch (special & Piece.Mask) {
                case Piece.Empty:
                    switch (piece & Piece.Mask) {
                        case Piece.Pawn:
                            FiftyMovesClock = 0;
                            if ((from - to) * (from - to) == 256) {
                                ZobristKey ^= Zobrist.EnPassant[from];
                                EnPassantHistory[HalfMoves] = EnPassantSquare = (from + to) / 2;
                            }
                            break;
                        case Piece.Rook:
                            if ((SideToMove == Colour.White && from == 56) || (SideToMove == Colour.Black && from == 0)) {
                                if (CastleQueenside[SideToMove]-- > 0)
                                    ZobristKey ^= Zobrist.CastleQueenside[SideToMove];
                            } else if ((SideToMove == Colour.White && from == 63) || (SideToMove == Colour.Black && from == 7))
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
                    Bitboard[SideToMove] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedBitboard ^= (1UL << rookFrom) | (1UL << rookTo);
                    ZobristKey ^= Zobrist.PiecePosition[SideToMove | Piece.Rook][rookFrom];
                    ZobristKey ^= Zobrist.PiecePosition[SideToMove | Piece.Rook][rookTo];
                    Square[rookFrom] = Piece.Empty;
                    Square[rookTo] = SideToMove | Piece.Rook;
                    break;
                case Piece.Pawn:
                    Square[File(to) + Rank(from) * 8] = Piece.Empty;
                    Bitboard[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    Bitboard[(1 - SideToMove)] ^= 1UL << (File(to) + Rank(from) * 8);
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

            SideToMove = 1 - SideToMove;
            Square[from] = piece;
            Square[to] = capture;
            Bitboard[piece] ^= (1UL << from) | (1UL << to);
            Bitboard[SideToMove] ^= (1UL << from) | (1UL << to);
            OccupiedBitboard ^= (1UL << from) | (1UL << to);

            ZobristKey = ZobristKeyHistory[HalfMoves - 1];
            EnPassantHistory[HalfMoves] = InvalidSquare;
            EnPassantSquare = EnPassantHistory[HalfMoves - 1];
            FiftyMovesClock = FiftyMovesHistory[HalfMoves - 1];
            HalfMoves--;

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
                    Bitboard[(1 - SideToMove)] ^= 1UL << to;
                    OccupiedBitboard |= 1UL << to;
                    Material[1 - SideToMove] += Zero.PieceValue[capture];
                    break;
            }

            switch (special & Piece.Mask) {
                case Piece.Empty:
                    switch (piece & Piece.Mask) {
                        case Piece.Rook:
                            if ((SideToMove == Colour.White && from == 56) || (SideToMove == Colour.Black && from == 0)) {
                                CastleQueenside[SideToMove]++;
                            } else if ((SideToMove == Colour.White && from == 63) || (SideToMove == Colour.Black && from == 7))
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
                    Bitboard[SideToMove] ^= (1UL << rookFrom) | (1UL << rookTo);
                    OccupiedBitboard ^= (1UL << rookFrom) | (1UL << rookTo);
                    Square[rookFrom] = SideToMove | Piece.Rook;
                    Square[rookTo] = Piece.Empty;
                    break;
                case Piece.Pawn:
                    Square[File(to) + Rank(from) * 8] = special;
                    Bitboard[special] ^= 1UL << (File(to) + Rank(from) * 8);
                    Bitboard[(1 - SideToMove)] ^= 1UL << (File(to) + Rank(from) * 8);
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

        /// <summary>
        /// Generates the hash key for the position from scratch. 
        /// </summary>
        /// <returns>The hash key for the position.</returns>
        public UInt64 GenerateZobristKey() {
            UInt64 key = 0;

            for (Int32 square = 0; square < Square.Length; square++)
                if (Square[square] != Piece.Empty)
                    key ^= Zobrist.PiecePosition[Square[square]][square];

            if (EnPassantSquare != InvalidSquare)
                key ^= Zobrist.EnPassant[EnPassantSquare];

            if (SideToMove != Colour.White)
                key ^= Zobrist.Colour;

            for (Int32 colour = Colour.White; colour <= Colour.Black; colour++) {
                if (CastleQueenside[colour] > 0)
                    key ^= Zobrist.CastleQueenside[colour];
                if (CastleKingside[colour] > 0)
                    key ^= Zobrist.CastleKingside[colour];
            }
            return key;
        }

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
                for (Int32 colour = Colour.White; colour <= Colour.Black; colour++)
                    if ((Bitboard[colour | Piece.Knight] | Bitboard[colour | Piece.Bishop]) != 0)
                        return true;

            for (Int32 colour = Colour.White; colour <= Colour.Black; colour++)
                if (Bit.CountSparse(Bitboard[colour | Piece.Knight]) >= 2)
                    return true;

            if (Bitboard[Colour.White | Piece.Bishop] != 0 && Bitboard[Colour.Black | Piece.Bishop] != 0)
                return ((Bitboard[Colour.White | Piece.Bishop] & Bit.LightSquares) != 0)
                       == ((Bitboard[Colour.Black | Piece.Bishop] & Bit.LightSquares) != 0);
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
        /// Returns whether the position is equal to another position.  
        /// </summary>
        /// <param name="other">The position to compare with.</param>
        /// <returns>Whether the position is equal to another position</returns>
        public Boolean Equals(Position other) {
            if (other == null
             || ZobristKey != other.ZobristKey
             || OccupiedBitboard != other.OccupiedBitboard
             || HalfMoves != other.HalfMoves
             || FiftyMovesClock != other.FiftyMovesClock
             || EnPassantSquare != other.EnPassantSquare
             || SideToMove != other.SideToMove
             || Material[Colour.White] != other.Material[Colour.White] 
             || Material[Colour.Black] != other.Material[Colour.Black])
                return false;

            for (Int32 colour = Colour.White; colour <= Colour.Black; colour++) 
                if (CastleKingside[colour] != other.CastleKingside[colour]
                 || CastleQueenside[colour] != other.CastleQueenside[colour])
                    return false;

            for (Int32 ply = 0; ply < HalfMoves; ply++)
                if (FiftyMovesHistory[ply] != other.FiftyMovesHistory[ply]
                 || EnPassantHistory[ply] != other.EnPassantHistory[ply]
                 || ZobristKeyHistory[ply] != other.ZobristKeyHistory[ply])
                    return false;

            for (Int32 piece = 0; piece < Bitboard.Length; piece++)
                if (Bitboard[piece] != other.Bitboard[piece])
                    return false;

            for (Int32 square = 0; square < Square.Length; square++)
                if (Square[square] != other.Square[square])
                    return false;

            return true;
        }

        /// <summary>
        /// Returns a deep clone of the position.
        /// </summary>
        /// <returns>A deep clone of the position.</returns>
        public Position DeepClone() {
            return new Position() {
                Square = this.Square.Clone() as Int32[],
                Bitboard = this.Bitboard.Clone() as UInt64[],
                OccupiedBitboard = this.OccupiedBitboard,
                CastleKingside = this.CastleKingside.Clone() as Int32[],
                CastleQueenside = this.CastleQueenside.Clone() as Int32[],
                EnPassantHistory = this.EnPassantHistory.Clone() as Int32[],
                EnPassantSquare = this.EnPassantSquare,
                Material = this.Material.Clone() as Int32[],
                SideToMove = this.SideToMove,
                HalfMoves = this.HalfMoves,
                FiftyMovesHistory = this.FiftyMovesHistory.Clone() as Int32[],
                FiftyMovesClock = this.FiftyMovesClock,
                ZobristKey = this.ZobristKey,
                ZobristKeyHistory = this.ZobristKeyHistory.Clone() as UInt64[]
            };
        }

        /// <summary>
        /// Returns the FEN string that describes the position.
        /// </summary>
        /// <returns>The FEN string that describes the position.</returns>
        public String GetFEN() {
            StringBuilder sb = new StringBuilder();

            for (Int32 rank = 0; rank < 8; rank++) {
                Int32 spaces = 0;
                for (Int32 file = 0; file < 8; file++) {
                    Int32 square = file + rank * 8;
                    if (Square[square] == Piece.Empty)
                        spaces++;
                    else {
                        if (spaces > 0) {
                            sb.Append(spaces);
                            spaces = 0;
                        }
                        String piece = Stringify.PieceInitial(Square[square]);
                        if ((Square[square] & Colour.Mask) == Colour.Black)
                            piece = piece.ToLowerInvariant();
                        sb.Append(piece);
                    }
                }
                if (spaces > 0)
                    sb.Append(spaces);
                if (rank < 7)
                    sb.Append('/');
            }

            sb.Append(' ');
            sb.Append(SideToMove == Colour.White ? 'w' : 'b');
            sb.Append(' ');

            if (CastleKingside[Colour.White] > 0)
                sb.Append('K');
            if (CastleQueenside[Colour.White] > 0)
                sb.Append('Q');
            if (CastleKingside[Colour.Black] > 0)
                sb.Append('k');
            if (CastleQueenside[Colour.Black] > 0)
                sb.Append('q');
            if (sb[sb.Length - 1] == ' ')
                sb.Append('-');
            sb.Append(' ');

            if (EnPassantSquare != InvalidSquare)
                sb.Append(Stringify.Square(EnPassantSquare));
            else
                sb.Append('-');
            sb.Append(' ');

            sb.Append(FiftyMovesClock);
            sb.Append(' ');
            sb.Append(HalfMoves / 2 + 1);

            return sb.ToString();
        }

        /// <summary>
        /// Returns a text drawing of the position.
        /// </summary>
        /// <returns>A text drawing of the position</returns>
        public override String ToString() {
            return ToString();
        }

        /// <summary>
        /// Returns a text drawing of the position with the given comments displayed. 
        /// </summary>
        /// <param name="comments">The comments to display.</param>
        /// <returns>A text drawing of the position with the given comments displayed</returns>
        public String ToString(params String[] comments) {
            StringBuilder sb = new StringBuilder("   +------------------------+ ", 400);
            Int32 index = 0;
            if (index < comments.Length)
                sb.Append(comments[index++]);

            for (Int32 rank = 0; rank < 8; rank++) {
                sb.Append(Environment.NewLine);
                sb.Append(' ');
                sb.Append(8 - rank);
                sb.Append(" |");
                for (Int32 file = 0; file < 8; file++) {
                    Int32 piece = Square[file + rank * 8];
                    if (piece != Piece.Empty) {
                        sb.Append((piece & Colour.Mask) == Colour.White ? '<' : '[');
                        sb.Append(Stringify.PieceInitial(piece));
                        sb.Append((piece & Colour.Mask) == Colour.White ? '>' : ']');
                    } else
                        sb.Append((file + rank) % 2 == 1 ? ":::" : "   ");
                }
                sb.Append("| ");
                if (index < comments.Length)
                    sb.Append(comments[index++]);
            }

            sb.Append(Environment.NewLine);
            sb.Append("   +------------------------+ ");
            if (index < comments.Length)
                sb.Append(comments[index++]);

            sb.Append(Environment.NewLine);
            sb.Append("     a  b  c  d  e  f  g  h   ");
            if (index < comments.Length)
                sb.Append(comments[index++]);

            return sb.ToString();
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
        /// Returns the square at the given point. 
        /// </summary>
        /// <param name="point">The point to determine the square of.</param>
        /// <returns>The square at the given point</returns>
        public static Int32 SquareAt(Point point) {
            Int32 file = point.X / VisualPosition.SquareWidth;
            Int32 rank = (point.Y - Window.MenuHeight) / VisualPosition.SquareWidth;
            if (VisualPosition.Rotated)
                return 7 - file + (7 - rank) * 8;
            return file + rank * 8;
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
