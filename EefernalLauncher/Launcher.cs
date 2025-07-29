using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;






namespace EefernalLauncher
{
    public partial class Launcher : Form
    {
        public static class StartupData
        {
            public const string splashScreenFilePath = @"EefernalFog\SplashScreen.mfx";
            public const string logoFilePath = @"EefernalFog\Logo.mfx";

            public const string startupTargetFilePath = @"EefernalFog\StartupTarget.txt";
            public const string startupArgumentsFilePath = @"EefernalFog\StartupArguments.txt";

            public static volatile bool gameStartAttempted = false;
        }


        public static class StartupAnimation
        {
            public static Timer timer = new Timer();
            public static volatile bool fadeInComplete = false;
            public static volatile bool progressBarComplete = false;
        }






        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr ShellExecuteW
        (
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            int nShowCmd
        );






        private void Exit()
        {
            Environment.Exit(0);
        }
        private void Exit(string message, MessageBoxIcon messageType)
        {
            if (!string.IsNullOrEmpty(message))
            {
                StartupAnimation.timer.Enabled = false;
                MessageBox.Show(message, "Eefernal Launcher", MessageBoxButtons.OK, messageType, MessageBoxDefaultButton.Button1);
            }
                

            Exit();
        }






        private void Launcher_AnimationTick(object sender, EventArgs e)
        {
            double newValue = this.Opacity + 0.025;
            if (newValue <= 1.0)
                this.Opacity = newValue;
            else
            {
                this.Opacity = 1.0;
                StartupAnimation.fadeInComplete = true;
                StartupAnimation.timer.Tick -= Launcher_AnimationTick;
            }
        }
        private void startupProgressBar_AnimationTick(object sender, EventArgs e)
        {
            int newValue = startupProgressBar.Value + 1;
            if (newValue <= startupProgressBar.Maximum)
                startupProgressBar.Value = newValue;
            else
            {
                StartupAnimation.progressBarComplete = true;
                StartupAnimation.timer.Tick -= startupProgressBar_AnimationTick;
            }
                
        }






        void StartProcess(string startupTarget, string startupArguments)
        {
            using (Process newProcess = new Process())
            {
                newProcess.StartInfo.FileName = Path.GetFileName(startupTarget);
                newProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(startupTarget);
                newProcess.StartInfo.Arguments = startupArguments;
                newProcess.StartInfo.UseShellExecute = true;

                try
                {
                    newProcess.Start();
                    if (newProcess.HasExited)
                    {
                        Exit("Failed to start process! Process has exited.", MessageBoxIcon.Error);
                    }
                }
                catch (Win32Exception win32Ex)
                {
                    Exit($"Failed to start process! WIN32 exception: {win32Ex.NativeErrorCode}", MessageBoxIcon.Error);
                }
                catch
                {
                    Exit("Failed to start process! Process failed to start.", MessageBoxIcon.Error);
                }
            }
        }


        async void StartProcessCMD(string startupTarget, string startupArguments)
        {
            string executableName = Path.GetFileNameWithoutExtension(startupTarget);
            string command = $"/c start \"\" \"{startupTarget}\" {startupArguments}";
            using (Process newProcess = new Process())
            {
                newProcess.StartInfo.FileName = "cmd.exe";
                newProcess.StartInfo.Arguments = command;
                newProcess.StartInfo.UseShellExecute = false;
                newProcess.StartInfo.CreateNoWindow = true;

                try
                {
                    newProcess.Start();
                    await Task.Delay(3000);

                    Process[] foundProcesses = Process.GetProcessesByName(executableName);
                    if (foundProcesses.Length == 0)
                        Exit($"Failed to start process! Process wasn't found running.", MessageBoxIcon.Error);
                }
                catch (Win32Exception win32Ex)
                {
                    Exit($"Failed to start process! WIN32 exception: {win32Ex.NativeErrorCode}", MessageBoxIcon.Error);
                }
                catch
                {
                    Exit("Failed to start process! Process failed to start.", MessageBoxIcon.Error);
                }
            }
        }


