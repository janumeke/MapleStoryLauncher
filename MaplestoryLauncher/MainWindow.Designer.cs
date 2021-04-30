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
            this.autoSelect = new System.Windows.Forms.CheckBox();
            this.gamaotp_challenge_code_output = new System.Windows.Forms.Label();
            this.autoPaste = new System.Windows.Forms.CheckBox();
            this.rememberAccount = new System.Windows.Forms.CheckBox();
            this.autoLaunch = new System.Windows.Forms.CheckBox();
            this.autoLogin = new System.Windows.Forms.CheckBox();
            this.keepLogged = new System.Windows.Forms.CheckBox();
            this.rememberPwd = new System.Windows.Forms.CheckBox();
            this.getOtpButton = new System.Windows.Forms.Button();
            this.pwdInput = new System.Windows.Forms.TextBox();
            this.passLabel = new System.Windows.Forms.Label();
            this.otpDisplay = new System.Windows.Forms.TextBox();
            this.accountLabel = new System.Windows.Forms.Label();
            this.accountInput = new System.Windows.Forms.TextBox();
            this.accounts = new System.Windows.Forms.ListView();
            this.CharName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Account = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.loginButton = new System.Windows.Forms.Button();
            this.getOtpWorker = new System.ComponentModel.BackgroundWorker();
            this.loginWorker = new System.ComponentModel.BackgroundWorker();
            this.pingWorker = new System.ComponentModel.BackgroundWorker();
            this.Tip = new System.Windows.Forms.ToolTip(this.components);
            this.Notification = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.autoSelect);
            this.panel1.Controls.Add(this.gamaotp_challenge_code_output);
            this.panel1.Controls.Add(this.autoPaste);
            this.panel1.Controls.Add(this.rememberAccount);
            this.panel1.Controls.Add(this.autoLaunch);
            this.panel1.Controls.Add(this.autoLogin);
            this.panel1.Controls.Add(this.keepLogged);
            this.panel1.Controls.Add(this.rememberPwd);
            this.panel1.Controls.Add(this.getOtpButton);
            this.panel1.Controls.Add(this.pwdInput);
            this.panel1.Controls.Add(this.passLabel);
            this.panel1.Controls.Add(this.otpDisplay);
            this.panel1.Controls.Add(this.accountLabel);
            this.panel1.Controls.Add(this.accountInput);
            this.panel1.Controls.Add(this.accounts);
            this.panel1.Controls.Add(this.loginButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(340, 421);
            this.panel1.TabIndex = 0;
            // 
            // autoSelect
            // 
            this.autoSelect.AutoSize = true;
            this.autoSelect.Checked = global::MaplestoryLauncher.Properties.Settings.Default.autoSelect;
            this.autoSelect.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::MaplestoryLauncher.Properties.Settings.Default, "autoSelect", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.autoSelect.Location = new System.Drawing.Point(21, 334);
            this.autoSelect.Name = "autoSelect";
            this.autoSelect.Size = new System.Drawing.Size(75, 20);
            this.autoSelect.TabIndex = 7;
            this.autoSelect.Text = "記住選擇";
            this.autoSelect.UseVisualStyleBackColor = true;
            this.autoSelect.CheckedChanged += new System.EventHandler(this.autoSelect_CheckedChanged);
            // 
            // gamaotp_challenge_code_output
            // 
            this.gamaotp_challenge_code_output.AutoSize = true;
            this.gamaotp_challenge_code_output.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gamaotp_challenge_code_output.ForeColor = System.Drawing.Color.Red;
            this.gamaotp_challenge_code_output.Location = new System.Drawing.Point(97, 125);
            this.gamaotp_challenge_code_output.Name = "gamaotp_challenge_code_output";
            this.gamaotp_challenge_code_output.Size = new System.Drawing.Size(0, 21);
            this.gamaotp_challenge_code_output.TabIndex = 42;
            // 
            // autoPaste
            // 
            this.autoPaste.AutoSize = true;
            this.autoPaste.Checked = global::MaplestoryLauncher.Properties.Settings.Default.autoPaste;
            this.autoPaste.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoPaste.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::MaplestoryLauncher.Properties.Settings.Default, "autoPaste", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.autoPaste.Location = new System.Drawing.Point(101, 439);
            this.autoPaste.Name = "autoPaste";
            this.autoPaste.Size = new System.Drawing.Size(75, 20);
            this.autoPaste.TabIndex = 10;
            this.autoPaste.Text = "自動輸入";
            this.autoPaste.UseVisualStyleBackColor = true;
            this.autoPaste.Visible = false;
            this.autoPaste.CheckedChanged += new System.EventHandler(this.autoPaste_CheckedChanged);
            // 
            // rememberAccount
            // 
            this.rememberAccount.AutoSize = true;
            this.rememberAccount.Checked = global::MaplestoryLauncher.Properties.Settings.Default.rememberAccount;
            this.rememberAccount.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::MaplestoryLauncher.Properties.Settings.Default, "rememberAccount", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.rememberAccount.Location = new System.Drawing.Point(246, 26);
            this.rememberAccount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rememberAccount.Name = "rememberAccount";
            this.rememberAccount.Size = new System.Drawing.Size(75, 20);
            this.rememberAccount.TabIndex = 2;
            this.rememberAccount.Text = "記住帳號";
            this.rememberAccount.UseVisualStyleBackColor = true;
            this.rememberAccount.CheckedChanged += new System.EventHandler(this.rememberAccount_CheckedChanged);
            // 
            // autoLaunch
            // 
            this.autoLaunch.AutoSize = true;
            this.autoLaunch.Checked = global::MaplestoryLauncher.Properties.Settings.Default.opengame;
            this.autoLaunch.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::MaplestoryLauncher.Properties.Settings.Default, "opengame", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.autoLaunch.Location = new System.Drawing.Point(114, 334);
            this.autoLaunch.Name = "autoLaunch";
            this.autoLaunch.Size = new System.Drawing.Size(195, 20);
            this.autoLaunch.TabIndex = 8;
            this.autoLaunch.Text = "登入後自動啟動遊戲或取得密碼";
            this.autoLaunch.UseVisualStyleBackColor = true;
            this.autoLaunch.CheckedChanged += new System.EventHandler(this.autoLaunch_CheckedChanged);
            // 
            // autoLogin
            // 
            this.autoLogin.AutoSize = true;
            this.autoLogin.Location = new System.Drawing.Point(210, 122);
            this.autoLogin.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.autoLogin.Name = "autoLogin";
            this.autoLogin.Size = new System.Drawing.Size(111, 20);
            this.autoLogin.TabIndex = 4;
            this.autoLogin.Text = "啟動時自動登入";
            this.autoLogin.UseVisualStyleBackColor = true;
            this.autoLogin.CheckedChanged += new System.EventHandler(this.AutoLogin_CheckedChanged);
            // 
            // keepLogged
            // 
            this.keepLogged.AutoSize = true;
            this.keepLogged.Checked = global::MaplestoryLauncher.Properties.Settings.Default.keepLogged;
            this.keepLogged.CheckState = System.Windows.Forms.CheckState.Checked;
            this.keepLogged.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::MaplestoryLauncher.Properties.Settings.Default, "keepLogged", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keepLogged.Enabled = false;
            this.keepLogged.Location = new System.Drawing.Point(20, 439);
            this.keepLogged.Name = "keepLogged";
            this.keepLogged.Size = new System.Drawing.Size(75, 20);
            this.keepLogged.TabIndex = 8;
            this.keepLogged.Text = "保持登入";
            this.keepLogged.UseVisualStyleBackColor = true;
            this.keepLogged.Visible = false;
            this.keepLogged.CheckedChanged += new System.EventHandler(this.keepLogged_CheckedChanged);
            // 
            // rememberPwd
            // 
            this.rememberPwd.AutoSize = true;
            this.rememberPwd.Checked = global::MaplestoryLauncher.Properties.Settings.Default.rememberPwd;
            this.rememberPwd.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::MaplestoryLauncher.Properties.Settings.Default, "rememberPwd", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.rememberPwd.Location = new System.Drawing.Point(246, 66);
            this.rememberPwd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rememberPwd.Name = "rememberPwd";
            this.rememberPwd.Size = new System.Drawing.Size(75, 20);
            this.rememberPwd.TabIndex = 3;
            this.rememberPwd.Text = "記住密碼";
            this.rememberPwd.UseVisualStyleBackColor = true;
            this.rememberPwd.CheckedChanged += new System.EventHandler(this.rememberPwd_CheckedChanged);
            // 
            // getOtpButton
            // 
            this.getOtpButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.getOtpButton.Location = new System.Drawing.Point(20, 366);
            this.getOtpButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.getOtpButton.Name = "getOtpButton";
            this.getOtpButton.Size = new System.Drawing.Size(145, 27);
            this.getOtpButton.TabIndex = 9;
            this.getOtpButton.Text = "啟動遊戲";
            this.getOtpButton.UseVisualStyleBackColor = true;
            this.getOtpButton.Click += new System.EventHandler(this.getOtpButton_Click);
            // 
            // pwdInput
            // 
            this.pwdInput.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.pwdInput.Location = new System.Drawing.Point(73, 64);
            this.pwdInput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pwdInput.Name = "pwdInput";
            this.pwdInput.PasswordChar = '*';
            this.pwdInput.Size = new System.Drawing.Size(145, 23);
            this.pwdInput.TabIndex = 1;
            // 
            // passLabel
            // 
            this.passLabel.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.passLabel.Location = new System.Drawing.Point(17, 64);
            this.passLabel.Name = "passLabel";
            this.passLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.passLabel.Size = new System.Drawing.Size(50, 23);
            this.passLabel.TabIndex = 28;
            this.passLabel.Text = "密碼";
            this.passLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // otpDisplay
            // 
            this.otpDisplay.Location = new System.Drawing.Point(196, 368);
            this.otpDisplay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.otpDisplay.Name = "otpDisplay";
            this.otpDisplay.ReadOnly = true;
            this.otpDisplay.Size = new System.Drawing.Size(125, 23);
            this.otpDisplay.TabIndex = 10;
            this.otpDisplay.TabStop = false;
            this.otpDisplay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.otpDisplay.Click += new System.EventHandler(this.otpDisplay_OnClick);
            // 
            // accountLabel
            // 
            this.accountLabel.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.accountLabel.Location = new System.Drawing.Point(17, 24);
            this.accountLabel.Name = "accountLabel";
            this.accountLabel.Size = new System.Drawing.Size(50, 23);
            this.accountLabel.TabIndex = 27;
            this.accountLabel.Text = "帳號";
            this.accountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // accountInput
            // 
            this.accountInput.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.accountInput.Location = new System.Drawing.Point(73, 24);
            this.accountInput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.accountInput.Name = "accountInput";
            this.accountInput.Size = new System.Drawing.Size(145, 23);
            this.accountInput.TabIndex = 0;
            // 
            // accounts
            // 
            this.accounts.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CharName,
            this.Account});
            this.accounts.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.accounts.FullRowSelect = true;
            this.accounts.GridLines = true;
            this.accounts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.accounts.HideSelection = false;
            this.accounts.LabelEdit = true;
            this.accounts.Location = new System.Drawing.Point(21, 192);
            this.accounts.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.accounts.MultiSelect = false;
            this.accounts.Name = "accounts";
            this.accounts.Size = new System.Drawing.Size(300, 128);
            this.accounts.TabIndex = 6;
            this.accounts.UseCompatibleStateImageBehavior = false;
            this.accounts.View = System.Windows.Forms.View.Details;
            this.accounts.DoubleClick += new System.EventHandler(this.accounts_DoubleClick);
            // 
            // CharName
            // 
            this.CharName.Text = "遊戲帳號";
            this.CharName.Width = 170;
            // 
            // Account
            // 
            this.Account.Text = "登入帳號(雙擊複製)";
            this.Account.Width = 125;
            // 
            // loginButton
            // 
            this.loginButton.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.loginButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.loginButton.Location = new System.Drawing.Point(21, 116);
            this.loginButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(145, 29);
            this.loginButton.TabIndex = 5;
            this.loginButton.Text = "登入";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
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
            this.Tip.IsBalloon = true;
            this.Tip.ReshowDelay = 100;
            // 
            // Notification
            // 
            this.Notification.AutoPopDelay = 5000;
            this.Notification.InitialDelay = 0;
            this.Notification.ReshowDelay = 100;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 421);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "BeanfunLogin - By Kai";
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.main_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Label gamaotp_challenge_code_output;
        private System.Windows.Forms.CheckBox autoLaunch;
        private System.Windows.Forms.CheckBox autoPaste;
        private System.Windows.Forms.CheckBox autoSelect;
    }
}

