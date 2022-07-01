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
            const int initialWindowHeight = 220;
            const int loggedInHeight = 485;

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

            public void FormFocused()
            {
                if(MainWindow.status.loggedIn)
                    UpdateGetOtpButton();
            }

            public void AccountOpened()
            {
                if (!MainWindow.accountManager.Contains(MainWindow.accountInput.Text))
                    return;

                MainWindow.passwordInput.Text = MainWindow.accountManager.GetPassword(MainWindow.accountInput.Text);
                UpdateLoginButtonText();
                AccountManager.Settings settings = MainWindow.accountManager.GetSettings(MainWindow.accountInput.Text);
                MainWindow.rememberPwd.Checked = settings.rememberPwd;
                MainWindow.autoLogin.Checked = settings.autoLogin;
                MainWindow.autoSelect.Checked = settings.autoSelect;
                MainWindow.autoLaunch.Checked = settings.autoLaunch;
            }

            public void AccountClosed()
            {
                //Use status.username instead of accountInput.Text, since accountInput.Text will be the account being logged in when switching accounts
                if (!MainWindow.accountManager.Contains(MainWindow.status.username))
                    return;

                AccountManager.Settings settings = MainWindow.accountManager.GetSettings(MainWindow.status.username);
                settings.rememberPwd = MainWindow.rememberPwd.Checked;
                settings.autoSelect = MainWindow.autoSelect.Checked;
                settings.autoLaunch = MainWindow.autoLaunch.Checked;

                if (MainWindow.status.loggedIn)
                {
                    settings.autoLogin = MainWindow.autoLogin.Checked;
                    if (MainWindow.rememberPwd.Checked)
                        MainWindow.accountManager.SavePassword(MainWindow.status.username, MainWindow.passwordInput.Text);
                    else
                        MainWindow.accountManager.SavePassword(MainWindow.status.username, "");
                    if (!MainWindow.autoSelect.Checked)
                        settings.autoSelectAccount = default;
                }
                else
                {
                    settings.autoLogin = false;
                    if (!MainWindow.rememberPwd.Checked)
                        MainWindow.accountManager.SavePassword(MainWindow.status.username, "");
                }

                MainWindow.accountManager.SaveSettings(MainWindow.status.username, settings);
                MainWindow.accountManager.SaveToFile();
            }

            public void LoggingIn()
            {
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
                MainWindow.passwordInput.Enabled = true;
                UpdateLoginButtonText();
                MainWindow.loginButton.Enabled = true;
                UpdateAddRemoveAccount();
                MainWindow.UseWaitCursor = false;
            }

            public void LoggedIn()
            {
                if(!MainWindow.accountInput.Items.Contains(MainWindow.accountInput.Text))
                    MainWindow.accountInput.Items.Add(MainWindow.accountInput.Text);
                MainWindow.accountInput.SelectedItem = MainWindow.accountInput.Text;
                MainWindow.accountInput.DropDownStyle = ComboBoxStyle.DropDownList;
                MainWindow.passwordInput.Enabled = false;
                MainWindow.loginButton.Text = "登出";
                MainWindow.loginButton.Enabled = true;
                MainWindow.otpDisplay.Text = "";

                MainWindow.status.username = MainWindow.accountInput.Text;
                MainWindow.status.loggedIn = true;

                if (MainWindow.accountManager.Contains(MainWindow.accountInput.Text))
                {
                    if (MainWindow.rememberPwd.Checked)
                    {
                        MainWindow.accountManager.SavePassword(MainWindow.accountInput.Text, MainWindow.passwordInput.Text);
                        MainWindow.accountManager.SaveToFile();
                    }
                    AccountManager.Settings settings = MainWindow.accountManager.GetSettings(MainWindow.accountInput.Text);
                    MainWindow.status.autoSelectAccount = settings.autoSelectAccount;
                }

                if (!RedrawAccountListView())
                {
                    MainWindow.ShowError(new BeanfunBroker.TransactionResult { Status = BeanfunBroker.TransactionResultStatus.Failed, Message = "更新遊戲帳號列表失敗。" });
                    if(!MainWindow.beanfun.Logout())
                        MainWindow.ShowError(new BeanfunBroker.TransactionResult { Status = BeanfunBroker.TransactionResultStatus.Failed, Message = "自動登出失敗，請重開程式。" });
                    LoginFailed();
                    return;
                }
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

                UpdateAddRemoveAccount();
                MainWindow.AcceptButton = MainWindow.getOtpButton;
                MainWindow.UseWaitCursor = false;

                //Auto-launch
                if (MainWindow.autoLaunch.Checked && !MainWindow.IsGameRunning())
                    if (MainWindow.getOtpButton.Enabled)
                        MainWindow.getOtpButton_Click(null, null);
            }

            public void LoggedOut()
            {
                //Use status.username instead of accountInput.Text, since accountInput.Text will be the account being logged in when switching accounts
                MainWindow.accountListView.TabStop = false;
                MainWindow.autoSelect.TabStop = false;
                MainWindow.autoLaunch.TabStop = false;
                MainWindow.getOtpButton.TabStop = false;
                MainWindow.getOtpButton.Enabled = false;
                MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                if (MainWindow.accountManager.Contains(MainWindow.status.username))
                {
                    if (!MainWindow.autoSelect.Checked)
                    {
                        AccountManager.Settings settings = MainWindow.accountManager.GetSettings(MainWindow.status.username);
                        settings.autoSelectAccount = default;
                        MainWindow.accountManager.SaveSettings(MainWindow.status.username, settings);
                        MainWindow.accountManager.SaveToFile();
                    }
                }

                MainWindow.accountInput.DropDownStyle = ComboBoxStyle.DropDown;
                if (!MainWindow.accountManager.Contains(MainWindow.status.username))
                {
                    MainWindow.accountInput.Items.Remove(MainWindow.status.username);
                    MainWindow.accountInput.Text = MainWindow.status.username;
                }

                MainWindow.status.username = default;
                MainWindow.status.loggedIn = false;
                MainWindow.status.autoSelectAccount = default;

                MainWindow.passwordInput.Enabled = true;
                UpdateLoginButtonText();
                MainWindow.loginButton.Enabled = true;
                UpdateAddRemoveAccount();
                MainWindow.AcceptButton = MainWindow.loginButton;
                MainWindow.UseWaitCursor = false;
            }

            public void GettingOtp()
            {
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

                if (MainWindow.IsGameRunning())
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
                MainWindow.accountListView.Enabled = true;
                UpdateGetOtpButton();
                MainWindow.loginButton.Enabled = true;

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
            #endregion

            #region Operations
            [DllImport("user32.dll")]
            public static extern void SwitchToThisWindow(IntPtr hWnd);
            public static void CheckMultipleInstances()
            {
                string programPath = Application.ExecutablePath;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(programPath);
                int processId = Environment.ProcessId;
                IEnumerable<Process> sameProcesses =
                    from process in Process.GetProcessesByName(fileNameWithoutExtension)
                    where process.MainModule.FileName == programPath && process.Id != processId
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
                if (MainWindow.getOtpWorker.IsBusy)
                    return;

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