using System;
using System.Collections.Generic;
using System.IO;

namespace AbsoluteZero {
    class Tournament {
        private const Int32 UpdateInterval = 10;
        private const Int32 ColumnWidth = 8;

        public String ID = "Tournament " + DateTime.Now.ToString().Replace('/', '-').Replace(':', '.');

        public void Start(String[] parameters) {
            // engine declarations
            IEngine experimental = new Zero() {
                NewFeature = true
            };
            IEngine standard = new Zero();

            List<String> epd = TestSuite.Parse(parameters);
            Restrictions.Output = OutputType.None;
            Int32 wins = 0;
            Int32 losses = 0;
            Int32 draws = 0;

            using (StreamWriter sw = new StreamWriter(ID + ".txt")) {
                sw.WriteLine(Format.Pad(UpdateInterval) + "     " + Format.PadRightAll(ColumnWidth, "Games", "Wins", "Losses", "Draws", "Elo", "Error"));
                sw.WriteLine("-----------------------------------------------------------------");

                // tournament loop
                for (Int32 games = 1; ; games++) {
                    sw.Flush();
                    experimental.Reset();
                    standard.Reset();
                    Position position = new Position(epd[Random.Int32(epd.Count - 1)]);
                    MatchResult result = Match.Play(experimental, standard, position, MatchOptions.RandomizeColour);

                    // output
                    switch (result) {
                        case MatchResult.Win:
                            sw.Write('1');
                            wins++;
                            break;
                        case MatchResult.Loss:
                            sw.Write('0');
                            losses++;
                            break;
                        case MatchResult.Draw:
                            sw.Write('-');
                            draws++;
                            break;
                        case MatchResult.Unresolved:
                            sw.Write('*');
                            draws++;
                            break;
                    }

                    if (games % UpdateInterval == 0)
                        sw.WriteLine(
                            "     " +
                            Format.PadRight(games, ColumnWidth) +
                            Format.PadRight(wins, ColumnWidth) +
                            Format.PadRight(losses, ColumnWidth) +
                            Format.PadRight(draws, ColumnWidth) +
                            Format.PadRight(Format.PrecisionAndSign(Elo.GetDifference(wins, losses, draws)), ColumnWidth) +
                            "±" + Math.Round(Elo.GetError(wins, losses, draws))
                            );
                }
            }
        }
    }
}
