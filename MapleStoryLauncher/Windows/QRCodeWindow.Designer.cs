
namespace MapleStoryLauncher
{
    partial class QRCodeWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QRCodeWindow));
            this.qrcodeDisplay = new System.Windows.Forms.PictureBox();
            this.getQRCodeWorker = new System.ComponentModel.BackgroundWorker();
            this.checkQRCodeStatusTimer = new System.Windows.Forms.Timer(this.components);
            this.labelProgress = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.qrcodeDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // qrcodeDisplay
            // 
            this.qrcodeDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.qrcodeDisplay.Location = new System.Drawing.Point(7, 7);
            this.qrcodeDisplay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.qrcodeDisplay.Name = "qrcodeDisplay";
            this.qrcodeDisplay.Size = new System.Drawing.Size(256, 256);
            this.qrcodeDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.qrcodeDisplay.TabIndex = 0;
            this.qrcodeDisplay.TabStop = false;
            this.qrcodeDisplay.Visible = false;
            // 
            // getQRCodeWorker
            // 
            this.getQRCodeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.getQRCodeWorker_DoWork);
            this.getQRCodeWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.getQRCodeWorker_RunWorkerCompleted);
            // 
            // checkQRCodeStatusTimer
            // 
            this.checkQRCodeStatusTimer.Interval = 3000;
            this.checkQRCodeStatusTimer.Tick += new System.EventHandler(this.checkQRCodeStatusTimer_Tick);
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelProgress.Location = new System.Drawing.Point(17, 15);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(84, 25);
            this.labelProgress.TabIndex = 1;
            this.labelProgress.Text = "取得中...";
            this.labelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelProgress.Visible = false;
            // 
            // QRCodeWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(270, 271);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.qrcodeDisplay);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QRCodeWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QRCode";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QRCodeWindow_FormClosing);
            this.Shown += new System.EventHandler(this.QRCodeWindow_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.qrcodeDisplay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox qrcodeDisplay;
        private System.ComponentModel.BackgroundWorker getQRCodeWorker;
        private System.Windows.Forms.Timer checkQRCodeStatusTimer;
        private System.Windows.Forms.Label labelProgress;
    }
}