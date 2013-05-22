using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {
    static class Universal {
        public static void Run() {
            Restrictions.Output = OutputType.Universal;
            IEngine engine = new Zero();
            Position position = new Position(Position.StartingFEN);

            while (true) {
                String command = Console.ReadLine();
                List<String> terms = new List<String>(command.Split(' '));

                switch (terms[0]) {
                    default:
                        Log.WriteLine("Unknown command. Enter 'help' for assistance.");
                        break;
                    case "uci":
                        Log.WriteLine("id name Absolute Zero " + Zero.Version);
                        Log.WriteLine("id author Zong Zheng Li");
                        Log.WriteLine("uciok");
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
                                case "nodes":// <--------------------------
                                    Restrictions.Nodes = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "ponder":// <--------------------------
                                    break;
                                case "mate":// <--------------------------
                                    break;
                                case "movestogo":// <--------------------------
                                    break;
                            }
                        new Thread(new ThreadStart(delegate {
                            Int32 bestMove = engine.GetMove(position);
                            Log.WriteLine("bestmove " + Identify.Move(bestMove));
                        })) {
                            IsBackground = true
                        }.Start();
                        break;
                    case "stop":
                        engine.Stop();
                        break;
                    case "ponderhit":// <--------------------------
                        break;
                    case "isready":
                        Log.WriteLine("readyok");
                        break;
                    case "register":// <--------------------------
                        break;
                    case "quit":
                        Environment.Exit(0);
                        break;
                    case "perft":
                        Perft.Iterate(position, Int32.Parse(terms[1]));
                        break;
                    case "divide":
                        Perft.Divide(position, Int32.Parse(terms[1]));
                        break;
                    case "draw":
                        Log.WriteLine(position);
                        break;
                    case "fen":
                        Log.WriteLine(position.GetFEN());
                        break;
                    case "help":
                        Log.WriteLine("Command          Function");
                        Log.WriteLine("-----------------------------------------------------------------------");
                        Log.WriteLine("perft x          Runs perft(x) on the current position.");
                        Log.WriteLine("divide x         Runs divide(x) on the current position.");
                        Log.WriteLine("fen              Prints the FEN for the current position.");
                        Log.WriteLine("draw             Draws the current position.");
                        Log.WriteLine("-----------------------------------------------------------------------");
                        Log.WriteLine("position x       Sets the current position. Requires \"startpos\" or");
                        Log.WriteLine("                 \"fen x\".");
                        Log.WriteLine("go x             Searches the current position. Optional parameters");
                        Log.WriteLine("                 include \"movetime x\", \"depth x\", \"nodes x\", \"wtime x\",");
                        Log.WriteLine("                 \"btime x\", \"winc x\", and \"binc x\".");
                        Log.WriteLine("stop             Stops an ongoing search.");
                        Log.WriteLine("quit             Terminates the application.");
                        Log.WriteLine("-----------------------------------------------------------------------");
                        break;
                }
            }
        }
    }
}
