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
    enum LoginMethod : int {
        Regular = 0,
        Keypasco = 1,
        PlaySafe = 2,
        QRCode = 3
    };

    public partial class main : Form
    {
        public BeanfunClient bfClient;

        public BeanfunClient.QRCodeClass qrcodeClass;

        private string service_code = "610074" , service_region = "T9" , service_name = "新楓之谷";

        //public List<GameService> gameList = new List<GameService>();

        private CSharpAnalytics.Activities.AutoTimedEventActivity timedActivity = null;

        private Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

        private GamePathDB gamePaths = new GamePathDB();

        private const int initialWindowHeight = 200;

        public main()
        {
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

            this.FormClosing += new FormClosingEventHandler((sender, e) => {
                if (this.bfClient != null) this.bfClient.Logout();
            });

            timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("FormLoad", Properties.Settings.Default.loginMethod.ToString());
            InitializeComponent();
            Size = new Size(Size.Width, initialWindowHeight);
            init();

            if (Properties.Settings.Default.GAEnabled && this.timedActivity != null)
            {
                AutoMeasurement.Client.Track(this.timedActivity);
                this.timedActivity = null;
            }
        }

        public void ShowToolTip(IWin32Window ui, string title, string des, int iniDelay = 2000, bool repeat = false)
        {
            if (Properties.Settings.Default.showTip || repeat)
            {
                ToolTip toolTip = new ToolTip();
                toolTip.ToolTipTitle = title;
                toolTip.UseFading = true;
                toolTip.UseAnimation = true;
                toolTip.IsBalloon = true;
                toolTip.InitialDelay = iniDelay;

                toolTip.Show(string.Empty, ui, 3000);
                toolTip.Show(des, ui);
            }
        }

        public bool errexit(string msg, int method, string title = null)
        {
            //string originalMsg = msg;
            if (Properties.Settings.Default.GAEnabled) 
                AutoMeasurement.Client.TrackException(msg);

            switch (msg)
            {
                case "LoginNoResponse":
                    msg = "初始化失敗，請檢查網路連線。";
                    method = 0;
                    break;
                case "LoginNoSkey":
                    method = 0;
                    break;
                case "LoginNoAkey":
                    msg = "登入失敗，帳號或密碼錯誤。";
                    break;
                case "LoginNoAccountMatch":
                    msg = "登入失敗，無法取得帳號列表。";
                    break;
                case "LoginNoAccount":
                    msg = "找不到遊戲帳號。";
                    break;
                case "LoginNoResponseVakten":
                    msg = "登入失敗，與伺服器驗證失敗，請檢查是否安裝且已執行vakten程式。";
                    break;
                case "LoginUnknown":
                    msg = "登入失敗，請稍後再試";
                    method = 0;
                    break;
                case "OTPNoLongPollingKey":
                    if (Properties.Settings.Default.loginMethod == (int)LoginMethod.PlaySafe)
                        msg = "密碼獲取失敗，請檢查晶片卡是否插入讀卡機，且讀卡機運作正常。\n若仍出現此訊息，請嘗試重新登入。";
                    else
                    {
                        msg = "已從伺服器斷線，請重新登入。";
                        method = 1;
                    }
                    break;
                case "LoginNoReaderName":
                    msg = "登入失敗，找不到晶片卡或讀卡機，請檢查晶片卡是否插入讀卡機，且讀卡機運作正常。\n若還是發生此情形，請嘗試重新登入。";
                    break;
                case "LoginNoCardType":
                    msg = "登入失敗，晶片卡讀取失敗。";
                    break;
                case "LoginNoCardId":
                    msg = "登入失敗，找不到讀卡機。";
                    break;
                case "LoginNoOpInfo":
                    msg = "登入失敗，讀卡機讀取失敗。";
                    break;
                case "LoginNoEncryptedData":
                    msg = "登入失敗，晶片卡讀取失敗。";
                    break;
                case "OTPUnknown":
                    msg = "獲取密碼失敗，請嘗試重新登入。";
                    break;
                case "LoginNoPSDriver":
                    msg = "PlaySafe驅動初始化失敗，請檢查PlaySafe元件是否已正確安裝。";
                    break;
                default:
                    break;
            }

            MessageBox.Show(msg, title);
            if (method == 0)
                Application.Exit();
            else if (method == 1)
            {
                BackToLogin();
            }

            return false;
        }
        
        public void BackToLogin()
        {
            Size = new Size(Size.Width, initialWindowHeight);
            Properties.Settings.Default.autoLogin = false;
            Properties.Settings.Default.Save();
            init();
        }

        public bool init()
        {
            try
            {
                this.Text = $"新楓之谷啟動器";// - v{ currentVersion.Major }.{ currentVersion.Minor }.{ currentVersion.Build }({ currentVersion.Revision })";
                this.AcceptButton = this.loginButton;
                this.bfClient = null;
                // Properties.Settings.Default.Reset(); //SetToDefault.                  
                this.otpDisplay.Text = "";

                // Handle settings.
                autoLogin.Checked = Properties.Settings.Default.autoLogin;
                if (Properties.Settings.Default.rememberAccount == true)
                    this.accountInput.Text = Properties.Settings.Default.AccountID;
                if (Properties.Settings.Default.rememberPwd == true)
                {
                    // Load password.
                    if (File.Exists("UserState.dat"))
                    {
                        try
                        {
                            Byte[] cipher = File.ReadAllBytes("UserState.dat");
                            string entropy = Properties.Settings.Default.entropy;
                            byte[] plaintext = ProtectedData.Unprotect(cipher, Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser);
                            this.passwdInput.Text = System.Text.Encoding.UTF8.GetString(plaintext);
                        }
                        catch
                        {
                            File.Delete("UserState.dat");
                        }
                    }
                }
                
                /*if (gamePaths.Get("新楓之谷") == "")
                {
                    ModifyRegistry myRegistry = new ModifyRegistry();
                    myRegistry.BaseRegistryKey = Registry.CurrentUser;
                    myRegistry.SubKey = "Software\\Gamania\\MapleStory";
                    if (myRegistry.Read("Path") != "")
                    {
                        gamePaths.Set("新楓之谷", myRegistry.Read("Path"));
                        gamePaths.Save();
                    }
                }*/

                if (this.accountInput.Text == "")
                    this.ActiveControl = this.accountInput;
                else if (this.passwdInput.Text == "")
                    this.ActiveControl = this.passwdInput;
                else
                    this.ActiveControl = this.loginButton;
                if (Properties.Settings.Default.autoLogin == true)
                    loginButton_Click(null, null);

                // .NET textbox full mode bug.
                //this.accountInput.ImeMode = ImeMode.OnHalf;
                //this.passwdInput.ImeMode = ImeMode.OnHalf;
                return true;
            }
            catch (Exception e)
            { 
                return errexit("初始化失敗，未知的錯誤。" + e.Message, 0); 
            }
        }

        enum LogInState
        {
            LoggedOut,
            LoggedIn
        }

        private LogInState status = LogInState.LoggedOut;

        // The login/logout botton.
        private void loginButton_Click(object sender, EventArgs e)
        {
            switch(status)
            {
                case LogInState.LoggedOut:
                    //log in
                    loginButton.Enabled = false;
                    this.loginButton.Text = "請稍後...";
                    this.UseWaitCursor = true;
                    foreach (ListViewItem item in accounts.Items)
                        item.BackColor = DefaultBackColor;
                    if (this.pingWorker.IsBusy)
                    {
                        this.pingWorker.CancelAsync();
                    }

                    if (Properties.Settings.Default.GAEnabled)
                    {
                        timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("Login", Properties.Settings.Default.loginMethod.ToString());
                        AutoMeasurement.Client.TrackEvent("Login" + Properties.Settings.Default.loginMethod.ToString(), "Login");
                    }
                    this.loginWorker.RunWorkerAsync(Properties.Settings.Default.loginMethod);
                    break;
                case LogInState.LoggedIn:
                    //log out
                    bfClient.Logout();
                    status = LogInState.LoggedOut;
                    BackToLogin();
                    accountInput.Enabled = true;
                    passwdInput.Enabled = true;
                    loginButton.Text = "登入";
                    break;
            }
        }    

        // The get OTP button.
        private void getOtpButton_Click(object sender, EventArgs e)
        {
            if (this.pingWorker.IsBusy)
            {
                this.pingWorker.CancelAsync();
            }
            if (accounts.SelectedItems.Count <= 0 || this.loginWorker.IsBusy) return;

            if (!GameIsRunning())
            {
                bool pathOK;
                if (!File.Exists(gamePaths.Get(service_name)))
                    do
                    {
                        if (!RequestGamePath())
                            return;
                        pathOK = false;
                        if (!gamePaths.Get(service_name).EndsWith("MapleStory.exe"))
                            MessageBox.Show($"請選擇新楓之谷遊戲執行檔。", "錯誤檔案");
                        else
                            pathOK = true;
                    }
                    while (!pathOK);
            }
            else
                this.otpDisplay.Text = "取得密碼中...";
            
            this.accounts.Enabled = false;
            this.getOtpButton.Enabled = false;
            if (Properties.Settings.Default.GAEnabled)
            {
                timedActivity = new CSharpAnalytics.Activities.AutoTimedEventActivity("GetOTP", Properties.Settings.Default.loginMethod.ToString());
                AutoMeasurement.Client.TrackEvent("GetOTP" + Properties.Settings.Default.loginMethod.ToString(), "GetOTP");
            }
            this.getOtpWorker.RunWorkerAsync(accounts.SelectedItems[0].Index);
        }

        private bool RequestGamePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string identName = service_name;//comboBox2.SelectedItem.ToString();
            string binaryName = gamePaths.GetAlias(identName);
            if (binaryName == identName) binaryName = "*.exe";
            openFileDialog.Filter = String.Format("{0} ({1})|{1}|All files (*.*)|*.*", identName, binaryName);
            openFileDialog.Title = "選擇遊戲執行檔";
            openFileDialog.InitialDirectory = gamePaths.Get(identName);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = openFileDialog.FileName;
                Debug.WriteLine($"service_name={service_name}, gamePaths.Get={gamePaths.Get(service_name)}, file={file}");
                gamePaths.Set(identName, file);
                gamePaths.Save();

                if (Properties.Settings.Default.GAEnabled)
                {
                    AutoMeasurement.Client.TrackEvent("set game path", "set game path");
                }
                return true;
            }
            else
                return false;
        }

        // Building ciphertext by 3DES.
        private byte[] ciphertext(string plaintext, string key)
        {
            byte[] plainByte = Encoding.UTF8.GetBytes(plaintext);
            byte[] entropy = Encoding.UTF8.GetBytes(key);
            return ProtectedData.Protect(plainByte, entropy, DataProtectionScope.CurrentUser);
        }


        /* Handle other elements' statements. */
        /*private void BackToLogin_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackToLogin();
        }*/

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

        private void otpDisplay_OnClick(object sender, EventArgs e)
        {
            if (otpDisplay.Text == "" || otpDisplay.Text == "獲取失敗") return;
            try
            {
                Clipboard.SetText(otpDisplay.Text);
            }
            catch
            {

            }
        }

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

        private void accounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!GameIsRunning())
                getOtpButton.Text = "啟動遊戲";
            else
                getOtpButton.Text = "取得一次性密碼";
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

        private void main_FormClosed(object sender, FormClosedEventArgs e)
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
