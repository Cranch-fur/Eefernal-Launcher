namespace EefernalLauncher
{
    partial class Launcher
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.startupLogo = new System.Windows.Forms.PictureBox();
            this.backgroundWorkerExit = new System.ComponentModel.BackgroundWorker();
            this.startupProgressBar = new GradientProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.startupLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // startupLogo
            // 
            this.startupLogo.Image = global::EefernalLauncher.Properties.Resources.Img_Logo;
            this.startupLogo.Location = new System.Drawing.Point(12, 363);
            this.startupLogo.Name = "startupLogo";
            this.startupLogo.Size = new System.Drawing.Size(200, 65);
            this.startupLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.startupLogo.TabIndex = 1;
            this.startupLogo.TabStop = false;
            // 
            // backgroundWorkerExit
            // 
            this.backgroundWorkerExit.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerExit_DoWork);
            // 
            // startupProgressBar
            // 
            this.startupProgressBar.BorderColor = System.Drawing.Color.Black;
            this.startupProgressBar.BorderThickness = 0;
            this.startupProgressBar.EndColor = System.Drawing.Color.PaleTurquoise;
            this.startupProgressBar.Location = new System.Drawing.Point(0, 440);
            this.startupProgressBar.Maximum = 150;
            this.startupProgressBar.Name = "startupProgressBar";
            this.startupProgressBar.ShowBorder = false;
            this.startupProgressBar.Size = new System.Drawing.Size(800, 10);
            this.startupProgressBar.StartColor = System.Drawing.Color.DodgerBlue;
            this.startupProgressBar.TabIndex = 0;
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::EefernalLauncher.Properties.Resources.Img_Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.startupLogo);
            this.Controls.Add(this.startupProgressBar);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Launcher";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Eefernal Fog";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Launcher_Load);
            this.Shown += new System.EventHandler(this.Launcher_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.startupLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private GradientProgressBar startupProgressBar;
        private System.Windows.Forms.PictureBox startupLogo;
        private System.ComponentModel.BackgroundWorker backgroundWorkerExit;
    }
}

