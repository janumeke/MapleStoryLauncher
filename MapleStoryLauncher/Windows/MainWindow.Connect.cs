using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public BeanfunBroker beanfun = new();

        List<BeanfunBroker.GameAccount> gameAccounts = default;

        # region Login
        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (accountInput.Text == String.Empty && passwordInput.Text == String.Empty)
            {
                QRCodeWindow qrcodeWindow = new(this);
                qrcodeWindow.ShowDialog();
                e.Result = qrcodeWindow.GetResult();
                if (e.Result == default(BeanfunBroker.TransactionResult))
                    e.Cancel = true;
            }
            else
            {
                BeanfunBroker.TransactionResult result = beanfun.Login(accountInput.Text, passwordInput.Text);
                switch (result.Status)
                {
                    case BeanfunBroker.TransactionResultStatus.RequireAppAuthentication:
                        AppAuthWindow appAuthWindow = new(this);
                        appAuthWindow.ShowDialog();
                        e.Result = appAuthWindow.GetResult();
                        if (e.Result == default(BeanfunBroker.TransactionResult))
                            e.Cancel = true;
                        break;
                    default:
                        e.Result = result;
                        break;
                }
            }

            if( !e.Cancel &&
                ((BeanfunBroker.TransactionResult)e.Result).Status == BeanfunBroker.TransactionResultStatus.Success)
            {
                BeanfunBroker.GameAccountResult result = beanfun.GetGameAccounts();
                if (result.Status == BeanfunBroker.TransactionResultStatus.Success)
                    gameAccounts = result.GameAccounts;
                e.Result = result;
            }
        }

        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                ui.LoginFailed();
            else
                switch (((BeanfunBroker.TransactionResult)e.Result).Status)
                {
                    case BeanfunBroker.TransactionResultStatus.Success:
                        pingTimer.Interval = pingTimeout;
                        pingTimer.Start();
                        ui.LoggedIn();
                        break;
                    default:
                        ShowError((BeanfunBroker.TransactionResult)e.Result);
                        ui.LoginFailed();
                        break;
                }
        }
        #endregion

        #region Ping
        private const int pingTimeout = 10 * 60 * 1000; //10 mins
        private const int pingMaxFailedTries = 5;
        private int pingFailedTries = 0;

        private void pingTimer_Tick(object sender, EventArgs e)
        {
            if (!pingTimer.Enabled)
                return;

            BeanfunBroker.TransactionResult result = beanfun.Ping(); //beanfun has reader-writer lock
            switch (result.Status)
            {
                case BeanfunBroker.TransactionResultStatus.Failed:
                    pingTimer.Stop();
                    beanfun.Logout();
                    ShowError(result);
                    ui.LoggedOut();
                    break;
                case BeanfunBroker.TransactionResultStatus.ConnectionLost:
                    pingTimer.Interval = 3;
                    if (++pingFailedTries >= pingMaxFailedTries)
                    {
                        pingTimer.Stop();
                        beanfun.Logout();
                        ShowError(result);
                        ui.LoggedOut();
                    }
                    break;
                case BeanfunBroker.TransactionResultStatus.LoginFirst:
                    pingTimer.Stop();
                    ShowError(result);
                    ui.LoggedOut();
                    break;
                case BeanfunBroker.TransactionResultStatus.Success:
                    pingTimer.Interval = pingTimeout;
                    break;
            }
        }
        #endregion

        #region OTP
        private void getOtpWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BeanfunBroker.TransactionResult ping = beanfun.Ping(); //beanfun has reader-writer lock
            if (ping.Status != BeanfunBroker.TransactionResultStatus.Success)
            {
                e.Result = ping;
                return;
            }

            BeanfunBroker.GameAccount gameAccount = (BeanfunBroker.GameAccount)e.Argument;
            e.Result = beanfun.GetOTP(gameAccount); //beanfun has reader-writer lock
        }

        private void getOtpWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BeanfunBroker.TransactionResult result = (BeanfunBroker.TransactionResult)e.Result;
            switch (result.Status)
            {
                case BeanfunBroker.TransactionResultStatus.Success:
                    if (IsGameRunning())
                        ui.OtpGot(result.Message);
                    else
                    {
                        StartGame(((BeanfunBroker.OTPResult)result).Username, result.Message);
                        ui.OtpGot("");
                    }
                    break;
                case BeanfunBroker.TransactionResultStatus.LoginFirst:
                    pingTimer.Stop();
                    ShowError(result);
                    ui.OtpGot("");
                    ui.LoggedOut();
                    break;
                default:
                    ShowError(result);
                    ui.OtpGot("");
                    break;
            }
        }
        #endregion

        private bool IsGameRunning()
        {
            string processName = Path.GetFileNameWithoutExtension(gameFileName);
            if (Process.GetProcessesByName(processName).Length != 0)
                return true;
            else
                return false;
        }

        private static bool StartGame()
        {
            return StartProcess(Properties.Settings.Default.gamePath, "");
        }

        private static bool StartGame(string username, string otp)
        {
            return StartProcess(Properties.Settings.Default.gamePath, "tw.login.maplestory.gamania.com 8484 BeanFun " + username + " " + otp);
        }

        private static bool StartProcess(string path, string arg)
        {
            try
            {
                ProcessStartInfo psInfo = new()
                {
                    FileName = path,
                    Arguments = arg,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(path),
                };
                Process.Start(psInfo);
                return true;
            }
            catch
            {
                MessageBox.Show("啟動失敗，請嘗試手動以系統管理員身分啟動遊戲。", "");
                return false;
            }
        }

        private void ShowError(BeanfunBroker.TransactionResult result)
        {
            switch (result.Status)
            {
                case BeanfunBroker.TransactionResultStatus.Expired:
                case BeanfunBroker.TransactionResultStatus.Denied:
                    MessageBox.Show(result.Message, "");
                    break;
                case BeanfunBroker.TransactionResultStatus.Failed:
                    if (MessageBox.Show(result.Message + "\n請回報給開發者。\n是否產生記錄檔?", "預期外的錯誤", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{typeof(MainWindow).Namespace}\\LastResponse.txt";
                        HttpResponseMessage res = beanfun.GetLastResponse();
                        try
                        {
                            File.WriteAllText(path, $"{res.RequestMessage.Method} {res.RequestMessage.RequestUri}\n" +
                                                    $"{res}\n\n" +
                                                    $"{res.Content.ReadAsStringAsync().Result}");
                        }
                        catch
                        {
                            MessageBox.Show("無法寫入至檔案。");
                        }
                    }
                    break;
                case BeanfunBroker.TransactionResultStatus.ConnectionLost:
                    MessageBox.Show(result.Message + "\n請稍後再試一次。", "網路錯誤");
                    break;
                case BeanfunBroker.TransactionResultStatus.LoginFirst:
                    MessageBox.Show("帳號已登出。\n可能已從其他地方登入。", "");
                    break;
            }
        }
    }
}
