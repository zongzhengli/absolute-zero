using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {
    class Human : IPlayer {
        static readonly SolidBrush SelectionBrush = new SolidBrush(Color.White);

        private ManualResetEvent waitForMove = new ManualResetEvent(false);
        private Boolean moving = false;
        private Int32 initial;
        private Int32 final;
        private Position currentBoard;
        private Int32 currentColour;

        public Int32 GetMove(Position position) {
            currentBoard = position;
            currentColour = position.Colour;
            moving = true;
            initial = final = Position.InvalidSquare;
            waitForMove.WaitOne();
            waitForMove.Reset();
            moving = false;
            return Create(position, initial, final);
        }

        public Boolean AcceptDraw() {
            return false;
        }

        public void Stop() {
            Reset();
        }

        public void Reset() {
            moving = false;
            waitForMove.Reset();
        }

        public String GetName() {
            return "Human";
        }

        private Int32 Create(Position position, Int32 from, Int32 to) {
            foreach (Int32 move in position.LegalMoves())
                if (from == Move.GetFrom(move) && to == Move.GetTo(move)) {
                    Int32 special = Move.GetSpecial(move);
                    if (Move.IsPromotion(move))
                        switch (SelectionBox.Show("What piece would you like to promote to?", "Queen", "Rook", "Bishop", "Knight")) {
                            case "Queen":
                                special = position.Colour | Piece.Queen;
                                break;
                            case "Rook":
                                special = position.Colour | Piece.Rook;
                                break;
                            case "Bishop":
                                special = position.Colour | Piece.Bishop;
                                break;
                            case "Knight":
                                special = position.Colour | Piece.Knight;
                                break;
                        }
                    return Move.Create(position, from, to, special);
                }
            return Move.Invalid;
        }

        public void MouseUpEvent(MouseEventArgs e) {
            if (!moving)
                return;
            Int32 square = Position.Square(e.Location);
            if (currentBoard.Element[square] != Piece.Empty && (currentBoard.Element[square] & Piece.Colour) == currentColour) {
                if (initial == square)
                    initial = Position.InvalidSquare;
                else {
                    final = Position.InvalidSquare;
                    initial = square;
                }
            } else {
                final = square;
                waitForMove.Set();
            }
        }

        public void Draw(Graphics g) {
            if (moving && initial != Position.InvalidSquare)
                VisualPosition.FillSquare(g, SelectionBrush, initial);
        }
    }
}
