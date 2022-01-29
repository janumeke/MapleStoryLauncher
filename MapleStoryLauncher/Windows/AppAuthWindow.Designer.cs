
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppAuthWindow));
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelWaiting = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkAppAuthStatusTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelMessage.Location = new System.Drawing.Point(40, 20);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(231, 38);
            this.labelMessage.TabIndex = 0;
            this.labelMessage.Text = "此帳號已開啟[進階防護]，\r\n請在 beanfun! App 中允許登入要求。";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelWaiting
            // 
            this.labelWaiting.AutoSize = true;
            this.labelWaiting.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelWaiting.Location = new System.Drawing.Point(125, 75);
            this.labelWaiting.Name = "labelWaiting";
            this.labelWaiting.Size = new System.Drawing.Size(87, 19);
            this.labelWaiting.TabIndex = 1;
            this.labelWaiting.Text = "等待回應中 ...";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::MapleStoryLauncher.Properties.Resources.loading;
            this.pictureBox1.Location = new System.Drawing.Point(95, 75);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // checkAppAuthStatusTimer
            // 
            this.checkAppAuthStatusTimer.Enabled = true;
            this.checkAppAuthStatusTimer.Interval = 3000;
            this.checkAppAuthStatusTimer.Tick += new System.EventHandler(this.checkAppAuthStatusTimer_Tick);
            // 
            // AppAuthWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(305, 111);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelWaiting);
            this.Controls.Add(this.labelMessage);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppAuthWindow";
            this.Text = "兩階段驗證";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AppAuthWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelWaiting;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer checkAppAuthStatusTimer;
    }
}