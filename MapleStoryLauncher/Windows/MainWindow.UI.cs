using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MapleStoryLauncher
{
    using ExtentionMethods;

    public partial class MainWindow : Form
    {
        private sealed class UI
        {
            readonly MainWindow MainWindow;
            const int initialWindowHeight = 223;
            const int loggedInHeight = 482;

            public UI(MainWindow handle)
            {
                MainWindow = handle;
            }

            #region Events
            public void FormLoaded()
            {
                MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                MainWindow.Text += $" v{version.Major}.{version.Minor}";
                if (version.Build != 0)
                    MainWindow.Text += $".{version.Build}";

                MainWindow.Tip.SetToolTip(MainWindow.pointsLabel, "點擊更新點數");
                MainWindow.Tip.SetToolTip(MainWindow.accountListView, "雙擊複製登入帳號");
                MainWindow.Tip.SetToolTip(MainWindow.otpDisplay, "點擊複製密碼\n雙擊顯示/隱藏密碼");
                
                MainWindow.ActiveControl = MainWindow.accountInput;
                MainWindow.AcceptButton = MainWindow.loginButton;

                MainWindow.accountInput.Items.Clear();
                MainWindow.accountInput.Items.AddRange(MainWindow.accountManager.GetListOfUsernames().ToArray());
                MainWindow.accountInput.Text = "";
                UpdateAddRemoveAccount();
                MainWindow.passwordInput.Text = "";
                UpdateLoginButtonText();
                if(MainWindow.accountManager.Contains(MainWindow.accountInput.Text))
                    MainWindow.accountInput_SelectionChangeCommitted(this, null);
            }

            public void AccountOpened()
            {
                Debug.WriteLine($"AccountOpened: {MainWindow.accountInput.SelectedItem}");
                string openedAccount = (string)MainWindow.accountInput.SelectedItem;
                if (!MainWindow.accountManager.Contains(openedAccount))
                    return;

                MainWindow.status.openedAccount = openedAccount;

                //Restore Settings
                MainWindow.passwordInput.Text = MainWindow.accountManager.GetPassword(openedAccount);
                UpdateLoginButtonText();
                AccountManager.Settings settings = MainWindow.accountManager.GetSettings(openedAccount);
                MainWindow.rememberPwd.Checked = settings.rememberPassword;
                MainWindow.autoLogin.Checked = settings.autoLogin;

                //Auto Login
                if (MainWindow.autoLogin.Checked)
                    if (MainWindow.loginButton.Enabled)
                    {
                        MainWindow.accountInput.Text = openedAccount;
                        MainWindow.loginButton_Click(this, null);
                    }
            }

            public bool AccountClosed()
            {
                Debug.WriteLine($"AccountClosed:{MainWindow.status.openedAccount}");
                string closedAccount = MainWindow.status.openedAccount;
                if (!MainWindow.accountManager.Contains(closedAccount))
                    return true;
                bool loggedInBeforAutoLogout = MainWindow.status.loggedIn;

                //Auto Logout
                if (loggedInBeforAutoLogout)
                {
                    if (MainWindow.loginButton.Enabled)
                        MainWindow.loginButton_Click(this, new AccountClosedEventArgs());
                    else
                        return false;
                }

                //Save Settings
                AccountManager.Settings settings = MainWindow.accountManager.GetSettings(closedAccount);
                settings.rememberPassword = MainWindow.rememberPwd.Checked;

                if (loggedInBeforAutoLogout)
                {
                    settings.autoLogin = MainWindow.autoLogin.Checked;
                    if (MainWindow.rememberPwd.Checked)
                        MainWindow.accountManager.SavePassword(closedAccount, MainWindow.passwordInput.Text);
                    else
                        MainWindow.accountManager.SavePassword(closedAccount, "");
                }
                else
                {
                    settings.autoLogin = false;
                }

                MainWindow.accountManager.SaveSettings(closedAccount, settings);
                MainWindow.accountManager.SaveToFile();

                MainWindow.status.openedAccount = default;
                return true;
            }

            public void LoggingIn()
            {
                Debug.WriteLine($"LoggingIn:{MainWindow.accountInput.Text}");
                MainWindow.accountInput.Enabled = false;
                MainWindow.passwordInput.Enabled = false;
                MainWindow.loginButton.Enabled = false;
                MainWindow.loginButton.Text = "請稍候...";
                MainWindow.UseWaitCursor = true;

                if (MainWindow.accountManager.Contains(MainWindow.accountInput.Text))
                {
                    MainWindow.accountManager.SavePassword(MainWindow.accountInput.Text, "");
                    MainWindow.accountManager.SaveToFile();
                }
            }

            public void LoginFailed()
            {
                MainWindow.accountInput.Enabled = true;
                MainWindow.passwordInput.Enabled = true;
                UpdateLoginButtonText();
                MainWindow.loginButton.Enabled = true;
                MainWindow.UseWaitCursor = false;
            }

            public void LoggedIn(bool UIEnabled = true)
            {
                Debug.WriteLine($"LoggedIn{(UIEnabled ? "" : "(NoUI)")}:{MainWindow.accountInput.Text}");
                string loggedInUsername = MainWindow.accountInput.Text;

                MainWindow.status.loggedIn = true;
                MainWindow.status.loggedInUsername = loggedInUsername;

                if (MainWindow.accountManager.Contains(loggedInUsername))
                {
                    if (MainWindow.rememberPwd.Checked)
                    {
                        MainWindow.accountManager.SavePassword(loggedInUsername, MainWindow.passwordInput.Text);
                        MainWindow.accountManager.SaveToFile();
                    }
                    AccountManager.Settings settings = MainWindow.accountManager.GetSettings(loggedInUsername);
                    MainWindow.status.autoSelectAccount = settings.autoSelectAccount;
                }

                if (UIEnabled)
                {
                    if (!MainWindow.accountInput.Items.Contains(loggedInUsername))
                        MainWindow.accountInput.Items.Add(loggedInUsername);
                    MainWindow.accountInput.SelectedItem = loggedInUsername;
                    MainWindow.accountInput.DropDownStyle = ComboBoxStyle.DropDownList;
                    MainWindow.accountInput.Enabled = true;
                    MainWindow.passwordInput.Enabled = false;
                    MainWindow.loginButton.Text = "登出";
                    MainWindow.loginButton.Enabled = true;
                    MainWindow.otpDisplay.Text = "";

                    if (!RedrawAccountListView())
                    {
                        MainWindow.ShowError(new BeanfunBroker.TransactionResult { Status = BeanfunBroker.TransactionResultStatus.Failed, Message = "更新遊戲帳號列表失敗。" });
                        if (!MainWindow.beanfun.Logout())
                            MainWindow.ShowError(new BeanfunBroker.TransactionResult { Status = BeanfunBroker.TransactionResultStatus.Failed, Message = "自動登出失敗，請重開程式。" });
                        LoginFailed();
                        return;
                    }

                    bool accountRemembered = MainWindow.accountManager.Contains(loggedInUsername);
                    MainWindow.autoSelect.Enabled = accountRemembered;
                    MainWindow.autoLaunch.Enabled = accountRemembered;
                    AccountManager.Settings settings = MainWindow.accountManager.GetSettings(loggedInUsername);
                    MainWindow.autoSelect.Checked = settings.autoSelect;
                    MainWindow.autoLaunch.Checked = settings.autoLaunch;
                    EmboldenAutoSelection(MainWindow.autoSelect.Checked);

                    MainWindow.Size = new Size(MainWindow.Size.Width, loggedInHeight);
                    MainWindow.accountListView.TabStop = true;
                    MainWindow.autoSelect.TabStop = true;
                    MainWindow.autoLaunch.TabStop = true;
                    MainWindow.getOtpButton.TabStop = true;
                    UpdateGetOtpButton();

                    MainWindow.pointsLabel_Click(null, null);

                    //Auto-select
                    if (MainWindow.autoSelect.Checked
                        && MainWindow.status.autoSelectAccount != default) //There is selection
                        if (MainWindow.accountListView.Enabled)
                        {
                            MainWindow.accountListView.SelectedItems.Clear();
                            ListViewItem item = (
                                    from ListViewItem itemE in MainWindow.accountListView.Items
                                    where itemE.SubItems[0].Text == MainWindow.status.autoSelectAccount
                                    select itemE
                                ).FirstOrDefault();
                            if (item != default)
                                item.Selected = true;
                            UpdateGetOtpButton();
                        }

                    MainWindow.AcceptButton = MainWindow.getOtpButton;
                    MainWindow.UseWaitCursor = false;

                    //Auto-launch
                    if (MainWindow.autoLaunch.Checked && !MainWindow.IsGameRunning())
                        if (MainWindow.getOtpButton.Enabled)
                            MainWindow.getOtpButton_Click(null, null);
                }
            }

            public void LoggedOut(bool triggeredByAccountClosed = false, bool UIEnabled = true)
            {
                Debug.WriteLine($"LoggedOut{(UIEnabled ? "" : "(NoUI)")}:{MainWindow.status.loggedInUsername}");
                string loggedOutUsername = MainWindow.status.loggedInUsername;

                if (MainWindow.accountManager.Contains(loggedOutUsername))
                {
                    AccountManager.Settings settings = MainWindow.accountManager.GetSettings(loggedOutUsername);
                    settings.autoSelect = MainWindow.autoSelect.Checked;
                    settings.autoLaunch = MainWindow.autoLaunch.Checked;
                    if (!triggeredByAccountClosed)
                        MainWindow.accountManager.SavePassword(loggedOutUsername, "");
                    if (!MainWindow.autoSelect.Checked)
                        settings.autoSelectAccount = default;
                    MainWindow.accountManager.SaveSettings(loggedOutUsername, settings);
                    MainWindow.accountManager.SaveToFile();
                }

                MainWindow.status.loggedIn = false;
                MainWindow.status.loggedInUsername = default;
                MainWindow.status.autoSelectAccount = default;

                if (UIEnabled)
                {
                    MainWindow.accountListView.TabStop = false;
                    MainWindow.autoSelect.TabStop = false;
                    MainWindow.autoLaunch.TabStop = false;
                    MainWindow.getOtpButton.TabStop = false;
                    MainWindow.getOtpButton.Enabled = false;
                    MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                    MainWindow.accountInput.Leave -= MainWindow.accountInput_Leave;
                    MainWindow.accountInput.DropDownStyle = ComboBoxStyle.DropDown;
                    MainWindow.accountInput.Leave += MainWindow.accountInput_Leave;
                    if (!MainWindow.accountManager.Contains(loggedOutUsername))
                        MainWindow.accountInput.Items.Remove(loggedOutUsername);

                    MainWindow.passwordInput.Enabled = true;
                    UpdateLoginButtonText();
                    MainWindow.loginButton.Enabled = true;
                    UpdateAddRemoveAccount();
                    MainWindow.AcceptButton = MainWindow.loginButton;
                    MainWindow.UseWaitCursor = false;
                }
            }

            public void GettingOtp()
            {
                MainWindow.Activated -= MainWindow.MainWindow_Activated;
                MainWindow.accountInput.Enabled = false;
                MainWindow.loginButton.Enabled = false;
                MainWindow.accountListView.Enabled = false;
                MainWindow.getOtpButton.Enabled = false;

                if (MainWindow.autoSelect.Checked)
                {
                    EmboldenAutoSelection(false);
                    MainWindow.status.autoSelectAccount = MainWindow.accountListView.SelectedItems[0].SubItems[0].Text;
                    EmboldenAutoSelection();
                    UpdateAutoLaunchCheckBoxText();

                    AccountManager.Settings settings = MainWindow.accountManager.GetSettings(MainWindow.accountInput.Text);
                    settings.autoSelectAccount = MainWindow.status.autoSelectAccount;
                    MainWindow.accountManager.SaveSettings(MainWindow.accountInput.Text, settings);
                    MainWindow.accountManager.SaveToFile();
                }

                MainWindow.otpDisplay.Text = "取得密碼中...";
                MainWindow.otpDisplay.PasswordChar = default;
            }

            public void OtpGot(string otp)
            {
                MainWindow.otpDisplay.Text = otp;
                if (otp == "" || !otp.IsAllDigits())
                    MainWindow.otpDisplay.PasswordChar = default;
                else
                    MainWindow.otpDisplay.PasswordChar = '*';

                MainWindow.accountListView.Enabled = true;
                UpdateGetOtpButton();
                MainWindow.loginButton.Enabled = true;
                MainWindow.accountInput.Enabled = true;
                MainWindow.Activated += MainWindow.MainWindow_Activated;

                try
                {
                    if (otp != "" && otp.IsAllDigits())
                    {
                        Clipboard.SetText(otp);
                        MainWindow.Notification.Hide(MainWindow.otpDisplay);
                        MainWindow.Notification.Show("已複製密碼！", MainWindow.otpDisplay, (int)(0.25 * MainWindow.otpDisplay.Width), (int)(-1.5 * MainWindow.otpDisplay.Height), 1000);
                    }
                }
                catch { }
            }

            public void StartingGame()
            {
                MainWindow.Activated -= MainWindow.MainWindow_Activated;
                MainWindow.accountInput.Enabled = false;
                MainWindow.loginButton.Enabled = false;
                MainWindow.accountListView.Enabled = false;
                MainWindow.getOtpButton.Enabled = false;

                MainWindow.otpDisplay.Text = "啟動遊戲中...";
                MainWindow.otpDisplay.PasswordChar = default;
            }

            public void GameStarted()
            {
                MainWindow.otpDisplay.Text = "";
                MainWindow.otpDisplay.PasswordChar = default;

                MainWindow.accountListView.Enabled = true;
                UpdateGetOtpButton();
                MainWindow.loginButton.Enabled = true;
                MainWindow.accountInput.Enabled = true;
                MainWindow.Activated += MainWindow.MainWindow_Activated;
            }
            #endregion

            #region Operations
            [DllImport("user32.dll")]
            public static extern void SwitchToThisWindow(IntPtr hWnd);
            public static void CheckMultipleInstances()
            {
                Process thisProcess = Process.GetCurrentProcess();
                IEnumerable<Process> sameProcesses =
                    from process in Process.GetProcessesByName(thisProcess.ProcessName)
                    where process.Id != thisProcess.Id
                    select process;
                if (sameProcesses.Any())
                {
                    if (MessageBox.Show(
                        "同時執行多份程式可能使設定檔儲存不正確。\n" +
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

            public void UpdateAddRemoveAccount()
            {
                if (MainWindow.accountManager.Contains(MainWindow.accountInput.Text))
                    MainWindow.AddRemoveAccount.Image = Properties.Resources.minus;
                else
                    MainWindow.AddRemoveAccount.Image = Properties.Resources.plus;
            }

            public void UpdateLoginButtonText()
            {
                if(MainWindow.status.loggedIn)
                    MainWindow.loginButton.Text = "登出";
                else
                    if (MainWindow.accountInput.Text == "" && MainWindow.passwordInput.Text == "")
                        MainWindow.loginButton.Text = "顯示 QRCode";
                    else
                        MainWindow.loginButton.Text = "登入";
            }

            public void UpdateAutoLaunchCheckBoxText()
            {
                if (MainWindow.autoSelect.Checked && MainWindow.status.autoSelectAccount != default)
                    MainWindow.autoLaunch.Text = "自動啟動遊戲並登入";
                else
                    MainWindow.autoLaunch.Text = "自動啟動遊戲";
            }

            public void UpdateGetOtpButton()
            {
                if (!MainWindow.IsGameRunning())
                    if (MainWindow.accountListView.SelectedItems.Count == 0)
                        MainWindow.getOtpButton.Text = "啟動遊戲";
                    else
                        MainWindow.getOtpButton.Text = "啟動遊戲並登入";
                else
                    MainWindow.getOtpButton.Text = "取得一次性密碼";

                if (MainWindow.IsGameRunning() && MainWindow.accountListView.SelectedItems.Count == 0)
                    MainWindow.getOtpButton.Enabled = false;
                else
                    MainWindow.getOtpButton.Enabled = true;
            }

            public bool CheckGamePath()
            {
                if (Properties.Settings.Default.gamePath == "") //Use default value
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GAMANIA\MapleStory");
                    if(key != null && key.GetValue("Path") != null)
                        Properties.Settings.Default.gamePath = key.GetValue("Path").ToString();
                    //For 32-bit client in 32-bit OS or 64-bit client in 64-bit OS
                    else if (File.Exists(@"C:\Program Files\Gamania\MapleStory\" + MainWindow.gameFileName))
                        Properties.Settings.Default.gamePath = @"C:\Program Files\Gamania\MapleStory\" + MainWindow.gameFileName;
                    //For 32-bit client in 64-bit OS
                    else if (File.Exists(@"C:\Program Files (x86)\Gamania\MapleStory\" + MainWindow.gameFileName))
                        Properties.Settings.Default.gamePath = @"C:\Program Files (x86)\Gamania\MapleStory\" + MainWindow.gameFileName;
                    Properties.Settings.Default.Save();
                }

                if (!Properties.Settings.Default.gamePath.EndsWith(MainWindow.gameFileName) ||
                    !File.Exists(Properties.Settings.Default.gamePath))
                {
                    bool pathOK;
                    do
                    {
                        if (!RequestGamePath())
                            return false;
                        if (!Properties.Settings.Default.gamePath.EndsWith(MainWindow.gameFileName))
                        {
                            MessageBox.Show($"請選擇{MainWindow.gameName}遊戲執行檔。", "錯誤檔案");
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
                if (MainWindow.status.autoSelectAccount != default)
                {
                    ListViewItem item = (
                            from ListViewItem itemE in MainWindow.accountListView.Items
                            where itemE.SubItems[0].Text == MainWindow.status.autoSelectAccount
                            select itemE
                        ).FirstOrDefault();
                    if (item != default)
                        item.Font = new Font(
                            item.Font,
                            toggle ? FontStyle.Bold : FontStyle.Regular
                        );
                }
            }
            #endregion

            private bool RedrawAccountListView()
            {
                if (MainWindow.gameAccounts == default)
                    return false;

                MainWindow.accountListView.SelectedItems.Clear();
                MainWindow.accountListView.Items.Clear();
                foreach (BeanfunBroker.GameAccount account in MainWindow.gameAccounts)
                {
                    string[] row = { account.friendlyName, account.username };
                    ListViewItem listViewItem = new(row);
                    MainWindow.accountListView.Items.Add(listViewItem);
                }

                //padding: 4 pixels
                //scroll width: 16 pixels
                if (MainWindow.accountListView.Items.Count > 5)
                    MainWindow.accountListView.Columns[1].Width = MainWindow.accountListView.Width - 4 - MainWindow.accountListView.Columns[0].Width - 16;
                else
                    MainWindow.accountListView.Columns[1].Width = MainWindow.accountListView.Width - 4 - MainWindow.accountListView.Columns[0].Width;

                return true;
            }

            private bool RequestGamePath()
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = String.Format("{0} ({1})|{1}|所有檔案 (*.*)|*.*", MainWindow.gameName, MainWindow.gameFileName),
                    Title = "選擇遊戲執行檔",
                    InitialDirectory = Properties.Settings.Default.gamePath
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.gamePath = openFileDialog.FileName;
                    Properties.Settings.Default.Save();
                    return true;
                }
                else
                    return false;
            }
        }
    }
}