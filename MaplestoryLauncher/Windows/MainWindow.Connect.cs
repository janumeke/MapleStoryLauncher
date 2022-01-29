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
using System.Threading;
using System.IO;

namespace MaplestoryLauncher
{
    public partial class MainWindow : Form
    {
        public BeanfunBroker beanfun = new BeanfunBroker();

        List<BeanfunBroker.GameAccount> gameAccounts = default;

        # region Login
        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("loginWorker starting");
            Thread.CurrentThread.Name = "Login Worker";
            if (pingWorker.IsBusy)
                pingWorker.CancelAsync();
            if (accountInput.Text == "" && pwdInput.Text == "")
            {
                QRCodeWindow qrcodeWindow = new QRCodeWindow(this);
                qrcodeWindow.ShowDialog();
                e.Result = qrcodeWindow.result;
                if (e.Result == default(BeanfunBroker.LoginResult))
                    e.Cancel = true;
            }
            else
            {
                BeanfunBroker.LoginResult result = beanfun.Login(accountInput.Text, pwdInput.Text);
                switch (result.Status)
                {
                    case BeanfunBroker.LoginStatus.RequireAppAuthentication:
                        AppAuthWindow appAuthWindow = new AppAuthWindow(this);
                        appAuthWindow.ShowDialog();
                        e.Result = appAuthWindow.result;
                        if (e.Result == default(BeanfunBroker.LoginResult))
                            e.Cancel = true;
                        break;
                    default:
                        e.Result = result;
                        break;
                }
            }

            if( !e.Cancel &&
                ((BeanfunBroker.LoginResult)e.Result).Status == BeanfunBroker.LoginStatus.Success)
            {
                BeanfunBroker.GameAccountResult result = beanfun.GetGameAccounts();
                if (result.Status == BeanfunBroker.LoginStatus.Success)
                    gameAccounts = result.gameAccounts;
                e.Result = result;
            }
        }

        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("loginWorker end");
            if (e.Cancelled)
            {
                ui.LoginFailed();
                return;
            }
            switch (((BeanfunBroker.LoginResult)e.Result).Status)
            {
                case BeanfunBroker.LoginStatus.Success:
                    if (!pingWorker.IsBusy)
                        pingWorker.RunWorkerAsync();
                    status = LogInState.LoggedIn;
                    ui.LoggedIn();
                    break;
                default:
                    ShowError((BeanfunBroker.LoginResult)e.Result);
                    ui.LoginFailed();
                    break;
            }
            return;
        }
        #endregion

        #region OTP
        private void getOtpWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("getOtpWorker start");
            Thread.CurrentThread.Name = "GetOTP Worker";

            BeanfunBroker.GameAccount gameAccount = (BeanfunBroker.GameAccount)e.Argument;
            BeanfunBroker.OTPResult result = beanfun.GetOTP(gameAccount); //beanfun has reader-writer lock
            e.Result = result;
        }

        private void getOtpWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("getOtpWorker end");
            BeanfunBroker.OTPResult result = (BeanfunBroker.OTPResult)e.Result;
            switch (result.Status)
            {
                case BeanfunBroker.LoginStatus.Success:
                    if (IsGameRunning())
                        ui.OtpGot(result.Message);
                    else
                    {
                        StartGame(result.Username, result.Message);
                        ui.OtpGot("");
                    }
                    break;
                default:
                    ShowError((BeanfunBroker.OTPResult)e.Result);
                    ui.OtpGot("");
                    break;
            }
        }
        #endregion

        #region Ping
        private void pingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.CurrentThread.Name = "ping Worker";
            Debug.WriteLine("pingWorker start");
            const int WaitSecs = 600; //10 mins
            bool cancel = false;

            while (!cancel)
            {   
                if(!pingWorker.CancellationPending)
                {
                    BeanfunBroker.LoginResult result = beanfun.Ping(); //beanfun has reader-writer lock
                    switch (result.Status)
                    {
                        case BeanfunBroker.LoginStatus.Failed:
                        case BeanfunBroker.LoginStatus.LoginFirst:
                            e.Result = result;
                            cancel = true;
                            break;
                        case BeanfunBroker.LoginStatus.ConnectionLost:
                        case BeanfunBroker.LoginStatus.Success:
                            break;
                    }
                }

                for (int i = 1; i <= WaitSecs; ++i)
                {
                    if (pingWorker.CancellationPending)
                        cancel = true;
                    System.Threading.Thread.Sleep(1000 * 1);
                }
            }
        }

        private void pingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("ping.done");

            if (!e.Cancelled)
            {
                ShowError((BeanfunBroker.LoginResult)e.Result);
                ui.LoggedOut();
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

        private bool StartGame()
        {
            return StartProcess(Properties.Settings.Default.gamePath, "");
        }

        private bool StartGame(string username, string otp)
        {
            return StartProcess(Properties.Settings.Default.gamePath, "tw.login.maplestory.gamania.com 8484 BeanFun " + username + " " + otp);
        }

        private bool StartProcess(string path, string arg)
        {
            try
            {
                Debug.WriteLine("try open game");
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = path;
                psInfo.Arguments = arg;
                psInfo.WorkingDirectory = Path.GetDirectoryName(path);
                Process.Start(psInfo);
                Debug.WriteLine("try open game done");
                return true;
            }
            catch
            {
                MessageBox.Show("啟動失敗，請嘗試手動以系統管理員身分啟動遊戲。", "");
                return false;
            }
        }

        private void ShowError(BeanfunBroker.LoginResult result)
        {
            switch (result.Status)
            {
                case BeanfunBroker.LoginStatus.Denied:
                case BeanfunBroker.LoginStatus.Expired:
                    MessageBox.Show(result.Message, "");
                    break;
                case BeanfunBroker.LoginStatus.Failed:
                    if (MessageBox.Show(result.Message + "\n請回報給開發者。\n是否產生記錄檔?", "預期外的錯誤", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        File.WriteAllText(Environment.SpecialFolder.LocalApplicationData + "\\MaplestoryLauncher\\LastResponse.txt", beanfun.GetLastResponse().ToString());
                    break;
                case BeanfunBroker.LoginStatus.ConnectionLost:
                    MessageBox.Show(result.Message + "\n請稍後再試一次。", "網路錯誤");
                    break;
                case BeanfunBroker.LoginStatus.LoginFirst:
                    MessageBox.Show("帳號已登出。\n可能已從其他地方登入。", "");
                    break;
            }
        }
    }
}
