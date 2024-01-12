using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        private ReCaptchaWindow reCaptchaWindow = new();

        public MainWindow()
        {
            CheckMultipleInstances();
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;

            Model();
            Controller_MainWindow();

            Controller_AccountInput();
            Controller_AddRemoveAccount();
            Controller_PasswordInput();
            Controller_RememberPwd();
            Controller_AutoLogin();
            Controller_LoginButton();
            Controller_PointsLabel();
            Controller_AccountListView();
            Controller_AutoSelect();
            Controller_AutoLaunch();
            Controller_GetOTPButton();
            Controller_OTPDisplay();
        }

        const int initialWindowHeight = 223;
        const int loggedInHeight = 482;

        private void Controller_MainWindow()
        {
            SyncEvents.LoggingIn += username =>
            {
                this.UseWaitCursor = true;
            };

            SyncEvents.LoginFailed += () =>
            {
                this.UseWaitCursor = false;
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                this.Size = new Size(this.Size.Width, loggedInHeight);
                this.UseWaitCursor = false;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                this.Size = new Size(this.Size.Width, initialWindowHeight);
            };
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.Size = new Size(this.Size.Width, initialWindowHeight);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text += $" v{version.Major}.{version.Minor}";
            if (version.Build != 0)
                this.Text += $".{version.Build}";
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            beanfun.Cancel();

            bool loggedInBefore = loggedIn;
            if (!AutoLogOut())
                e.Cancel = true;
            else if (activeAccount != null)
                SyncEvents.CloseAccount(activeAccount, loggedInBefore);
        }

        public BeanfunBroker beanfun = new();

        # region Login
        private QRCodeWindow qrcodeWindow = default;
        private AppAuthWindow appAuthWindow = default;

        private class LoginWorkerArgs
        {
            public bool useQRCode;
            public string username;
            public string password;
        }

        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoginWorkerArgs args = (LoginWorkerArgs)e.Argument;
            if (args.useQRCode)
            {
                if (qrcodeWindow == default)
                    qrcodeWindow = new(this);
                qrcodeWindow.ShowDialog();
                e.Result = qrcodeWindow.GetResult();
                if (e.Result == default(BeanfunBroker.TransactionResult))
                    e.Cancel = true;
            }
            else
            {
                BeanfunBroker.TransactionResult result;
                result = beanfun.GetReCaptcha();
                if(result.Status == BeanfunBroker.TransactionResultStatus.Success)
                {
                    reCaptchaWindow.SetAddress(result.Message);
                    Invoke(() => {
                        reCaptchaWindow.SetCookies(beanfun.GetAllCookies());
                        reCaptchaWindow.ShowDialog();
                    });
                    string reCaptchaResponse = reCaptchaWindow.GetResult();
                    if (reCaptchaResponse == default)
                    {
                        beanfun.LocalLogout();
                        e.Cancel = true;
                    }
                    else if(reCaptchaResponse == "NULL")
                        result = beanfun.Login(args.username, args.password);
                    else
                        result = beanfun.Login(args.username, args.password, reCaptchaResponse);
                }
                switch (result.Status)
                {
                    case BeanfunBroker.TransactionResultStatus.RequireAppAuthentication:
                        if (appAuthWindow == default)
                            appAuthWindow = new(this);
                        appAuthWindow.ShowDialog();
                        e.Result = appAuthWindow.GetResult();
                        if (e.Result == default(BeanfunBroker.TransactionResult))
                        {
                            beanfun.LocalLogout();
                            e.Cancel = true;
                        }
                        break;
                    default:
                        e.Result = result;
                        break;
                }
            }

            if (!e.Cancel &&
                ((BeanfunBroker.TransactionResult)e.Result).Status == BeanfunBroker.TransactionResultStatus.Success)
            {
                e.Result = beanfun.GetGameAccounts();
                if (((BeanfunBroker.TransactionResult)e.Result).Status == BeanfunBroker.TransactionResultStatus.Success)
                    ((BeanfunBroker.TransactionResult)e.Result).Message = args.username;
            }
        }

        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                SyncEvents.CancelLogin();
            else
                switch (((BeanfunBroker.TransactionResult)e.Result).Status)
                {
                    case BeanfunBroker.TransactionResultStatus.Success:
                        gameAccounts = ((BeanfunBroker.GameAccountResult)e.Result).GameAccounts;
                        pingTimer.Interval = pingInterval;
                        pingTimer.Start();
                        SyncEvents.SucceedLogin(((BeanfunBroker.TransactionResult)e.Result).Message);
                        break;
                    default:
                        ShowTransactionError((BeanfunBroker.TransactionResult)e.Result);
                        SyncEvents.CancelLogin();
                        break;
                }
        }
        #endregion

        #region Points
        private void getPointsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = beanfun.GetRemainingPoints();
        }

        private void getPointsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SyncEvents.UpdatePoints((int)e.Result);
        }
        #endregion

        #region Ping
        private const int pingInterval = 10 * 60 * 1000; //10 mins
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
                    ShowTransactionError(result);
                    SyncEvents.LogOut(loggedInUsername, false);
                    break;
                case BeanfunBroker.TransactionResultStatus.ConnectionLost:
                    if (++pingFailedTries >= pingMaxFailedTries)
                    {
                        pingTimer.Stop();
                        beanfun.Logout();
                        ShowTransactionError(result);
                        SyncEvents.LogOut(loggedInUsername, false);
                    }
                    pingTimer.Interval = 3;
                    break;
                case BeanfunBroker.TransactionResultStatus.LoginFirst:
                    pingTimer.Stop();
                    ShowTransactionError(result);
                    SyncEvents.LogOut(loggedInUsername, false);
                    break;
                case BeanfunBroker.TransactionResultStatus.Success:
                    pingTimer.Interval = pingInterval;
                    break;
            }
        }
        #endregion

        #region OTP
        //Arguments:
        //e.Argument: gameAccount
        private void getOTPWorker_DoWork(object sender, DoWorkEventArgs e)
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

        private void getOTPWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BeanfunBroker.TransactionResult result = (BeanfunBroker.TransactionResult)e.Result;
            switch (result.Status)
            {
                case BeanfunBroker.TransactionResultStatus.Success:
                    if (IsGameRunning())
                        SyncEvents.FinishGettingOTP(result.Message);
                    else
                    {
                        SyncEvents.FinishGettingOTP("");
                        SyncEvents.LaunchGame();
                        StartGame(((BeanfunBroker.OTPResult)result).ArgPrefix, ((BeanfunBroker.OTPResult)result).Username, result.Message);
                        SyncEvents.FinishLaunchingGame();
                    }
                    break;
                case BeanfunBroker.TransactionResultStatus.LoginFirst:
                    pingTimer.Stop();
                    ShowTransactionError(result);
                    SyncEvents.FinishGettingOTP("");
                    SyncEvents.LogOut(loggedInUsername, false);
                    break;
                default:
                    ShowTransactionError(result);
                    SyncEvents.FinishGettingOTP("");
                    break;
            }
        }
        #endregion
    }
}