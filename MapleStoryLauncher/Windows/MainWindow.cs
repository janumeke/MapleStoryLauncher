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

        class State
        {
            public string username { get; set; } = default;
            public bool loggedIn { get; set; } = false;
            public string autoSelectAccount { get; set; } = default;
        }

        private State status = new();

        private readonly UI ui;
        private readonly AccountManager accountManager;

        public MainWindow()
        {
            UI.CheckMultipleInstances();
            ui = new UI(this);
            string savePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{typeof(MainWindow).Namespace}\\UserData";
            accountManager = new AccountManager(savePath);
            InitializeComponent();
            ui.FormLoaded();
        }

        #region ButtonEvents
        private void AddRemoveAccount_Click(object sender, EventArgs e)
        {
            if (accountManager.Contains(accountInput.Text))
            {
                accountManager.RemoveAccount(accountInput.Text);
                if (!status.loggedIn)
                {
                    string text = accountInput.Text;
                    accountInput.Items.Remove(accountInput.Text);
                    accountInput.Text = text;
                }
            }
            else
            {
                accountManager.AddAccount(accountInput.Text);
                AccountManager.Settings settings = accountManager.GetSettings(accountInput.Text);
                settings.autoSelectAccount = status.autoSelectAccount;
                accountManager.SaveSettings(accountInput.Text, settings);
                if(!accountInput.Items.Contains(accountInput.Text))
                    accountInput.Items.Add(accountInput.Text);
            }
            accountManager.SaveToFile();
            ui.UpdateAddRemoveAccount();
        }

        // The login/logout botton.
        private void loginButton_Click(object sender, EventArgs e)
        {
            if (!status.loggedIn)
            {
                //Check formats of the account and password only when not using QRCode
                if (accountInput.Text != String.Empty || passwordInput.Text != String.Empty)
                {
                    bool checkAccount = true, checkPassword = true;
                    if (!Regex.Match(accountInput.Text, "^[0-9A-Za-z]{8,20}$").Success)
                        try { new MailAddress(accountInput.Text); }
                        catch { checkAccount = false; }
                    if (!Regex.Match(passwordInput.Text, "^[0-9A-Za-z]{8,20}$").Success)
                        checkPassword = false;
                    if (!checkAccount || !checkPassword)
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
            }
            else
            {
                //beanfun has reader-writer lock
                if (beanfun.Logout())
                {
                    pingTimer.Stop();

                    if (accountManager.Contains(status.username))
                    {
                        AccountManager.Settings settings = accountManager.GetSettings(status.username);
                        if (e is not AccountClosedEventArgs)
                        {
                            if (!autoSelect.Checked)
                                settings.autoSelectAccount = default;
                        }
                        accountManager.SaveSettings(status.username, settings);
                        accountManager.SaveToFile();
                    }

                    ui.LoggedOut();
                }
                else
                    ShowError(new BeanfunBroker.TransactionResult { Status = BeanfunBroker.TransactionResultStatus.Failed, Message = "登出失敗，請重開程式。" });
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
        private void rememberPwd_CheckedChanged(object sender, EventArgs e)
        {
            if (!rememberPwd.Checked)
                autoLogin.Checked = false;
        }

        private void autoLogin_CheckedChanged(object sender, EventArgs e)
        {
            if (autoLogin.Checked)
                rememberPwd.Checked = true;
        }

        private void autoSelect_CheckedChanged(object sender, EventArgs e)
        {
            ui.EmboldenAutoSelection(autoSelect.Checked);
            ui.UpdateAutoLaunchCheckBoxText();
        }
        #endregion

        private class AccountClosedEventArgs : EventArgs {}

        private void accountInput_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if ((string)accountInput.SelectedItem == status.username)
                return;

            if (status.loggedIn)
            {
                ui.AccountClosed();
                loginButton_Click(this, new AccountClosedEventArgs());
            }
            accountInput.Text = (string)accountInput.SelectedItem; //This is needed because accountInput.Text is updated after this event finishes
            ui.AccountOpened();
            if (autoLogin.Checked)
                if (loginButton.Enabled)
                    loginButton_Click(this, null);
        }

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
        private void accountInput_TextChanged(object sender, EventArgs e)
        {
            ui.UpdateLoginButtonText();
            ui.UpdateAddRemoveAccount();
        }

        private void passwordInput_TextChanged(object sender, EventArgs e)
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
            if (status.loggedIn)
                if (loginButton.Enabled)
                {
                    ui.AccountClosed();
                    loginButton_Click(null, new AccountClosedEventArgs());
                }
                else
                {
                    e.Cancel = true;
                    MessageBox.Show("請先登出再關閉程式。", "");
                }
        }
    }
}