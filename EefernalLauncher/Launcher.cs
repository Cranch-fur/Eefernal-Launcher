using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Timer = System.Windows.Forms.Timer;






namespace EefernalLauncher
{
    public partial class Launcher : Form
    {
        const string splashScreenFilePath = @"EefernalFog\SplashScreen.png";
        const string logoFilePath         = @"EefernalFog\Logo.png";

        const string startupTargetFilePath    = @"EefernalFog\StartupTarget.txt";
        const string startupArgumentsFilePath = @"EefernalFog\StartupArguments.txt";


        Timer startupAnimation = new Timer();






        private void Exit()
        {
            Environment.Exit(0);
        }
        private void Exit(int millisecondsDelay)
        {
            if (millisecondsDelay > 0)
                Thread.Sleep(millisecondsDelay);


            Exit();
        }
        private void Exit(string message, MessageBoxIcon messageType)
        {
            if (!string.IsNullOrEmpty(message))
            {
                startupAnimation.Enabled = false;
                MessageBox.Show(message, "Eefernal Launcher", MessageBoxButtons.OK, messageType, MessageBoxDefaultButton.Button1);
            }
                

            Exit();
        }




        private void startupProgressBar_AnimationTick(object sender, EventArgs e)
        {
            int newValue = startupProgressBar.Value + 1;
            if (newValue <= startupProgressBar.Maximum)
                startupProgressBar.Value = newValue;
            else
                startupAnimation.Tick -= startupProgressBar_AnimationTick;
        }






        public Launcher()
        {
            InitializeComponent();
        }




        private void Launcher_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.Icon;


            if (File.Exists(splashScreenFilePath)) 
            { 
                this.BackgroundImage = Image.FromFile(splashScreenFilePath);
            }


            if (File.Exists(logoFilePath))
            {
                startupLogo.Parent = this;
                startupLogo.BackColor = Color.Transparent;

                startupLogo.Image = Image.FromFile(logoFilePath);
            }
        }


        private void Launcher_Shown(object sender, EventArgs e)
        {
            startupAnimation = new Timer();
            startupAnimation.Interval = 1;

            startupAnimation.Tick += startupProgressBar_AnimationTick;
            startupAnimation.Start();


            if (!File.Exists(startupTargetFilePath))
                Exit($"Startup target file wasn't found!\n\n\"{startupTargetFilePath}\"", MessageBoxIcon.Error);

            string startupTarget = File.ReadAllText(startupTargetFilePath);
            if (string.IsNullOrEmpty(startupTarget))
                Exit($"Startup target file is empty!\n\n\"{startupTargetFilePath}\"", MessageBoxIcon.Error);

            if (!File.Exists(startupTarget))
                Exit($"Startup target doesn't exist!\n\n\"{startupTarget}\"", MessageBoxIcon.Error);


            string startupArguments = string.Empty;
            if (File.Exists(startupArgumentsFilePath))
            {
                startupArguments = File.ReadAllText(startupArgumentsFilePath);
            }


            try
            {
                using (Process targetProcess = new Process())
                {
                    targetProcess.StartInfo.FileName = startupTarget;
                    targetProcess.StartInfo.Arguments = startupArguments;


                    targetProcess.Start();
                    if (targetProcess.HasExited)
                    {
                        Exit("Failed to start process", MessageBoxIcon.Error);
                    }
                }
            }
            catch { }


            Exit(1000);
        }
    }
}
