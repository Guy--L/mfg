using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Poster
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (Stream stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Poster." + "BuildDate.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                Form1._version = reader.ReadToEnd();
            }

            Application.Run(new Form1());
        }
    }
}
