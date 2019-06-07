using System;
using System.Windows.Forms;

namespace DR.NummerStripper
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SystemParametersInfo(int uAction, int uParam, int lpvParam, int fuWinIni);

        private const int SPI_SETKEYBOARDCUES = 4107; //100B
        private const int SPIF_SENDWININICHANGE = 2;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // always show accelerator underlines
            SystemParametersInfo(SPI_SETKEYBOARDCUES, 0, 1, 0);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayIconContext(args));
            SystemParametersInfo(SPI_SETKEYBOARDCUES, 0, 0, 0);

        }
    }
}
