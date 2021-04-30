using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using Utility.ModifyRegistry;
using Microsoft.Win32;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using CSharpAnalytics;
using System.Reflection;

namespace MaplestoryLauncher
{
    using ExtentionMethods;

    enum LoginMethod : int {
        Regular = 0,
        Keypasco = 1,
        PlaySafe = 2,
        QRCode = 3
    };

    public partial class MainWindow : Form
    {
        public BeanfunClient bfClient;

        public BeanfunClient.QRCodeClass qrcodeClass;

        private string service_code = "610074" , service_region = "T9" , service_name = "新楓之谷";

        //public List<GameService> gameList = new List<GameService>();

        private CSharpAnalytics.Activities.AutoTimedEventActivity timedActivity = null;

        private Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

        private GamePathDB gamePaths = new GamePathDB();

        enum LogInState
        {
            LoggedOut,
            LoggedIn
        }

        private LogInState status = LogInState.LoggedOut;

        readonly HelperFunctions.UI UI;

        public MainWindow()
        {
            UI = new HelperFunctions.UI(this, 200);

            if (Properties.Settings.Default.GAEnabled)
            {
                try
                {
                    AutoMeasurement.Instance = new WinFormAutoMeasurement();
                    AutoMeasurement.DebugWriter = d => Debug.WriteLine(d);
                    AutoMeasurement.Start(new MeasurementConfiguration("UA-75983216-4", Assembly.GetExecutingAssembly().GetName().Name, currentVersion.ToString()));
                }
                catch
                {
                    this.timedActivity = null;
                    Properties.Settings.Default.GAEnabled = false;
                    Properties.Settings.Default.Save();
                }
            }

            timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("FormLoad", Properties.Settings.Default.loginMethod.ToString());

            InitializeComponent();
            UI.FormLoaded();

            if (Properties.Settings.Default.GAEnabled && this.timedActivity != null)
            {
                AutoMeasurement.Client.Track(this.timedActivity);
                this.timedActivity = null;
            }
        }

        #region ButtonEvents
        // The login/logout botton.
        private void loginButton_Click(object sender, EventArgs e)
        {
            switch(status)
            {
                case LogInState.LoggedOut:
                    //log in
                    UI.LoggingIn();
                    if (Properties.Settings.Default.GAEnabled)
                    {
                        timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("Login", Properties.Settings.Default.loginMethod.ToString());
                        AutoMeasurement.Client.TrackEvent("Login" + Properties.Settings.Default.loginMethod.ToString(), "Login");
                    }
                    this.loginWorker.RunWorkerAsync(Properties.Settings.Default.loginMethod);
                    break;
                case LogInState.LoggedIn:
                    //log out
                    if (pingWorker.IsBusy)
                        pingWorker.CancelAsync();
                    bfClient.Logout();
                    status = LogInState.LoggedOut;
                    UI.LoggedOut();
                    break;
            }
        }    

        // The start game/get OTP button.
        private void getOtpButton_Click(object sender, EventArgs e)
        {
            if (accounts.SelectedItems.Count <= 0)
                return;

            UI.GettingOtp();
            if (Properties.Settings.Default.GAEnabled)
            {
                timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("GetOTP", Properties.Settings.Default.loginMethod.ToString());
                AutoMeasurement.Client.TrackEvent("GetOTP" + Properties.Settings.Default.loginMethod.ToString(), "GetOTP");
            }
            this.getOtpWorker.RunWorkerAsync(accounts.SelectedItems[0].Index);
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
                    else if (runIfNot)
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

        #region CheckBoxEvents
        private void rememberAccount_CheckedChanged(object sender, EventArgs e)
        {
            bool check = rememberAccount.Checked;
            if (!check)
            {
                rememberPwd.Checked = false;
                autoLogin.Checked = false;
            }
            Properties.Settings.Default.rememberAccount = check;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.rememberAccount.Checked ? "rememberAccountOn" : "rememberAccountOff", "rememberAccountCheckbox");
            }
        }


