using System;
using System.Windows.Forms;

namespace AbsoluteZero {
   static class Launch {
        [STAThread]
        public static void Main(String[] args) {
            Log.Initialize();
            if (args.Length <= 0) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Settings());
            } else {
                String[] parameters = new String[args.Length - 1];
                Array.Copy(args, 1, parameters, 0, parameters.Length);
                for (Int32 i = 0; i < parameters.Length; i++)
                    parameters[i] = parameters[i].ToLowerInvariant();
                switch (args[0]) {
                    default:
                    case "uci":
                        Universal.Run();
                        break;
                    case "tournament":
                        new Tournament().Start(parameters);
                        break;
                    case "testsuite":
                        TestSuite.Run(parameters);
                        break;
                }
            }
        }
    }
}
