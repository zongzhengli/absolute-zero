using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Provides methods for UCI/command-line parsing. 
    /// </summary>
    static class Universal {

        /// <summary>
        /// Executes the parsing. 
        /// </summary>
        public static void Run() {
            Restrictions.Output = OutputType.Universal;
            IEngine engine = new Zero();
            Position position = new Position(Position.StartingFEN);

            String command;
            while ((command= Console.ReadLine()) != null) {
                List<String> terms = new List<String>(command.Split(' '));

                switch (terms[0]) {
                    default:
                        Terminal.WriteLine("Unknown command: {0}", terms[0]);
                        Terminal.WriteLine("Enter \"help\" for assistance.");
                        break;

                    case "uci":
                        Terminal.WriteLine("id name " + engine.Name);
                        Terminal.WriteLine("id author Zong Zheng Li");
                        Terminal.WriteLine("option name Hash type spin default " + Zero.DefaultHashAllocation + " min 1 max 2047");
                        Terminal.WriteLine("uciok");
                        break;

                    case "ucinewgame":
                        engine.Reset();
                        break;

                    case "setoption":
                        if (terms.Contains("Hash"))
                            engine.HashAllocation = Int32.Parse(terms[terms.IndexOf("value") + 1]);
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
                                    Restrictions.TimeControl[Colour.White] = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "btime":
                                    Restrictions.TimeControl[Colour.Black] = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "winc":
                                    Restrictions.TimeIncrement[Colour.White] = Int32.Parse(terms[i + 1]);
                                    Restrictions.UseTimeControls = true;
                                    break;
                                case "binc":
                                    Restrictions.TimeIncrement[Colour.Black] = Int32.Parse(terms[i + 1]);
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
                            Terminal.WriteLine("bestmove " + Stringify.Move(bestMove));
                        })) {
                            IsBackground = true
                        }.Start();
                        break;

                    case "stop":
                        engine.Stop();
                        break;

                    case "isready":
                        Terminal.WriteLine("readyok");
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

                    case "ponderhit":
                        // TODO: implement command. 
                        break;

                    case "register":
                        // TODO: implement command. 
                        break;

                    case "help":
                        Terminal.WriteLine("Command             Function");
                        Terminal.WriteLine("-----------------------------------------------------------------------");
                        Terminal.WriteLine("position [fen]      Sets the current position to the position denoted");
                        Terminal.WriteLine("                    by the given FEN. \"startpos\" is accepted for the");
                        Terminal.WriteLine("                    starting position");
                        Terminal.WriteLine("go [type] [number]  Searches the current position. Search types include");
                        Terminal.WriteLine("                    \"movetime\", \"depth\", \"nodes\", \"wtime\", \"btime\",");
                        Terminal.WriteLine("                    \"winc\", and \"binc\"");
                        Terminal.WriteLine("perft [number]      Runs perft() on the current position to the given");
                        Terminal.WriteLine("                    depth");
                        Terminal.WriteLine("divide [number]     Runs divide() on the current position for the given");
                        Terminal.WriteLine("                    depth");
                        Terminal.WriteLine("fen                 Prints the FEN of the current position.");
                        Terminal.WriteLine("draw                Draws the current position");
                        Terminal.WriteLine("stop                Stops an ongoing search");
                        Terminal.WriteLine("quit                Exits the application");
                        Terminal.WriteLine("-----------------------------------------------------------------------");
                        break;
                }
            }
        }
    }
}
