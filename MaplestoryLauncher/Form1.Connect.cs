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
    public partial class main : Form
    {
        private string otp;

        // Login do work.
        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.rememberAccount.Checked == true)
            {
                Properties.Settings.Default.AccountID = this.accountInput.Text;
                Properties.Settings.Default.Save();
            }

            while (this.pingWorker.IsBusy)
                Thread.Sleep(137);
            Debug.WriteLine("loginWorker starting");
            Thread.CurrentThread.Name = "Login Worker";
            e.Result = "";
            try
            {
                if (Properties.Settings.Default.loginMethod != (int)LoginMethod.QRCode)
                    this.bfClient = new BeanfunClient();
                this.bfClient.Login(this.accountInput.Text, this.passwdInput.Text, Properties.Settings.Default.loginMethod, this.qrcodeClass, this.service_code, this.service_region);
                if (this.bfClient.errmsg != null)
                    e.Result = this.bfClient.errmsg;
                else
                    e.Result = null;
            }
            catch (Exception ex)
            {
                e.Result = "登入失敗，未知的錯誤。\n\n" + ex.Message + "\n" + ex.StackTrace;
            }
        }

        // Login completed.
        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Properties.Settings.Default.GAEnabled && this.timedActivity != null)
            {
                AutoMeasurement.Client.Track(this.timedActivity);
                this.timedActivity = null;
            }
            Debug.WriteLine("loginWorker end");

            this.loginButton.Text = "登入";
            this.loginButton.Enabled = true;
            this.UseWaitCursor = false;
            if (e.Error != null)
            {
                errexit(e.Error.Message, 1);
                return;
            }
            if ((string)e.Result != null)
            {
                errexit((string)e.Result, 1);
                return;
            }

            if (Properties.Settings.Default.keepLogged && !this.pingWorker.IsBusy)
                this.pingWorker.RunWorkerAsync();
            this.accountInput.Enabled = false;
            this.passwdInput.Enabled = false;
            if (this.rememberPwd.Checked == true)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open("UserState.dat", FileMode.Create)))
                {
                    // Create random entropy of 8 characters.
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    string entropy = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());

                    Properties.Settings.Default.entropy = entropy;
                    writer.Write(ciphertext(this.passwdInput.Text, entropy));
                }
            }
            Properties.Settings.Default.autoLogin = autoLogin.Checked;
            Properties.Settings.Default.Save();
            status = LogInState.LoggedIn;
            this.loginButton.Text = "登出";
            if (!GameIsRunning())
                getOtpButton.Text = "啟動遊戲";
            else
                getOtpButton.Text = "取得一次性密碼";

            try
            {
                redrawSAccountList();

                // Handle panel switching.
                Size = new Size(Size.Width, 450);
                this.AcceptButton = this.getOtpButton;
                if (Properties.Settings.Default.autoSelect && Properties.Settings.Default.autoSelectIndex < this.accounts.Items.Count)
                    this.accounts.Items[Properties.Settings.Default.autoSelectIndex].Selected = true;
                //this.accounts.Select();
                if (Properties.Settings.Default.opengame && Properties.Settings.Default.autoSelectIndex < this.bfClient.accountList.Count())
                    getOtpButton_Click(null, null);
                if (Properties.Settings.Default.keepLogged && !this.pingWorker.IsBusy)
                    this.pingWorker.RunWorkerAsync();
                ShowToolTip(accounts, "步驟1", "選擇欲開啟的遊戲帳號，雙擊以複製帳號。");
                ShowToolTip(getOtpButton, "步驟2", "按下以啟動遊戲並登入。或是在右側產生並自動複製密碼，至遊戲中貼上帳密登入。");
                //Tip.SetToolTip(getOtpButton, "點擊取得密碼");
                Tip.SetToolTip(accounts, "雙擊即自動複製");
                Tip.SetToolTip(otpDisplay, "點擊一次即自動複製");
                Properties.Settings.Default.showTip = false;
                Properties.Settings.Default.Save();
            }
            catch
            {
                errexit("登入失敗，無法取得帳號列表。", 1);
            }

        }

        private void redrawSAccountList()
        {
            accounts.Items.Clear();
            foreach (var account in this.bfClient.accountList)
            {
                string[] row = { WebUtility.HtmlDecode(account.sname), account.sacc };
                var listViewItem = new ListViewItem(row);
                this.accounts.Items.Add(listViewItem);
            }
        }

        enum GameState
        {
            Running,
            Run,
            Failed
        }

        // getOTP do work.
        private void getOtpWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (this.pingWorker.IsBusy)
                Thread.Sleep(133);
            Debug.WriteLine("getOtpWorker start");
            Thread.CurrentThread.Name = "GetOTP Worker";
            int index = (int)e.Argument;
            e.Result = new int[2];
            ref int resultIndex = ref ((int[])e.Result)[0];
            ref int resultGameRun = ref ((int[])e.Result)[1];
            resultIndex = index;
            Debug.WriteLine("Count = " + this.bfClient.accountList.Count + " | index = " + index);
            if (this.bfClient.accountList.Count <= index)
            {
                resultIndex = -1;
                return;
            }
            Debug.WriteLine("call GetOTP");
            this.otp = this.bfClient.GetOTP(Properties.Settings.Default.loginMethod, this.bfClient.accountList[index], this.service_code, this.service_region);
            Debug.WriteLine("call GetOTP done");
            if (this.otp == null)
            {
                resultIndex = -1;
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
                new string(this.otp.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray()),
                ref gameStarted
            ))
            {
                resultGameRun = (int)GameState.Running;
            }
            else if (gameStarted == true)
                resultGameRun = (int)GameState.Run;
            else
                resultGameRun = (int)GameState.Failed;
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
                    if (Process.GetProcessesByName("Maplestory").Length != 0)
                    {
                        Debug.WriteLine("find game");
                        started = null;
                        return true;
                    }
                    else if(runIfNot)
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
                errexit("啟動失敗，請嘗試手動以系統管理員身分啟動遊戲。", 2);
                return false;
            }
        }

        // getOTP completed.
        private void getOtpWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Properties.Settings.Default.GAEnabled && this.timedActivity != null)
            {
                AutoMeasurement.Client.Track(this.timedActivity);
                this.timedActivity = null;
            }
            Debug.WriteLine("getOtpWorker end");

            /*const int VK_TAB = 0x09;
            const byte VK_CONTROL = 0x11;
            const int VK_V = 0x56;
            const int VK_ENTER = 0x0d;
            const byte KEYEVENTF_EXTENDEDKEY = 0x1;
            const byte KEYEVENTF_KEYUP = 0x2;*/

            int resultIndex = ((int[])e.Result)[0];
            GameState resultGameRun = (GameState)((int[])e.Result)[1];
            if (resultGameRun == GameState.Failed)
                getOtpButton.Text = "啟動遊戲";
            else
                getOtpButton.Text = "取得一次性密碼";
            this.accounts.Enabled = true;
            this.getOtpButton.Enabled = true;
            //this.comboBox2.Enabled = true;
            if (e.Error != null)
            {
                this.otpDisplay.Text = "取得失敗";
                errexit(e.Error.Message, 2);
                return;
            }

            if (resultIndex == -1)
            {
                this.otpDisplay.Text = "取得失敗";
                errexit(this.bfClient.errmsg, 2);
            }
            else
            {
                /*int accIndex = accounts.SelectedItems[0].Index;
                string acc = this.bfClient.accountList[index].sacc;
                this.Text = "進行遊戲 - " + WebUtility.HtmlDecode(this.bfClient.accountList[index].sname);

                try
                {
                    Clipboard.SetText(acc);
                }
                catch
                {
                    return;
                }

                IntPtr hWnd;
                if (autoPaste.Checked == true && (hWnd = WindowsAPI.FindWindow(null, "MapleStory")) != IntPtr.Zero)
                {
                    WindowsAPI.SetForegroundWindow(hWnd);

                    WindowsAPI.keybd_event(VK_CONTROL, 0x9d, KEYEVENTF_EXTENDEDKEY, 0);
                    WindowsAPI.keybd_event(VK_V, 0x9e, 0, 0);
                    Thread.Sleep(200);
                    WindowsAPI.keybd_event(VK_V, 0x9e, KEYEVENTF_KEYUP, 0);
                    Thread.Sleep(200);
                    WindowsAPI.keybd_event(VK_CONTROL, 0x9d, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

                    WindowsAPI.keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    WindowsAPI.keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP, 0);
                }*/

                if (resultGameRun == GameState.Running)
                {
                    otpDisplay.Text = otp;
                    try
                    {
                        Clipboard.SetText(otpDisplay.Text);
                    }
                    catch
                    {

                    }
                }
                else
                    otpDisplay.Text = "";

                /*Thread.Sleep(250);

                if (autoPaste.Checked == true && (hWnd = WindowsAPI.FindWindow(null, "MapleStory")) != IntPtr.Zero)
                {
                    WindowsAPI.keybd_event(VK_CONTROL, 0x9d, KEYEVENTF_EXTENDEDKEY, 0);
                    WindowsAPI.keybd_event(VK_V, 0x9e, 0, 0);
                    Thread.Sleep(200);
                    WindowsAPI.keybd_event(VK_V, 0x9e, KEYEVENTF_KEYUP, 0);
                    Thread.Sleep(200);
                    WindowsAPI.keybd_event(VK_CONTROL, 0x9d, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

                    WindowsAPI.keybd_event(VK_ENTER, 0, 0, 0);
                    WindowsAPI.keybd_event(VK_ENTER, 0, KEYEVENTF_KEYUP, 0);

                    accounts.Items[accIndex].BackColor = Color.Green;
                    accounts.Items[accIndex].Selected = false;
                }*/


            }

            if (Properties.Settings.Default.keepLogged && !this.pingWorker.IsBusy)
                this.pingWorker.RunWorkerAsync();
        }

        // Ping to Beanfun website.
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

        /*private void qrWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.bfClient = new BeanfunClient();
            string skey = this.bfClient.GetSessionkey();
            this.qrcodeClass = this.bfClient.GetQRCodeValue(skey, (bool)e.Argument);
        }

        private void qrWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.loginMethodInput.Enabled = true;
            wait_qrWorker_notify.Visible = false;
            if (this.qrcodeClass == null)
                wait_qrWorker_notify.Text = "QRCode取得失敗";
            else
            {
                qrcodeImg.Image = qrcodeClass.bitmap;
                qrCheckLogin.Enabled = true;
            }
        }

        private void qrCheckLogin_Tick(object sender, EventArgs e)
        {
            if (this.qrcodeClass == null)
            {
                MessageBox.Show("QRCode not get yet");
                return;
            }
            int res = this.bfClient.QRCodeCheckLoginStatus(this.qrcodeClass);
            if (res != 0)
                this.qrCheckLogin.Enabled = false;
            if (res == 1)
            {
                loginButton_Click(null, null);
            }
            if (res == -2)
            {
                comboBox1_SelectedIndexChanged(null, null);
            }
        }*/
    }
}
