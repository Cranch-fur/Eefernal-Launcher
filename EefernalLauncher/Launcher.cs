using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;






namespace EefernalLauncher
{
    public partial class Launcher : Form
    {
        const string splashScreenFilePath = @"EefernalFog\SplashScreen.mfx";
        const string logoFilePath         = @"EefernalFog\Logo.mfx";

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
            {
                Task.Run(async () =>
                {
                    await Task.Delay(millisecondsDelay);
                    Exit();
                });
            }
            else
            {
                Exit();
            }
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






        private void Launcher_AnimationTick(object sender, EventArgs e)
        {
            double newValue = this.Opacity + 0.025;
            if (newValue <= 1.0)
                this.Opacity = newValue;
            else
                startupAnimation.Tick -= Launcher_AnimationTick;

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
                Image backgroundImage = Image.FromFile(splashScreenFilePath);
                this.BackgroundImage = backgroundImage;
            }


            if (File.Exists(logoFilePath))
            {
                startupLogo.Parent = this;
                startupLogo.BackColor = Color.Transparent;

                Image startupLogoImage = Image.FromFile(logoFilePath);
                startupLogo.Image = startupLogoImage;
            }
        }


        private void Launcher_Shown(object sender, EventArgs e)
        {
            startupAnimation = new Timer();
            startupAnimation.Interval = 1;

            startupAnimation.Tick += Launcher_AnimationTick;
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


            using (Process targetProcess = new Process())
            {
                targetProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(startupTarget);
                targetProcess.StartInfo.FileName = Path.GetFileName(startupTarget);
                targetProcess.StartInfo.Arguments = startupArguments;


                try
                {
                    targetProcess.Start();
                    if (targetProcess.HasExited)
                    {
                        Exit("Failed to start process", MessageBoxIcon.Error);
                    }
                }
                catch 
                {
                    Exit("Failed to start process", MessageBoxIcon.Error);
                }
            }


            Exit(3500);
        }
    }
}
