using ExtentionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_LoginButton()
        {
            void UpdateText()
            {
                if (loggedIn)
                    loginButton.Text = "登出";
                else
                    if (accountInput.Text == "" && passwordInput.Text == "")
                        loginButton.Text = "顯示 QRCode";
                    else
                        loginButton.Text = "登入";
            }

            this.Load += (sender, _) =>
            {
                this.AcceptButton = loginButton;

                UpdateText();
            };

            accountInput.TextChanged += (sender, _) =>
            {
                UpdateText();
            };

            passwordInput.TextChanged += (sender, _) =>
            {
                UpdateText();
            };

            bool CheckInputsAndShowError()
            {
                bool accOK, passOK;

                if (Regex.Match(accountInput.Text, "^[0-9A-Za-z]{8,20}$").Success)
                    accOK = true;
                else
                    accOK = accountInput.Text.IsEmail();

                if (Regex.Match(passwordInput.Text, "^[0-9A-Za-z]{8,20}$").Success)
                    passOK = true;
                else
                    passOK = false;

                if (!accOK || !passOK)
                {
                    string message = "";

                    if (!accOK)
                        message += "帳號或認證 Email 格式錯誤。\n";
                    if (!passOK)
                        message += "密碼格式錯誤。\n";
                    message += "\n";

                    message += $"{(!accOK ? "帳號" : "")}" +
                               $"{(!accOK && !passOK ? "與" : "")}" +
                               $"{(!passOK ? "密碼" : "")}";
                    message += "格式必須是：\n" +
                               "1. 英文字母與數字\n" +
                               "2. 長度為 8 至 20";

                    MessageBox.Show(message, "");
                    return false;
                }
                else
                    return true;
            }

            loginButton.Click += (sender, _) =>
            {
                if (!loggedIn)
                {
                    bool useQRCode = accountInput.Text == String.Empty && passwordInput.Text == String.Empty;

                    //Check formats of the account and password only when not using QRCode
                    if (!useQRCode && !CheckInputsAndShowError())
                        return;

                    SyncEvents.LogIn(accountInput.Text);
                    loginWorker.RunWorkerAsync(new LoginWorkerArgs{
                        useQRCode = useQRCode,
                        username = accountInput.Text,
                        password = passwordInput.Text,
                    });
                }
                else
                {
                    if (!beanfun.Logout() //beanfun has reader-writer lock
                        && MessageBox.Show(
                            "登出失敗！ 不登出直接關閉帳號？",
                            "",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2
                        ) == DialogResult.No)
                        return;

                    pingTimer.Stop();
                    SyncEvents.LogOut(loggedInUsername, false);
                }
            };

            SyncEvents.AccountLoaded += key =>
            {
                if (autoLogin.Checked && loginButton.Enabled)
                    loginButton.PerformClick();
            };

            SyncEvents.LoggingIn += username =>
            {
                loginButton.Enabled = false;
                loginButton.Text = "請稍候...";
            };

            SyncEvents.LoginFailed += () =>
            {
                UpdateText();
                loginButton.Enabled = true;
            };

            SyncEvents.LoggedIn_Loaded += username =>
            {
                UpdateText();
                loginButton.Enabled = true;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                UpdateText();
                loginButton.Enabled = true;
                this.AcceptButton = loginButton;
            };

            SyncEvents.OTPGetting += gameAccount =>
            {
                loginButton.Enabled = false;
            };

            SyncEvents.OTPGot += otp =>
            {
                loginButton.Enabled = true;
            };

            SyncEvents.GameLaunching += () =>
            {
                loginButton.Enabled = false;
            };

            SyncEvents.GameLaunched += () =>
            {
                loginButton.Enabled = true;
            };
        }
    }
}
