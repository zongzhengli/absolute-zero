using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace AbsoluteZero {

    /// <summary>
    /// Represents the chess position in the visual interface.  
    /// </summary>
    static class VisualPosition {

        /// <summary>
        /// The colour of light squares on the chessboard. 
        /// </summary>
        public static readonly Color LightColor = Color.FromArgb(230, 230, 230);

        /// <summary>
        /// The colour of dark squares on the chessboard. 
        /// </summary>
        public static readonly Color DarkColor = Color.FromArgb(215, 215, 215);

        /// <summary>
        /// The brush for drawing light squares on the chessboard. 
        /// </summary>
        private static readonly SolidBrush LightBrush = new SolidBrush(LightColor);

        /// <summary>
        /// The brush for drawing dark squares on the chessboard. 
        /// </summary>
        private static readonly SolidBrush DarkBrush = new SolidBrush(DarkColor);

        /// <summary>
        /// The collection of rectangles representing the dark squares. 
        /// </summary>
        private static readonly Rectangle[] DarkSquares = new Rectangle[32];

        /// <summary>
        /// The lock for modifying the collection of pieces. 
        /// </summary>
        private static readonly Object PiecesLock = new Object();

        /// <summary>
        /// The multiplicative factor for averaging piece locations during animation. 
        /// </summary>
        public const Double AnimationEasing = 0.3;

        /// <summary>
        /// The target number of milliseconds between drawing frames. 
        /// </summary>
        public const Int32 AnimationInterval = 33;

        /// <summary>
        /// The width of squares on the chessboard in pixels. 
        /// </summary>
        public const Int32 SquareWidth = 50;

        /// <summary>
        /// The width of the chessboard in pixels.
        /// </summary>
        public const Int32 Width = 8 * SquareWidth;

        /// <summary>
        /// Whether piece movements are animated. 
        /// </summary>
        public static Boolean Animations = true;

        /// <summary>
        /// Whether the chessboard is rotated when drawn. 
        /// </summary>
        public static Boolean Rotated = false;

        /// <summary>
        /// The collection of visual pieces for drawing. 
        /// </summary>
        private static List<VisualPiece> _pieces = new List<VisualPiece>(32);

        /// <summary>
        /// Initializes the collection of dark squares. 
        /// </summary>
        static VisualPosition() {
            for (Int32 file = 0; file < 8; file++)
                for (Int32 rank = 0; rank < 8; rank++)
                    if (((file + rank) & 1) > 0)
                        DarkSquares[file / 2 + rank * 4] = new Rectangle(file * SquareWidth, rank * SquareWidth, SquareWidth, SquareWidth);
        }

        /// <summary>
        /// Sets the visual position to draw to the given position. 
        /// </summary>
        /// <param name="position">The position to draw.</param>
        public static void Set(Position position) {
            lock (PiecesLock) {
                _pieces.Clear();
                for (Int32 square = 0; square < position.Square.Length; square++)
                    if (position.Square[square] != Piece.Empty)
                        _pieces.Add(new VisualPiece(position.Square[square], Position.File(square) * SquareWidth, Position.Rank(square) * SquareWidth));
            }
        }

        /// <summary>
        /// Makes the given move on the visual position. 
        /// </summary>
        /// <param name="move">The move to make.</param>
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

        /// <summary>
        /// Animates the given move between the given initial and final locations. 
        /// </summary>
        /// <param name="move">The move to animate.</param>
        /// <param name="initial">The initial location of the moving piece.</param>
        /// <param name="final">The final location of the moving piece.</param>
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

        /// <summary>
        /// Draws the dark squares of the chessboard. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public static void DrawDarkSquares(Graphics g) {
            g.FillRectangles(DarkBrush, DarkSquares);
        }

        /// <summary>
        /// Draws the given square using the given brush. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        /// <param name="brush">The brush for drawing the square.</param>
        /// <param name="square">The square to draw.</param>
        public static void DrawSquare(Graphics g, SolidBrush brush, Int32 square) {
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

        /// <summary>
        /// Draws the pieces on the chessboard. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public static void DrawPieces(Graphics g) {
            _pieces.ForEach(piece => {
                if (piece != null)
                    piece.Draw(g);
            });
        }
    }
}
