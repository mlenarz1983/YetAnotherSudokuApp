using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace YetAnotherSudokuPlayer.WinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string preferencesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Preferences.xml");
            try
            {
                if (File.Exists(preferencesPath))
                {
                    Preferences = Utils.XmlDeserializeObject<SudokuPreferences>(File.ReadAllText(preferencesPath));
                }
                else
                {
                    Preferences = new SudokuPreferences();
                    Preferences.HotTrackingColor = Color.Goldenrod;
                    Preferences.SelectedColor = Color.CornflowerBlue;
                    Preferences.RelatedSquareColor = Color.LightSteelBlue;
                    Preferences.ErrorColor = Color.Red;
                    Preferences.ShowMoveHistory = false;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            finally
            {

                File.WriteAllText(preferencesPath, Utils.XmlSerializeObject(Preferences));
            }
        }

        public static SudokuPreferences Preferences { get; set; }
    }
}
