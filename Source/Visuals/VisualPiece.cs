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
        /// The absolute x coordinate. 
        /// </summary>
        private Int32 _realX;

        /// <summary>
        /// The absolute y coordinate. 
        /// </summary>
        private Int32 _realY;

        /// <summary>
        /// The dynamic x coordinate for animation. 
        /// </summary>
        private Double _dynamicX;

        /// <summary>
        /// The dynamic y coordinate for animation. 
        /// </summary>
        private Double _dynamicY;

        /// <summary>
        /// Contructs a visual piece at a given location. 
        /// </summary>
        /// <param name="piece">The piece to represent.</param>
        /// <param name="x">The x coodinate.</param>
        /// <param name="y">The y coodinate.</param>
        public VisualPiece(Int32 piece, Int32 x, Int32 y) {
            this._piece = piece;
            _dynamicX = _realX = x;
            _dynamicY = _realY = y;
        }

        /// <summary>
        /// Draws the visual piece. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public void Draw(Graphics g) {
            Boolean isWhite = (_piece & Piece.Colour) == Piece.White;
            Int32 x = (Int32)Math.Round(_dynamicX);
            Int32 y = (Int32)Math.Round(_dynamicY);
            if (VisualPosition.Rotated) {
                x = VisualPosition.SquareWidth * 7 - x;
                y = VisualPosition.SquareWidth * 7 - y;
            }
            x += PieceOffset.X;
            y += PieceOffset.Y;
            switch (_piece & Piece.Type) {
                case Piece.Empty:
                    break;
                case Piece.Pawn:
                    g.DrawString(isWhite ? "\u2659" : "\u265F", PieceFont, PieceBrush, x, y);
                    break;
                case Piece.Rook:
                    g.DrawString(isWhite ? "\u2656" : "\u265C", PieceFont, PieceBrush, x, y);
                    break;
                case Piece.Knight:
                    g.DrawString(isWhite ? "\u2658" : "\u265E", PieceFont, PieceBrush, x, y);
                    break;
                case Piece.Bishop:
                    g.DrawString(isWhite ? "\u2657" : "\u265D", PieceFont, PieceBrush, x, y);
                    break;
                case Piece.King:
                    g.DrawString(isWhite ? "\u2654" : "\u265A", PieceFont, PieceBrush, x, y);
                    break;
                case Piece.Queen:
                    g.DrawString(isWhite ? "\u2655" : "\u265B", PieceFont, PieceBrush, x, y);
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
            Double easing = VisualPosition.Animations ? VisualPosition.AnimationEasing : 1;
            Int32 currentX = _realX = point.X;
            Int32 currentY = _realY = point.Y;
            while (true) {
                _dynamicX += (_realX - _dynamicX) * easing;
                _dynamicY += (_realY - _dynamicY) * easing;
                if (Math.Abs(_realX - _dynamicX) < 1 && Math.Abs(_realY - _dynamicY) < 1) {
                    _dynamicX = _realX;
                    _dynamicY = _realY;
                    return;
                }
                if (currentX != _realX || currentY != _realY)
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
            return _realX == x && _realY == y;
        }
    }
}
