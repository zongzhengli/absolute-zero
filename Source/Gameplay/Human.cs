using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {
    class Human : IPlayer {
        static readonly SolidBrush SelectionBrush = new SolidBrush(Color.White);

        private ManualResetEvent WaitForMove = new ManualResetEvent(false);
        private Boolean IsMoving = false;
        private Int32 InitialSquare;
        private Int32 FinalSquare;
        private Position CurrentPosition;
        private Int32 currentColour;

        public Int32 GetMove(Position position) {
            CurrentPosition = position;
            currentColour = position.SideToMove;
            IsMoving = true;
            InitialSquare = FinalSquare = Position.InvalidSquare;
            WaitForMove.WaitOne();
            WaitForMove.Reset();
            IsMoving = false;
            return CreateMove(position, InitialSquare, FinalSquare);
        }

        public Boolean AcceptDraw() {
            return false;
        }

        public void Stop() {
            Reset();
        }

        public void Reset() {
            IsMoving = false;
            WaitForMove.Reset();
        }

        public String GetName() {
            return "Human";
        }

        private Int32 CreateMove(Position position, Int32 from, Int32 to) {
            foreach (Int32 move in position.LegalMoves())
                if (from == Move.From(move) && to == Move.To(move)) {
                    Int32 special = Move.Special(move);
                    if (Move.IsPromotion(move))
                        switch (SelectionBox.Show("What piece would you like to promote to?", "Queen", "Rook", "Bishop", "Knight")) {
                            case "Queen":
                                special = position.SideToMove | Piece.Queen;
                                break;
                            case "Rook":
                                special = position.SideToMove | Piece.Rook;
                                break;
                            case "Bishop":
                                special = position.SideToMove | Piece.Bishop;
                                break;
                            case "Knight":
                                special = position.SideToMove | Piece.Knight;
                                break;
                        }
                    return Move.Create(position, from, to, special);
                }
            return Move.Invalid;
        }

        public void MouseUpEvent(MouseEventArgs e) {
            if (!IsMoving)
                return;
            Int32 square = Position.SquareAt(e.Location);
            if (CurrentPosition.Square[square] != Piece.Empty && (CurrentPosition.Square[square] & Piece.Colour) == currentColour) {
                if (InitialSquare == square)
                    InitialSquare = Position.InvalidSquare;
                else {
                    FinalSquare = Position.InvalidSquare;
                    InitialSquare = square;
                }
            } else {
                FinalSquare = square;
                WaitForMove.Set();
            }
        }

        public void Draw(Graphics g) {
            if (IsMoving && InitialSquare != Position.InvalidSquare)
                VisualPosition.FillSquare(g, SelectionBrush, InitialSquare);
        }
    }
}
