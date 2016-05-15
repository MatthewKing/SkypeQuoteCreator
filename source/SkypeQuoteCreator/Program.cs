using System;
using System.IO;
using System.Windows.Forms;

namespace SkypeQuoteCreator
{
    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
            {
                if (!Directory.Exists(Settings.DefaultDirectory))
                {
                    Directory.CreateDirectory(Settings.DefaultDirectory);
                }
            }
            catch
            {
                // Swallow all exceptions.
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
