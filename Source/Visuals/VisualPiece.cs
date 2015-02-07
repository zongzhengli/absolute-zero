using System;
using System.Drawing;
using System.Threading;

namespace AbsoluteZero {

    /// <summary>
    /// Represents a chess piece in the visual interface. 
    /// </summary>
    class VisualPiece {

        /// <summary>
        /// The offset for centering pieces on squares when drawing.
        /// </summary>
        private static readonly Point PieceOffset = new Point(-4, 2);

        /// <summary>
        /// The font for drawing pieces. 
        /// </summary>
        private static readonly Font PieceFont = new Font("Tahoma", 30);

        /// <summary>
        /// The brush for drawing pieces. 
        /// </summary>
        private static readonly Brush PieceBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// The piece to represent. 
        /// </summary>
        private Int32 _piece;

        /// <summary>
        /// The real location of the visual piece. 
        /// </summary>
        private Point _real;

        /// <summary>
        /// The dynamic location of the visual piece for animation. 
        /// </summary>
        private PointF _dynamic;

        /// <summary>
        /// Contructs a visual piece at a given location. 
        /// </summary>
        /// <param name="piece">The piece to represent.</param>
        /// <param name="x">The x coodinate.</param>
        /// <param name="y">The y coodinate.</param>
        public VisualPiece(Int32 piece, Int32 x, Int32 y) {
            this._piece = piece;
            _real = new Point(x, y);
            _dynamic = new PointF(x, y);
        }

        /// <summary>
        /// Draws the visual piece. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public void Draw(Graphics g) {
            Boolean isWhite = (_piece & Colour.Mask) == Colour.White;

            PointF location = new PointF(_dynamic.X, _dynamic.Y);
            if (VisualPosition.Rotated) {
                location.X = VisualPosition.SquareWidth * 7 - location.X;
                location.Y = VisualPosition.SquareWidth * 7 - location.Y;
            }
            location.X += PieceOffset.X;
            location.Y += PieceOffset.Y;

            switch (_piece & Piece.Mask) {
                case Piece.Empty:
                    break;
                case Piece.Pawn:
                    g.DrawString(isWhite ? "\u2659" : "\u265F", PieceFont, PieceBrush, location);
                    break;
                case Piece.Rook:
                    g.DrawString(isWhite ? "\u2656" : "\u265C", PieceFont, PieceBrush, location);
                    break;
                case Piece.Knight:
                    g.DrawString(isWhite ? "\u2658" : "\u265E", PieceFont, PieceBrush, location);
                    break;
                case Piece.Bishop:
                    g.DrawString(isWhite ? "\u2657" : "\u265D", PieceFont, PieceBrush, location);
                    break;
                case Piece.King:
                    g.DrawString(isWhite ? "\u2654" : "\u265A", PieceFont, PieceBrush, location);
                    break;
                case Piece.Queen:
                    g.DrawString(isWhite ? "\u2655" : "\u265B", PieceFont, PieceBrush, location);
                    break;
            }
        }

        /// <summary>
        /// Promotes the piece represented to the given piece. 
        /// </summary>
        /// <param name="promotion">The new piece to represent.</param>
        public void Promote(Int32 promotion) {
            _piece = promotion;
        }

        /// <summary>
        /// Moves the piece to the given location.
        /// </summary>
        /// <param name="point">The location to move the piece to.</param>
        public void MoveTo(Point point) {
            Single easing = VisualPosition.Animations ? VisualPosition.AnimationEasing : 1;
            Point current = _real = point;

            while (true) {
                _dynamic.X += (_real.X - _dynamic.X) * easing;
                _dynamic.Y += (_real.Y - _dynamic.Y) * easing;

                if (Math.Abs(_real.X - _dynamic.X) < 1 && Math.Abs(_real.Y - _dynamic.Y) < 1) {
                    _dynamic.X = _real.X;
                    _dynamic.Y = _real.Y;
                    return;
                }

                // Another move has been made with the same piece. 
                if (current.X != _real.X || current.Y != _real.Y)
                    return;

                Thread.Sleep(VisualPosition.AnimationInterval);
            }
        }

        /// <summary>
        /// Returns whether the visual piece is at the given location.
        /// </summary>
        /// <param name="point">The location to check.</param>
        /// <returns>Whether the visual piece is at the given location.</returns>
        public Boolean IsAt(Point point) {
            return IsAt(point.X, point.Y);
        }

        /// <summary>
        /// Returns whether the visual piece is at the given location.
        /// </summary>
        /// <param name="x">The x coordinate of the location.</param>
        /// <param name="y">The y coordinate of the location.</param>
        /// <returns>Whether the visual piece is at the given location.</returns>
        public Boolean IsAt(Int32 x, Int32 y) {
            return _real.X == x && _real.Y == y;
        }
    }
}
