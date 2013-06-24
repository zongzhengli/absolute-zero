using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {
    class Game {
        private static readonly SolidBrush OverlayBrush = new SolidBrush(Color.FromArgb(190, Color.White));
        private static readonly SolidBrush MessageBrush = new SolidBrush(Color.Black);
        private static readonly Font MessageFont = new Font("Arial", 20);
        private enum GameState { Default, Ingame, WhiteWon, BlackWon, Draw };

        private List<Int32> moves = new List<Int32>();
        private List<Type> types = new List<Type>();
        private Position initialPosition;
        public IPlayer White;
        public IPlayer Black;

        private GameState state = GameState.Default;
        private String date;
        private String message;
        private Thread thread;

        public Game(IPlayer white, IPlayer black, String fen = Position.StartingFEN) {
            White = white;
            Black = black;
            Start(fen);
        }

        public void Start(String fen = Position.StartingFEN) {
            // This is a convenient place to put test positions for quick and dirty testing. 

            //fen = "q/n2BNp/5k1P/1p5P/1p2RP/1K w";// zugzwang mate in 6 (hard)
            //fen = "r2qb1nr/pp1n3p/4k1p/1P1pPpP/1B3P1P/2R/3Q/R3KB w"; Restrictions.MoveTime = 10000;// olithink mate in 7 (hard)
            //fen = "r2qb1nr/pp1n3p/6p/1P1kPpP/1B3P1P/2R//R3KB w"; Restrictions.MoveTime = 10000;// olithink mate in 6 (hard)
            //fen = "////3k///4K2R w"; Restrictions.MoveTime = 300000;// 300000 rook mate in 13 (hard)
            //fen = "///1p1N///P/k1K w";// pawn sacrifice mate in 8 (hard)
            //fen = "r1bq1rk/p1p2p1p/1p3Pp/3pP/3Q/P1P2N/2P1BPPP/R1B2RK w"; Restrictions.MoveTime = 4000;// real mate in 5 (medium)
            //fen = "rnb2rk/1pb2ppp/p1q1p/2Pp/1R5B/4PN/1QP1BPPP/5RK w"; Restrictions.MoveTime = 10000;// queen sacrifice mate in 8 (medium)
            //fen = "rn3rk/pbppq1pp/1p2pb/4N2Q/3PN/3B/PPP2PPP/R3K2R w KQ"; Restrictions.MoveTime = 5000;// rookie mate in 7 (medium)

            //fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -";// perft 193690690
            //fen = "/k/3p/p2P1p/P2P1P///K w - -";// fine 70
            //fen = "5B/6P/1p//1N/kP/2K w";// knight promotion mate in 3
            //fen = "/7K/////R/7k w";// distance pruning mate in 8
            //fen = "2KQ///////k w";// custom mate in 7
            //fen = "5k//4pPp/3pP1P/2pP/2P3K w";// pawn endgame mate in 19
            //Perft.Iterate(new Position(fen), 5);

            Start(new Position(fen));
        }

        public void Start(Position position) {
            date = DateTime.Now.ToString("yyyy.MM.dd");
            initialPosition = position.DeepClone();
            Play(position);
        }

        private void Play(Position position) {
            thread = new Thread(new ThreadStart(delegate {
                VisualPosition.Set(position);
                state = GameState.Ingame;
                while (true) {
                    IPlayer player = (position.SideToMove == Piece.White) ? White : Black;
                    List<Int32> legalMoves = position.LegalMoves();

                    //*/ Adjudicate game. 
                    if (legalMoves.Count == 0) {
                        if (position.InCheck(position.SideToMove)) {
                            message = "Checkmate. " + Identify.Colour(1 - position.SideToMove) + " wins!";
                            state = player.Equals(White) ? GameState.BlackWon : GameState.WhiteWon;
                        } else {
                            message = "Stalemate. It's a draw!";
                            state = GameState.Draw;
                        }
                        return;
                    }
                    if (position.InsufficientMaterial()) {
                        message = "Draw by insufficient material!";
                        state = GameState.Draw;
                        return;
                    }
                    if (player is IEngine && player.AcceptDraw()) {
                        if (position.FiftyMovesClock >= 100) {
                            message = "Draw by fifty-move rule!";
                            state = GameState.Draw;
                            return;
                        }
                        if (position.HasRepeated(3)) {
                            message = "Draw by threefold repetition!";
                            state = GameState.Draw;
                            return;
                        }
                    }
                    //*/

                    Position copy = position.DeepClone();
                    Int32 move = Move.Invalid;
                    while (!legalMoves.Contains(move))
                        move = player.GetMove(copy);
                    if (!position.Equals(copy))
                        Log.WriteLine("Board modified!");

                    position.Make(move);
                    VisualPosition.Make(move);
                    moves.Add(move);
                    types.Add(player.GetType());
                }
            })) {
                IsBackground = true
            };
            thread.Start();
        }

        public void End() {
            White.Stop();
            Black.Stop();
            thread.Abort();
        }

        public void Reset() {
            state = GameState.Default;
            moves.Clear();
            types.Clear();
            White.Reset();
            Black.Reset();
        }

        public void OfferDraw() {
            IPlayer offeree = White is IEngine ? White : Black;
            if (offeree.AcceptDraw()) {
                End();
                message = "Draw by agreement!";
                state = GameState.Draw;
            } else
                MessageBox.Show("The draw offer was declined.");
        }

        public void MouseUpEvent(MouseEventArgs e) {
            if (White is Human)
                (White as Human).MouseUpEvent(e);
            if (Black is Human)
                (Black as Human).MouseUpEvent(e);
        }

        public void Draw(Graphics g) {
            VisualPosition.FillDarkSquares(g);
            if (White is Human)
                (White as Human).Draw(g);
            if (Black is Human)
                (Black as Human).Draw(g);
            VisualPosition.DrawPieces(g);
            if (state != GameState.Ingame) {
                g.FillRectangle(OverlayBrush, 0, 0, VisualPosition.Width, VisualPosition.Width);
                g.DrawString(message, MessageFont, MessageBrush, 20, 20);
            }
        }

        public void UndoMove() {
            End();
            Int32 length = 0;
            for (Int32 i = types.Count - 1; i >= 0; i--)
                if (types[i] == typeof(Human)) {
                    length = i;
                    break;
                }
            moves.RemoveRange(length, moves.Count - length);
            types.RemoveRange(length, types.Count - length);
            Position position = initialPosition.DeepClone();
            foreach (Int32 move in moves)
                position.Make(move);
            Play(position);
        }

        public String GetFEN() {
            Position position = initialPosition.DeepClone();
            moves.ForEach(move => {
                position.Make(move);
            });
            return position.GetFEN();
        }

        public void SavePGN(String path) {
            using (StreamWriter sw = new StreamWriter(path))
                sw.WriteLine(GetPGN());
        }

        public String GetPGN() {
            StringBuilder sequence = new StringBuilder();
            sequence.Append("[Date \"" + date + "\"]");
            sequence.Append(Environment.NewLine);
            sequence.Append("[White \"" + White.GetName() + "\"]");
            sequence.Append(Environment.NewLine);
            sequence.Append("[Black \"" + Black.GetName() + "\"]");
            sequence.Append(Environment.NewLine);
            String result = "*";
            switch (state) {
                case GameState.WhiteWon:
                    result = "1-0";
                    break;
                case GameState.BlackWon:
                    result = "0-1";
                    break;
                case GameState.Draw:
                    result = "1/2-1/2";
                    break;
            }
            sequence.Append("[Result \"" + result + "\"]");
            sequence.Append(Environment.NewLine);

            String initialFEN = initialPosition.GetFEN();
            if (initialFEN != Position.StartingFEN) {
                sequence.Append("[SepUp \"1\"]");
                sequence.Append(Environment.NewLine);
                sequence.Append("[FEN \"" + initialFEN + "\"]");
                sequence.Append(Environment.NewLine);
            }

            sequence.Append(Environment.NewLine);
            sequence.Append(Identify.MovesAlgebraically(initialPosition, moves, IdentificationOptions.Proper));
            if (result != "*")
                sequence.Append(" " + result);

            return sequence.ToString();
        }
    }
}
