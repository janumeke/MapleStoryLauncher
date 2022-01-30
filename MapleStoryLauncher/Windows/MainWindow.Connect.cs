﻿using System;
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

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public BeanfunBroker beanfun = new();

        List<BeanfunBroker.GameAccount> gameAccounts = default;

        # region Login
        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("loginWorker starting");
            Thread.CurrentThread.Name = "Login Worker";

            if (accountInput.Text == "" && pwdInput.Text == "")
            {
                QRCodeWindow qrcodeWindow = new(this);
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
                        AppAuthWindow appAuthWindow = new(this);
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
                    gameAccounts = result.GameAccounts;
                e.Result = result;
            }
        }

        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("loginWorker end");

            if (e.Cancelled)
                ui.LoginFailed();
            else
                switch (((BeanfunBroker.LoginResult)e.Result).Status)
                {
                    case BeanfunBroker.LoginStatus.Success:
                        status = LogInState.LoggedIn;
                        pingTimer.Interval = pingTimeout;
                        pingTimer.Start();
                        ui.LoggedIn();
                        break;
                    default:
                        ShowError((BeanfunBroker.LoginResult)e.Result);
                        ui.LoginFailed();
                        break;
                }
        }
        #endregion

        #region Ping
        private const int pingTimeout = 10 * 60 * 1000; //10 mins
        private int pingFailedTries = 0;
        private void pingTimer_Tick(object sender, EventArgs e)
        {
            if (!pingTimer.Enabled)
                return;

            BeanfunBroker.LoginResult result = beanfun.Ping(); //beanfun has reader-writer lock
            switch (result.Status)
            {
                case BeanfunBroker.LoginStatus.Failed:
                    pingTimer.Stop();
                    ShowError(result);
                    break;
                case BeanfunBroker.LoginStatus.ConnectionLost:
                    pingTimer.Interval = 3;
                    if (++pingFailedTries > 5)
                    {
                        pingTimer.Stop();
                        ShowError(result);
                        status = LogInState.LoggedOut;
                        ui.LoggedOut();
                    }
                    break;
                case BeanfunBroker.LoginStatus.LoginFirst:
                    pingTimer.Stop();
                    status = LogInState.LoggedOut;
                    ui.LoggedOut();
                    ShowError(result);
                    break;
                case BeanfunBroker.LoginStatus.Success:
                    pingTimer.Interval = pingTimeout;
                    break;
            }
        }
        #endregion

        #region OTP
        private void getOtpWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("getOtpWorker start");
            Thread.CurrentThread.Name = "GetOTP Worker";

            BeanfunBroker.LoginResult ping = beanfun.Ping(); //beanfun has reader-writer lock
            if (ping.Status != BeanfunBroker.LoginStatus.Success)
            {
                e.Result = ping;
                return;
            }

            BeanfunBroker.GameAccount gameAccount = (BeanfunBroker.GameAccount)e.Argument;
            e.Result = beanfun.GetOTP(gameAccount); //beanfun has reader-writer lock
        }

        private void getOtpWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("getOtpWorker end");
            BeanfunBroker.LoginResult result = (BeanfunBroker.LoginResult)e.Result;
            switch (result.Status)
            {
                case BeanfunBroker.LoginStatus.Success:
                    if (IsGameRunning())
                        ui.OtpGot(result.Message);
                    else
                    {
                        StartGame(((BeanfunBroker.OTPResult)result).Username, result.Message);
                        ui.OtpGot("");
                    }
                    break;
                case BeanfunBroker.LoginStatus.LoginFirst:
                    ui.OtpGot("");
                    ui.LoggedOut();
                    ShowError((BeanfunBroker.LoginResult)e.Result);
                    break;
                default:
                    ShowError((BeanfunBroker.LoginResult)e.Result);
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
                Debug.WriteLine("try open game");
                ProcessStartInfo psInfo = new()
                {
                    FileName = path,
                    Arguments = arg,
                    WorkingDirectory = Path.GetDirectoryName(path)
                };
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
                case BeanfunBroker.LoginStatus.Expired:
                case BeanfunBroker.LoginStatus.Denied:
                    MessageBox.Show(result.Message, "");
                    break;
                case BeanfunBroker.LoginStatus.Failed:
                    if (MessageBox.Show(result.Message + "\n請回報給開發者。\n是否產生記錄檔?", "預期外的錯誤", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\MaplestoryLauncher\\LastResponse.txt", beanfun.GetLastResponse().ToString());
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
