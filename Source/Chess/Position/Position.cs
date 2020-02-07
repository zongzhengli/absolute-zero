using System;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the more object-oriented, and less chess-related, components 
    /// of the chess position. 
    /// </summary>
    public sealed partial class Position : IEquatable<Position> {

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
             || Value[Colour.White] != other.Value[Colour.White] 
             || Value[Colour.Black] != other.Value[Colour.Black])
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
                Value = this.Value.Clone() as Int32[],
                SideToMove = this.SideToMove,
                HalfMoves = this.HalfMoves,
                FiftyMovesHistory = this.FiftyMovesHistory.Clone() as Int32[],
                FiftyMovesClock = this.FiftyMovesClock,
                ZobristKey = this.ZobristKey,
                ZobristKeyHistory = this.ZobristKeyHistory.Clone() as UInt64[]
            };
        }

        /// <summary>
        /// Returns a text drawing of the position.
        /// </summary>
        /// <returns>A text drawing of the position</returns>
        public override String ToString() {
            return ToString(String.Empty);
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
        /// Generates the hash key for the position from scratch. This is expensive 
        /// and should only be used for initialization and testing. Normally, use the
        /// incrementally updated ZobristKey field.
        /// </summary>
        /// <returns>The hash key for the position.</returns>
        public UInt64 GetZobristKey() {
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
    }
}
