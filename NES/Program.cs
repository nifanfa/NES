using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NES
{
    static class Program
    {

        public static NES NES;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.SetCompatibleTextRenderingDefault(false);
            

            using (NES = new NES())
            {
                Application.EnableVisualStyles();

                NES.Show();

                while (NES.Created)
                {
                    int updateForm = 0;

                    Application.DoEvents();

                    while (NES.bolRunGame)
                    {
                        NES.runGame();

                        if (updateForm++ >= 1000)
                        {
                            Application.DoEvents();
                            updateForm = 0;
                        }
                    }
                }
            }
        }
    }
}
