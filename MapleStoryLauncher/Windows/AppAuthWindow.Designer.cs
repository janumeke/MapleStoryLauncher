
namespace MapleStoryLauncher
{
    partial class AppAuthWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppAuthWindow));
            labelMessage = new Label();
            labelWaiting = new Label();
            pictureBoxWaiting = new PictureBox();
            checkAppAuthStatusTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)pictureBoxWaiting).BeginInit();
            SuspendLayout();
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            labelMessage.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelMessage.Location = new Point(43, 17);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(231, 38);
            labelMessage.TabIndex = 0;
            labelMessage.Text = "此帳號已開啟「進階防護」，\r\n請在 beanfun! App 中允許登入要求。";
            labelMessage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelWaiting
            // 
            labelWaiting.AutoSize = true;
            labelWaiting.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelWaiting.Location = new Point(129, 71);
            labelWaiting.Name = "labelWaiting";
            labelWaiting.Size = new Size(87, 19);
            labelWaiting.TabIndex = 1;
            labelWaiting.Text = "等待回應中 ...";
            // 
            // pictureBoxWaiting
            // 
            pictureBoxWaiting.Image = Properties.Resources.loading;
            pictureBoxWaiting.Location = new Point(101, 71);
            pictureBoxWaiting.Name = "pictureBoxWaiting";
            pictureBoxWaiting.Size = new Size(20, 20);
            pictureBoxWaiting.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxWaiting.TabIndex = 2;
            pictureBoxWaiting.TabStop = false;
            // 
            // checkAppAuthStatusTimer
            // 
            checkAppAuthStatusTimer.Interval = 3000;
            checkAppAuthStatusTimer.Tick += checkAppAuthStatusTimer_Tick;
            // 
            // AppAuthWindow
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(306, 110);
            Controls.Add(pictureBoxWaiting);
            Controls.Add(labelWaiting);
            Controls.Add(labelMessage);
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AppAuthWindow";
            Text = "雙重驗證";
            VisibleChanged += AppAuthWindow_VisibleChanged;
            ((System.ComponentModel.ISupportInitialize)pictureBoxWaiting).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelMessage;
        private Label labelWaiting;
        private PictureBox pictureBoxWaiting;
        private System.Windows.Forms.Timer checkAppAuthStatusTimer;
    }
}