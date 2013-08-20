using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Defines a human player in the chess game. 
    /// </summary>
    class Human : IPlayer {

        /// <summary>
        /// The brush used to paint the piece selection background. 
        /// </summary>
        static readonly SolidBrush SelectionBrush = new SolidBrush(Color.White);

        /// <summary>
        /// The ManualResetEvent that blocks the running thread to wait for the 
        /// player to move. 
        /// </summary>
        private ManualResetEvent WaitForMove = new ManualResetEvent(false);

        /// <summary>
        /// Whether the player is current deciding on a move. 
        /// </summary>
        private Boolean IsMoving = false;

        /// <summary>
        /// The initial square for the player's move.
        /// </summary>
        private Int32 InitialSquare;

        /// <summary>
        /// The final square for the player's move. 
        /// </summary>
        private Int32 FinalSquare;

        /// <summary>
        /// The current position the player is moving on.
        /// </summary>
        private Position CurrentPosition;

        /// <summary>
        /// Returns the player's move for the given position. 
        /// </summary>
        /// <param name="position">The position to make a move on.</param>
        /// <returns>The player's move.</returns>
        public Int32 GetMove(Position position) {
            Reset();
            CurrentPosition = position;
            IsMoving = true;
            WaitForMove.WaitOne();
            IsMoving = false;
            return CreateMove(position, InitialSquare, FinalSquare);
        }

        /// <summary>
        /// Return whether the player is willing to accept a draw offer. 
        /// </summary>
        /// <returns></returns>
        public Boolean AcceptDraw() {
            return false;
        }
        
        /// <summary>
        /// Stops the wait for the player's move if applicable. 
        /// </summary>
        public void Stop() {
            Reset();
        }

        /// <summary>
        /// Resets the player's fields. 
        /// </summary>
        public void Reset() {
            InitialSquare = Position.InvalidSquare;
            FinalSquare = Position.InvalidSquare;
            IsMoving = false;
            WaitForMove.Reset();
        }

        /// <summary>
        /// Returns the name of the player. 
        /// </summary>
        /// <returns></returns>
        public String GetName() {
            return "Human";
        }

        /// <summary>
        /// Returns the move specified by the given information.
        /// </summary>
        /// <param name="position">The position the move is to be played on.</param>
        /// <param name="from">The initial square of the move.</param>
        /// <param name="to">The final square of the move.</param>
        /// <returns>The move specified by the given information.</returns>
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

        /// <summary>
        /// Handles a mouse up event.
        /// </summary>
        /// <param name="e">The mouse event.</param>
        public void MouseUpEvent(MouseEventArgs e) {
            if (!IsMoving)
                return;
            Int32 square = Position.SquareAt(e.Location);
            if (CurrentPosition.Square[square] != Piece.Empty && (CurrentPosition.Square[square] & Piece.Colour) == CurrentPosition.SideToMove) {
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

        /// <summary>
        /// Draws the player's graphical elements. 
        /// </summary>
        /// <param name="g">The drawing surface.</param>
        public void Draw(Graphics g) {
            if (IsMoving && InitialSquare != Position.InvalidSquare)
                VisualPosition.FillSquare(g, SelectionBrush, InitialSquare);
        }
    }
}
