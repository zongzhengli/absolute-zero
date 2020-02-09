using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using TMove = AbsoluteZero.Move;

namespace AbsoluteZero {

    /// <summary>
    /// Represents an input dialog box. 
    /// </summary>
    partial class AnalysisBox : Form {

        /// <summary>
        /// The target number of milliseconds between draw frames. 
        /// </summary>
        private const Int32 DrawInterval = 33;

        /// <summary>
        /// The Unicode string for the X.
        /// </summary>
        private const String CrossString = "\u2715";

        /// <summary>
        /// The Unicode string for the curved arrow.
        /// </summary>
        private const String ArrowString = "\u293A";

        /// <summary>
        /// Special value used in place of a piece to denote curved arrow selection.
        /// </summary>
        private const Int32 ArrowCode = -1;

        /// <summary>
        /// The font for drawing the X. 
        /// </summary>
        private static readonly Font CrossFont = new Font("Tahoma", 20);

        /// <summary>
        /// The font for drawing the curved arrow. 
        /// </summary>
        private static readonly Font ArrowFont = new Font("Tahoma", 30);

        /// <summary>
        /// The brush for drawing pieces that can be selected. 
        /// </summary>
        private static readonly Brush EnabledPieceBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// The brush for drawing pieces that can't be selected. 
        /// </summary>
        private static readonly Brush DisabledPieceBrush = new SolidBrush(Color.Gray);

        /// <summary>
        /// The brush used to paint the piece selection background. 
        /// </summary>
        private static readonly SolidBrush SelectionBrush = new SolidBrush(Color.White);

        /// <summary>
        /// The brush used to paint the piece selection background in the main window. 
        /// </summary>
        private static readonly SolidBrush WindowSelectionBrush = new SolidBrush(Color.White);

