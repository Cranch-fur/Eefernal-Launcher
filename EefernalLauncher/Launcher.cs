using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Thread = System.Threading.Thread;






namespace EefernalLauncher
{
    public partial class Launcher : Form
    {
        public readonly static string launcherDirectory = AppDomain.CurrentDomain.BaseDirectory;


        public static class GameStartData
        {
            public readonly static string splashScreenFilePath = Path.Combine(launcherDirectory, @"EefernalFog\SplashScreen.mfx");
            public readonly static string logoFilePath = Path.Combine(launcherDirectory, @"EefernalFog\Logo.mfx");

            public readonly static string startupTargetFilePath = Path.Combine(launcherDirectory, @"EefernalFog\StartupTarget.txt");
            public readonly static string startupArgumentsFilePath = Path.Combine(launcherDirectory, @"EefernalFog\StartupArguments.txt");
            public readonly static string startupDependenciesPath = Path.Combine(launcherDirectory, @"EefernalFog\StartupDependencies.txt");
        }


        public static class GameStartAnimation
        {
            public static readonly Timer timer = new Timer();
            public static class WindowFadeIn
            {
                public const double stepping = 0.020;
                public static readonly int totalFrames = (int)Math.Ceiling(1.0 / stepping); // HARDCODED VALUE! 1.0 represents maximum form opacity value (100%).
                public static volatile int currentFrame = 0;
                public static volatile bool complete = false;
            }
            public static class ProgressBar
            {
                public const double stepping = 10.0;
                public static readonly int totalFrames = (int)Math.Ceiling(1000.0 / stepping); // HARDCODED VALUE! 1000.0 represents progress bar maximum value.
                public static volatile int currentFrame = 0;
                public static volatile bool complete = false;
            }
        }






        private void Exit()
        {
            Environment.Exit(0);
        }
        private void ExitWithMessage(string message, MessageBoxIcon messageType)
        {
            if (!string.IsNullOrEmpty(message))
            {
                GameStartAnimation.timer.Enabled = false;
                MessageBox.Show(message, "Eefernal Launcher", MessageBoxButtons.OK, messageType, MessageBoxDefaultButton.Button1);
            }
                
            Exit();
        }


        void StartProcess(string startupTarget, string startupArguments)
        {
            if (File.Exists(startupTarget) == false)
                ExitWithMessage($"Failed to start process! Startup target doesn't exist!", MessageBoxIcon.Error);


            /* 
                * Modern applications often rely on relative paths (paths calculated from {workingDirectory} + "..\..").
                * In order to achive best compatibility and avoid potential issues, we need to resolve working directory
                * from path to target executable, this path will later be specified when creating a process.
            */
            string workingDirectory = Path.GetDirectoryName(startupTarget);
            if (Directory.Exists(workingDirectory) == false)
                workingDirectory = Environment.CurrentDirectory;


            /*
                * When constructing a command line, we need to account for scenario where startup arguments wasn't specified.
                * Interacting with low-level libraries always involve proper understanding and accuracy of actions taken, no extra spaces must be left. 
            */
            string command = string.IsNullOrEmpty(startupArguments) 
                                                                    ? $"\"{startupTarget}\"" 
                                                                    : $"\"{startupTarget}\" {startupArguments}";
            StringBuilder commandLine = new StringBuilder(command);


            /*
                * Create new instance of Startup Info struct to initialize it with default values.
                * Parameters of newly created instance can then be adjusted to meet our goals.
            */
            KERNEL32.Struct.STARTUPINFO startupInfo = new KERNEL32.Struct.STARTUPINFO();
            startupInfo.cb = (uint)Marshal.SizeOf<KERNEL32.Struct.STARTUPINFO>();
            startupInfo.dwFlags += 0x00000040; // STARTF_FORCEONFEEDBACK: Indicates that the cursor is in feedback mode for two seconds after CreateProcess is called.


            /*
                * Create new instance of Process Information struct to initialize it with default values.
                * This instance will be referenced as output destination for process creation, later on allowing us to read process specific data from it.
            */
            KERNEL32.Struct.PROCESS_INFORMATION procInfo = new KERNEL32.Struct.PROCESS_INFORMATION();


            bool createProcessResult = KERNEL32.CreateProcessW
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

            if (createProcessResult == false)
            {
                int win32Error = Marshal.GetLastWin32Error();
                ExitWithMessage($"Failed to start process! WIN32 ERROR: {win32Error}.", MessageBoxIcon.Error);
            }
        }






