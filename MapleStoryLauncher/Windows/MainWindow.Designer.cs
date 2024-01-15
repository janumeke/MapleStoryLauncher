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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            panel1 = new Panel();
            pointsLabel = new Label();
            maplePictureBox = new PictureBox();
            beanfunPictureBox = new PictureBox();
            groupBoxBeanfun = new GroupBox();
            AddRemoveAccount = new PictureBox();
            accountInput = new ComboBox();
            accountLabel = new Label();
            loginButton = new Button();
            passLabel = new Label();
            passwordInput = new TextBox();
            rememberPwd = new CheckBox();
            autoLogin = new CheckBox();
            autoSelect = new CheckBox();
            autoLaunch = new CheckBox();
            getOtpButton = new Button();
            otpDisplay = new TextBox();
            accountListView = new ListView();
            Nickname = new ColumnHeader();
            Account = new ColumnHeader();
            getOTPWorker = new System.ComponentModel.BackgroundWorker();
            loginWorker = new System.ComponentModel.BackgroundWorker();
            Tip = new ToolTip(components);
            Notification = new ToolTip(components);
            pingTimer = new System.Windows.Forms.Timer(components);
            getPointsWorker = new System.ComponentModel.BackgroundWorker();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)maplePictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)beanfunPictureBox).BeginInit();
            groupBoxBeanfun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AddRemoveAccount).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(pointsLabel);
            panel1.Controls.Add(maplePictureBox);
            panel1.Controls.Add(beanfunPictureBox);
            panel1.Controls.Add(groupBoxBeanfun);
            panel1.Controls.Add(autoSelect);
            panel1.Controls.Add(autoLaunch);
            panel1.Controls.Add(getOtpButton);
            panel1.Controls.Add(otpDisplay);
            panel1.Controls.Add(accountListView);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(340, 446);
            panel1.TabIndex = 0;
            // 
            // pointsLabel
            // 
            pointsLabel.AutoSize = true;
            pointsLabel.BorderStyle = BorderStyle.Fixed3D;
            pointsLabel.Cursor = Cursors.Hand;
            pointsLabel.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            pointsLabel.Location = new Point(293, 189);
            pointsLabel.Name = "pointsLabel";
            pointsLabel.Size = new Size(33, 19);
            pointsLabel.TabIndex = 11;
            pointsLabel.Text = "0 點";
            // 
            // maplePictureBox
            // 
            maplePictureBox.Image = Properties.Resources.maple;
            maplePictureBox.Location = new Point(16, 189);
            maplePictureBox.Name = "maplePictureBox";
            maplePictureBox.Size = new Size(18, 18);
            maplePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            maplePictureBox.TabIndex = 101;
            maplePictureBox.TabStop = false;
            // 
            // beanfunPictureBox
            // 
            beanfunPictureBox.Image = Properties.Resources.beanfun;
            beanfunPictureBox.Location = new Point(11, 10);
            beanfunPictureBox.Name = "beanfunPictureBox";
            beanfunPictureBox.Size = new Size(18, 18);
            beanfunPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            beanfunPictureBox.TabIndex = 29;
            beanfunPictureBox.TabStop = false;
            // 
            // groupBoxBeanfun
            // 
            groupBoxBeanfun.Controls.Add(AddRemoveAccount);
            groupBoxBeanfun.Controls.Add(accountInput);
            groupBoxBeanfun.Controls.Add(accountLabel);
            groupBoxBeanfun.Controls.Add(loginButton);
            groupBoxBeanfun.Controls.Add(passLabel);
            groupBoxBeanfun.Controls.Add(passwordInput);
            groupBoxBeanfun.Controls.Add(rememberPwd);
            groupBoxBeanfun.Controls.Add(autoLogin);
            groupBoxBeanfun.Font = new Font("Microsoft YaHei UI Light", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            groupBoxBeanfun.Location = new Point(10, 0);
            groupBoxBeanfun.Name = "groupBoxBeanfun";
            groupBoxBeanfun.Size = new Size(320, 176);
            groupBoxBeanfun.TabIndex = 100;
            groupBoxBeanfun.TabStop = false;
            // 
            // AddRemoveAccount
            // 
            AddRemoveAccount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddRemoveAccount.Location = new Point(253, 34);
            AddRemoveAccount.Name = "AddRemoveAccount";
            AddRemoveAccount.Size = new Size(42, 21);
            AddRemoveAccount.SizeMode = PictureBoxSizeMode.Zoom;
            AddRemoveAccount.TabIndex = 104;
            AddRemoveAccount.TabStop = false;
            // 
            // accountInput
            // 
            accountInput.AutoCompleteMode = AutoCompleteMode.Suggest;
            accountInput.AutoCompleteSource = AutoCompleteSource.ListItems;
            accountInput.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            accountInput.Location = new Point(65, 32);
            accountInput.Name = "accountInput";
            accountInput.Size = new Size(164, 25);
            accountInput.Sorted = true;
            accountInput.TabIndex = 0;
            // 
            // accountLabel
            // 
            accountLabel.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            accountLabel.Location = new Point(13, 33);
            accountLabel.Name = "accountLabel";
            accountLabel.Size = new Size(50, 23);
            accountLabel.TabIndex = 101;
            accountLabel.Text = "帳號";
            accountLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // loginButton
            // 
            loginButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            loginButton.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            loginButton.ImageAlign = ContentAlignment.MiddleLeft;
            loginButton.Location = new Point(22, 125);
            loginButton.Name = "loginButton";
            loginButton.Size = new Size(148, 31);
            loginButton.TabIndex = 5;
            loginButton.Text = "登入";
            loginButton.UseVisualStyleBackColor = true;
            // 
            // passLabel
            // 
            passLabel.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            passLabel.Location = new Point(13, 75);
            passLabel.Name = "passLabel";
            passLabel.RightToLeft = RightToLeft.No;
            passLabel.Size = new Size(50, 23);
            passLabel.TabIndex = 102;
            passLabel.Text = "密碼";
            passLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // passwordInput
            // 
            passwordInput.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            passwordInput.ImeMode = ImeMode.Disable;
            passwordInput.Location = new Point(65, 75);
            passwordInput.Name = "passwordInput";
            passwordInput.PasswordChar = '*';
            passwordInput.Size = new Size(148, 23);
            passwordInput.TabIndex = 1;
            // 
            // rememberPwd
            // 
            rememberPwd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            rememberPwd.AutoSize = true;
            rememberPwd.Enabled = false;
            rememberPwd.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            rememberPwd.Location = new Point(236, 78);
            rememberPwd.Name = "rememberPwd";
            rememberPwd.Size = new Size(75, 21);
            rememberPwd.TabIndex = 3;
            rememberPwd.Text = "記住密碼";
            rememberPwd.UseVisualStyleBackColor = true;
            // 
            // autoLogin
            // 
            autoLogin.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            autoLogin.AutoSize = true;
            autoLogin.Enabled = false;
            autoLogin.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            autoLogin.Location = new Point(201, 131);
            autoLogin.Name = "autoLogin";
            autoLogin.Size = new Size(111, 21);
            autoLogin.TabIndex = 4;
            autoLogin.Text = "選取時自動登入";
            autoLogin.UseVisualStyleBackColor = true;
            // 
            // autoSelect
            // 
            autoSelect.AutoSize = true;
            autoSelect.Enabled = false;
            autoSelect.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            autoSelect.Location = new Point(38, 371);
            autoSelect.Name = "autoSelect";
            autoSelect.Size = new Size(75, 21);
            autoSelect.TabIndex = 7;
            autoSelect.TabStop = false;
            autoSelect.Text = "記住選擇";
            autoSelect.UseVisualStyleBackColor = true;
            // 
            // autoLaunch
            // 
            autoLaunch.AutoSize = true;
            autoLaunch.Enabled = false;
            autoLaunch.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            autoLaunch.Location = new Point(143, 371);
            autoLaunch.Name = "autoLaunch";
            autoLaunch.Size = new Size(99, 21);
            autoLaunch.TabIndex = 8;
            autoLaunch.TabStop = false;
            autoLaunch.Text = "自動啟動遊戲";
            autoLaunch.UseVisualStyleBackColor = true;
            // 
            // getOtpButton
            // 
            getOtpButton.Enabled = false;
            getOtpButton.FlatStyle = FlatStyle.System;
            getOtpButton.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            getOtpButton.Location = new Point(13, 401);
            getOtpButton.Name = "getOtpButton";
            getOtpButton.Size = new Size(148, 29);
            getOtpButton.TabIndex = 9;
            getOtpButton.TabStop = false;
            getOtpButton.Text = "啟動遊戲";
            getOtpButton.UseVisualStyleBackColor = true;
            // 
            // otpDisplay
            // 
            otpDisplay.Cursor = Cursors.Hand;
            otpDisplay.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            otpDisplay.Location = new Point(205, 404);
            otpDisplay.Name = "otpDisplay";
            otpDisplay.PasswordChar = '*';
            otpDisplay.ReadOnly = true;
            otpDisplay.Size = new Size(121, 23);
            otpDisplay.TabIndex = 10;
            otpDisplay.TabStop = false;
            otpDisplay.TextAlign = HorizontalAlignment.Center;
            // 
            // accountListView
            // 
            accountListView.Activation = ItemActivation.TwoClick;
            accountListView.Columns.AddRange(new ColumnHeader[] { Nickname, Account });
            accountListView.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            accountListView.FullRowSelect = true;
            accountListView.GridLines = true;
            accountListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            accountListView.Location = new Point(14, 215);
            accountListView.MultiSelect = false;
            accountListView.Name = "accountListView";
            accountListView.Size = new Size(312, 143);
            accountListView.TabIndex = 6;
            accountListView.TabStop = false;
            accountListView.UseCompatibleStateImageBehavior = false;
            accountListView.View = View.Details;
            // 
            // Nickname
            // 
            Nickname.Text = "遊戲帳號";
            Nickname.Width = 185;
            // 
            // Account
            // 
            Account.Text = "登入帳號";
            Account.Width = 123;
            // 
            // getOTPWorker
            // 
            getOTPWorker.DoWork += getOTPWorker_DoWork;
            getOTPWorker.RunWorkerCompleted += getOTPWorker_RunWorkerCompleted;
            // 
            // loginWorker
            // 
            loginWorker.DoWork += loginWorker_DoWork;
            loginWorker.RunWorkerCompleted += loginWorker_RunWorkerCompleted;
            // 
            // Tip
            // 
            Tip.AutoPopDelay = 5000;
            Tip.InitialDelay = 500;
            Tip.ReshowDelay = 100;
            // 
            // Notification
            // 
            Notification.AutoPopDelay = 5000;
            Notification.InitialDelay = 0;
            Notification.IsBalloon = true;
            Notification.ReshowDelay = 100;
            // 
            // pingTimer
            // 
            pingTimer.Tick += pingTimer_Tick;
            // 
            // getPointsWorker
            // 
            getPointsWorker.DoWork += getPointsWorker_DoWork;
            getPointsWorker.RunWorkerCompleted += getPointsWorker_RunWorkerCompleted;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(340, 446);
            Controls.Add(panel1);
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainWindow";
            Text = "新楓之谷啟動器";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)maplePictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)beanfunPictureBox).EndInit();
            groupBoxBeanfun.ResumeLayout(false);
            groupBoxBeanfun.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)AddRemoveAccount).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private ListView accountListView;
        private ColumnHeader Nickname;
        private ColumnHeader Account;
        private CheckBox autoLogin;
        private CheckBox rememberPwd;
        private TextBox passwordInput;
        private Label passLabel;
        private Label accountLabel;
        private Button loginButton;
        private TextBox otpDisplay;
        private Button getOtpButton;
        private System.ComponentModel.BackgroundWorker getOTPWorker;
        private System.ComponentModel.BackgroundWorker loginWorker;
        private ToolTip Tip;
        private ToolTip Notification;
        private CheckBox autoLaunch;
        private CheckBox autoSelect;
        private GroupBox groupBoxBeanfun;
        private PictureBox beanfunPictureBox;
        private PictureBox maplePictureBox;
        private System.Windows.Forms.Timer pingTimer;
        private Label pointsLabel;
        private ComboBox accountInput;
        private PictureBox AddRemoveAccount;
        private System.ComponentModel.BackgroundWorker getPointsWorker;
    }
}

