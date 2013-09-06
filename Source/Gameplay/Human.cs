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
        private static readonly SolidBrush SelectionBrush = new SolidBrush(Color.White);

        /// <summary>
        /// The ManualResetEvent that blocks the running thread to wait for the 
        /// player to move. 
        /// </summary>
        private ManualResetEvent _waitForMove = new ManualResetEvent(false);

        /// <summary>
        /// Whether the player is current deciding on a move. 
        /// </summary>
        private Boolean _isMoving = false;

        /// <summary>
        /// The initial square for the player's move.
        /// </summary>
        private Int32 _initialSquare;

        /// <summary>
        /// The final square for the player's move. 
        /// </summary>
        private Int32 _finalSquare;

        /// <summary>
        /// The current position the player is moving on.
        /// </summary>
        private Position _currentPosition;

        /// <summary>
        /// Returns the player's move for the given position. 
        /// </summary>
        /// <param name="position">The position to make a move on.</param>
        /// <returns>The player's move.</returns>
        public Int32 GetMove(Position position) {
            Reset();
            _currentPosition = position;
            _isMoving = true;
            _waitForMove.WaitOne();
            _isMoving = false;
            return CreateMove(position, _initialSquare, _finalSquare);
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
            _initialSquare = Position.InvalidSquare;
            _finalSquare = Position.InvalidSquare;
            _isMoving = false;
            _waitForMove.Reset();
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
            if (!_isMoving)
                return;
            Int32 square = Position.SquareAt(e.Location);
            if (_currentPosition.Square[square] != Piece.Empty && (_currentPosition.Square[square] & Piece.Colour) == _currentPosition.SideToMove) {
                if (_initialSquare == square)
                    _initialSquare = Position.InvalidSquare;
                else {
                    _finalSquare = Position.InvalidSquare;
                    _initialSquare = square;
                }
            } else {
                _finalSquare = square;
                _waitForMove.Set();
            }
        }

        /// <summary>
        /// Draws the player's graphical elements. 
        /// </summary>
        /// <param name="g">The drawing surface.</param>
        public void Draw(Graphics g) {
            if (_isMoving && _initialSquare != Position.InvalidSquare)
                VisualPosition.FillSquare(g, SelectionBrush, _initialSquare);
        }
    }
}
