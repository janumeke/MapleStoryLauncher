using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_GetOTPButton()
        {
            void Update()
            {
                if (!IsGameRunning())
                    if (accountListView.SelectedItems.Count == 0)
                        getOtpButton.Text = "啟動遊戲";
                    else
                        getOtpButton.Text = "啟動遊戲並登入";
                else
                    getOtpButton.Text = "取得一次性密碼";

                if (IsGameRunning() && accountListView.SelectedItems.Count == 0)
                    getOtpButton.Enabled = false;
                else
                    getOtpButton.Enabled = true;
            }

            void OnMainWindowActivated(object sender, EventArgs _)
            {
                if (loggedIn)
                    Update();
            }

            SyncEvents.LoggedIn_Loading += username =>
            {
                Update();
                this.Activated += OnMainWindowActivated;

                getOtpButton.TabStop = true;
                this.AcceptButton = getOtpButton;
            };

            SyncEvents.LoggedIn_Loaded += username =>
            {
                if (autoLaunch.Checked && !IsGameRunning())
                    if (getOtpButton.Enabled)
                        getOtpButton.PerformClick();
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                this.Activated -= OnMainWindowActivated;

                getOtpButton.TabStop = false;
            };

            accountListView.ItemSelectionChanged += (sender, _) =>
            {
                Update();
            };

            getOtpButton.Click += (sender, _) =>
            {
                if (IsGameRunning() && accountListView.SelectedItems.Count != 1)
                    return;
                if (!IsGameRunning() && !CheckGamePath())
                    return;

                if (accountListView.SelectedItems.Count == 0)
                {
                    SyncEvents.LaunchGame();
                    StartGame();
                    SyncEvents.FinishLaunchingGame();
                }
                if (accountListView.SelectedItems.Count == 1 && !getOTPWorker.IsBusy)
                {
                    SyncEvents.GetOTP(accountListView.SelectedItems[0].SubItems[0].Text);
                    getOTPWorker.RunWorkerAsync(gameAccounts[accountListView.SelectedIndices[0]]);
                }
            };

            SyncEvents.OTPGetting += gameAccount =>
            {
                this.Activated -= OnMainWindowActivated;
                getOtpButton.Enabled = false;
            };

            SyncEvents.OTPGot += otp =>
            {
                Update();
                this.Activated += OnMainWindowActivated;
            };

            SyncEvents.GameLaunching += () =>
            {
                this.Activated -= OnMainWindowActivated;
                getOtpButton.Enabled = false;
            };

            SyncEvents.GameLaunched += () =>
            {
                Update();
                this.Activated += OnMainWindowActivated;
            };

        }
    }
}
