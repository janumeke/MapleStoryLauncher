namespace MapleStoryLauncher
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
            this.pointsLabel = new System.Windows.Forms.Label();
            this.maplePictureBox = new System.Windows.Forms.PictureBox();
            this.beanfunPictureBox = new System.Windows.Forms.PictureBox();
            this.groupBoxBeanfun = new System.Windows.Forms.GroupBox();
            this.AddRemoveAccount = new System.Windows.Forms.PictureBox();
            this.accountInput = new System.Windows.Forms.ComboBox();
            this.accountLabel = new System.Windows.Forms.Label();
            this.loginButton = new System.Windows.Forms.Button();
            this.passLabel = new System.Windows.Forms.Label();
            this.passwordInput = new System.Windows.Forms.TextBox();
            this.rememberPwd = new System.Windows.Forms.CheckBox();
            this.autoLogin = new System.Windows.Forms.CheckBox();
            this.autoSelect = new System.Windows.Forms.CheckBox();
            this.autoLaunch = new System.Windows.Forms.CheckBox();
            this.getOtpButton = new System.Windows.Forms.Button();
            this.otpDisplay = new System.Windows.Forms.TextBox();
            this.accountListView = new System.Windows.Forms.ListView();
            this.Nickname = new System.Windows.Forms.ColumnHeader();
            this.Account = new System.Windows.Forms.ColumnHeader();
            this.getOtpWorker = new System.ComponentModel.BackgroundWorker();
            this.loginWorker = new System.ComponentModel.BackgroundWorker();
            this.Tip = new System.Windows.Forms.ToolTip(this.components);
            this.Notification = new System.Windows.Forms.ToolTip(this.components);
            this.pingTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maplePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.beanfunPictureBox)).BeginInit();
            this.groupBoxBeanfun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AddRemoveAccount)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pointsLabel);
            this.panel1.Controls.Add(this.maplePictureBox);
            this.panel1.Controls.Add(this.beanfunPictureBox);
            this.panel1.Controls.Add(this.groupBoxBeanfun);
            this.panel1.Controls.Add(this.autoSelect);
            this.panel1.Controls.Add(this.autoLaunch);
            this.panel1.Controls.Add(this.getOtpButton);
            this.panel1.Controls.Add(this.otpDisplay);
            this.panel1.Controls.Add(this.accountListView);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(340, 446);
            this.panel1.TabIndex = 0;
            // 
            // pointsLabel
            // 
            this.pointsLabel.AutoSize = true;
            this.pointsLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pointsLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pointsLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.pointsLabel.Location = new System.Drawing.Point(293, 189);
            this.pointsLabel.Name = "pointsLabel";
            this.pointsLabel.Size = new System.Drawing.Size(33, 19);
            this.pointsLabel.TabIndex = 11;
            this.pointsLabel.Text = "0 點";
            this.pointsLabel.Click += new System.EventHandler(this.pointsLabel_Click);
            // 
            // maplePictureBox
            // 
            this.maplePictureBox.Image = global::MapleStoryLauncher.Properties.Resources.maple;
            this.maplePictureBox.Location = new System.Drawing.Point(16, 189);
            this.maplePictureBox.Name = "maplePictureBox";
            this.maplePictureBox.Size = new System.Drawing.Size(18, 18);
            this.maplePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.maplePictureBox.TabIndex = 101;
            this.maplePictureBox.TabStop = false;
            // 
            // beanfunPictureBox
            // 
            this.beanfunPictureBox.Image = global::MapleStoryLauncher.Properties.Resources.beanfun;
            this.beanfunPictureBox.Location = new System.Drawing.Point(11, 10);
            this.beanfunPictureBox.Name = "beanfunPictureBox";
            this.beanfunPictureBox.Size = new System.Drawing.Size(18, 18);
            this.beanfunPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.beanfunPictureBox.TabIndex = 29;
            this.beanfunPictureBox.TabStop = false;
            // 
            // groupBoxBeanfun
            // 
            this.groupBoxBeanfun.Controls.Add(this.AddRemoveAccount);
            this.groupBoxBeanfun.Controls.Add(this.accountInput);
            this.groupBoxBeanfun.Controls.Add(this.accountLabel);
            this.groupBoxBeanfun.Controls.Add(this.loginButton);
            this.groupBoxBeanfun.Controls.Add(this.passLabel);
            this.groupBoxBeanfun.Controls.Add(this.passwordInput);
            this.groupBoxBeanfun.Controls.Add(this.rememberPwd);
            this.groupBoxBeanfun.Controls.Add(this.autoLogin);
            this.groupBoxBeanfun.Font = new System.Drawing.Font("Microsoft YaHei UI Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.groupBoxBeanfun.Location = new System.Drawing.Point(10, 0);
            this.groupBoxBeanfun.Name = "groupBoxBeanfun";
            this.groupBoxBeanfun.Size = new System.Drawing.Size(320, 176);
            this.groupBoxBeanfun.TabIndex = 100;
            this.groupBoxBeanfun.TabStop = false;
            // 
            // AddRemoveAccount
            // 
            this.AddRemoveAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemoveAccount.Location = new System.Drawing.Point(253, 34);
            this.AddRemoveAccount.Name = "AddRemoveAccount";
            this.AddRemoveAccount.Size = new System.Drawing.Size(42, 21);
            this.AddRemoveAccount.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.AddRemoveAccount.TabIndex = 104;
            this.AddRemoveAccount.TabStop = false;
            this.AddRemoveAccount.Click += new System.EventHandler(this.AddRemoveAccount_Click);
            // 
            // accountInput
            // 
            this.accountInput.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.accountInput.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.accountInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.accountInput.Location = new System.Drawing.Point(65, 32);
            this.accountInput.Name = "accountInput";
            this.accountInput.Size = new System.Drawing.Size(164, 25);
            this.accountInput.Sorted = true;
            this.accountInput.TabIndex = 0;
            this.accountInput.SelectionChangeCommitted += new System.EventHandler(this.accountInput_SelectionChangeCommitted);
            this.accountInput.TextChanged += new System.EventHandler(this.accountInput_TextChanged);
            this.accountInput.Leave += new System.EventHandler(this.accountInput_Leave);
            // 
            // accountLabel
            // 
            this.accountLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.accountLabel.Location = new System.Drawing.Point(13, 33);
            this.accountLabel.Name = "accountLabel";
            this.accountLabel.Size = new System.Drawing.Size(50, 23);
            this.accountLabel.TabIndex = 101;
            this.accountLabel.Text = "帳號";
            this.accountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // loginButton
            // 
            this.loginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.loginButton.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.loginButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.loginButton.Location = new System.Drawing.Point(22, 125);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(148, 31);
            this.loginButton.TabIndex = 5;
            this.loginButton.Text = "登入";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // passLabel
            // 
            this.passLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.passLabel.Location = new System.Drawing.Point(13, 75);
            this.passLabel.Name = "passLabel";
            this.passLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.passLabel.Size = new System.Drawing.Size(50, 23);
            this.passLabel.TabIndex = 102;
            this.passLabel.Text = "密碼";
            this.passLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // passwordInput
            // 
            this.passwordInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.passwordInput.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.passwordInput.Location = new System.Drawing.Point(65, 75);
            this.passwordInput.Name = "passwordInput";
            this.passwordInput.PasswordChar = '*';
            this.passwordInput.Size = new System.Drawing.Size(148, 23);
            this.passwordInput.TabIndex = 1;
            this.passwordInput.TextChanged += new System.EventHandler(this.passwordInput_TextChanged);
            // 
            // rememberPwd
            // 
            this.rememberPwd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rememberPwd.AutoSize = true;
            this.rememberPwd.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rememberPwd.Location = new System.Drawing.Point(236, 78);
            this.rememberPwd.Name = "rememberPwd";
            this.rememberPwd.Size = new System.Drawing.Size(75, 21);
            this.rememberPwd.TabIndex = 3;
            this.rememberPwd.Text = "記住密碼";
            this.rememberPwd.UseVisualStyleBackColor = true;
            this.rememberPwd.CheckedChanged += new System.EventHandler(this.rememberPwd_CheckedChanged);
            // 
            // autoLogin
            // 
            this.autoLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.autoLogin.AutoSize = true;
            this.autoLogin.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.autoLogin.Location = new System.Drawing.Point(201, 131);
            this.autoLogin.Name = "autoLogin";
            this.autoLogin.Size = new System.Drawing.Size(111, 21);
            this.autoLogin.TabIndex = 4;
            this.autoLogin.Text = "選取時自動登入";
            this.autoLogin.UseVisualStyleBackColor = true;
            this.autoLogin.CheckedChanged += new System.EventHandler(this.autoLogin_CheckedChanged);
            // 
            // autoSelect
            // 
            this.autoSelect.AutoSize = true;
            this.autoSelect.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.autoSelect.Location = new System.Drawing.Point(38, 371);
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
            this.autoLaunch.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.autoLaunch.Location = new System.Drawing.Point(143, 371);
            this.autoLaunch.Name = "autoLaunch";
            this.autoLaunch.Size = new System.Drawing.Size(99, 21);
            this.autoLaunch.TabIndex = 8;
            this.autoLaunch.TabStop = false;
            this.autoLaunch.Text = "自動啟動遊戲";
            this.autoLaunch.UseVisualStyleBackColor = true;
            // 
            // getOtpButton
            // 
            this.getOtpButton.Enabled = false;
            this.getOtpButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.getOtpButton.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.getOtpButton.Location = new System.Drawing.Point(13, 401);
            this.getOtpButton.Name = "getOtpButton";
            this.getOtpButton.Size = new System.Drawing.Size(148, 29);
            this.getOtpButton.TabIndex = 9;
            this.getOtpButton.TabStop = false;
            this.getOtpButton.Text = "啟動遊戲";
            this.getOtpButton.UseVisualStyleBackColor = true;
            this.getOtpButton.Click += new System.EventHandler(this.getOtpButton_Click);
            // 
            // otpDisplay
            // 
            this.otpDisplay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.otpDisplay.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.otpDisplay.Location = new System.Drawing.Point(205, 404);
            this.otpDisplay.Name = "otpDisplay";
            this.otpDisplay.PasswordChar = '*';
            this.otpDisplay.ReadOnly = true;
            this.otpDisplay.Size = new System.Drawing.Size(121, 23);
            this.otpDisplay.TabIndex = 10;
            this.otpDisplay.TabStop = false;
            this.otpDisplay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.otpDisplay.Click += new System.EventHandler(this.otpDisplay_OnClick);
            this.otpDisplay.DoubleClick += new System.EventHandler(this.otpDisplay_DoubleClick);
            // 
            // accountListView
            // 
            this.accountListView.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.accountListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Nickname,
            this.Account});
            this.accountListView.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.accountListView.FullRowSelect = true;
            this.accountListView.GridLines = true;
            this.accountListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.accountListView.Location = new System.Drawing.Point(14, 215);
            this.accountListView.MultiSelect = false;
            this.accountListView.Name = "accountListView";
            this.accountListView.Size = new System.Drawing.Size(312, 143);
            this.accountListView.TabIndex = 6;
            this.accountListView.TabStop = false;
            this.accountListView.UseCompatibleStateImageBehavior = false;
            this.accountListView.View = System.Windows.Forms.View.Details;
            this.accountListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.accountListView_ItemSelectionChanged);
            this.accountListView.DoubleClick += new System.EventHandler(this.accountListView_DoubleClick);
            // 
            // Nickname
            // 
            this.Nickname.Text = "遊戲帳號";
            this.Nickname.Width = 185;
            // 
            // Account
            // 
            this.Account.Text = "登入帳號";
            this.Account.Width = 123;
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
            // pingTimer
            // 
            this.pingTimer.Tick += new System.EventHandler(this.pingTimer_Tick);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(340, 446);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "新楓之谷啟動器";
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maplePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.beanfunPictureBox)).EndInit();
            this.groupBoxBeanfun.ResumeLayout(false);
            this.groupBoxBeanfun.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AddRemoveAccount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView accountListView;
        private System.Windows.Forms.ColumnHeader Nickname;
        private System.Windows.Forms.ColumnHeader Account;
        private System.Windows.Forms.CheckBox autoLogin;
        private System.Windows.Forms.CheckBox rememberPwd;
        private System.Windows.Forms.TextBox passwordInput;
        private System.Windows.Forms.Label passLabel;
        private System.Windows.Forms.Label accountLabel;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.TextBox otpDisplay;
        private System.Windows.Forms.Button getOtpButton;
        private System.ComponentModel.BackgroundWorker getOtpWorker;
        private System.ComponentModel.BackgroundWorker loginWorker;
        private System.Windows.Forms.ToolTip Tip;
        private System.Windows.Forms.ToolTip Notification;
        private System.Windows.Forms.CheckBox autoLaunch;
        private System.Windows.Forms.CheckBox autoSelect;
        private System.Windows.Forms.GroupBox groupBoxBeanfun;
        private System.Windows.Forms.PictureBox beanfunPictureBox;
        private System.Windows.Forms.PictureBox maplePictureBox;
        private System.Windows.Forms.Timer pingTimer;
        private Label pointsLabel;
        private ComboBox accountInput;
        private PictureBox AddRemoveAccount;
    }
}

