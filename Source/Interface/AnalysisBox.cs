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
        /// A reference to an instance of this class if it is active.
        /// </summary>
        public static AnalysisBox Instance {
            get;
            private set;
        }

        /// <summary>
        /// The target number of milliseconds between draw frames. 
        /// </summary>
        private const Int32 DrawInterval = 33;

        /// <summary>
        /// The Unicode string for the X.
        /// </summary>
        private const String CrossString = "\u2715";

        /// <summary>
        /// The font for drawing the X. 
        /// </summary>
        private static readonly Font CrossFont = new Font("Tahoma", 20);

        /// <summary>
        /// The brush for drawing pieces. 
        /// </summary>
        private static readonly Brush PieceBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// The brush used to paint the piece selection background. 
        /// </summary>
        private static readonly SolidBrush SelectionBrush = new SolidBrush(Color.White);

        private readonly Dictionary<Int32, Rectangle> SelectionPieces = new Dictionary<Int32, Rectangle> {
            { Piece.Empty, new Rectangle(10, 10, VisualPosition.SquareWidth, VisualPosition.SquareWidth) },
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

        private Position _position = Position.Create(Position.StartingFEN);

        private Engine _engine = new Engine();

        private Int32 _pvMove = TMove.Invalid;

        private Int32 _selectedPiece = Piece.Empty;

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
            Instance = this;
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

            foreach (KeyValuePair<Int32, Rectangle> pair in SelectionPieces) {
                if (pair.Key == _selectedPiece)
                    g.FillRectangle(SelectionBrush, pair.Value);
                if (pair.Key == Piece.Empty)
                    g.DrawString(CrossString, CrossFont, PieceBrush, 17, 18);
                else
                    VisualPiece.DrawAt(g, pair.Key, pair.Value.Location, PieceBrush);
            }
        }

        /// <summary>
        /// Handles a mouse up event. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        public void MouseUpHandler(Object sender, MouseEventArgs e) {
            foreach (KeyValuePair<Int32, Rectangle> pair in SelectionPieces)
                if (pair.Value.Contains(e.Location))
                    _selectedPiece = pair.Key;
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
            Int32 square = VisualPosition.SquareAt(e.Location);
            Int32 previousPiece = _position.Square[square];
            Int32 colour = _selectedPiece & Colour.Mask;

            for (Int32 piece = Colour.White; piece <= Piece.Max; piece++)
                _position.Bitboard[piece] &= ~(1UL << square);

            if (_selectedPiece == Piece.Empty) {
                _position.Square[square] = Piece.Empty;
                _position.Bitboard[_selectedPiece] &= ~(1UL << square);
                _position.OccupiedBitboard &= ~(1UL << square);
            } else {
                _position.Square[square] = _selectedPiece;
                _position.Bitboard[colour] |= 1UL << square;
                _position.Bitboard[_selectedPiece] |= 1UL << square;
                _position.OccupiedBitboard |= 1UL << square;
            }

            if ((previousPiece & Piece.Mask) != Piece.King)
                _position.Material[previousPiece & Colour.Mask] -= Engine.PieceValue[previousPiece];
            if ((_selectedPiece & Piece.Mask) != Piece.King)
                _position.Material[colour] += Engine.PieceValue[_selectedPiece];
            
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

            UpdatePosition(_position);
            VisualPosition.Set(_position);
        }

        /// <summary>
        /// Handles changes to the FEN text box. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void FenTextBoxChanged(Object sender, EventArgs e) {
            Position position = Position.Create(fenTextBox.Text);
            if (position != null) {
                _position = position;
                UpdateSearchButton();
                VisualPosition.Set(_position);
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
                UpdateFenTextBox();
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
                UpdateFenTextBox();
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
            _pvMove = TMove.Invalid;
            UpdateSearchButton();
            UpdatePlayPVButton();
            Position positionClone = _position.DeepClone();

            new Thread(new ThreadStart(() => {
                Int32 move = _engine.GetMove(positionClone);
                _isSearching = false;
                if (positionClone.ZobristKey == _position.ZobristKey)
                    _pvMove = move;
                UpdateSearchButton();
                UpdatePlayPVButton();
            })) {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Handles the play PV button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void PlayPVClick(Object sender, EventArgs e) {
            Int32 move = _pvMove;
            if (move != TMove.Invalid) {
                _position.Make(move);
                UpdatePosition(_position);
                VisualPosition.Make(move);
            }
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
        /// Updates the Search button state.
        /// </summary>
        private void UpdateSearchButton() {
            searchButton.Invoke(new MethodInvoker(delegate {
                searchButton.Text = _isSearching ? "Stop" : "Search";
                searchButton.Enabled = _isSearching ||
                    (Bit.Count(_position.Bitboard[Colour.White | Piece.King]) == 1 &&
                    Bit.Count(_position.Bitboard[Colour.Black | Piece.King]) == 1 &&
                    !_position.InCheck(1 - _position.SideToMove) &&
                    _position.LegalMoves().Count > 0);
            }));
        }

        /// <summary>
        /// Updates the FEN text box state.
        /// </summary>
        private void UpdateFenTextBox() {
            fenTextBox.Invoke(new MethodInvoker(delegate {
                fenTextBox.TextChanged -= FenTextBoxChanged;
                fenTextBox.Text = _position.GetFEN();
                fenTextBox.TextChanged += FenTextBoxChanged;
            }));
        }

        /// <summary>
        /// Updates the White and Black radio button states.
        /// </summary>
        private void UpdateColourRadioButtons() {
            whiteRadio.Invoke(new MethodInvoker(delegate {
                whiteRadio.Checked = _position.SideToMove == Colour.White;
            }));
            blackRadio.Invoke(new MethodInvoker(delegate {
                blackRadio.Checked = _position.SideToMove == Colour.Black;
            }));
        }

        /// <summary>
        /// Updates the Play PV button state.
        /// </summary>
        private void UpdatePlayPVButton() {
            playPVButton.Invoke(new MethodInvoker(delegate {
                playPVButton.Enabled = _pvMove != TMove.Invalid;
            }));
        }

        /// <summary>
        /// Updates the position, which in term updates various UI elements.
        /// </summary>
        /// <param name="position">The position to update to.</param>
        private void UpdatePosition(Position position) {
            _position = position;
            _pvMove = TMove.Invalid;
            UpdateSearchButton();
            UpdateFenTextBox();
            UpdatePlayPVButton();
            UpdateColourRadioButtons();
        }
    }
}