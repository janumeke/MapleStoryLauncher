using ExtentionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_OTPDisplay()
        {
            this.Load += (sender, _) =>
            {
                this.Tip.SetToolTip(otpDisplay, "點擊複製密碼\n雙擊顯示/隱藏密碼");
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                otpDisplay.Text = "";
            };

            void CopyOTPToClipboard()
            {
                if (otpDisplay.Text != "" && otpDisplay.Text.IsAllDigits())
                {
                    try
                    {
                        Clipboard.SetText(otpDisplay.Text);
                        Notification.Hide(otpDisplay);
                        Notification.Show("已複製密碼！", otpDisplay, (int)(0.125 * otpDisplay.Width), (int)(-2 * otpDisplay.Height), 1000);
                    }
                    catch { }
                }
            }

            otpDisplay.Click += (sender, _) =>
            {
                CopyOTPToClipboard();
            };

            otpDisplay.DoubleClick += (sender, _) =>
            {
                if (otpDisplay.Text == "" || !otpDisplay.Text.IsAllDigits())
                    return;

                if (otpDisplay.PasswordChar == default)
                    otpDisplay.PasswordChar = '*';
                else
                    otpDisplay.PasswordChar = default;
            };

            SyncEvents.OTPGetting += gameAccount =>
            {
                otpDisplay.Text = "取得密碼中...";
                otpDisplay.PasswordChar = default;
            };

            SyncEvents.OTPGot += otp =>
            {
                if (otp == "" || !otp.IsAllDigits())
                    otpDisplay.PasswordChar = default;
                else
                    otpDisplay.PasswordChar = '*';
                otpDisplay.Text = otp;
                CopyOTPToClipboard();
            };

            SyncEvents.GameLaunching += () =>
            {
                otpDisplay.Text = "啟動遊戲中...";
                otpDisplay.PasswordChar = default;
            };

            SyncEvents.GameLaunched += () =>
            {
                otpDisplay.Text = "";
            };
        }
    }
}
