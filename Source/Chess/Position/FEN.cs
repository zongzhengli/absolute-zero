using System;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the FEN handling component of the chess position. 
    /// </summary>
    public sealed partial class Position : IEquatable<Position> {

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

            // Initialize bitboards and positional value. 
            for (Int32 square = 0; square < Square.Length; square++) {
                if (Square[square] != Piece.Empty) {
                    Int32 piece = Square[square];
                    Int32 colour = piece & Colour.Mask;
                    Bitboard[piece] |= 1UL << square;
                    Bitboard[colour] |= 1UL << square;
                    OccupiedBitboard |= 1UL << square;
                    if ((piece & Piece.Mask) != Piece.King)
                        Value[colour] += Zero.PieceValue[piece];
                }
            }
            for (Int32 square = 0; square < Square.Length; square++) {
                if (Square[square] != Piece.Empty) {
                    Int32 piece = Square[square];
                    Int32 colour = piece & Colour.Mask;
                    Value[colour] += GetIncrementalValue(square, piece);
                }
            }

            // Initialize Zobrist key and history. 
            ZobristKey = GetZobristKey();
            ZobristKeyHistory[HalfMoves] = ZobristKey;
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
    }
}
