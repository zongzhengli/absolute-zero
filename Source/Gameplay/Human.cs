using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Represents a human player in the chess game. 
    /// </summary>
    public sealed class Human : IPlayer {

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
        /// Whether the player is to stop deciding on a move. 
        /// </summary>
        private Boolean _stop = false;

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
        /// The name of the player. 
        /// </summary>
        public String Name {
            get {
                return "Human";
            }
        }

        /// <summary>
        /// Whether the player is willing to accept a draw offer. 
        /// </summary>
        public Boolean AcceptsDraw {
            get {
                return false;
            }
        }

        /// <summary>
        /// Returns the player's move for the given position. 
        /// </summary>
        /// <param name="position">The position to make a move on.</param>
        /// <returns>The player's move.</returns>
        public Int32 GetMove(Position position) {
            Reset();
            _currentPosition = position;
            _stop = false;
            _isMoving = true;

            List<Int32> moves = position.LegalMoves();
            Int32 move;
            do {
                _waitForMove.WaitOne();
                move = CreateMove(position, _initialSquare, _finalSquare);

                _initialSquare = Position.InvalidSquare;
                _finalSquare = Position.InvalidSquare;
                _waitForMove.Reset();
            } while (!_stop && !moves.Contains(move));

            _isMoving = false;
            return move;
        }

        /// <summary>
        /// Stops the player's move if applicable. 
        /// </summary>
        public void Stop() {
            _waitForMove.Set();
            Reset();
            _stop = true;
        }

        /// <summary>
        /// Resets the player. 
        /// </summary>
        public void Reset() {
            _initialSquare = Position.InvalidSquare;
            _finalSquare = Position.InvalidSquare;
            _isMoving = false;
            _stop = false;
            _waitForMove.Reset();
        }

        /// <summary>
        /// Handles a mouse up event.
        /// </summary>
        /// <param name="e">The mouse event.</param>
        public void MouseUpHandler(MouseEventArgs e) {
            if (_isMoving) {
                Int32 square = VisualPosition.SquareAt(e.Location);
                Int32 piece = _currentPosition.Square[square];

                if (piece != Piece.Empty && (piece & Colour.Mask) == _currentPosition.SideToMove)
                    _initialSquare = (_initialSquare == square) ? Position.InvalidSquare : square;
                else {
                    _finalSquare = square;
                    _waitForMove.Set();
                }
            }
        }

        /// <summary>
        /// Draws the player's graphical elements. 
        /// </summary>
        /// <param name="g">The drawing surface.</param>
        public void Draw(Graphics g) {
            if (_isMoving && _initialSquare != Position.InvalidSquare)
                VisualPosition.DrawSquare(g, SelectionBrush, _initialSquare);
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
    }
}