        private void Launcher_AnimationTick(object sender, EventArgs e)
        {
            GameStartAnimation.WindowFadeIn.currentFrame++;
            if (GameStartAnimation.WindowFadeIn.currentFrame > GameStartAnimation.WindowFadeIn.totalFrames)
            {
                this.Opacity = 1.0;
                GameStartAnimation.WindowFadeIn.complete = true;
                GameStartAnimation.timer.Tick -= Launcher_AnimationTick;
            }
            else
            {
                /* Calculate normalized progress of the animation as a value between 0.0 (start) and 1.0 (end). */
                double normalizedProgress = (double)GameStartAnimation.WindowFadeIn.currentFrame / (double)GameStartAnimation.WindowFadeIn.totalFrames;

                /* Quadratic ease-in - slow at start, accelerating over time. */
                this.Opacity = normalizedProgress * normalizedProgress;
            }
        }
        private void startupProgressBar_AnimationTick(object sender, EventArgs e)
        {
            GameStartAnimation.ProgressBar.currentFrame++;
            if (GameStartAnimation.ProgressBar.currentFrame > GameStartAnimation.ProgressBar.totalFrames)
            {
                startupProgressBar.Value = startupProgressBar.Maximum;
                GameStartAnimation.ProgressBar.complete = true;
                GameStartAnimation.timer.Tick -= startupProgressBar_AnimationTick;
            }
            else
            {
                /* Calculate normalized progress of the animation as a value between 0.0 (start) and 1.0 (end). */
                double normalizedProgress = (double)GameStartAnimation.ProgressBar.currentFrame / (double)GameStartAnimation.ProgressBar.totalFrames;

                /* Quadratic ease-in - slow at start, accelerating over time. */
                double quadraticEase = normalizedProgress * normalizedProgress;

                /* Scale quadratic ease-in curve to the progress bar maximum value */
                int newValue = (int)Math.Round(quadraticEase * startupProgressBar.Maximum);

                startupProgressBar.Value = newValue;
            }
        }


        private void Launcher_StartAnimation()
        {
            /*
                * Timer can only be started once, we would only want to start timer if it's not yet running.
                * In case to avoid something going terribly wrong, check if it's not already enabled.
            */
            if (GameStartAnimation.timer.Enabled == false)
            {
                GameStartAnimation.timer.Interval = 1;
                GameStartAnimation.timer.Tick += Launcher_AnimationTick;
                GameStartAnimation.timer.Tick += startupProgressBar_AnimationTick;
                GameStartAnimation.timer.Start();
            }
        }


        public Launcher()
        {
            InitializeComponent();
        }


        private void Launcher_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Icon = Properties.Resources.Icon;

            if (File.Exists(GameStartData.splashScreenFilePath))
            {
                Image backgroundImage = Image.FromFile(GameStartData.splashScreenFilePath);
                this.BackgroundImage = backgroundImage;
            }

            if (File.Exists(GameStartData.logoFilePath))
            {
                startupLogo.Parent = this;
                startupLogo.BackColor = Color.Transparent;

                Image startupLogoImage = Image.FromFile(GameStartData.logoFilePath);
                startupLogo.Image = startupLogoImage;
            }

            this.launcherTerminationWorker.RunWorkerAsync();
        }


        private void Launcher_Shown(object sender, EventArgs e)
        {
            /* The moment launcher was drawn on screen, start animation sequence. */
            Launcher_StartAnimation();


            /*
                *  In order to make launcher as flexible and as compatible with any game given...
                *  instead of hardcoding path to game executable, we're reading it from within a text file.
                *  
                *  Thefore attempting to start a process, it's important to verify that file exists in first place and that it isn't empty.
            */
            if (File.Exists(GameStartData.startupTargetFilePath) == false)
                ExitWithMessage($"Failed to initialize! File with startup target is missing!\n\n\"{GameStartData.startupTargetFilePath}\"", MessageBoxIcon.Error);

            string startupTarget = File.ReadAllText(GameStartData.startupTargetFilePath);
            if (string.IsNullOrEmpty(startupTarget))
                ExitWithMessage($"Failed to initialize! File with startup target is empty!\n\n\"{GameStartData.startupTargetFilePath}\"", MessageBoxIcon.Error);

            if (File.Exists(startupTarget) == false)
            {
                string startupTargetFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, startupTarget);
                if (File.Exists(startupTargetFullPath))
                    startupTarget = startupTargetFullPath;
                else
                    ExitWithMessage($"Failed to initialize! Couldn't locate startup target!\n\n\"{startupTarget}\".\n\nEnsure that game files are in place, they may have been downloaded or unpacked incorrectly.", MessageBoxIcon.Error);
            }


            /*
                *  In order to make launcher as flexible and as compatible with any game given...
                *  instead of hardcoding command line arguments, we're reading them from within a text file.
            */
            string startupArguments = string.Empty;
            if (File.Exists(GameStartData.startupArgumentsFilePath))
                startupArguments = File.ReadAllText(GameStartData.startupArgumentsFilePath);


            /*
                *  In order to make launcher as flexible and as compatible with any game given...
                *  launcher can account for dependencies game wouldn't function without of.
                *  
                *  Dependencies in file must be listed in different lines, one dependancy per line.
            */
            if (File.Exists(GameStartData.startupDependenciesPath))
            {
                string[] startupDependencies = File.ReadAllLines(GameStartData.startupDependenciesPath);
                foreach (string dependency in startupDependencies)
                {
                    if (File.Exists(dependency) == false)
                    {
                        string dependencyFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dependency);
                        if (File.Exists(dependencyFullPath) == false)
                            ExitWithMessage($"Failed to initialize! Couldn't locate required dependency!\n\n\"{dependency}\".\n\nEnsure that dependency is in place, it may have been removed by your local anti-virus.", MessageBoxIcon.Error);
                    }
                }
            }


            /* Starting a new process may lead to an security pop up to appear, we want to ensure user will be able to see & interact with it.  */
            this.TopMost = false;


            /* Attempt starting a new process using data we've obtained. */
            StartProcess(startupTarget, startupArguments);
        }


        private void launcherTerminationWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (GameStartAnimation.WindowFadeIn.complete == false || GameStartAnimation.ProgressBar.complete == false)
            {
                Thread.Sleep(10);
            }

            Thread.Sleep(1500);
            Exit();
        }
    }
}
