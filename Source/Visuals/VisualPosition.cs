using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace AbsoluteZero {
    static class VisualPosition {
        public static Boolean Animations = true;
        public static Boolean Rotated = false;
        public const Double AnimationEasing = .3;
        public const Int32 AnimationInterval = 33;

        public const Int32 SquareWidth = 50;
        public const Int32 Width = 8 * SquareWidth;
        public static readonly Color LightColor = Color.FromArgb(230, 230, 230);
        public static readonly Color DarkColor = Color.FromArgb(215, 215, 215);
        private static readonly SolidBrush LightBrush = new SolidBrush(LightColor);
        private static readonly SolidBrush DarkBrush = new SolidBrush(DarkColor);
        private static readonly Rectangle[] DarkSquares = new Rectangle[32];
        private static readonly Object ElementLock = new Object();

        private static List<VisualPiece> element = new List<VisualPiece>(32);

        static VisualPosition() {
            for (Int32 file = 0; file < 8; file++)
                for (Int32 rank = 0; rank < 8; rank++)
                    if (((file + rank) & 1) > 0)
                        DarkSquares[file / 2 + rank * 4] = new Rectangle(file * SquareWidth, rank * SquareWidth, SquareWidth, SquareWidth);
        }

        public static void Set(Position position) {
            lock (ElementLock) {
                element.Clear();
                for (Int32 square = 0; square < position.Element.Length; square++)
                    if (position.Element[square] != Piece.Empty)
                        element.Add(new VisualPiece(position.Element[square], Position.File(square) * SquareWidth, Position.Rank(square) * SquareWidth));
            }
        }

        public static void Make(Int32 move) {
            Int32 from = Move.GetFrom(move);
            Int32 to = Move.GetTo(move);
            Point initial = new Point(Position.File(from) * SquareWidth, Position.Rank(from) * SquareWidth);
            Point final = new Point(Position.File(to) * SquareWidth, Position.Rank(to) * SquareWidth);

            // remove captured pieces
            lock (ElementLock)
                for (Int32 i = 0; i < element.Count; i++)
                    if (element[i].IsAt(final)) {
                        element.RemoveAt(i);
                        break;
                    }

            // perform special moves
            switch (Move.GetSpecial(move) & Piece.Type) {
                case Piece.King:
                    Point rookInitial = new Point(7 * (Position.File(to) - 2) / 4 * SquareWidth, Position.Rank(to) * SquareWidth);
                    Point rookFinal = new Point((Position.File(to) / 2 + 2) * SquareWidth, Position.Rank(to) * SquareWidth);
                    Animate(move, rookInitial, rookFinal);
                    break;
                case Piece.Pawn:
                    Point enPassant = new Point(Position.File(to) * SquareWidth, Position.Rank(from) * SquareWidth);
                    lock (ElementLock)
                        for (Int32 i = 0; i < element.Count; i++)
                            if (element[i].IsAt(enPassant)) {
                                element.RemoveAt(i);
                                break;
                            }
                    break;
            }
            Animate(move, initial, final);
        }

        private static void Animate(Int32 move, Point initial, Point final) {
            VisualPiece piece = null;
            lock (ElementLock)
                for (Int32 i = 0; i < element.Count; i++)
                    if (element[i].IsAt(initial)) {
                        piece = element[i];
                        break;
                    }
            new Thread(new ThreadStart(delegate {
                piece.MoveTo(final);
                if (Move.IsPromotion(move))
                    piece.Promote(Move.GetSpecial(move));
            })) {
                IsBackground = true
            }.Start();
        }

        public static void FillDarkSquares(Graphics g) {
            g.FillRectangles(DarkBrush, DarkSquares);
        }

        public static void FillSquare(Graphics g, SolidBrush brush, Int32 square) {
            Int32 x = Position.File(square);
            Int32 y = Position.Rank(square);
            if (Rotated) {
                x = 7 - x;
                y = 7 - y;
            }
            x *= SquareWidth;
            y *= SquareWidth;
            g.FillRectangle(brush, x, y, SquareWidth, SquareWidth);
        }

        public static void DrawPieces(Graphics g) {
            element.ForEach(piece => {
                if (piece != null)
                    piece.Draw(g);
            });
        }
    }
}
