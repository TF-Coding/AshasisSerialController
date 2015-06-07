using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SerialNet
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Count() > 0) Application.Run(new Form1(args[0]));
            else Application.Run(new Form1());

        }
    }
}
