﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace AbsoluteZero {

    /// <summary>
    /// Represents the chess position in the visual interface.  
    /// </summary>
    static class VisualPosition {
        public static readonly Color LightColor = Color.FromArgb(230, 230, 230);
        public static readonly Color DarkColor = Color.FromArgb(215, 215, 215);
        private static readonly SolidBrush LightBrush = new SolidBrush(LightColor);
        private static readonly SolidBrush DarkBrush = new SolidBrush(DarkColor);
        private static readonly Rectangle[] DarkSquares = new Rectangle[32];
        private static readonly Object PiecesLock = new Object();

        public const Double AnimationEasing = 0.3;
        public const Int32 AnimationInterval = 33;
        public const Int32 SquareWidth = 50;
        public const Int32 Width = 8 * SquareWidth;

        public static Boolean Animations = true;
        public static Boolean Rotated = false;

        private static List<VisualPiece> _pieces = new List<VisualPiece>(32);

        static VisualPosition() {
            for (Int32 file = 0; file < 8; file++)
                for (Int32 rank = 0; rank < 8; rank++)
                    if (((file + rank) & 1) > 0)
                        DarkSquares[file / 2 + rank * 4] = new Rectangle(file * SquareWidth, rank * SquareWidth, SquareWidth, SquareWidth);
        }

        public static void Set(Position position) {
            lock (PiecesLock) {
                _pieces.Clear();
                for (Int32 square = 0; square < position.Square.Length; square++)
                    if (position.Square[square] != Piece.Empty)
                        _pieces.Add(new VisualPiece(position.Square[square], Position.File(square) * SquareWidth, Position.Rank(square) * SquareWidth));
            }
        }

        public static void Make(Int32 move) {
            Int32 from = Move.From(move);
            Int32 to = Move.To(move);
            Point initial = new Point(Position.File(from) * SquareWidth, Position.Rank(from) * SquareWidth);
            Point final = new Point(Position.File(to) * SquareWidth, Position.Rank(to) * SquareWidth);

            // Remove captured pieces.
            lock (PiecesLock)
                for (Int32 i = 0; i < _pieces.Count; i++)
                    if (_pieces[i].IsAt(final)) {
                        _pieces.RemoveAt(i);
                        break;
                    }

            // Perform special moves.
            switch (Move.Special(move) & Piece.Type) {
                case Piece.King:
                    Point rookInitial = new Point(7 * (Position.File(to) - 2) / 4 * SquareWidth, Position.Rank(to) * SquareWidth);
                    Point rookFinal = new Point((Position.File(to) / 2 + 2) * SquareWidth, Position.Rank(to) * SquareWidth);
                    Animate(move, rookInitial, rookFinal);
                    break;
                case Piece.Pawn:
                    Point enPassant = new Point(Position.File(to) * SquareWidth, Position.Rank(from) * SquareWidth);
                    lock (PiecesLock)
                        for (Int32 i = 0; i < _pieces.Count; i++)
                            if (_pieces[i].IsAt(enPassant)) {
                                _pieces.RemoveAt(i);
                                break;
                            }
                    break;
            }
            Animate(move, initial, final);
        }

        private static void Animate(Int32 move, Point initial, Point final) {
            VisualPiece piece = null;
            lock (PiecesLock)
                for (Int32 i = 0; i < _pieces.Count; i++)
                    if (_pieces[i].IsAt(initial)) {
                        piece = _pieces[i];
                        break;
                    }

            new Thread(new ThreadStart(() => {
                piece.MoveTo(final);
                if (Move.IsPromotion(move))
                    piece.Promote(Move.Special(move));
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
            _pieces.ForEach(piece => {
                if (piece != null)
                    piece.Draw(g);
            });
        }
    }
}