        async void StartProcessPS(string startupTarget, string startupArguments)
        {
            string executableName = Path.GetFileNameWithoutExtension(startupTarget);
            string command = $"Start-Process -FilePath '{startupTarget}' -ArgumentList '{startupArguments}'";
            using (Process newProcess = new Process())
            {
                newProcess.StartInfo.FileName = "powershell.exe";
                newProcess.StartInfo.Arguments = $"-NoProfile -Command \"{command}\"";
                newProcess.StartInfo.UseShellExecute = false;
                newProcess.StartInfo.CreateNoWindow = true;

                try
                {
                    newProcess.Start();
                    await Task.Delay(3000);

                    Process[] foundProcesses = Process.GetProcessesByName(executableName);
                    if (foundProcesses.Length == 0)
                        Exit($"Failed to start process! Process wasn't found running.", MessageBoxIcon.Error);
                }
                catch (Win32Exception win32Ex)
                {
                    Exit($"Failed to start process! WIN32 exception: {win32Ex.NativeErrorCode}", MessageBoxIcon.Error);
                }
                catch
                {
                    Exit("Failed to start process! Process failed to start.", MessageBoxIcon.Error);
                }
            }
        }


        async void StartProcessSH(string startupTarget, string startupArguments)
        {
            string executableName = Path.GetFileNameWithoutExtension(startupTarget);

            ShellExecuteW(IntPtr.Zero, "open", startupTarget, startupArguments, Path.GetDirectoryName(startupTarget), 1 /*SW_SHOWNORMAL*/);
            await Task.Delay(3000);

            Process[] foundProcesses = Process.GetProcessesByName(executableName);
            if (foundProcesses.Length == 0)
                Exit($"Failed to start process! Process wasn't found running.", MessageBoxIcon.Error);
        }






        public Launcher()
        {
            InitializeComponent();
        }


        private void backgroundWorkerExit_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (StartupAnimation.fadeInComplete == false || StartupAnimation.progressBarComplete == false || StartupData.gameStartAttempted == false)
            {
                System.Threading.Thread.Sleep(100);
            }

            System.Threading.Thread.Sleep(1350);
            Exit();
        }




        private void Launcher_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.Icon;


            if (File.Exists(StartupData.splashScreenFilePath)) 
            { 
                Image backgroundImage = Image.FromFile(StartupData.splashScreenFilePath);
                this.BackgroundImage = backgroundImage;
            }


            if (File.Exists(StartupData.logoFilePath))
            {
                startupLogo.Parent = this;
                startupLogo.BackColor = Color.Transparent;

                Image startupLogoImage = Image.FromFile(StartupData.logoFilePath);
                startupLogo.Image = startupLogoImage;
            }
        }


        private void Launcher_Shown(object sender, EventArgs e)
        {
            backgroundWorkerExit.RunWorkerAsync();


            StartupAnimation.timer.Interval = 1;
            StartupAnimation.timer.Tick += Launcher_AnimationTick;
            StartupAnimation.timer.Tick += startupProgressBar_AnimationTick;
            StartupAnimation.timer.Start();


            if (!File.Exists(StartupData.startupTargetFilePath))
                Exit($"Startup target file wasn't found!\n\n\"{StartupData.startupTargetFilePath}\"", MessageBoxIcon.Error);

            string startupTarget = File.ReadAllText(StartupData.startupTargetFilePath);
            if (string.IsNullOrEmpty(startupTarget))
                Exit($"Startup target file is empty!\n\n\"{StartupData.startupTargetFilePath}\"", MessageBoxIcon.Error);

            startupTarget = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, startupTarget);
            if (!File.Exists(startupTarget))
                Exit($"Startup target doesn't exist!\n\n\"{startupTarget}\"", MessageBoxIcon.Error);


            string startupArguments = string.Empty;
            if (File.Exists(StartupData.startupArgumentsFilePath))
                startupArguments = File.ReadAllText(StartupData.startupArgumentsFilePath);


            this.TopMost = false; // Ensure that user will be able to interact with Windows security pop up.
            StartProcessSH(startupTarget, startupArguments);
            StartupData.gameStartAttempted = true;
        }
    }
}
