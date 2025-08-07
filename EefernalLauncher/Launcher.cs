// #define START_METHOD_DEFAULT
// #define START_METHOD_CMD
// #define START_METHOD_POWERSHELL
#define START_METHOD_KERNEL






using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


#if START_METHOD_DEFAULT
using System.Diagnostics;
using System.ComponentModel;
#endif

#if START_METHOD_CMD || START_METHOD_POWERSHELL
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
#endif

#if START_METHOD_KERNEL
using System.Runtime.InteropServices;
using System.Text;
#endif






namespace EefernalLauncher
{
    public partial class Launcher : Form
    {
#if START_METHOD_KERNEL
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct KERNEL_STARTUPINFO // https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/ns-processthreadsapi-startupinfow
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct KERNEL_PROCESS_INFORMATION // https://learn.microsoft.com/ru-ru/windows/win32/api/processthreadsapi/ns-processthreadsapi-process_information
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessW // https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw
        (
            string lpApplicationName,
            StringBuilder lpCommandLine, // StringBuilder for mutable buffer.
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref KERNEL_STARTUPINFO lpStartupInfo,
            out KERNEL_PROCESS_INFORMATION lpProcessInformation
        );
#endif





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






#if START_METHOD_DEFAULT
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
#endif


#if START_METHOD_CMD
        async void StartProcessCMD(string startupTarget, string startupArguments)
        {
            string commandLine = $"/c start \"\" \"{startupTarget}\" {startupArguments}";
            using (Process newProcess = new Process())
            {
                newProcess.StartInfo.FileName = "cmd.exe";
                newProcess.StartInfo.Arguments = commandLine;
                newProcess.StartInfo.UseShellExecute = false;
                newProcess.StartInfo.CreateNoWindow = true;

                try
                {
                    newProcess.Start();
                    await Task.Delay(3000);

                    string friendlyExecutableName = Path.GetFileNameWithoutExtension(startupTarget);
                    Process[] foundProcesses = Process.GetProcessesByName(friendlyExecutableName);
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
#endif


#if START_METHOD_POWERSHELL
        async void StartProcessPowerShell(string startupTarget, string startupArguments)
        {
            string commandLine = $"Start-Process -FilePath '{startupTarget}' -ArgumentList '{startupArguments}'";
            using (Process newProcess = new Process())
            {
                newProcess.StartInfo.FileName = "powershell.exe";
                newProcess.StartInfo.Arguments = $"-NoProfile -Command \"{commandLine}\"";
                newProcess.StartInfo.UseShellExecute = false;
                newProcess.StartInfo.CreateNoWindow = true;

                try
                {
                    newProcess.Start();
                    await Task.Delay(3000);

                    string friendlyExecutableName = Path.GetFileNameWithoutExtension(startupTarget);
                    Process[] foundProcesses = Process.GetProcessesByName(friendlyExecutableName);
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
#endif


#if START_METHOD_KERNEL
        void StartProcessKernel(string startupTarget, string startupArguments)
        {
            string workingDirectory = Path.GetDirectoryName(startupTarget);
            StringBuilder commandLine = new StringBuilder($"\"{startupTarget}\" {startupArguments}");

            KERNEL_STARTUPINFO startupInfo = new KERNEL_STARTUPINFO();
            startupInfo.cb = (uint)Marshal.SizeOf<KERNEL_STARTUPINFO>();
            startupInfo.dwFlags += 0x00000040; // STARTF_FORCEONFEEDBACK: Indicates that the cursor is in feedback mode for two seconds after CreateProcess is called.
            KERNEL_PROCESS_INFORMATION procInfo = new KERNEL_PROCESS_INFORMATION();

            bool wasProcessCreated = CreateProcessW
            (
                null,
                commandLine,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                0,
                IntPtr.Zero,
                workingDirectory,
                ref startupInfo,
                out procInfo
            );

            if (wasProcessCreated == false)
            {
                int win32Error = Marshal.GetLastWin32Error();
                Exit($"Failed to start process! WIN32 exception: {win32Error}", MessageBoxIcon.Error);
            }
        }
#endif






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
#if START_METHOD_DEFAULT
            StartProcess(startupTarget, startupArguments);
#elif START_METHOD_CMD
            StartProcessCMD(startupTarget, startupArguments);
#elif START_METHOD_POWERSHELL
            StartProcessPowerShell(startupTarget, startupArguments);
#elif START_METHOD_KERNEL
            StartProcessKernel(startupTarget, startupArguments);
#else
#error No startup method specified.
#endif
            StartupData.gameStartAttempted = true;
        }
    }
}