        private void rememberPwd_CheckedChanged(object sender, EventArgs e)
        {
            bool check = rememberPwd.Checked;
            if (check)
                rememberAccount.Checked = true;
            else
                autoLogin.Checked = false;
            Properties.Settings.Default.rememberPwd = check;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.rememberPwd.Checked ? "rememberPwdOn" : "rememberPwdOff", "rememberPwdCheckbox");
            }
        }

        private void AutoLogin_CheckedChanged(object sender, EventArgs e)
        {
            if (autoLogin.Checked)
            {
                rememberAccount.Checked = true;
                rememberPwd.Checked = true;
            }

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.autoLogin.Checked ? "autoLoginOn" : "autoLoginOff", "loginCheckbox");
            }
        }

        private void autoSelect_CheckedChanged(object sender, EventArgs e)
        {
            bool check = autoSelect.Checked;
            if (!check)
                autoLaunch.Checked = false;
            Properties.Settings.Default.autoSelect = check;
            if (Properties.Settings.Default.autoSelect)
                if (accounts.SelectedItems.Count == 0)
                    Properties.Settings.Default.autoSelectIndex = 0;
                else
                    Properties.Settings.Default.autoSelectIndex = accounts.SelectedItems[0].Index;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.autoSelect.Checked ? "autoSelectOn" : "autoSelectOff", "autoSelectCheckbox");
            }
        }

        private void autoLaunch_CheckedChanged(object sender, EventArgs e)
        {
            bool check = autoLaunch.Checked;
            if (check)
                autoSelect.Checked = true;
            Properties.Settings.Default.opengame = check;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.autoLaunch.Checked ? "autoLaunchOn" : "autoLaunchOff", "autoLaunchCheckbox");
            }
        }

        private void keepLogged_CheckedChanged(object sender, EventArgs e)
        {
            if (keepLogged.Checked)
                if (!this.pingWorker.IsBusy)
                    this.pingWorker.RunWorkerAsync();
                else
                    if (this.pingWorker.IsBusy)
                {
                    this.pingWorker.CancelAsync();
                }
            Properties.Settings.Default.Save();
        }

        private void autoPaste_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.autoPaste.Checked ? "autoPasteOn" : "autoPasteOff", "autoPasteCheckbox");
            }
        }
        #endregion

        private void accounts_DoubleClick(object sender, EventArgs e)
        {
            if (accounts.SelectedItems.Count == 1)
            {
                try
                {
                    Clipboard.SetText(this.bfClient.accountList[this.accounts.SelectedItems[0].Index].sacc);
                }
                catch
                {

                }
            }
        }

        private void otpDisplay_OnClick(object sender, EventArgs e)
        {
            
            if (otpDisplay.Text == "" || !otpDisplay.Text.IsAllDigits()) return;
            try
            {
                Clipboard.SetText(otpDisplay.Text);
            }
            catch
            {

            }
        }

        private void accounts_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (accounts.SelectedItems.Count == 0)
                getOtpButton.Enabled = false;
            else
                getOtpButton.Enabled = true;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            UI.Refresh();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (status == LogInState.LoggedIn)
            {
                bool autoLogin = Properties.Settings.Default.autoLogin;
                if (loginButton.Enabled)
                    loginButton_Click(null, null);
                else
                {
                    e.Cancel = true;
                    UI.ShowError("請先登出再關閉程式。");
                }
                Properties.Settings.Default.autoLogin = autoLogin;
                Properties.Settings.Default.Save();
            }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!rememberAccount.Checked)
                Properties.Settings.Default.AccountID = "";
            if (!rememberPwd.Checked)
            {
                Properties.Settings.Default.entropy = "";
                File.Delete("UserState.dat");
            }
            if (!autoLogin.Checked)
                Properties.Settings.Default.autoLogin = false;
            //gamePaths.Save();
            Properties.Settings.Default.Save();
        }
    }
}