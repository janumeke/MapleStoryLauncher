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

    public partial class MainWindow : Form
    {
        public BeanfunClient bfClient;

        private string service_name = "新楓之谷";

        private GamePathDB gamePaths = new GamePathDB();

        //public List<GameService> gameList = new List<GameService>();

        private CSharpAnalytics.Activities.AutoTimedEventActivity timedActivity = null;
        
        enum LogInState
        {
            LoggedOut,
            LoggedIn
        }

        private LogInState status = LogInState.LoggedOut;

        readonly HelperFunctions.UI UI;

        public MainWindow()
        {
            UI = new HelperFunctions.UI(this);

            if (Properties.Settings.Default.GAEnabled)
            {
                try
                {
                    AutoMeasurement.Instance = new WinFormAutoMeasurement();
                    AutoMeasurement.DebugWriter = d => Debug.WriteLine(d);
                    AutoMeasurement.Start(new MeasurementConfiguration("UA-75983216-4", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString()));
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
                    //Log in
                    UI.LoggingIn();
                    if (Properties.Settings.Default.GAEnabled)
                    {
                        timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("Login", Properties.Settings.Default.loginMethod.ToString());
                        AutoMeasurement.Client.TrackEvent("Login" + Properties.Settings.Default.loginMethod.ToString(), "Login");
                    }
                    loginWorker.RunWorkerAsync();
                    break;
                case LogInState.LoggedIn:
                    //Log out or close window
                    if (pingWorker.IsBusy)
                        pingWorker.CancelAsync();
                    bfClient.Logout();
                    if (!(e is FormClosingEventArgs)) //Log out only
                    {
                        status = LogInState.LoggedOut; //Closing does't change the state
                        Password.Delete();
                    }
                    if (!autoSelect.Checked)
                    {
                        Properties.Settings.Default.autoSelectIndex = -1;
                        Properties.Settings.Default.Save();
                    }
                    UI.LoggedOut();
                    break;
            }
        }    

        // The start game/get OTP button.
        private void getOtpButton_Click(object sender, EventArgs e)
        {
            if (GameIsRunning() && accounts.SelectedItems.Count == 0)
                return;
            if (!GameIsRunning() && !UI.CheckGamePath())
                return;

            if (accounts.SelectedItems.Count == 0)
            {
                bool? gameStarted = null;
                GameIsRunning(true, "", "", ref gameStarted);
            }
            else
            {
                UI.GettingOtp();
                if (Properties.Settings.Default.GAEnabled)
                {
                    timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("GetOTP", Properties.Settings.Default.loginMethod.ToString());
                    AutoMeasurement.Client.TrackEvent("GetOTP" + Properties.Settings.Default.loginMethod.ToString(), "GetOTP");
                }
                getOtpWorker.RunWorkerAsync(accounts.SelectedItems[0].Index);
            }
        }
        #endregion

        #region CheckBoxEvents
        private void rememberAccount_CheckedChanged(object sender, EventArgs e)
        {
            if (!rememberAccount.Checked)
            {
                rememberPwd.Checked = false;
                autoLogin.Checked = false;
            }

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.rememberAccount.Checked ? "rememberAccountOn" : "rememberAccountOff", "rememberAccountCheckbox");
            }
        }


        private void rememberPwd_CheckedChanged(object sender, EventArgs e)
        {
            if (rememberPwd.Checked)
                rememberAccount.Checked = true;
            else
                autoLogin.Checked = false;
            
            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.rememberPwd.Checked ? "rememberPwdOn" : "rememberPwdOff", "rememberPwdCheckbox");
            }
        }

        private void autoLogin_CheckedChanged(object sender, EventArgs e)
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
            UI.EmboldenAutoSelection(autoSelect.Checked);
            UI.UpdateAutoLaunchCheckBoxText();

            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.autoSelect.Checked ? "autoSelectOn" : "autoSelectOff", "autoSelectCheckbox");
            }
        }

        private void autoLaunch_CheckedChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.GAEnabled)
            {
                AutoMeasurement.Client.TrackEvent(this.autoLaunch.Checked ? "autoLaunchOn" : "autoLaunchOff", "autoLaunchCheckbox");
            }
        }

        private void keepLogged_CheckedChanged(object sender, EventArgs e)
        {
            if (keepLogged.Checked)
            {
                if (!this.pingWorker.IsBusy)
                    this.pingWorker.RunWorkerAsync();
            }
            else
                if (this.pingWorker.IsBusy)
                    this.pingWorker.CancelAsync();
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

        //Copy on click
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
                Notification.Hide(otpDisplay);
                Notification.Show("已複製密碼！", otpDisplay, (int)(0.25 * otpDisplay.Width), (int)(-1.5 * otpDisplay.Height), 1000);
            }
            catch
            {

            }
        }

        private void otpDisplay_DoubleClick(object sender, EventArgs e)
        {
            if (otpDisplay.Text == "" || !otpDisplay.Text.IsAllDigits()) return;
            if (otpDisplay.PasswordChar == default)
                otpDisplay.PasswordChar = '*';
            else
                otpDisplay.PasswordChar = default;
        }

        //Refresh UI when changed
        private void Input_TextChanged(object sender, EventArgs e)
        {
            UI.UpdateLoginButtonText();
        }

        private void accounts_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UI.UpdateGetOtpButton();
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            UI.FormFocused();
        }

        //Cleanup before closing
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (status == LogInState.LoggedIn)
                if (loginButton.Enabled)
                    loginButton_Click(null, e);
                else
                {
                    e.Cancel = true;
                    UI.ShowError("請先登出再關閉程式。");
                }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

            Properties.Settings.Default.rememberAccount = rememberAccount.Checked;
            Properties.Settings.Default.rememberPwd = rememberPwd.Checked;
            Properties.Settings.Default.autoSelect = autoSelect.Checked;
            Properties.Settings.Default.autoLaunch = autoLaunch.Checked;

            switch (status)
            {
                case LogInState.LoggedIn:
                    if (rememberAccount.Checked)
                        Properties.Settings.Default.accountID = accountInput.Text;
                    else
                        Properties.Settings.Default.accountID = "";
                    if (rememberPwd.Checked)
                        Password.Save(pwdInput.Text);
                    else
                        Password.Delete();
                    if (!autoSelect.Checked)
                        Properties.Settings.Default.autoSelectIndex = -1;
                    Properties.Settings.Default.autoLogin = autoLogin.Checked;
                    break;
                case LogInState.LoggedOut:
                    if (!rememberAccount.Checked)
                        Properties.Settings.Default.accountID = "";
                    if (!rememberPwd.Checked)
                        Password.Delete();
                    Properties.Settings.Default.autoLogin = false;
                    break;
            }

            Properties.Settings.Default.Save();
        }
    }
}