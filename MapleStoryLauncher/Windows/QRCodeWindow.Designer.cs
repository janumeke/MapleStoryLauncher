
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QRCodeWindow));
            qrcodeDisplay = new PictureBox();
            getQRCodeWorker = new System.ComponentModel.BackgroundWorker();
            checkQRCodeStatusTimer = new System.Windows.Forms.Timer(components);
            labelProgress = new Label();
            ((System.ComponentModel.ISupportInitialize)qrcodeDisplay).BeginInit();
            SuspendLayout();
            // 
            // qrcodeDisplay
            // 
            qrcodeDisplay.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            qrcodeDisplay.Location = new Point(7, 7);
            qrcodeDisplay.Margin = new Padding(3, 4, 3, 4);
            qrcodeDisplay.Name = "qrcodeDisplay";
            qrcodeDisplay.Size = new Size(256, 256);
            qrcodeDisplay.SizeMode = PictureBoxSizeMode.Zoom;
            qrcodeDisplay.TabIndex = 0;
            qrcodeDisplay.TabStop = false;
            qrcodeDisplay.Visible = false;
            // 
            // getQRCodeWorker
            // 
            getQRCodeWorker.DoWork += getQRCodeWorker_DoWork;
            getQRCodeWorker.RunWorkerCompleted += getQRCodeWorker_RunWorkerCompleted;
            // 
            // checkQRCodeStatusTimer
            // 
            checkQRCodeStatusTimer.Interval = 3000;
            checkQRCodeStatusTimer.Tick += checkQRCodeStatusTimer_Tick;
            // 
            // labelProgress
            // 
            labelProgress.AutoSize = true;
            labelProgress.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelProgress.Location = new Point(17, 15);
            labelProgress.Name = "labelProgress";
            labelProgress.Size = new Size(84, 25);
            labelProgress.TabIndex = 1;
            labelProgress.Text = "取得中...";
            labelProgress.TextAlign = ContentAlignment.MiddleCenter;
            labelProgress.Visible = false;
            // 
            // QRCodeWindow
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(270, 271);
            Controls.Add(labelProgress);
            Controls.Add(qrcodeDisplay);
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "QRCodeWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "QRCode";
            TopMost = true;
            VisibleChanged += QRCodeWindow_VisibleChanged;
            ((System.ComponentModel.ISupportInitialize)qrcodeDisplay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox qrcodeDisplay;
        private System.ComponentModel.BackgroundWorker getQRCodeWorker;
        private System.Windows.Forms.Timer checkQRCodeStatusTimer;
        private System.Windows.Forms.Label labelProgress;
    }
}