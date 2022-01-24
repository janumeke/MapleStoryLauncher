namespace MaplestoryLauncher
{
    partial class MainWindow
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBoxBeanfun = new System.Windows.Forms.GroupBox();
            this.accountLabel = new System.Windows.Forms.Label();
            this.loginButton = new System.Windows.Forms.Button();
            this.accountInput = new System.Windows.Forms.TextBox();
            this.passLabel = new System.Windows.Forms.Label();
            this.rememberAccount = new System.Windows.Forms.CheckBox();
            this.pwdInput = new System.Windows.Forms.TextBox();
            this.rememberPwd = new System.Windows.Forms.CheckBox();
            this.autoLogin = new System.Windows.Forms.CheckBox();
            this.autoSelect = new System.Windows.Forms.CheckBox();
            this.autoLaunch = new System.Windows.Forms.CheckBox();
            this.keepLogged = new System.Windows.Forms.CheckBox();
            this.getOtpButton = new System.Windows.Forms.Button();
            this.otpDisplay = new System.Windows.Forms.TextBox();
            this.accounts = new System.Windows.Forms.ListView();
            this.CharName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Account = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.getOtpWorker = new System.ComponentModel.BackgroundWorker();
            this.loginWorker = new System.ComponentModel.BackgroundWorker();
            this.pingWorker = new System.ComponentModel.BackgroundWorker();
            this.Tip = new System.Windows.Forms.ToolTip(this.components);
            this.Notification = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxBeanfun.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.groupBoxBeanfun);
            this.panel1.Controls.Add(this.autoSelect);
            this.panel1.Controls.Add(this.autoLaunch);
            this.panel1.Controls.Add(this.keepLogged);
            this.panel1.Controls.Add(this.getOtpButton);
            this.panel1.Controls.Add(this.otpDisplay);
            this.panel1.Controls.Add(this.accounts);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(340, 441);
            this.panel1.TabIndex = 0;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(17, 195);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(18, 18);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 101;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(15, 15);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(18, 18);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 29;
            this.pictureBox1.TabStop = false;
            // 
            // groupBoxBeanfun
            // 
            this.groupBoxBeanfun.Controls.Add(this.accountLabel);
            this.groupBoxBeanfun.Controls.Add(this.loginButton);
            this.groupBoxBeanfun.Controls.Add(this.accountInput);
            this.groupBoxBeanfun.Controls.Add(this.passLabel);
            this.groupBoxBeanfun.Controls.Add(this.rememberAccount);
            this.groupBoxBeanfun.Controls.Add(this.pwdInput);
            this.groupBoxBeanfun.Controls.Add(this.rememberPwd);
            this.groupBoxBeanfun.Controls.Add(this.autoLogin);
            this.groupBoxBeanfun.Font = new System.Drawing.Font("Microsoft YaHei UI Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxBeanfun.Location = new System.Drawing.Point(13, 4);
            this.groupBoxBeanfun.Name = "groupBoxBeanfun";
            this.groupBoxBeanfun.Size = new System.Drawing.Size(315, 175);
            this.groupBoxBeanfun.TabIndex = 100;
            this.groupBoxBeanfun.TabStop = false;
            // 
            // accountLabel
            // 
            this.accountLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.accountLabel.Location = new System.Drawing.Point(10, 35);
            this.accountLabel.Name = "accountLabel";
            this.accountLabel.Size = new System.Drawing.Size(50, 23);
            this.accountLabel.TabIndex = 101;
            this.accountLabel.Text = "帳號";
            this.accountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // loginButton
            // 
            this.loginButton.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loginButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.loginButton.Location = new System.Drawing.Point(19, 130);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(145, 31);
            this.loginButton.TabIndex = 5;
            this.loginButton.Text = "登入";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // accountInput
            // 
            this.accountInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.accountInput.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.accountInput.Location = new System.Drawing.Point(65, 35);
            this.accountInput.Name = "accountInput";
            this.accountInput.Size = new System.Drawing.Size(145, 23);
            this.accountInput.TabIndex = 0;
            this.accountInput.TextChanged += new System.EventHandler(this.Input_TextChanged);
            // 
            // passLabel
            // 
            this.passLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passLabel.Location = new System.Drawing.Point(10, 75);
            this.passLabel.Name = "passLabel";
            this.passLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.passLabel.Size = new System.Drawing.Size(50, 23);
            this.passLabel.TabIndex = 102;
            this.passLabel.Text = "密碼";
            this.passLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rememberAccount
            // 
            this.rememberAccount.AutoSize = true;
            this.rememberAccount.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rememberAccount.Location = new System.Drawing.Point(230, 38);
            this.rememberAccount.Name = "rememberAccount";
            this.rememberAccount.Size = new System.Drawing.Size(75, 21);
            this.rememberAccount.TabIndex = 2;
            this.rememberAccount.Text = "記住帳號";
            this.rememberAccount.UseVisualStyleBackColor = true;
            this.rememberAccount.CheckedChanged += new System.EventHandler(this.rememberAccount_CheckedChanged);
            // 
            // pwdInput
            // 
            this.pwdInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pwdInput.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.pwdInput.Location = new System.Drawing.Point(65, 75);
            this.pwdInput.Name = "pwdInput";
            this.pwdInput.PasswordChar = '*';
            this.pwdInput.Size = new System.Drawing.Size(145, 23);
            this.pwdInput.TabIndex = 1;
            this.pwdInput.TextChanged += new System.EventHandler(this.Input_TextChanged);
            // 
            // rememberPwd
            // 
            this.rememberPwd.AutoSize = true;
            this.rememberPwd.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rememberPwd.Location = new System.Drawing.Point(230, 78);
            this.rememberPwd.Name = "rememberPwd";
            this.rememberPwd.Size = new System.Drawing.Size(75, 21);
            this.rememberPwd.TabIndex = 3;
            this.rememberPwd.Text = "記住密碼";
            this.rememberPwd.UseVisualStyleBackColor = true;
            this.rememberPwd.CheckedChanged += new System.EventHandler(this.rememberPwd_CheckedChanged);
            // 
            // autoLogin
            // 
            this.autoLogin.AutoSize = true;
            this.autoLogin.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoLogin.Location = new System.Drawing.Point(195, 136);
            this.autoLogin.Name = "autoLogin";
            this.autoLogin.Size = new System.Drawing.Size(111, 21);
            this.autoLogin.TabIndex = 4;
            this.autoLogin.Text = "啟動時自動登入";
            this.autoLogin.UseVisualStyleBackColor = true;
            this.autoLogin.CheckedChanged += new System.EventHandler(this.autoLogin_CheckedChanged);
            // 
            // autoSelect
            // 
            this.autoSelect.AutoSize = true;
            this.autoSelect.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoSelect.Location = new System.Drawing.Point(33, 370);
            this.autoSelect.Name = "autoSelect";
            this.autoSelect.Size = new System.Drawing.Size(75, 21);
            this.autoSelect.TabIndex = 7;
            this.autoSelect.TabStop = false;
            this.autoSelect.Text = "記住選擇";
            this.autoSelect.UseVisualStyleBackColor = true;
            this.autoSelect.CheckedChanged += new System.EventHandler(this.autoSelect_CheckedChanged);
            // 
            // autoLaunch
            // 
            this.autoLaunch.AutoSize = true;
            this.autoLaunch.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoLaunch.Location = new System.Drawing.Point(128, 370);
            this.autoLaunch.Name = "autoLaunch";
            this.autoLaunch.Size = new System.Drawing.Size(99, 21);
            this.autoLaunch.TabIndex = 8;
            this.autoLaunch.TabStop = false;
            this.autoLaunch.Text = "自動啟動遊戲";
            this.autoLaunch.UseVisualStyleBackColor = true;
            // 
            // keepLogged
            // 
            this.keepLogged.AutoSize = true;
            this.keepLogged.Checked = global::MaplestoryLauncher.Properties.Settings.Default.keepLogged;
            this.keepLogged.CheckState = System.Windows.Forms.CheckState.Checked;
            this.keepLogged.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::MaplestoryLauncher.Properties.Settings.Default, "keepLogged", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keepLogged.Enabled = false;
            this.keepLogged.Location = new System.Drawing.Point(20, 465);
            this.keepLogged.Name = "keepLogged";
            this.keepLogged.Size = new System.Drawing.Size(75, 21);
            this.keepLogged.TabIndex = 8;
            this.keepLogged.Text = "保持登入";
            this.keepLogged.UseVisualStyleBackColor = true;
            this.keepLogged.Visible = false;
            this.keepLogged.CheckedChanged += new System.EventHandler(this.keepLogged_CheckedChanged);
            // 
            // getOtpButton
            // 
            this.getOtpButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.getOtpButton.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getOtpButton.Location = new System.Drawing.Point(18, 400);
            this.getOtpButton.Name = "getOtpButton";
            this.getOtpButton.Size = new System.Drawing.Size(145, 29);
            this.getOtpButton.TabIndex = 9;
            this.getOtpButton.TabStop = false;
            this.getOtpButton.Text = "啟動遊戲";
            this.getOtpButton.UseVisualStyleBackColor = true;
            this.getOtpButton.Click += new System.EventHandler(this.getOtpButton_Click);
            // 
            // otpDisplay
            // 
            this.otpDisplay.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.otpDisplay.Location = new System.Drawing.Point(198, 403);
            this.otpDisplay.Name = "otpDisplay";
            this.otpDisplay.PasswordChar = '*';
            this.otpDisplay.ReadOnly = true;
            this.otpDisplay.Size = new System.Drawing.Size(125, 23);
            this.otpDisplay.TabIndex = 10;
            this.otpDisplay.TabStop = false;
            this.otpDisplay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.otpDisplay.Click += new System.EventHandler(this.otpDisplay_OnClick);
            this.otpDisplay.DoubleClick += new System.EventHandler(this.otpDisplay_DoubleClick);
            // 
            // accounts
            // 
            this.accounts.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CharName,
            this.Account});
            this.accounts.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.accounts.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.accounts.FullRowSelect = true;
            this.accounts.GridLines = true;
            this.accounts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.accounts.HideSelection = false;
            this.accounts.Location = new System.Drawing.Point(18, 220);
            this.accounts.MultiSelect = false;
            this.accounts.Name = "accounts";
            this.accounts.Size = new System.Drawing.Size(305, 138);
            this.accounts.TabIndex = 6;
            this.accounts.TabStop = false;
            this.accounts.UseCompatibleStateImageBehavior = false;
            this.accounts.View = System.Windows.Forms.View.Details;
            this.accounts.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.accounts_ItemSelectionChanged);
            this.accounts.DoubleClick += new System.EventHandler(this.accounts_DoubleClick);
            // 
            // CharName
            // 
            this.CharName.Text = "遊戲帳號";
            this.CharName.Width = 185;
            // 
            // Account
            // 
            this.Account.Text = "登入帳號";
            this.Account.Width = 115;
            // 
            // getOtpWorker
            // 
            this.getOtpWorker.WorkerReportsProgress = true;
            this.getOtpWorker.WorkerSupportsCancellation = true;
            this.getOtpWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.getOtpWorker_DoWork);
            this.getOtpWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.getOtpWorker_RunWorkerCompleted);
            // 
            // loginWorker
            // 
            this.loginWorker.WorkerReportsProgress = true;
            this.loginWorker.WorkerSupportsCancellation = true;
            this.loginWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.loginWorker_DoWork);
            this.loginWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.loginWorker_RunWorkerCompleted);
            // 
            // pingWorker
            // 
            this.pingWorker.WorkerReportsProgress = true;
            this.pingWorker.WorkerSupportsCancellation = true;
            this.pingWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.pingWorker_DoWork);
            this.pingWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.pingWorker_RunWorkerCompleted);
            // 
            // Tip
            // 
            this.Tip.AutoPopDelay = 5000;
            this.Tip.InitialDelay = 500;
            this.Tip.ReshowDelay = 100;
            // 
            // Notification
            // 
            this.Notification.AutoPopDelay = 5000;
            this.Notification.InitialDelay = 0;
            this.Notification.IsBalloon = true;
            this.Notification.ReshowDelay = 100;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 441);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "BeanfunLogin - By Kai";
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxBeanfun.ResumeLayout(false);
            this.groupBoxBeanfun.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView accounts;
        private System.Windows.Forms.ColumnHeader CharName;
        private System.Windows.Forms.ColumnHeader Account;
        private System.Windows.Forms.CheckBox autoLogin;
        private System.Windows.Forms.CheckBox rememberPwd;
        private System.Windows.Forms.CheckBox rememberAccount;
        private System.Windows.Forms.TextBox pwdInput;
        private System.Windows.Forms.Label passLabel;
        private System.Windows.Forms.Label accountLabel;
        private System.Windows.Forms.TextBox accountInput;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.TextBox otpDisplay;
        private System.Windows.Forms.Button getOtpButton;
        private System.ComponentModel.BackgroundWorker getOtpWorker;
        private System.ComponentModel.BackgroundWorker loginWorker;
        private System.ComponentModel.BackgroundWorker pingWorker;
        private System.Windows.Forms.CheckBox keepLogged;
        private System.Windows.Forms.ToolTip Tip;
        private System.Windows.Forms.ToolTip Notification;
        private System.Windows.Forms.CheckBox autoLaunch;
        private System.Windows.Forms.CheckBox autoSelect;
        private System.Windows.Forms.GroupBox groupBoxBeanfun;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}

