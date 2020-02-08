using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Represents a game between two players. 
    /// </summary>
    public sealed class Game {

        // Graphics constants. 
        private static readonly SolidBrush OverlayBrush = new SolidBrush(Color.FromArgb(190, Color.White));
        private static readonly SolidBrush MessageBrush = new SolidBrush(Color.Black);
        private static readonly Font MessageFont = new Font("Arial", 20);

        // Specifies the game state. 
        private enum GameState { NotStarted, Ingame, Stopped, WhiteWon, BlackWon, Draw };

        // Game fields. 
        private Position _initialPosition;
        private List<Int32> _moves = new List<Int32>();
        private List<Type> _types = new List<Type>();
        private GameState _state = GameState.NotStarted;
        private ManualResetEvent _waitForStop = new ManualResetEvent(false);
        private String _message;
        private String _date;

        public IPlayer White;
        public IPlayer Black;

        /// <summary>
        /// Constructs a Game with the given players and initial position. If the 
        /// initial position is not specified the default starting chess position is 
        /// used. 
        /// </summary>
        /// <param name="white">The player to play as white.</param>
        /// <param name="black">The player to play as black.</param>
        /// <param name="fen">An optional FEN for the starting position.</param>
        public Game(IPlayer white, IPlayer black, String fen = Position.StartingFEN) {
            White = white;
            Black = black;
            Start(fen);
        }

        /// <summary>
        /// Starts a game between the two players starting from the position with the 
        /// given FEN. This method is non-blocking. 
        /// </summary>
        /// <param name="fen">The FEN of the position to start the game from.</param>
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

            Start(Position.Create(fen));
        }

        /// <summary>
        /// Starts a game between the two players starting from the given position. 
        /// This method is non-blocking and does not modify the given position. 
        /// </summary>
        /// <param name="position">The position to start the game from.</param>
        public void Start(Position position) {
            _date = DateTime.Now.ToString("yyyy.MM.dd");
            _initialPosition = position;
            Play(position);
        }

        /// <summary>
        /// Starts play between the two players on the current position for the game. 
        /// This method is non-blocking and does not modify the given position. 
        /// </summary>
        /// <param name="p">The position to start playing from.</param>
        private void Play(Position p) {
            Position position = p.DeepClone();
            VisualPosition.Set(position);
            _state = GameState.Ingame;
            _waitForStop.Reset();

            new Thread(new ThreadStart(() => {
                while (true) {
                    IPlayer player = (position.SideToMove == Colour.White) ? White : Black;
                    List<Int32> legalMoves = position.LegalMoves();

                    // Adjudicate checkmate and stalemate. 
                    if (legalMoves.Count == 0) {
                        if (position.InCheck(position.SideToMove)) {
                            _message = "Checkmate. " + Stringify.Colour(1 - position.SideToMove) + " wins!";
                            _state = player.Equals(White) ? GameState.BlackWon : GameState.WhiteWon;
                        } else {
                            _message = "Stalemate. It's a draw!";
                            _state = GameState.Draw;
                        }
                    }

                    // Adjudicate draw.  
                    if (position.InsufficientMaterial()) {
                        _message = "Draw by insufficient material!";
                        _state = GameState.Draw;
                    }
                    if (player is Engine && player.AcceptsDraw) {
                        if (position.FiftyMovesClock >= 100) {
                            _message = "Draw by fifty-move rule!";
                            _state = GameState.Draw;
                        }
                        if (position.HasRepeated(3)) {
                            _message = "Draw by threefold repetition!";
                            _state = GameState.Draw;
                        }
                    }

                    // Consider game end. 
                    if (_state != GameState.Ingame) {
                        _waitForStop.Set();
                        return;
                    }

                    // Get move from player. 
                    Position copy = position.DeepClone();
                    Int32 move = player.GetMove(copy);
                    if (!position.Equals(copy))
                        Terminal.WriteLine("Board modified!");

                    // Consider game stop. 
                    if (_state != GameState.Ingame) {
                        _waitForStop.Set();
                        return;
                    }

                    // Make the move. 
                    position.Make(move);
                    VisualPosition.Make(move);
                    _moves.Add(move);
                    _types.Add(player.GetType());
                }
            })) {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Stops play between the two players. 
        /// </summary>
        public void End() {
            _state = GameState.Stopped;
            White.Stop();
            Black.Stop();
            _waitForStop.WaitOne();
        }

        /// <summary>
        /// Resets play between the two players so that the game is restored to the 
        /// state at which no moves have been played. 
        /// </summary>
        public void Reset() {
            _state = GameState.NotStarted;
            _moves.Clear();
            _types.Clear();
            White.Reset();
            Black.Reset();
        }

        /// <summary>
        /// Offers a draw to the engine if applicable. 
        /// </summary>
        public void OfferDraw() {
            IPlayer offeree = White is Engine ? White : Black;
            if (offeree.AcceptsDraw) {
                End();
                _message = "Draw by agreement!";
                _state = GameState.Draw;
            } else
                MessageBox.Show("The draw offer was declined.");
        }

        /// <summary>
        /// Handles a mouse up event. 
        /// </summary>
        /// <param name="e">The mouse event.</param>
        public void MouseUpHandler(MouseEventArgs e) {
            if (White is Human)
                (White as Human).MouseUpHandler(e);
            if (Black is Human)
                (Black as Human).MouseUpHandler(e);
        }

        /// <summary>
        /// Draws the position and animations associated with the game. 
        /// </summary>
        /// <param name="g">The drawing surface.</param>
        public void Draw(Graphics g) {
            VisualPosition.DrawDarkSquares(g);
            White.Draw(g);
            Black.Draw(g);
            VisualPosition.DrawPieces(g);

            if (_state != GameState.Ingame && _state != GameState.Stopped) {
                g.FillRectangle(OverlayBrush, 0, 0, VisualPosition.Width, VisualPosition.Width);
                g.DrawString(_message, MessageFont, MessageBrush, 20, 20);
            }
        }

        /// <summary>
        /// Undoes the last move made by a human player. 
        /// </summary>
        public void UndoMove() {
            End();
            Int32 length = 0;
            for (Int32 i = _types.Count - 1; i >= 0; i--)
                if (_types[i] == typeof(Human)) {
                    length = i;
                    break;
                }
            _moves.RemoveRange(length, _moves.Count - length);
            _types.RemoveRange(length, _types.Count - length);
            Position position = _initialPosition.DeepClone();
            _moves.ForEach(move => {
                position.Make(move);
            });
            Play(position);
        }

        /// <summary>
        /// Returns the FEN string of the position in the game. 
        /// </summary>
        /// <returns>The FEN of the position in the game.</returns>
        public String GetFEN() {
            Position position = _initialPosition.DeepClone();
            _moves.ForEach(move => {
                position.Make(move);
            });
            return position.GetFEN();
        }

        /// <summary>
        /// Saves the PGN string of the game to a file with the given path.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        public void SavePGN(String path) {
            File.WriteAllText(path, GetPGN());
        }

        /// <summary>
        /// Returns the PGN string of the game.
        /// </summary>
        /// <returns>The PGN string of the game.</returns>
        public String GetPGN() {
            StringBuilder sb = new StringBuilder();
            sb.Append("[Date \"" + _date + "\"]");
            sb.Append(Environment.NewLine);
            sb.Append("[White \"" + White.Name + "\"]");
            sb.Append(Environment.NewLine);
            sb.Append("[Black \"" + Black.Name + "\"]");
            sb.Append(Environment.NewLine);
            String result = "*";
            switch (_state) {
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
            sb.Append("[Result \"" + result + "\"]");
            sb.Append(Environment.NewLine);

            String initialFEN = _initialPosition.GetFEN();
            if (initialFEN != Position.StartingFEN) {
                sb.Append("[SetUp \"1\"]");
                sb.Append(Environment.NewLine);
                sb.Append("[FEN \"" + initialFEN + "\"]");
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine);
            sb.Append(Stringify.MovesAlgebraically(_initialPosition, _moves, StringifyOptions.Proper));
            if (result != "*")
                sb.Append(" " + result);

            return sb.ToString();
        }
    }
}
