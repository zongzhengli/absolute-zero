using System;
using System.Drawing;
using System.Threading;

namespace AbsoluteZero {
    class VisualPiece {
        private static readonly Point PieceOffset = new Point(-4, 6);
        private static readonly Font PieceFont = new Font("Arial", 30);
        private static readonly Brush PieceBrush = new SolidBrush(Color.Black);

        private Int32 _piece;
        private Int32 _realX;
        private Int32 _realY;
        private Double _dynamicX;
        private Double _dynamicY;

        public VisualPiece(Int32 piece, Int32 x, Int32 y) {
            this._piece = piece;
            _dynamicX = _realX = x;
            _dynamicY = _realY = y;
        }

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

        public void Promote(Int32 promotion) {
            _piece = promotion;
        }

        public void MoveTo(Point final) {
            Double easing = VisualPosition.Animations ? VisualPosition.AnimationEasing : 1;
            Int32 currentX = _realX = final.X;
            Int32 currentY = _realY = final.Y;
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

        public Boolean IsAt(Point p) {
            return IsAt(p.X, p.Y);
        }

        public Boolean IsAt(Int32 x, Int32 y) {
            return _realX == x && _realY == y;
        }
    }
}
