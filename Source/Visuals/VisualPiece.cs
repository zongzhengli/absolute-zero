using System;
using System.Drawing;
using System.Threading;

namespace AbsoluteZero {
    class VisualPiece {
        private static readonly Point PieceOffset = new Point(-4, 6);
        private static readonly Font PieceFont = new Font("Arial", 30);
        private static readonly Brush PieceBrush = new SolidBrush(Color.Black);

        private Int32 piece;
        private Int32 realX;
        private Int32 realY;
        private Double dynamicX;
        private Double dynamicY;

        public VisualPiece(Int32 piece, Int32 x, Int32 y) {
            this.piece = piece;
            dynamicX = realX = x;
            dynamicY = realY = y;
        }

        public void Draw(Graphics g) {
            Boolean isWhite = (piece & Piece.Colour) == Piece.White;
            Int32 x = (Int32)Math.Round(dynamicX);
            Int32 y = (Int32)Math.Round(dynamicY);
            if (VisualPosition.Rotated) {
                x = VisualPosition.SquareWidth * 7 - x;
                y = VisualPosition.SquareWidth * 7 - y;
            }
            x += PieceOffset.X;
            y += PieceOffset.Y;
            switch (piece & Piece.Type) {
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
            piece = promotion;
        }

        public void MoveTo(Point final) {
            Double easing = VisualPosition.Animations ? VisualPosition.AnimationEasing : 1;
            Int32 currentX = realX = final.X;
            Int32 currentY = realY = final.Y;
            while (true) {
                dynamicX += (realX - dynamicX) * easing;
                dynamicY += (realY - dynamicY) * easing;
                if (Math.Abs(realX - dynamicX) < 1 && Math.Abs(realY - dynamicY) < 1) {
                    dynamicX = realX;
                    dynamicY = realY;
                    return;
                }
                if (currentX != realX || currentY != realY)
                    return;
                Thread.Sleep(VisualPosition.AnimationInterval);
            }
        }

        public Boolean IsAt(Point p) {
            return IsAt(p.X, p.Y);
        }

        public Boolean IsAt(Int32 x, Int32 y) {
            return realX == x && realY == y;
        }
    }
}
