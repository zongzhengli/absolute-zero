using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {
    partial class Window : Form {
        public const Int32 MenuHeight = 24;
        private const Int32 DrawInterval = 33;

        private Game game;
        private Int32 hashSize = Zero.HashAllocation;

        public Window() {
            InitializeComponent();
            Icon = Properties.Resources.Icon;
            ClientSize = new Size(VisualPosition.Width, VisualPosition.Width + MenuHeight);
            MouseUp += new MouseEventHandler(MouseUpEvent);
            Paint += new PaintEventHandler(DrawEvent);
            FormClosed += delegate {
                Application.Exit();
            };
            BackColor = VisualPosition.LightColor;
            UpdateChecked();

            new Thread(new ThreadStart(DrawThread)) {
                IsBackground = true
            }.Start();
        }

        public Window(Game parameter)
            : this() {
            game = parameter;
            UpdateChecked();
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

        private void UpdateChecked() {
            rotateBoardToolStripMenuItem.Checked = VisualPosition.Rotated;
            animationsToolStripMenuItem.Checked = VisualPosition.Animations;
            Boolean gameIsNotNull = game != null;
            savePGNToolStripMenuItem.Enabled = gameIsNotNull;
            enterFENToolStripMenuItem.Enabled = gameIsNotNull;
            copyFENToolStripMenuItem.Enabled = gameIsNotNull;
            offerDrawToolStripMenuItem.Enabled = gameIsNotNull;
            restartToolStripMenuItem.Enabled = gameIsNotNull;
            undoMoveToolStripMenuItem.Enabled = gameIsNotNull;
            if (gameIsNotNull) {
                Boolean hasHuman = game.White is Human || game.Black is Human;
                Boolean hasEngine = game.White is IEngine || game.Black is IEngine;
                saveOuputToolStripMenuItem.Enabled = hasEngine;
                offerDrawToolStripMenuItem.Enabled = hasHuman && hasEngine;
                undoMoveToolStripMenuItem.Enabled = hasHuman;
                searchToolStripMenuItem.Enabled = hasEngine;
                hashSizeToolStripMenuItem.Enabled = hasEngine;
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
                    return;
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
                    return;
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
                    return;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        private void HashSizeClick(Object sender, EventArgs e) {
            while (true) {
                String input = InputBox.Show("Please specify the hash size in megabytes.", hashSize.ToString());
                Int32 value;
                if (Int32.TryParse(input, out value) && value > 0) {
                    if (game.White is IEngine)
                        (game.White as IEngine).AllocateHash(value);
                    if (game.Black is IEngine)
                        (game.Black as IEngine).AllocateHash(value);
                    hashSize = value;
                    return;
                } else
                    MessageBox.Show("Input must be a positive integer.");
            }
        }

        private void RotateBoardClick(Object sender, EventArgs e) {
            VisualPosition.Rotated ^= true;
            UpdateChecked();
        }

        private void AnimationsClick(Object sender, EventArgs e) {
            VisualPosition.Animations ^= true;
            UpdateChecked();
        }

        private void AboutClick(Object sender, EventArgs e) {
            MessageBox.Show("Absolute Zero is a chess engine written in C#, developed for fun and to learn more about AI. Its playing strength has been increasing steadily as more techniques are added to its arsenal. It supports the UCI protocol when ran with command-line parameter \"uci\".\n\nZONGZHENGLI 2012");
        }
    }
}
