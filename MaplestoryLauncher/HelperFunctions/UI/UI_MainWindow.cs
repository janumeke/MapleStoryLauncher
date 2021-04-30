using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using CSharpAnalytics;

namespace MaplestoryLauncher
{
    public partial class MainWindow
    {
        private sealed partial class HelperFunctions
        {
            public sealed class UI
            {
                readonly MainWindow MainWindow;
                readonly int initialWindowHeight;
                public UI(MainWindow handle, int initialWindowHeight)
                {
                    MainWindow = handle;
                    this.initialWindowHeight = initialWindowHeight;
                }

                #region Events
                public void FormLoaded()
                {
                    MainWindow.Text = $"新楓之谷啟動器";
                    MainWindow.Tip.SetToolTip(MainWindow.accounts, "雙擊即自動複製");
                    MainWindow.Tip.SetToolTip(MainWindow.getOtpButton, "點擊啟動遊戲並登入或取得密碼");
                    MainWindow.Tip.SetToolTip(MainWindow.otpDisplay, "點擊一次即自動複製");
                    MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                    if (Properties.Settings.Default.rememberAccount == true)
                        MainWindow.accountInput.Text = Properties.Settings.Default.AccountID;
                    if (Properties.Settings.Default.rememberPwd == true)
                        MainWindow.pwdInput.Text = Password.Load();
                    MainWindow.autoLogin.Checked = Properties.Settings.Default.autoLogin;

                    if (MainWindow.accountInput.Text == "")
                        MainWindow.ActiveControl = MainWindow.accountInput;
                    else if (MainWindow.pwdInput.Text == "")
                        MainWindow.ActiveControl = MainWindow.pwdInput;
                    else
                        MainWindow.ActiveControl = MainWindow.loginButton;

                    if (Properties.Settings.Default.autoLogin == true)
                        if (MainWindow.loginButton.Enabled)
                            MainWindow.loginButton_Click(null, null);
                }

                public void LoggedOut()
                {
                    MainWindow.otpDisplay.Text = "";
                    MainWindow.Size = new Size(MainWindow.Size.Width, initialWindowHeight);

                    bool autoLogin = MainWindow.autoLogin.Checked;
                    Properties.Settings.Default.autoLogin = false;
                    Properties.Settings.Default.Save();
                    MainWindow.autoLogin.Checked = autoLogin;

                    MainWindow.accountInput.Enabled = true;
                    MainWindow.pwdInput.Enabled = true;
                    MainWindow.loginButton.Text = "登入";
                }

                public void LoggingIn()
                {
                    MainWindow.accountInput.Enabled = false;
                    MainWindow.pwdInput.Enabled = false;
                    MainWindow.loginButton.Enabled = false;
                    MainWindow.loginButton.Text = "請稍後...";
                    MainWindow.UseWaitCursor = true;
                    if (MainWindow.rememberAccount.Checked)
                    {
                        Properties.Settings.Default.AccountID = MainWindow.accountInput.Text;
                        Properties.Settings.Default.Save();
                    }
                    foreach (ListViewItem item in MainWindow.accounts.Items)
                        item.BackColor = DefaultBackColor;
                }

                public void LoggedIn(bool successful = true)
                {
                    if (successful)
                    {
                        if (MainWindow.rememberPwd.Checked == true)
                            Password.Save(MainWindow.pwdInput.Text);
                        Properties.Settings.Default.autoLogin = MainWindow.autoLogin.Checked;
                        Properties.Settings.Default.Save();
                        MainWindow.loginButton.Text = "登出";
                        MainWindow.loginButton.Enabled = true;

                        try
                        {
                            RedrawSAccountList();
                            if (Properties.Settings.Default.autoSelect && Properties.Settings.Default.autoSelectIndex < MainWindow.accounts.Items.Count)
                                if(MainWindow.accounts.Enabled)
                                    MainWindow.accounts.Items[Properties.Settings.Default.autoSelectIndex].Selected = true;
                            if (Properties.Settings.Default.opengame && Properties.Settings.Default.autoSelectIndex < MainWindow.bfClient.accountList.Count())
                                if(MainWindow.getOtpButton.Enabled)
                                    MainWindow.getOtpButton_Click(null, null);
                        }
                        catch
                        {
                            ShowError("登入失敗，無法取得帳號列表。", HelperFunctions.UI.ErrorType.LoginFailed);
                        }

                        if (!MainWindow.GameIsRunning())
                            MainWindow.getOtpButton.Text = "啟動遊戲";
                        else
                            MainWindow.getOtpButton.Text = "取得一次性密碼";
                        MainWindow.AcceptButton = MainWindow.getOtpButton;
                        MainWindow.Size = new Size(MainWindow.Size.Width, 450);
                    }
                    else
                    {
                        MainWindow.accountInput.Enabled = true;
                        MainWindow.pwdInput.Enabled = true;
                        MainWindow.loginButton.Text = "登入";
                    }
                    MainWindow.UseWaitCursor = false;
                }

