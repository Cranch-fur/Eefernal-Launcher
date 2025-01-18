using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace EefernalLauncher
{
    internal static class Program
    {
        static void ExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            string exceptionData = e.Exception.ToString();


            try
            {
                string tempFolder = Path.GetTempPath();
                string logFile = Path.Combine(tempFolder, $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss")}] Eefernal Launcher Fatal Error.txt");


                File.WriteAllText(logFile, exceptionData);
                using (Process textviewer = Process.Start(new ProcessStartInfo(logFile)))
                {
                    textviewer.Dispose();
                }
            }
            catch
            {
                MessageBox.Show(exceptionData, "Eefernal Launcher Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

        }




        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.ThreadException += new ThreadExceptionEventHandler(ExceptionHandler);
            try
            {
                Application.Run(new Launcher());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Eefernal Launcher Application.Run() Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
