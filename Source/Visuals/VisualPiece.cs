using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace AbsoluteZero {

    /// <summary>
    /// Represents a chess piece in the visual interface. 
    /// </summary>
    public sealed class VisualPiece {

        /// <summary>
        /// The offset for centering pieces on squares when drawing.
        /// </summary>
        public static readonly Point PieceOffset = new Point(-4, 2);

        /// <summary>
        /// The font for drawing pieces. 
        /// </summary>
        public static readonly Font PieceFont = new Font("Tahoma", 30);

        /// <summary>
        /// The mapping from piece to Unicode string.
        /// </summary>
        private static readonly Dictionary<Int32, String> PieceString = new Dictionary<Int32, string> {
            { Colour.White | Piece.King, "\u2654" },
            { Colour.White | Piece.Queen, "\u2655" },
            { Colour.White | Piece.Rook, "\u2656" },
            { Colour.White | Piece.Bishop, "\u2657" },
            { Colour.White | Piece.Knight, "\u2658" },
            { Colour.White | Piece.Pawn, "\u2659" },
            { Colour.Black | Piece.King, "\u265A" },
            { Colour.Black | Piece.Queen, "\u265B" },
            { Colour.Black | Piece.Rook, "\u265C" },
            { Colour.Black | Piece.Bishop, "\u265D" },
            { Colour.Black | Piece.Knight, "\u265E" },
            { Colour.Black | Piece.Pawn, "\u265F" },
        };

        /// <summary>
        /// The brush for drawing pieces. 
        /// </summary>
        private static readonly Brush PieceBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// The piece to represent. 
        /// </summary>
        public Int32 ActualPiece {
            get;
            private set;
        }

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
            this.ActualPiece = piece;
            _real = new Point(x, y);
            _dynamic = new PointF(x, y);
        }

        /// <summary>
        /// Draws the visual piece. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public void Draw(Graphics g) {
            PointF location = new PointF(_dynamic.X, _dynamic.Y);
            if (VisualPosition.Rotated) {
                location.X = VisualPosition.SquareWidth * 7 - location.X;
                location.Y = VisualPosition.SquareWidth * 7 - location.Y;
            }
            DrawAt(g, ActualPiece, location, PieceBrush);
        }

        /// <summary>
        /// Promotes the piece represented to the given piece. 
        /// </summary>
        /// <param name="promotion">The new piece to represent.</param>
        public void Promote(Int32 promotion) {
            ActualPiece = promotion;
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

        /// <summary>
        /// Draws the piece at the given location. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        /// <param name="piece">The piece to draw.</param>
        /// <param name="location">The location to draw at.</param>
        /// <param name="brush">The brush to draw with.</param>
        public static void DrawAt(Graphics g, Int32 piece, PointF location, Brush brush) {
            if (piece != Piece.Empty) {
                location.X += PieceOffset.X;
                location.Y += PieceOffset.Y;
                g.DrawString(PieceString[piece], PieceFont, brush, location);
            }
        }
    }
}
