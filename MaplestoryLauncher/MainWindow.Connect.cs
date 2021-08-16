using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using CSharpAnalytics;

namespace MaplestoryLauncher
{
    public partial class MainWindow : Form
    {
        # region Login
        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            pingWorker.CancelAsync();
            while (pingWorker.IsBusy)
                Thread.Sleep(133);

            Debug.WriteLine("loginWorker starting");
            Thread.CurrentThread.Name = "Login Worker";
            try
            {
                if (accountInput.Text == "" && pwdInput.Text == "")
                {
                    QRCodeWindow qrcodeWindow = new QRCodeWindow(this);
                    qrcodeWindow.ShowDialog();
                    if (qrcodeWindow.result == BeanfunClient.QRCodeLoginState.Successful)
                        bfClient.Login("", "", BeanfunClient.LoginMethod.QRCode);
                    else if (qrcodeWindow.result == BeanfunClient.QRCodeLoginState.Pending)
                        e.Cancel = true;
                        
                }
                else
                {
                    bfClient = new BeanfunClient();
                    bfClient.Login(accountInput.Text, pwdInput.Text, BeanfunClient.LoginMethod.Regular);
                }
                    
                if (bfClient.errmsg != null)
                    e.Result = bfClient.errmsg;
                else
                    e.Result = null;
            }
            catch (Exception ex)
            {
                e.Result = "登入失敗，未知的錯誤。\n\n" + ex.Message + "\n" + ex.StackTrace;
            }
        }

        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Properties.Settings.Default.GAEnabled && this.timedActivity != null)
            {
                AutoMeasurement.Client.Track(this.timedActivity);
                this.timedActivity = null;
            }
            Debug.WriteLine("loginWorker end");

            if (e.Cancelled)
            {
                UI.LoginFailed();
                return;
            }
            if ((string)e.Result != null)
            {
                UI.ShowError((string)e.Result, HelperFunctions.UI.ErrorType.LoginFailed);
                return;
            }

            if (Properties.Settings.Default.keepLogged && !this.pingWorker.IsBusy)
                this.pingWorker.RunWorkerAsync();
            status = LogInState.LoggedIn;
            UI.LoggedIn();
        }
        #endregion

        #region OTP
        enum GameState
        {
            Running,
            Run,
            Failed
        }

        private void getOtpWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("getOtpWorker start");
            Thread.CurrentThread.Name = "GetOTP Worker";
            int index = (int)e.Argument;
            Debug.WriteLine("Count = " + this.bfClient.accountList.Count + " | index = " + index);

            e.Result = new object[3]; //{index, otp, gameRun}
            ref object resultIndex = ref ((object[])e.Result)[0];
            ref object resultOtp = ref ((object[])e.Result)[1];
            ref object resultGameRun = ref ((object[])e.Result)[2];
            
            resultIndex = index;
            if (this.bfClient.accountList.Count <= index)
            {
                resultIndex = -1;
                if (GameIsRunning())
                    resultGameRun = GameState.Running;
                else
                    resultGameRun = GameState.Failed;
                return;
            }
            Debug.WriteLine("call GetOTP");
            resultOtp = bfClient.GetOTP(this.bfClient.accountList[index]);
            Debug.WriteLine("call GetOTP done");
            if (resultOtp == null)
            {
                resultIndex = -1;
                if (GameIsRunning())
                    resultGameRun = GameState.Running;
                else
                    resultGameRun = GameState.Failed;
                return;
            }

            if (Properties.Settings.Default.GAEnabled)
            {
                try
                {
                    AutoMeasurement.Client.TrackEvent(Path.GetFileName(gamePaths.Get(service_name)), "processName");
                }
                catch
                {
                    Debug.WriteLine("invalid path:" + gamePaths.Get(service_name));
                }
            }

            bool? gameStarted = null;
            if (GameIsRunning(
                true,
                bfClient.accountList[index].sacc,
                new string(Convert.ToString(resultOtp).Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray()),
                ref gameStarted
            ))
            {
                resultGameRun = GameState.Running;
            }
            else if (gameStarted == true)
                resultGameRun = GameState.Run;
            else
                resultGameRun = GameState.Failed;
        }

        private void getOtpWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Properties.Settings.Default.GAEnabled && this.timedActivity != null)
            {
                AutoMeasurement.Client.Track(this.timedActivity);
                this.timedActivity = null;
            }
            Debug.WriteLine("getOtpWorker end");

            int resultIndex = (int)((object[])e.Result)[0];
            string resultOtp = Convert.ToString(((object[])e.Result)[1]);
            GameState resultGameRun = (GameState)((object[])e.Result)[2];
            
            if (e.Error != null)
            {
                UI.OtpGot("取得失敗");
                UI.ShowError(e.Error.Message);
                return;
            }
            if (resultIndex == -1)
            {
                UI.OtpGot("取得失敗");
                UI.ShowError(this.bfClient.errmsg);
            }
            else
            {
                /*int accIndex = accounts.SelectedItems[0].Index;
                string acc = this.bfClient.accountList[index].sacc;
                this.Text = "進行遊戲 - " + WebUtility.HtmlDecode(this.bfClient.accountList[index].sname);*/
                
                if (resultGameRun == GameState.Running)
                    UI.OtpGot(resultOtp);
                else
                    UI.OtpGot("");
            }

            if (Properties.Settings.Default.keepLogged && !this.pingWorker.IsBusy)
                this.pingWorker.RunWorkerAsync();
        }

        private bool GameIsRunning()
        {
            bool? tmp = null;
            return GameIsRunning(false, "", "", ref tmp);
        }

        private bool GameIsRunning(bool runIfNot, string sacc, string otp, ref bool? started)
        {
            switch (service_name)
            {
                case "新楓之谷":
                    string fileName = gamePaths.GetAlias(service_name);
                    string processName = fileName.Substring(0, fileName.Length - 4); //Remove '.exe'
                    if (Process.GetProcessesByName(processName).Length != 0)
                    {
                        Debug.WriteLine("find game");
                        started = null;
                        return true;
                    }
                    else if (runIfNot)
                        if(otp == "")
                            started = processStart(gamePaths.Get(service_name), "");
                        else
                            started = processStart(gamePaths.Get(service_name), "tw.login.maplestory.gamania.com 8484 BeanFun " + sacc + " " + otp);
                    break;
            }
            return false;
        }

        private bool processStart(string prog, string arg)
        {
            try
            {
                Debug.WriteLine("try open game");
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = prog;
                psInfo.Arguments = arg;
                psInfo.WorkingDirectory = Path.GetDirectoryName(prog);
                Process.Start(psInfo);
                Debug.WriteLine("try open game done");
                return true;
            }
            catch
            {
                UI.ShowError("啟動失敗，請嘗試手動以系統管理員身分啟動遊戲。");
                return false;
            }
        }
        #endregion

        #region Ping
        private void pingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.CurrentThread.Name = "ping Worker";
            Debug.WriteLine("pingWorker start");
            const int WaitSecs = 60; // 1min

            while (Properties.Settings.Default.keepLogged)
            {
                if (this.pingWorker.CancellationPending)
                {
                    Debug.WriteLine("break due to cancel");
                    break;
                }

                if (this.getOtpWorker.IsBusy || this.loginWorker.IsBusy)
                {
                    Debug.WriteLine("ping.busy sleep 1s");
                    System.Threading.Thread.Sleep(1000 * 1);
                    continue;
                }

                if (this.bfClient != null)
                    this.bfClient.Ping();

                for (int i = 0; i < WaitSecs; ++i)
                {
                    if (this.pingWorker.CancellationPending)
                        break;
                    System.Threading.Thread.Sleep(1000 * 1);
                }
            }
        }

        private void pingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("ping.done");
        }
        #endregion
    }
}
