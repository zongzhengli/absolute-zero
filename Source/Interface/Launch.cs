using System;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Handles the application launch. 
    /// </summary>
    static class Launch {

        /// <summary>
        /// The main entry point for the application. 
        /// </summary>
        /// <param name="args">The command-line arugments.</param>
        [STAThread]
        public static void Main(String[] args) {

            // Display terminal window by default. We will hide it later if necessary. 
            Terminal.Initialize();

            // Run as GUI application if there are no command-line arguments. Display 
            // the Settings window, which will display the main board window as needed. 
            if (args.Length == 0) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Settings());
            }

            // Run as command-line application if there are command-line arguments. 
            else {

                // Get the subroutine arguments. 
                String[] subArgs = new String[args.Length - 1];
                Array.Copy(args, 1, subArgs, 0, subArgs.Length);

                switch (args[0]) {

                    // Unrecognized command.
                    default:
                        Terminal.WriteLine("Unrecognized command-line parameter. Valid parameters are:");
                        Terminal.WriteLine("-u    UCI/command-line mode");
                        Terminal.WriteLine("-t    Tournament mode");
                        Terminal.WriteLine("-s    Test suite mode");
                        Terminal.Write("Press any key to continue . . . ");
                        Console.ReadKey();
                        break;

                    // UCI mode.
                    case "-u":
                        Universal.Run();
                        break;
                    
                    // Tournament mode. 
                    case "-t":
                        Tournament.Run(subArgs);
                        break;

                    // Test suite mode. 
                    case "-s":
                        TestSuite.Run(subArgs);
                        break;
                }
            }
        }
    }
}
