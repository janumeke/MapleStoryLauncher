using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using CSharpAnalytics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MaplestoryLauncher
{
    using ExtentionMethods;

    public partial class MainWindow : Form
    {
        private sealed partial class HelperFunctions
        {
            public sealed class UI
            {
                readonly MainWindow MainWindow;
                const int initialWindowHeight = 215;
                const int loggedInHeight = 470;

                public UI(MainWindow handle)
                {
                    MainWindow = handle;
                }

                #region Events
                public void FormLoaded()
                {
                    MainWindow.Text = $"新楓之谷啟動器";
                    Version version = Assembly.GetExecutingAssembly().GetName().Version;
                    MainWindow.Text += $" v{version.Major}.{version.Minor}";
                    if (version.Build != 0)
                        MainWindow.Text += $".{version.Build}";

                    MainWindow.accountInput.Text = Properties.Settings.Default.accountID;
                    MainWindow.pwdInput.Text =  Password.Load();
                    UpdateLoginButtonText();
                    MainWindow.rememberAccount.Checked = Properties.Settings.Default.rememberAccount;
                    MainWindow.rememberPwd.Checked = Properties.Settings.Default.rememberPwd;
                    MainWindow.autoLogin.Checked = Properties.Settings.Default.autoLogin;
                    MainWindow.autoSelect.Checked = Properties.Settings.Default.autoSelect;
                    MainWindow.autoLaunch.Checked = Properties.Settings.Default.autoLaunch;

                    MainWindow.Tip.SetToolTip(MainWindow.accounts, "雙擊複製");
                    MainWindow.Tip.SetToolTip(MainWindow.otpDisplay, "點擊複製\n雙擊顯示/隱藏密碼");
                    MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                    
                    if (MainWindow.accountInput.Text == "")
                        MainWindow.ActiveControl = MainWindow.accountInput;
                    else if (MainWindow.pwdInput.Text == "")
                        MainWindow.ActiveControl = MainWindow.pwdInput;
                    else
                        MainWindow.ActiveControl = MainWindow.loginButton;
                    MainWindow.AcceptButton = MainWindow.loginButton;

                    if (MainWindow.autoLogin.Checked && MainWindow.loginButton.Enabled)
                        MainWindow.loginButton_Click(null, null);
                }

                public void FormFocused()
                {
                    UpdateGetOtpButton();
                }

                public void LoggingIn()
                {
                    MainWindow.accountInput.Enabled = false;
                    MainWindow.pwdInput.Enabled = false;
                    MainWindow.loginButton.Enabled = false;
                    MainWindow.loginButton.Text = "請稍候...";
                    MainWindow.UseWaitCursor = true;

                    if (MainWindow.rememberAccount.Checked)
                        Properties.Settings.Default.accountID = MainWindow.accountInput.Text;
                    else
                        Properties.Settings.Default.accountID = "";
                    Properties.Settings.Default.Save();
                }

                public void LoginFailed()
                {
                    MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                    MainWindow.accountInput.Enabled = true;
                    MainWindow.pwdInput.Enabled = true;
                    UpdateLoginButtonText();
                    MainWindow.loginButton.Enabled = true;
                    MainWindow.UseWaitCursor = false;
                }

                public void LoggedIn()
                {
                    MainWindow.accountInput.Enabled = false;
                    MainWindow.pwdInput.Enabled = false;
                    if (MainWindow.rememberPwd.Checked)
                        Password.Save(MainWindow.pwdInput.Text);
                    else
                        Password.Delete();
                    MainWindow.loginButton.Text = "登出";
                    MainWindow.loginButton.Enabled = true;
                    MainWindow.otpDisplay.Text = "";

                    try
                    {
                        RedrawSAccountList();
                    }
                    catch
                    {
                        ShowError("登入失敗，無法取得帳號列表。", ErrorType.LoginFailed);
                        LoginFailed();
                    }

                    EmboldenAutoSelection(MainWindow.autoSelect.Checked);
                    //Auto-select
                    if (MainWindow.autoSelect.Checked
                        && Properties.Settings.Default.autoSelectIndex >= 0 //There is selection
                        && Properties.Settings.Default.autoSelectIndex < MainWindow.accounts.Items.Count) //Selection is within the range of the list
                        if (MainWindow.accounts.Enabled)
                            MainWindow.accounts.Items[Properties.Settings.Default.autoSelectIndex].Selected = true;
                    UpdateGetOtpButton();
                    MainWindow.Size = new Size(MainWindow.Size.Width, loggedInHeight);
                    MainWindow.UseWaitCursor = false;

                    MainWindow.accounts.TabStop = true;
                    MainWindow.autoSelect.TabStop = true;
                    MainWindow.autoLaunch.TabStop = true;
                    MainWindow.getOtpButton.TabStop = true;
                    MainWindow.AcceptButton = MainWindow.getOtpButton;

                    //Auto-launch
                    if (MainWindow.autoLaunch.Checked && !MainWindow.GameIsRunning())
                        if (MainWindow.getOtpButton.Enabled)
                            MainWindow.getOtpButton_Click(null, null);
                }

                public void LoggedOut()
                {
                    MainWindow.accounts.TabStop = false;
                    MainWindow.autoSelect.TabStop = false;
                    MainWindow.autoLaunch.TabStop = false;
                    MainWindow.getOtpButton.TabStop = false;
                    MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                    if (!MainWindow.autoSelect.Checked)
                    {
                        Properties.Settings.Default.autoSelectIndex = -1;
                        Properties.Settings.Default.Save();
                    }

                    MainWindow.accountInput.Enabled = true;
                    MainWindow.pwdInput.Enabled = true;
                    UpdateLoginButtonText();
                    MainWindow.loginButton.Enabled = true;
                    MainWindow.AcceptButton = MainWindow.loginButton;
                    MainWindow.UseWaitCursor = false;
                }

                public void GettingOtp()
                {
                    MainWindow.loginButton.Enabled = false;
                    MainWindow.accounts.Enabled = false;
                    MainWindow.getOtpButton.Enabled = false;

                    if (MainWindow.autoSelect.Checked)
                    {
                        EmboldenAutoSelection(false);
                        Properties.Settings.Default.autoSelectIndex = MainWindow.accounts.SelectedItems[0].Index;
                        Properties.Settings.Default.Save();
                        EmboldenAutoSelection();
                        UpdateAutoLaunchCheckBoxText();
                    }

                    if (MainWindow.GameIsRunning())
                    {
                        MainWindow.otpDisplay.PasswordChar = default;
                        MainWindow.otpDisplay.Text = "取得密碼中...";
                    }
                }

                public void OtpGot(string otp)
                {
                    if (otp == "" || !otp.IsAllDigits())
                        MainWindow.otpDisplay.PasswordChar = default;
                    else
                        MainWindow.otpDisplay.PasswordChar = '*';
                    MainWindow.otpDisplay.Text = otp;
                    UpdateGetOtpButton();
                    MainWindow.accounts.Enabled = true;
                    MainWindow.loginButton.Enabled = true;

                    try
                    {
                        if(otp != "" && otp.IsAllDigits())
                        {
                            Clipboard.SetText(otp);
                            MainWindow.Notification.Hide(MainWindow.otpDisplay);
                            MainWindow.Notification.Show("已複製密碼！", MainWindow.otpDisplay, (int)(0.25 * MainWindow.otpDisplay.Width), (int)(-1.5 * MainWindow.otpDisplay.Height), 1000);
                        }
                    }
                    catch
                    {

                    }
                }
                #endregion

                #region Operations
                [DllImport("user32.dll")]
                public static extern void SwitchToThisWindow(IntPtr hWnd);
                public void CheckMultipleInstances()
                {
                    string filePath = Application.ExecutablePath;
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    int processId = Process.GetCurrentProcess().Id;
                    IEnumerable<Process> sameProcesses =
                        from process in Process.GetProcessesByName(fileNameWithoutExtension)
                        where process.MainModule.FileName == filePath && process.Id != processId
                        select process;
                    if(sameProcesses.Count() != 0)
                    {
                        if(MessageBox.Show(
                            "同時執行多份相同路徑的執行檔可能使設定檔儲存不正確。\n" +
                            "你可以複製此執行檔成多個不同檔案位置或名稱的執行檔。\n" +
                            "你確定仍要執行？",
                            "警告 - 偵測到多個實例",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                           == DialogResult.Cancel)
                        {
                            SwitchToThisWindow(sameProcesses.First().MainWindowHandle);
                            Environment.Exit(0);
                        }
                    }
                }

                public void UpdateLoginButtonText()
                {
                    if (MainWindow.accountInput.Text == "" && MainWindow.pwdInput.Text == "")
                        MainWindow.loginButton.Text = "顯示 QRCode";
                    else
                        MainWindow.loginButton.Text = "登入";
                }

                public void UpdateAutoLaunchCheckBoxText()
                {
                    if (MainWindow.autoSelect.Checked && Properties.Settings.Default.autoSelectIndex >= 0)
                        MainWindow.autoLaunch.Text = "自動啟動遊戲並登入";
                    else
                        MainWindow.autoLaunch.Text = "自動啟動遊戲";
                }

                public void UpdateGetOtpButton()
                {
                    if (!MainWindow.GameIsRunning())
                        if (MainWindow.accounts.SelectedItems.Count == 0)
                            MainWindow.getOtpButton.Text = "啟動遊戲";
                        else
                            MainWindow.getOtpButton.Text = "啟動遊戲並登入";
                    else
                        MainWindow.getOtpButton.Text = "取得一次性密碼";

                    if (MainWindow.GameIsRunning() && MainWindow.accounts.SelectedItems.Count == 0)
                        MainWindow.getOtpButton.Enabled = false;
                    else
                        MainWindow.getOtpButton.Enabled = true;
                }

                public bool CheckGamePath()
                {
                    bool pathOK;
                    if (!File.Exists(MainWindow.gamePaths.Get(MainWindow.service_name)) ||
                        !MainWindow.gamePaths.Get(MainWindow.service_name).EndsWith("\\" + MainWindow.gamePaths.GetAlias(MainWindow.service_name)))
                    {
                        do
                        {
                            if (!RequestGamePath())
                                return false;
                            if (!MainWindow.gamePaths.Get(MainWindow.service_name).EndsWith("\\" + MainWindow.gamePaths.GetAlias(MainWindow.service_name)))
                            {
                                MessageBox.Show($"請選擇{MainWindow.service_name}遊戲執行檔。", "錯誤檔案");
                                pathOK = false;
                            }
                            else
                                pathOK = true;
                        }
                        while (!pathOK);
                    }
                    return true;
                }

                public void EmboldenAutoSelection(bool toggle = true)
                {
                    if (Properties.Settings.Default.autoSelectIndex >= 0
                       && Properties.Settings.Default.autoSelectIndex < MainWindow.accounts.Items.Count)
                    {
                        MainWindow.accounts.Items[Properties.Settings.Default.autoSelectIndex].Font
                        = new Font(MainWindow.accounts.Items[Properties.Settings.Default.autoSelectIndex].Font,
                                   toggle ? FontStyle.Bold : FontStyle.Regular);
                    }
                }

                public enum ErrorType
                {
                    Unspecified,
                    Fatal,
                    LoginFailed
                }

                public void ShowError(string msg, ErrorType type = ErrorType.Unspecified, string title = null)
                {
                    if (Properties.Settings.Default.GAEnabled)
                        AutoMeasurement.Client.TrackException(msg);

                    switch (msg)
                    {
                        case "LoginNoResponse":
                            msg = "初始化失敗，請檢查網路連線。";
                            type = ErrorType.Fatal;
                            break;
                        case "LoginNoSkey":
                            type = ErrorType.Fatal;
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
                        case "LoginUnknown":
                            msg = "登入失敗，請稍後再試";
                            type = ErrorType.Fatal;
                            break;
                        case "OTPNoLongPollingKey":
                            msg = "已從伺服器斷線，請重新登入。";
                            type = ErrorType.LoginFailed;
                            break;
                        case "OTPUnknown":
                            msg = "取得密碼失敗，請嘗試重新登入。";
                            break;
                        default:
                            break;
                    }

                    MessageBox.Show(msg, title);
                    if (type == ErrorType.Fatal)
                        Application.Exit();
                    else if (type == ErrorType.LoginFailed)
                        LoginFailed();
                }
                #endregion

                private void RedrawSAccountList()
                {
                    MainWindow.accounts.SelectedItems.Clear();
                    MainWindow.accounts.Items.Clear();
                    foreach (var account in MainWindow.bfClient.accountList)
                    {
                        string[] row = { WebUtility.HtmlDecode(account.sname), account.sacc };
                        var listViewItem = new ListViewItem(row);
                        MainWindow.accounts.Items.Add(listViewItem);
                    }
                }

                private bool RequestGamePath()
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    string identName = MainWindow.service_name;//comboBox2.SelectedItem.ToString();
                    string binaryName = MainWindow.gamePaths.GetAlias(identName);
                    if (binaryName == identName) binaryName = "*.exe";
                    openFileDialog.Filter = String.Format("{0} ({1})|{1}|All files (*.*)|*.*", identName, binaryName);
                    openFileDialog.Title = "選擇遊戲執行檔";
                    openFileDialog.InitialDirectory = MainWindow.gamePaths.Get(identName);

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string file = openFileDialog.FileName;
                        MainWindow.gamePaths.Set(identName, file);
                        MainWindow.gamePaths.Save();

                        if (Properties.Settings.Default.GAEnabled)
                        {
                            AutoMeasurement.Client.TrackEvent("set game path", "set game path");
                        }
                        return true;
                    }
                    else
                        return false;
                }
            }
        }
    }
}