        /// <summary>
        /// Mapping from piece to location and area for that piece used to
        /// implement piece selection in the control panel. The location of
        /// the rectangle is where the piece will be drawn. the area of the
        /// rectangle is the mouse hitbox and selection background.
        /// </summary>
        private readonly Dictionary<Int32, Rectangle> SelectionPieces = new Dictionary<Int32, Rectangle> {
            { ArrowCode, new Rectangle(10, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Piece.Empty, new Rectangle(10, 60, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.White | Piece.King, new Rectangle(60, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.White | Piece.Queen, new Rectangle(110, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.White | Piece.Rook, new Rectangle(160, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.White | Piece.Bishop, new Rectangle(210, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.White | Piece.Knight, new Rectangle(260, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.White | Piece.Pawn, new Rectangle(310, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.Black | Piece.King, new Rectangle(60, 60, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.Black | Piece.Queen, new Rectangle(110, 60, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.Black | Piece.Rook, new Rectangle(160, 60, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.Black | Piece.Bishop, new Rectangle(210, 60, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.Black | Piece.Knight, new Rectangle(260, 60, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
            { Colour.Black | Piece.Pawn, new Rectangle(310, 60, VisualPosition.SquareWidth, VisualPosition.SquareWidth) }
        };

        /// <summary>
        /// The Zobrist key of the starting position.
        /// </summary>
        private readonly UInt64 StartingPositionKey;

        /// <summary>
        /// The position to search.
        /// </summary>
        private Position _position = Position.Create(Position.StartingFEN);

        /// <summary>
        /// The engine used to search the position.
        /// </summary>
        private Engine _engine = new Engine();

        /// <summary>
        /// The principal variation from the most recent search.
        /// </summary>
        private List<Int32> _pv = new List<Int32>();

        /// <summary>
        /// The index in the principal variation to play at.
        /// </summary>
        private Int32 _pvIndex = 0;

        /// <summary>
        /// The selected piece in the control panel.
        /// </summary>
        private Int32 _selectedPiece = ArrowCode;

        /// <summary>
        /// The selected square to move in the control panel.
        /// </summary>
        private Int32 _selectedSquare = Position.InvalidSquare;

        /// <summary>
        /// Whether the engine is currently searching the position.
        /// </summary>
        private Boolean _isSearching = false;

        /// <summary>
        /// Constructs an AnalysisBox. 
        /// </summary>
        public AnalysisBox() {
            InitializeComponent();

            // Initialize event handlers. 
            MouseUp += MouseUpHandler;
            Paint += DrawHandler;

            // Close the application when the window is closed. 
            FormClosed += (sender, e) => {
                Application.Exit();
            };

            BackColor = VisualPosition.LightColor;
            StartingPositionKey = _position.ZobristKey;
            VisualPosition.Set(_position);
            Restrictions.PrincipalVariations = 16;

            // Start draw thread. 
            new Thread(new ThreadStart(() => {
                while (true) {
                    Invalidate();
                    Thread.Sleep(DrawInterval);
                }
            })) {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Draws the Analysis control panel. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The paint event.</param>
        private void DrawHandler(Object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;

            Brush brush = _isSearching ? DisabledPieceBrush : EnabledPieceBrush;

            foreach (KeyValuePair<Int32, Rectangle> pair in SelectionPieces) {
                if (pair.Key == _selectedPiece)
                    g.FillRectangle(SelectionBrush, pair.Value);

                switch (pair.Key) {
                    default:
                        VisualPiece.DrawAt(g, pair.Key, pair.Value.Location, brush);
                        break;
                    case Piece.Empty:
                        g.DrawString(CrossString, CrossFont, brush, 17, 69);
                        break;
                    case ArrowCode:
                        g.DrawString(ArrowString, ArrowFont, brush, 11, 7);
                        break;
                }
            }
        }

        /// <summary>
        /// Draw the main window. 
        /// </summary>
        /// <param name="g">The drawing surface.</param>
        public void DrawWindow(Graphics g) {
            VisualPosition.DrawDarkSquares(g);
            if (_selectedSquare != Position.InvalidSquare)
                VisualPosition.DrawSquare(g, WindowSelectionBrush, _selectedSquare);
            VisualPosition.DrawPieces(g);
        }

        /// <summary>
        /// Handles a mouse up event. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        public void MouseUpHandler(Object sender, MouseEventArgs e) {
            if (!_isSearching) {
                foreach (KeyValuePair<Int32, Rectangle> pair in SelectionPieces)
                    if (pair.Value.Contains(e.Location))
                        _selectedPiece = pair.Key;
            }
        }

        /// <summary>
        /// Handles a mouse up event in the main window. The selected piece
        /// in the analysis control panel is set at the mouse position on the
        /// board. This is an unusual thing to do, so this method has to
        /// handle the nuances of position state itself.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        public void WindowMouseUpHandler(MouseEventArgs e) {
            if (_isSearching) {
                return;
            }
            Int32 square = VisualPosition.SquareAt(e.Location);

            if (_selectedPiece == ArrowCode) {
                if (_selectedSquare == Position.InvalidSquare) {
                    if (_position.Square[square] != Piece.Empty)
                        _selectedSquare = square;
                } else if (_selectedSquare == square) {
                    _selectedSquare = Position.InvalidSquare;
                } else {
                    Int32 initialSquare = _selectedSquare;
                    SetPieceAt(_position.Square[initialSquare], square);
                    SetPieceAt(Piece.Empty, initialSquare);
                    _selectedSquare = Position.InvalidSquare;
                    UpdatePosition(_position);
                    VisualPosition.Make(TMove.Create(_position, initialSquare, square));
                }
            } else {
                SetPieceAt(_selectedPiece, square);
                UpdatePosition(_position);
                VisualPosition.Set(_position);
            }
        }

        private void SetPieceAt(Int32 piece, Int32 square) {
            Int32 previousPiece = _position.Square[square];
            Int32 colour = piece & Colour.Mask;

            for (Int32 p = Colour.White; p <= Piece.Max; p++)
                _position.Bitboard[p] &= ~(1UL << square);

            if (piece == Piece.Empty) {
                _position.Square[square] = Piece.Empty;
                _position.Bitboard[piece] &= ~(1UL << square);
                _position.OccupiedBitboard &= ~(1UL << square);
            } else {
                _position.Square[square] = piece;
                _position.Bitboard[colour] |= 1UL << square;
                _position.Bitboard[piece] |= 1UL << square;
                _position.OccupiedBitboard |= 1UL << square;
            }

            if ((previousPiece & Piece.Mask) != Piece.King)
                _position.Material[previousPiece & Colour.Mask] -= Engine.PieceValue[previousPiece];
            if ((piece & Piece.Mask) != Piece.King)
                _position.Material[colour] += Engine.PieceValue[piece];

            _position.ZobristKey = _position.GetZobristKey();
            _position.ZobristKeyHistory[_position.HalfMoves] = _position.ZobristKey;

            if (square == 0 || square == 4) {
                _position.CastleQueenside[Colour.Black] =
                    _position.Square[0] == (Colour.Black | Piece.Rook) &&
                    _position.Square[4] == (Colour.Black | Piece.King) ? 1 : 0;
            }
            if (square == 4 || square == 7) {
                _position.CastleKingside[Colour.Black] =
                    _position.Square[4] == (Colour.Black | Piece.King) &&
                    _position.Square[7] == (Colour.Black | Piece.Rook) ? 1 : 0;
            }
            if (square == 56 || square == 60) {
                _position.CastleQueenside[Colour.White] =
                    _position.Square[56] == (Colour.White | Piece.Rook) &&
                    _position.Square[60] == (Colour.White | Piece.King) ? 1 : 0;
            }
            if (square == 60 || square == 63) {
                _position.CastleKingside[Colour.White] =
                    _position.Square[60] == (Colour.White | Piece.King) &&
                    _position.Square[63] == (Colour.White | Piece.Rook) ? 1 : 0;
            }
        }

        /// <summary>
        /// Handles changes to the FEN text box. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void FenTextBoxChanged(Object sender, EventArgs e) {
            if (fenTextBox.Focused) {
                Position position = Position.Create(fenTextBox.Text);
                if (position != null) {
                    _position = position;
                    UpdateGuiStateWithoutFenTextBox();
                    VisualPosition.Set(_position);
                }
            }
        }

        /// <summary>
        /// Handles the White radio button check. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void WhiteRadioChecked(Object sender, EventArgs e) {
            if (whiteRadio.Checked) {
                _position.SideToMove = Colour.White;
                UpdateGuiState();
            }
        }

        /// <summary>
        /// Handles the Black radio button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void BlackRadioChecked(Object sender, EventArgs e) {
            if (blackRadio.Checked) {
                _position.SideToMove = Colour.Black;
                UpdateGuiState();
            }
        }

        /// <summary>
        /// Handles changes to the track bar. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void TrackBarScroll(Object sender, EventArgs e) {
            Restrictions.PrincipalVariations = 1 << trackBar.Value;
            pvsTextBox.Text = Restrictions.PrincipalVariations.ToString();
        }

        /// <summary>
        /// Handles the Search button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void SearchClick(Object sender, EventArgs e) {
            if (_isSearching) {
                _engine.Stop();
                return;
            }
            _isSearching = true;
            _pv.Clear();
            _pvIndex = 0;
            _selectedSquare = Position.InvalidSquare;
            UpdateGuiState();
            Position positionClone = _position.DeepClone();

            new Thread(new ThreadStart(() => {
                _engine.GetMove(positionClone);
                _isSearching = false;
                if (positionClone.ZobristKey == _position.ZobristKey)
                    _pv = _engine.PrincipalVariation;
                UpdateGuiState();
            })) {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Handles the previous button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void PreviousClick(object sender, EventArgs e) {
            Int32 move = _pv[--_pvIndex];
            _position.Unmake(move);
            UpdateGuiState();
            VisualPosition.Unmake(move);
        }

        /// <summary>
        /// Handles the next button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void NextClick(Object sender, EventArgs e) {
            Int32 move = _pv[_pvIndex++];
            _position.Make(move);
            UpdateGuiState();
            VisualPosition.Make(move);
        }

        /// <summary>
        /// Handles the Reset Board button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void ResetBoardClick(Object sender, EventArgs e) {
            UpdatePosition(Position.Create(Position.StartingFEN));
            VisualPosition.Set(_position);
        }

        /// <summary>
        /// Handles the Clear Board button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void ClearBoardClick(Object sender, EventArgs e) {
            UpdatePosition(Position.Create("/ w"));
            VisualPosition.Set(_position);
        }

        /// <summary>
        /// Updates the state of GUI elements.
        /// </summary>
        private void UpdateGuiState() {
            UpdateGuiStateWithoutFenTextBox();
            fenTextBox.Invoke(new MethodInvoker(delegate {
                fenTextBox.Enabled = !_isSearching;
                fenTextBox.Text = _position.GetFEN();
            }));
        }

        /// <summary>
        /// Updates the state of GUI elements except for the FEN text box
        /// to avoid listener loop issues. 
        /// </summary>
        private void UpdateGuiStateWithoutFenTextBox() {
            searchButton.Invoke(new MethodInvoker(delegate {
                searchButton.Text = _isSearching ? "Stop" : "Search";
                searchButton.Enabled = _isSearching ||
                    (Bit.Count(_position.Bitboard[Colour.White | Piece.King]) == 1 &&
                    Bit.Count(_position.Bitboard[Colour.Black | Piece.King]) == 1 &&
                    !_position.InCheck(1 - _position.SideToMove) &&
                    _position.LegalMoves().Count > 0);
            }));
            whiteRadio.Invoke(new MethodInvoker(delegate {
                whiteRadio.Enabled = !_isSearching;
                whiteRadio.Checked = _position.SideToMove == Colour.White;
            }));
            blackRadio.Invoke(new MethodInvoker(delegate {
                blackRadio.Enabled = !_isSearching;
                blackRadio.Checked = _position.SideToMove == Colour.Black;
            }));
            trackBar.Invoke(new MethodInvoker(delegate {
                trackBar.Enabled = !_isSearching;
            }));
            previousButton.Invoke(new MethodInvoker(delegate {
                previousButton.Enabled = _pvIndex > 0;
            }));
            nextButton.Invoke(new MethodInvoker(delegate {
                nextButton.Enabled = _pvIndex < _pv.Count;
            }));
            resetBoardButton.Invoke(new MethodInvoker(delegate {
                resetBoardButton.Enabled =
                    !_isSearching && _position.ZobristKey != StartingPositionKey;
            }));
            clearBoardButton.Invoke(new MethodInvoker(delegate {
                clearBoardButton.Enabled =
                    !_isSearching && _position.OccupiedBitboard != 0;
            }));
        }

        /// <summary>
        /// Updates the position, which in term updates various UI elements.
        /// </summary>
        /// <param name="position">The position to update to.</param>
        private void UpdatePosition(Position position) {
            _position = position;
            _pv.Clear();
            _pvIndex = 0;
            UpdateGuiState();
        }
    }
}