                public void GettingOtp()
                {
                    MainWindow.loginButton.Enabled = false;
                    MainWindow.accounts.Enabled = false;
                    MainWindow.getOtpButton.Enabled = false;

                    if (!MainWindow.GameIsRunning())
                    {
                        bool pathOK;
                        if (!File.Exists(MainWindow.gamePaths.Get(MainWindow.service_name)))
                            do
                            {
                                if (!RequestGamePath())
                                    return;
                                pathOK = false;
                                if (!MainWindow.gamePaths.Get(MainWindow.service_name).EndsWith("MapleStory.exe"))
                                    MessageBox.Show($"請選擇新楓之谷遊戲執行檔。", "錯誤檔案");
                                else
                                    pathOK = true;
                            }
                            while (!pathOK);
                    }
                    else
                        MainWindow.otpDisplay.Text = "取得密碼中...";
                }

                public void GameRun(bool successful = true)
                {
                    if (successful)
                        MainWindow.getOtpButton.Text = "取得一次性密碼";
                    else
                        MainWindow.getOtpButton.Text = "啟動遊戲";
                    MainWindow.loginButton.Enabled = true;
                    MainWindow.accounts.Enabled = true;
                    MainWindow.accounts_ItemSelectionChanged(null, null);
                }

                public void OtpGot(string otp)
                {
                    MainWindow.otpDisplay.Text = otp;
                }
                #endregion

                #region Operations
                public void Refresh()
                {
                    if (!MainWindow.GameIsRunning())
                        MainWindow.getOtpButton.Text = "啟動遊戲";
                    else
                        MainWindow.getOtpButton.Text = "取得一次性密碼";
                }

                public enum ErrorType
                {
                    Unspecified,
                    Fatal,
                    LoginFailed
                }

                public bool ShowError(string msg, ErrorType type = ErrorType.Unspecified, string title = null)
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
                        case "LoginNoResponseVakten":
                            msg = "登入失敗，與伺服器驗證失敗，請檢查是否安裝且已執行vakten程式。";
                            break;
                        case "LoginUnknown":
                            msg = "登入失敗，請稍後再試";
                            type = ErrorType.Fatal;
                            break;
                        case "OTPNoLongPollingKey":
                            if (Properties.Settings.Default.loginMethod == (int)LoginMethod.PlaySafe)
                                msg = "密碼獲取失敗，請檢查晶片卡是否插入讀卡機，且讀卡機運作正常。\n若仍出現此訊息，請嘗試重新登入。";
                            else
                            {
                                msg = "已從伺服器斷線，請重新登入。";
                                type = ErrorType.LoginFailed;
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
                            msg = "取得密碼失敗，請嘗試重新登入。";
                            break;
                        case "LoginNoPSDriver":
                            msg = "PlaySafe驅動初始化失敗，請檢查PlaySafe元件是否已正確安裝。";
                            break;
                        default:
                            break;
                    }

                    MessageBox.Show(msg, title);
                    if (type == ErrorType.Fatal)
                        Application.Exit();
                    else if (type == ErrorType.LoginFailed)
                        LoggedIn(false);

                    return false;
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
                    MainWindow.accounts_ItemSelectionChanged(null, null);
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
                        Debug.WriteLine($"service_name={MainWindow.service_name}, gamePaths.Get={MainWindow.gamePaths.Get(MainWindow.service_name)}, file={file}");
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