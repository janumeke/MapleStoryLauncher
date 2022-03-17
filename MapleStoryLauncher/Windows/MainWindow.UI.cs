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
            const int loggedInHeight = 475;

            public UI(MainWindow handle)
            {
                MainWindow = handle;
            }

            #region Events
            public void FormLoaded()
            {
                MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                MainWindow.Text = $"新楓之谷啟動器";
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                MainWindow.Text += $" v{version.Major}.{version.Minor}";
                if (version.Build != 0)
                    MainWindow.Text += $".{version.Build}";

                MainWindow.accountInput.Text = Properties.Settings.Default.accountID;
                MainWindow.pwdInput.Text = Password.Load();
                UpdateLoginButtonText();
                MainWindow.rememberAccount.Checked = Properties.Settings.Default.rememberAccount;
                MainWindow.rememberPwd.Checked = Properties.Settings.Default.rememberPwd;
                MainWindow.autoLogin.Checked = Properties.Settings.Default.autoLogin;
                MainWindow.autoSelect.Checked = Properties.Settings.Default.autoSelect;
                MainWindow.autoLaunch.Checked = Properties.Settings.Default.autoLaunch;

                MainWindow.Tip.SetToolTip(MainWindow.accountListView, "雙擊複製登入帳號");
                MainWindow.Tip.SetToolTip(MainWindow.otpDisplay, "點擊複製密碼\n雙擊顯示/隱藏密碼");


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

            public void FormClosed()
            {
                Properties.Settings.Default.rememberAccount = MainWindow.rememberAccount.Checked;
                Properties.Settings.Default.rememberPwd = MainWindow.rememberPwd.Checked;
                Properties.Settings.Default.autoSelect = MainWindow.autoSelect.Checked;
                Properties.Settings.Default.autoLaunch = MainWindow.autoLaunch.Checked;

                switch (MainWindow.status)
                {
                    case LogInState.LoggedIn:
                        Properties.Settings.Default.autoLogin = MainWindow.autoLogin.Checked;
                        if (MainWindow.rememberAccount.Checked)
                            Properties.Settings.Default.accountID = MainWindow.accountInput.Text;
                        else
                            Properties.Settings.Default.accountID = "";
                        if (MainWindow.rememberPwd.Checked)
                            Password.Save(MainWindow.pwdInput.Text);
                        else
                            Password.Delete();
                        if (!MainWindow.autoSelect.Checked)
                            Properties.Settings.Default.autoSelectIndex = -1;
                        break;
                    case LogInState.LoggedOut:
                        Properties.Settings.Default.autoLogin = false;
                        if (!MainWindow.rememberAccount.Checked)
                            Properties.Settings.Default.accountID = "";
                        if (!MainWindow.rememberPwd.Checked)
                            Password.Delete();
                        break;
                }

                Properties.Settings.Default.Save();
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
                Password.Delete();
                Properties.Settings.Default.Save();
            }

            public void LoginFailed()
            {
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
                MainWindow.loginButton.Text = "登出";
                MainWindow.loginButton.Enabled = true;
                MainWindow.otpDisplay.Text = "";

                if (MainWindow.rememberPwd.Checked)
                    Password.Save(MainWindow.pwdInput.Text);

                if (!RedrawAccountListView())
                {
                    MessageBox.Show("更新帳號列表失敗。\n請回報開發者。", "預期外的錯誤");
                    if(!MainWindow.beanfun.Logout())
                        MessageBox.Show("自動登出失敗，請重開程式。\n請回報開發者。", "預期外的錯誤");
                    LoginFailed();
                    return;
                }
                EmboldenAutoSelection(MainWindow.autoSelect.Checked);

                //Auto-select
                if (MainWindow.autoSelect.Checked
                    && Properties.Settings.Default.autoSelectIndex >= 0 //There is selection
                    && Properties.Settings.Default.autoSelectIndex < MainWindow.accountListView.Items.Count) //Selection is within the range of the list
                    if (MainWindow.accountListView.Enabled)
                    {
                        MainWindow.accountListView.SelectedItems.Clear();
                        MainWindow.accountListView.Items[Properties.Settings.Default.autoSelectIndex].Selected = true;
                    }

                MainWindow.Size = new Size(MainWindow.Size.Width, loggedInHeight);
                MainWindow.accountListView.TabStop = true;
                MainWindow.autoSelect.TabStop = true;
                MainWindow.autoLaunch.TabStop = true;
                MainWindow.getOtpButton.TabStop = true;
                UpdateGetOtpButton();
                MainWindow.AcceptButton = MainWindow.getOtpButton;
                MainWindow.UseWaitCursor = false;

                //Auto-launch
                if (MainWindow.autoLaunch.Checked && !MainWindow.IsGameRunning())
                    if (MainWindow.getOtpButton.Enabled)
                        MainWindow.getOtpButton_Click(null, null);
            }

            public void LoggedOut()
            {
                MainWindow.accountListView.TabStop = false;
                MainWindow.autoSelect.TabStop = false;
                MainWindow.autoLaunch.TabStop = false;
                MainWindow.getOtpButton.TabStop = false;
                MainWindow.getOtpButton.Enabled = false;
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
                MainWindow.accountListView.Enabled = false;
                MainWindow.getOtpButton.Enabled = false;

                if (MainWindow.autoSelect.Checked)
                {
                    EmboldenAutoSelection(false);
                    Properties.Settings.Default.autoSelectIndex = MainWindow.accountListView.SelectedItems[0].Index;
                    Properties.Settings.Default.Save();
                    EmboldenAutoSelection();
                    UpdateAutoLaunchCheckBoxText();
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
                    object value = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GAMANIA\MapleStory").GetValue("Path");
                    if(value != null)
                        Properties.Settings.Default.gamePath = value.ToString();
                    //For 32-bit client in 32-bit OS or 64-bit client in 64-bit OS
                    else if (File.Exists(@"C:\Program Files\Gamania\MapleStory\" + MainWindow.gameFileName))
                        Properties.Settings.Default.gamePath = @"C:\Program Files\Gamania\MapleStory\" + MainWindow.gameFileName;
                    //For 32-bit client in 64-bit OS
                    else if (File.Exists(@"C:\Program Files (x86)\Gamania\MapleStory\" + MainWindow.gameFileName))
                        Properties.Settings.Default.gamePath = @"C:\Program Files (x86)\Gamania\MapleStory\" + MainWindow.gameFileName;
                    Properties.Settings.Default.Save();
                }

                bool pathOK;
                if (!File.Exists(Properties.Settings.Default.gamePath) ||
                    !Properties.Settings.Default.gamePath.EndsWith(MainWindow.gameFileName))
                {
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
                if (Properties.Settings.Default.autoSelectIndex >= 0
                   && Properties.Settings.Default.autoSelectIndex < MainWindow.accountListView.Items.Count)
                {
                    MainWindow.accountListView.Items[Properties.Settings.Default.autoSelectIndex].Font
                    = new Font(MainWindow.accountListView.Items[Properties.Settings.Default.autoSelectIndex].Font,
                               toggle ? FontStyle.Bold : FontStyle.Regular);
                }
            }
            #endregion

            private bool RedrawAccountListView()
            {
                if (MainWindow.gameAccounts == default(List<BeanfunBroker.GameAccount>))
                    return false;

                MainWindow.accountListView.SelectedItems.Clear();
                MainWindow.accountListView.Items.Clear();
                foreach (BeanfunBroker.GameAccount account in MainWindow.gameAccounts)
                {
                    string[] row = { account.FriendlyName, account.Username };
                    ListViewItem listViewItem = new(row);
                    MainWindow.accountListView.Items.Add(listViewItem);
                }
                if (MainWindow.accountListView.Items.Count > 5)
                    MainWindow.accountListView.Columns[1].Width = MainWindow.accountListView.Width - 5 - MainWindow.accountListView.Columns[0].Width - 16;
                else
                    MainWindow.accountListView.Columns[1].Width = MainWindow.accountListView.Width - 5 - MainWindow.accountListView.Columns[0].Width;
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