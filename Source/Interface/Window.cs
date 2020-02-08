using AbsoluteZero.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Represents the main GUI window. 
    /// </summary>
    partial class Window : Form {

        /// <summary>
        /// The height of the menu bar. 
        /// </summary>
        public const Int32 MenuHeight = 24;

        /// <summary>
        /// The target number of milliseconds between draw frames. 
        /// </summary>
        private const Int32 DrawInterval = 33;

        /// <summary>
        /// The game associated with the window. 
        /// </summary>
        private Game _game;

        /// <summary>
        /// Constructs a window. 
        /// </summary>
        public Window() 
            : this(null) { }

        /// <summary>
        /// Constructs a Window for the specified Game.
        /// </summary>
        /// <param name="game">The game to associate with the window.</param>
        public Window(Game game) {
            InitializeComponent();

            // Initialize properties and fields. 
            Icon = Resources.Icon;
            ClientSize = new Size(VisualPosition.Width, VisualPosition.Width + MenuHeight);
            _game = game;

            // Initialize event handlers. 
            MouseUp += (sender, e) => {
                if (_game != null)
                    _game.MouseUpHandler(e);
            };
            Paint += DrawHandler;

            // Close the application when the window is closed. 
            FormClosed += (sender, e) => {
                Application.Exit();
            };

            // Set the background colour to the light colour of the chessboard so we 
            // don't need to draw the light squares. 
            BackColor = VisualPosition.LightColor;

            // Start draw thread. 
            new Thread(new ThreadStart(() => {
                while (true) {
                    Invalidate();
                    Thread.Sleep(DrawInterval);
                }
            })) {
                IsBackground = true
            }.Start();

            UpdateMenu();
        }

        /// <summary>
        /// Draws the Window. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The paint event.</param>
        private void DrawHandler(Object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;

            // Translate down so the chessboard can be draw from (0, 0). 
            g.TranslateTransform(0, MenuHeight);

            if (_game != null)
                _game.Draw(g);
            else {
                VisualPosition.DrawDarkSquares(g);
                VisualPosition.DrawPieces(g);
            }
        }

        /// <summary>
        /// Updates which menu components are enabled or checked. 
        /// </summary>
        private void UpdateMenu() {
            Boolean gameIsNotNull = _game != null;

            // Update File menu.
            savePGNMenuItem.Enabled = gameIsNotNull;
            enterFENMenuItem.Enabled = gameIsNotNull;
            copyFENMenuItem.Enabled = gameIsNotNull;

            // Update Game menu.
            offerDrawMenuItem.Enabled = gameIsNotNull;
            restartMenuItem.Enabled = gameIsNotNull;
            undoMoveMenuItem.Enabled = gameIsNotNull;

            // Update Display menu.
            rotateBoardMenuItem.Checked = VisualPosition.Rotated;
            animationsMenuItem.Checked = VisualPosition.Animations;

            if (gameIsNotNull) {
                Boolean hasHuman = _game.White is Human || _game.Black is Human;
                Boolean hasEngine = _game.White is Engine || _game.Black is Engine;

                // Update File menu.
                saveOuputMenuItem.Enabled = hasEngine;

                // Update Game menu.
                offerDrawMenuItem.Enabled = hasHuman && hasEngine;
                undoMoveMenuItem.Enabled = hasHuman;

                // Update Engine menu.
                searchTimeMenuItem.Enabled = hasEngine;
                searchDepthMenuItem.Enabled = hasEngine;
                searchNodesMenuItem.Enabled = hasEngine;
                hashSizeMenuItem.Enabled = hasEngine;
                multiPVMenuItem.Enabled = hasEngine;

                // Update Display menu.
                arrowsMenuItem.Enabled = hasEngine;
                arrowsMenuItem.Checked = VisualPosition.DrawArrows && hasEngine;
            } else
                arrowsMenuItem.Checked = VisualPosition.DrawArrows;
        }

        /// <summary>
        /// Handles the Save PGN button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void SavePGNClick(Object sender, EventArgs e) {
            using (SaveFileDialog dialog = new SaveFileDialog()) {
                dialog.Title = "Save PGN";
                dialog.Filter = "PGN File|*.pgn|Text File|*.txt";
                if (dialog.ShowDialog() == DialogResult.OK)
                    _game.SavePGN(dialog.FileName);
            }
        }

        /// <summary>
        /// Handles the Save Output button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void SaveOutputClick(Object sender, EventArgs e) {
            using (SaveFileDialog dialog = new SaveFileDialog()) {
                dialog.Title = "Save Engine Output";
                dialog.Filter = "Text File|*.txt";
                if (dialog.ShowDialog() == DialogResult.OK)
                    Terminal.SaveText(dialog.FileName);
            }
        }

        /// <summary>
        /// Handles the Enter FEN button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void EnterFENClick(Object sender, EventArgs e) {
            if (_game != null) {
                String fen = InputBox.Show("Please enter the FEN string.");
                if (fen.Length > 0) {
                    _game.End();
                    _game.Reset();
                    _game.Start(fen);
                }
            }
        }

        /// <summary>
        /// Handles the Copy FEN button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void CopyFENClick(Object sender, EventArgs e) {
            Clipboard.SetText(_game.GetFEN());
        }

        /// <summary>
        /// Handles the Offer Draw button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void OfferDrawClick(Object sender, EventArgs e) {
            if (_game != null)
                _game.OfferDraw();
        }

        /// <summary>
        /// Handles the Restart button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void RestartClick(Object sender, EventArgs e) {
            if (_game != null) {
                _game.End();
                _game.Reset();
                _game.Start();
            }
        }

        /// <summary>
        /// Handles the Undo Move button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void UndoMoveClick(Object sender, EventArgs e) {
            if (_game != null)
                _game.UndoMove();
        }

        /// <summary>
        /// Handles the Search Time button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void SearchTimeClick(Object sender, EventArgs e) {
            while (true) {
                String input = InputBox.Show("Please specify the search time in milliseconds.", Restrictions.MoveTime.ToString());
                Int32 value;
                if (Int32.TryParse(input, out value) && value > 0) {
                    Restrictions.MoveTime = value;
                    break;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        /// <summary>
        /// Handles the Search Depth button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void SearchDepthClick(Object sender, EventArgs e) {
            while (true) {
                String input = InputBox.Show("Please specify the search depth.", Restrictions.Depth.ToString());
                Int32 value;
                if (Int32.TryParse(input, out value) && value > 0) {
                    Restrictions.Depth = value;
                    break;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        /// <summary>
        /// Handles the Search Nodes button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void SearchNodesClick(Object sender, EventArgs e) {
            while (true) {
                String input = InputBox.Show("Please specify the nodes limit.", Restrictions.Nodes.ToString());
                Int64 value;
                if (Int64.TryParse(input, out value) && value > 0) {
                    Restrictions.Nodes = value;
                    break;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        /// <summary>
        /// Handles the Hash Size button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void HashSizeClick(Object sender, EventArgs e) {
            while (true) {
                Engine engine = _game.White as Engine ?? _game.Black as Engine;
                String input = InputBox.Show("Please specify the hash size in megabytes.", engine.HashAllocation.ToString());
                Int32 value;
                if (Int32.TryParse(input, out value) && value > 0) {
                    if (_game.White is Engine)
                        (_game.White as Engine).HashAllocation = value;
                    if (_game.Black is Engine)
                        (_game.Black as Engine).HashAllocation = value;
                    return;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        /// <summary>
        /// Handles the Multi PV button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void MultiPVClick(Object sender, EventArgs e) {
            while (true) {
                String input = InputBox.Show("Please specify the number of principal variations.", Restrictions.PrincipalVariations.ToString());
                Int32 value;
                if (Int32.TryParse(input, out value) && value > 0) {
                    Restrictions.PrincipalVariations = value;
                    break;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        /// <summary>
        /// Handles the Rotate Board button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void RotateBoardClick(Object sender, EventArgs e) {
            VisualPosition.Rotated ^= true;
            UpdateMenu();
        }

        /// <summary>
        /// Handles the Animations button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void AnimationsClick(Object sender, EventArgs e) {
            VisualPosition.Animations ^= true;
            UpdateMenu();
        }

        /// <summary>
        /// Handles the Arrows button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void ArrowsClick(Object sender, EventArgs e) {
            VisualPosition.DrawArrows ^= true;
            UpdateMenu();
        }

        /// <summary>
        /// Handles the About button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void AboutClick(Object sender, EventArgs e) {
            MessageBox.Show("Absolute Zero is a chess engine written in C#, developed for fun and to learn about game tree searching. Its playing strength has been and will continue to steadily increase as more techniques are added to its arsenal. \n\nIt supports the UCI protocol when ran with command-line parameter \"-u\". While in UCI mode it also accepts commands such as \"perft\" and \"divide\". Type \"help\" to see the full list of commands. \n\nZONG ZHENG LI");
        }
    }
}
