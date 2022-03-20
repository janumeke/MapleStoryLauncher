using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MapleStoryLauncher
{
    using ExtentionMethods;

    public partial class MainWindow : Form
    {
        private readonly string gameName = "新楓之谷";

        private readonly string gameFileName = "MapleStory.exe";
        
        enum LogInState
        {
            LoggedOut,
            LoggedIn
        }
        
        private LogInState status = LogInState.LoggedOut;

        readonly UI ui;

        public MainWindow()
        {
            ui = new UI(this);
            
            UI.CheckMultipleInstances();
            InitializeComponent();
            ui.FormLoaded();
        }

        #region ButtonEvents
        // The login/logout botton.
        private void loginButton_Click(object sender, EventArgs e)
        {
            switch(status)
            {
                case LogInState.LoggedOut:
                    //Check formats of the account and password only when not using QRCode
                    if (accountInput.Text != String.Empty || pwdInput.Text != String.Empty)
                    {
                        bool checkAccount = true, checkPassword = true;
                        if (!Regex.Match(accountInput.Text, "^[0-9A-Za-z]{8,20}$").Success)
                            try { new MailAddress(accountInput.Text); }
                            catch { checkAccount = false; }
                        if (!Regex.Match(pwdInput.Text, "^[0-9A-Za-z]{8,20}$").Success)
                            checkPassword = false;
                        if(!checkAccount || !checkPassword)
                        {
                            string message = "";

                            if (!checkAccount)
                                message += "帳號或認證 Email 格式錯誤。\n";
                            if (!checkPassword)
                                message += "密碼格式錯誤。\n";

                            message += "\n";
                            message += $"{(!checkAccount ? "帳號" : "")}" +
                                       $"{(!checkAccount && !checkPassword ? "與" : "")}" +
                                       $"{(!checkPassword ? "密碼" : "")}";
                            message += "格式必須是：\n" +
                                       "1. 英文字母與數字\n" +
                                       "2. 長度為 8 至 20";

                            ShowError(new BeanfunBroker.TransactionResult { Status = BeanfunBroker.TransactionResultStatus.Denied, Message = message });
                            return;
                        }
                    }
                    ui.LoggingIn();
                    loginWorker.RunWorkerAsync();
                    break;
                case LogInState.LoggedIn:
                    //beanfun has reader-writer lock
                    if (beanfun.Logout())
                    {
                        //Log out or close window
                        pingTimer.Stop();
                        if (!autoSelect.Checked)
                        {
                            Properties.Settings.Default.autoSelectIndex = -1;
                            Properties.Settings.Default.Save();
                        }
                        ui.LoggedOut();
                        //Log out only
                        if (e is not FormClosingEventArgs)
                        {
                            status = LogInState.LoggedOut; //Closing preserves the status, because some modules need the last state for decisions
                        }
                    }
                    else
                        ShowError(new BeanfunBroker.TransactionResult { Status = BeanfunBroker.TransactionResultStatus.Failed, Message = "登出失敗，請重開程式。" });
                    break;
            }
        }

        // The start game/get OTP button.
        private void getOtpButton_Click(object sender, EventArgs e)
        {
            if (IsGameRunning() && accountListView.SelectedItems.Count != 1)
                return;
            if (!IsGameRunning() && !ui.CheckGamePath())
                return;

            if (accountListView.SelectedItems.Count == 0)
                StartGame();
            if (accountListView.SelectedItems.Count == 1 &&
                !getOtpWorker.IsBusy)
            {
                ui.GettingOtp();
                getOtpWorker.RunWorkerAsync(gameAccounts[accountListView.SelectedIndices[0]]);
            }
        }
        #endregion

        #region CheckBoxEvents
        private void rememberAccount_CheckedChanged(object sender, EventArgs e)
        {
            if (!rememberAccount.Checked)
            {
                autoLogin.Checked = false;
                rememberPwd.Checked = false;
            }
        }


        private void rememberPwd_CheckedChanged(object sender, EventArgs e)
        {
            if (rememberPwd.Checked)
                rememberAccount.Checked = true;
            else
                autoLogin.Checked = false;
        }

        private void autoLogin_CheckedChanged(object sender, EventArgs e)
        {
            if (autoLogin.Checked)
            {
                rememberAccount.Checked = true;
                rememberPwd.Checked = true;
            }
        }

        private void autoSelect_CheckedChanged(object sender, EventArgs e)
        {
            ui.EmboldenAutoSelection(autoSelect.Checked);
            ui.UpdateAutoLaunchCheckBoxText();
        }
        #endregion

        private void pointsLabel_Click(object sender, EventArgs e)
        {
            int rightX = pointsLabel.Location.X + pointsLabel.Width;

            int points = beanfun.GetRemainingPoints();
            if (points < 0)
                pointsLabel.Text = "-- 點";
            else
                pointsLabel.Text = $"{points} 點";

            pointsLabel.Location = new Point(rightX - pointsLabel.Width, pointsLabel.Location.Y);
        }

        //Copy on click
        private void accountListView_DoubleClick(object sender, EventArgs e)
        {
            if (accountListView.SelectedItems.Count == 1)
            {
                try { Clipboard.SetText(gameAccounts[accountListView.SelectedIndices[0]].username); }
                catch { }
            }
        }

        private void otpDisplay_OnClick(object sender, EventArgs e)
        {
            if (otpDisplay.Text == "" || !otpDisplay.Text.IsAllDigits())
                return;
            try
            {
                Clipboard.SetText(otpDisplay.Text);
                Notification.Hide(otpDisplay);
                Notification.Show("已複製密碼！", otpDisplay, (int)(0.25 * otpDisplay.Width), (int)(-1.5 * otpDisplay.Height), 1000);
            }
            catch { }
        }

        private void otpDisplay_DoubleClick(object sender, EventArgs e)
        {
            if (otpDisplay.Text == "" || !otpDisplay.Text.IsAllDigits())
                return;
            if (otpDisplay.PasswordChar == default)
                otpDisplay.PasswordChar = '*';
            else
                otpDisplay.PasswordChar = default;
        }

        //Refresh UI when changed
        private void Input_TextChanged(object sender, EventArgs e)
        {
            ui.UpdateLoginButtonText();
        }

        private void accountListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ui.UpdateGetOtpButton();
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            ui.FormFocused();
        }

        //Cleanup before closing
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (status == LogInState.LoggedIn)
                if (loginButton.Enabled)
                    loginButton_Click(null, e);
                else
                {
                    e.Cancel = true;
                    MessageBox.Show("請先登出再關閉程式。", "");
                }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            ui.FormClosed();
        }
    }
}