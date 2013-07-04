using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// The code component of the Form for the GUI interface. 
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
        /// The Game associated and handled by the GUI. 
        /// </summary>
        private Game game;

        public Window() {
            InitializeComponent();
            Icon = Properties.Resources.Icon;
            ClientSize = new Size(VisualPosition.Width, VisualPosition.Width + MenuHeight);
            MouseUp += new MouseEventHandler(MouseUpEvent);
            Paint += new PaintEventHandler(DrawEvent);
            FormClosed += delegate {
                Application.Exit();
            };

            // Set the background colour to the light colour of the chessboard so we 
            // don't need to draw the light squares. 
            BackColor = VisualPosition.LightColor;

            // Start draw thread. 
            new Thread(new ThreadStart(DrawThread)) {
                IsBackground = true
            }.Start();

            // Update menu state. 
            UpdateMenu();
        }

        /// <summary>
        /// Constructs a Window for the specified Game.
        /// </summary>
        /// <param name="parameter"></param>
        public Window(Game parameter)
            : this() {
            game = parameter;
            UpdateMenu();
        }

        private void DrawThread() {
            while (true) {
                Invalidate();
                Thread.Sleep(DrawInterval);
            }
        }

        private void DrawEvent(Object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.TranslateTransform(0, MenuHeight);
            if (game != null)
                game.Draw(g);
            else {
                VisualPosition.FillDarkSquares(g);
                VisualPosition.DrawPieces(g);
            }
        }

        private void MouseUpEvent(Object sender, MouseEventArgs e) {
            if (game != null)
                game.MouseUpEvent(e);
        }

        private void UpdateMenu() {
            Boolean gameIsNotNull = game != null;

            // File menu.
            savePGNMenuItem.Enabled = gameIsNotNull;
            enterFENMenuItem.Enabled = gameIsNotNull;
            copyFENMenuItem.Enabled = gameIsNotNull;

            // Game menu.
            offerDrawMenuItem.Enabled = gameIsNotNull;
            restartMenuItem.Enabled = gameIsNotNull;
            undoMoveMenuItem.Enabled = gameIsNotNull;

            // Display menu.
            rotateBoardMenuItem.Checked = VisualPosition.Rotated;
            animationsMenuItem.Checked = VisualPosition.Animations;

            if (gameIsNotNull) {
                Boolean hasHuman = game.White is Human || game.Black is Human;
                Boolean hasEngine = game.White is IEngine || game.Black is IEngine;

                // File menu.
                saveOuputMenuItem.Enabled = hasEngine;

                // Game menu.
                offerDrawMenuItem.Enabled = hasHuman && hasEngine;
                undoMoveMenuItem.Enabled = hasHuman;

                // Engine menu.
                searchMenuItem.Enabled = hasEngine;
                hashSizeMenuItem.Enabled = hasEngine;
            }
        }

        private void SavePGNClick(Object sender, EventArgs e) {
            using (SaveFileDialog dialog = new SaveFileDialog()) {
                dialog.Title = "Save PGN";
                dialog.Filter = "PGN File|.pgn|Text File|*.txt";
                if (dialog.ShowDialog() == DialogResult.OK)
                    game.SavePGN(dialog.FileName);
            }
        }

        private void SaveOutputClick(Object sender, EventArgs e) {
            using (SaveFileDialog dialog = new SaveFileDialog()) {
                dialog.Title = "Save Engine Output";
                dialog.Filter = "Text File|*.txt";
                if (dialog.ShowDialog() == DialogResult.OK)
                    Log.SaveText(dialog.FileName);
            }
        }

        private void EnterFENClick(Object sender, EventArgs e) {
            if (game != null) {
                String fen = InputBox.Show("Please enter the FEN string.");
                if (fen.Length > 0) {
                    game.End();
                    game.Reset();
                    game.Start(fen);
                }
            }
        }

        private void CopyFENClick(Object sender, EventArgs e) {
            Clipboard.SetText(game.GetFEN());
        }

        private void OfferDrawClick(Object sender, EventArgs e) {
            if (game != null)
                game.OfferDraw();
        }

        private void RestartClick(Object sender, EventArgs e) {
            if (game != null) {
                game.End();
                game.Reset();
                game.Start();
            }
        }

        private void UndoMoveClick(Object sender, EventArgs e) {
            if (game != null)
                game.UndoMove();
        }

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

        private void HashSizeClick(Object sender, EventArgs e) {
            while (true) {
                String input = InputBox.Show("Please specify the hash size in megabytes.", Zero.HashAllocation.ToString());
                Int32 value;
                if (Int32.TryParse(input, out value) && value > 0) {
                    if (game.White is IEngine)
                        (game.White as IEngine).AllocateHash(value);
                    if (game.Black is IEngine)
                        (game.Black as IEngine).AllocateHash(value);
                    break;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        private void RotateBoardClick(Object sender, EventArgs e) {
            VisualPosition.Rotated ^= true;
            UpdateMenu();
        }

        private void AnimationsClick(Object sender, EventArgs e) {
            VisualPosition.Animations ^= true;
            UpdateMenu();
        }

        private void AboutClick(Object sender, EventArgs e) {
            MessageBox.Show("Absolute Zero is a chess engine written in C#, developed for fun and to learn about game tree searching. Its playing strength has been and will continue to steadily increase as more techniques are added to its arsenal. \n\nIt supports the UCI protocol when ran with command-line parameter \"uci\". While in UCI mode it also accepts commands such as \"perft\". Type \"help\" to see the full list of commands. \n\nZONG ZHENG LI");
        }
    }
}
