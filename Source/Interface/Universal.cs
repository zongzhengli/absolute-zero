using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// The parser for the UCI/command-line mode. 
    /// </summary>
    static class Universal {

        /// <summary>
        /// Executes the parsing. 
        /// </summary>
        public static void Run() {
            Restrictions.Output = OutputType.Universal;
            IEngine engine = new Zero();
            Position position = new Position(Position.StartingFEN);

            while (true) {
                String command = Console.ReadLine();
                List<String> terms = new List<String>(command.Split(' '));

                switch (terms[0]) {
                    default:
                        Terminal.WriteLine("Unknown command. Enter 'help' for assistance.");
                        break;
                    case "uci":
                        Terminal.WriteLine("id name " + engine.Name);
                        Terminal.WriteLine("id author Zong Zheng Li");
                        Terminal.WriteLine("uciok");
                        break;
                    case "ucinewgame":
                        engine.Reset();
                        break;
                    case "position":
                        String fen = Position.StartingFEN;
                        if (terms[1] != "startpos")
                            fen = command.Substring(command.IndexOf("fen") + 4);
                        position = new Position(fen);

                        Int32 movesIndex = terms.IndexOf("moves");
                        if (movesIndex >= 0)
                            for (Int32 i = movesIndex + 1; i < terms.Count; i++)
                                position.Make(Move.Create(position, terms[i]));
                        break;
                    case "go":
                        Restrictions.Reset();
                        for (Int32 i = 1; i < terms.Count; i++)
                            switch (terms[i]) {
                                default:
                                case "infinite":
                                    break;
                                case "depth":
                                    Restrictions.Depth = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = false;
                                    break;
                                case "movetime":
                                    Restrictions.MoveTime = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = false;
                                    break;
                                case "wtime":
                                    Restrictions.TimeControl[Piece.White] = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "btime":
                                    Restrictions.TimeControl[Piece.Black] = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "winc":
                                    Restrictions.TimeIncrement[Piece.White] = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "binc":
                                    Restrictions.TimeIncrement[Piece.Black] = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "nodes":
                                    Restrictions.Nodes = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = false;
                                    break;
                                case "ponder":
                                    // TODO: implement command. 
                                    break;
                                case "mate":
                                    // TODO: implement command. 
                                    break;
                                case "movestogo":
                                    // TODO: implement command. 
                                    break;
                            }
                        new Thread(new ThreadStart(() => {
                            Int32 bestMove = engine.GetMove(position);
                            Terminal.WriteLine("bestmove " + Identify.Move(bestMove));
                        })) {
                            IsBackground = true
                        }.Start();
                        break;
                    case "stop":
                        engine.Stop();
                        break;
                    case "ponderhit":
                        // TODO: implement command. 
                        break;
                    case "isready":
                        Terminal.WriteLine("readyok");
                        break;
                    case "register":
                        // TODO: implement command. 
                        break;
                    case "quit":
                        return;
                    case "perft":
                        Perft.Iterate(position, Int32.Parse(terms[1]));
                        break;
                    case "divide":
                        Perft.Divide(position, Int32.Parse(terms[1]));
                        break;
                    case "draw":
                        Terminal.WriteLine(position);
                        break;
                    case "fen":
                        Terminal.WriteLine(position.GetFEN());
                        break;
                    case "help":
                        Terminal.WriteLine("Command          Function");
                        Terminal.WriteLine("-----------------------------------------------------------------------");
                        Terminal.WriteLine("perft x          Runs perft(x) on the current position.");
                        Terminal.WriteLine("divide x         Runs divide(x) on the current position.");
                        Terminal.WriteLine("fen              Prints the FEN for the current position.");
                        Terminal.WriteLine("draw             Draws the current position.");
                        Terminal.WriteLine("-----------------------------------------------------------------------");
                        Terminal.WriteLine("position x       Sets the current position. Requires \"startpos\" or");
                        Terminal.WriteLine("                 \"fen x\".");
                        Terminal.WriteLine("go x             Searches the current position. Optional parameters");
                        Terminal.WriteLine("                 include \"movetime x\", \"depth x\", \"nodes x\", \"wtime x\",");
                        Terminal.WriteLine("                 \"btime x\", \"winc x\", and \"binc x\".");
                        Terminal.WriteLine("stop             Stops an ongoing search.");
                        Terminal.WriteLine("quit             Terminates the application.");
                        Terminal.WriteLine("-----------------------------------------------------------------------");
                        break;
                }
            }
        }
    }
}
