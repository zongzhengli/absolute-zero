using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;

namespace AbsoluteZero {

    /// <summary>
    /// Represents the chess position in the visual interface. Currently, all the
    /// visual components are static, which isn't great, but acceptable due to
    /// the rotate state being tied to a global user option.
    /// </summary>
    public static class VisualPosition {

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
        /// The font for drawing arrow labels.
        /// </summary>
        private static readonly Font LabelFont = new Font("Verdana", 8, FontStyle.Bold);

        /// <summary>
        /// The format to use for drawing arrow labels.
        /// </summary>
        private static readonly StringFormat LabelFormat = new StringFormat() {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };

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
        public const Single AnimationEasing = 0.3F;

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
        /// The amount to pull labels orthogonally to the arrow.
        /// </summary>
        private const Int32 LabelPullMagnitude = 10;

        /// <summary>
        /// Whether piece movements are animated. 
        /// </summary>
        public static Boolean Animations = true;

        /// <summary>
        /// Whether the chessboard is rotated when drawn. 
        /// </summary>
        public static Boolean Rotated = false;

        /// <summary>
        /// Whether to draw lines.
        /// </summary>
        public static Boolean DrawArrows = false;

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
            switch (Move.Special(move) & Piece.Mask) {
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
        public static void DrawSquare(Graphics g, Brush brush, Int32 square) {
            Int32 x = RotateIfNeeded(Position.File(square)) * SquareWidth;
            Int32 y = RotateIfNeeded(Position.Rank(square)) * SquareWidth;
            g.FillRectangle(brush, x, y, SquareWidth, SquareWidth);
        }
        
        /// <summary>
        /// Draws an arrow for the given move with the given pen. Optionally draws 
        /// a label if given. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        /// <param name="pen">The pen for drawing the arrow.</param>
        /// <param name="move">The move to draw an arrow for.</param>
        /// <param name="labelBrush">The brush for drawing the label.</param>
        /// <param name="label">The label to draw at the move's from square.</param>
        public static void DrawArrow(Graphics g, Pen pen, Int32 move, Brush labelBrush = null, String label = "") {
            if (!DrawArrows)
                return;

            Int32 from = Move.From(move);
            Int32 to = Move.To(move);
            Point initial = new Point(
                RotateIfNeeded(Position.File(from)) * SquareWidth + SquareWidth / 2, 
                RotateIfNeeded(Position.Rank(from)) * SquareWidth + SquareWidth / 2);
            Point final = new Point(
                RotateIfNeeded(Position.File(to)) * SquareWidth + SquareWidth / 2, 
                RotateIfNeeded(Position.Rank(to)) * SquareWidth + SquareWidth / 2);

            pen.StartCap = LineCap.NoAnchor;
            pen.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(pen, initial, final);

            if (labelBrush != null && !String.IsNullOrEmpty(label)) {
                Point delta = new Point(final.X - initial.X, final.Y - initial.Y);
                Single mag = (Single)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
                Single factor = delta.Y > 0 ? LabelPullMagnitude : -LabelPullMagnitude;
                PointF pull = new PointF(factor * delta.Y / mag, factor * -delta.X / mag);
                PointF mid = new PointF((initial.X + final.X) / 2 + pull.X, (initial.Y + final.Y) / 2 + pull.Y);
                g.DrawString(label, LabelFont, labelBrush, mid, LabelFormat);

            }
        }

        /// <summary>
        /// Draws the pieces on the chessboard. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public static void DrawPieces(Graphics g) {
            lock (PiecesLock) {
                _pieces.ForEach(piece => {
                    if (piece != null)
                        piece.Draw(g);
                });
            }
        }

        /// <summary>
        /// Returns the square at the given point. 
        /// </summary>
        /// <param name="point">The point to determine the square of.</param>
        /// <returns>The square at the given point</returns>
        public static Int32 SquareAt(Point point) {
            Int32 file = point.X / SquareWidth;
            Int32 rank = (point.Y - Window.MenuHeight) / SquareWidth;
            if (Rotated)
                return 7 - file + (7 - rank) * 8;
            return file + rank * 8;
        }

        /// <summary>
        /// Returns the given rank or file, rotating if Rotated is set.
        /// </summary>
        /// <param name="rankOrFile">The rank or file to rotate.</param>
        private static Int32 RotateIfNeeded(Int32 rankOrFile) {
            return Rotated ? 7 - rankOrFile : rankOrFile;
        }
    }
}
