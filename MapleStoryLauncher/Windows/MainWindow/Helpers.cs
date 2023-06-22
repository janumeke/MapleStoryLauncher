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
    using static System.Windows.Forms.VisualStyles.VisualStyleElement;

    public partial class MainWindow : Form
    {
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

        private void ShowError(BeanfunBroker.TransactionResult result)
        {
            switch (result.Status)
            {
                case BeanfunBroker.TransactionResultStatus.Expired:
                case BeanfunBroker.TransactionResultStatus.Denied:
                    MessageBox.Show(result.Message, "");
                    break;
                case BeanfunBroker.TransactionResultStatus.Failed:
                    if (MessageBox.Show(result.Message + "\n請回報給開發者。\n是否產生記錄檔?", "預期外的錯誤", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{typeof(MainWindow).Namespace}\\LastResponse.txt";
                        HttpResponseMessage res = beanfun.GetLastResponse();
                        try
                        {
                            File.WriteAllText(path, $"{res.RequestMessage.Method} {res.RequestMessage.RequestUri}\n" +
                                                    $"{res}\n\n" +
                                                    $"{res.Content.ReadAsStringAsync().Result}");
                        }
                        catch { MessageBox.Show("無法寫入至檔案。"); }
                    }
                    break;
                case BeanfunBroker.TransactionResultStatus.ConnectionLost:
                    MessageBox.Show(result.Message + "\n請稍後再試一次。", "網路錯誤");
                    break;
                case BeanfunBroker.TransactionResultStatus.LoginFirst:
                    MessageBox.Show("帳號已登出。\n可能已從其他地方登入。", "");
                    break;
            }
        }

        private bool AutoLogOut()
        {
            if (loggedIn)
            {
                if (!beanfun.Logout() //beanfun has reader-writer lock
                    && MessageBox.Show(
                        "自動登出失敗！ 不登出直接關閉帳號？",
                        "",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2
                    ) == DialogResult.No)
                    return false;

                pingTimer.Stop();
                SyncEvents.LogOut(loggedInUsername, true);
            }
            return true;
        }

        private const string gameFileName = "MapleStory.exe";

        private bool IsGameRunning()
        {
            string processName = Path.GetFileNameWithoutExtension(gameFileName);
            if (Process.GetProcessesByName(processName).Length != 0)
                return true;
            else
                return false;
        }


        private const string gameName = "新楓之谷";

        public bool CheckGamePath()
        {
            if (Properties.Settings.Default.gamePath == "")
            {
                //Use default value
                object path = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GAMANIA\MapleStory")?.GetValue("Path");
                if (path != null)
                    Properties.Settings.Default.gamePath = path.ToString();
                //For 32-bit client on 32-bit OS or 64-bit client on 64-bit OS
                else if (File.Exists(@"C:\Program Files\Gamania\MapleStory\" + gameFileName))
                    Properties.Settings.Default.gamePath = @"C:\Program Files\Gamania\MapleStory\" + gameFileName;
                //For 32-bit client on 64-bit OS
                else if (File.Exists(@"C:\Program Files (x86)\Gamania\MapleStory\" + gameFileName))
                    Properties.Settings.Default.gamePath = @"C:\Program Files (x86)\Gamania\MapleStory\" + gameFileName;
            }

            if (!Properties.Settings.Default.gamePath.EndsWith(gameFileName) ||
                !File.Exists(Properties.Settings.Default.gamePath))
            {
                bool pathOK;
                do
                {
                    if (!RequestGamePath())
                        return false;
                    if (!Properties.Settings.Default.gamePath.EndsWith(gameFileName))
                    {
                        MessageBox.Show($"請選取{gameName}遊戲執行檔。", "錯誤檔案");
                        pathOK = false;
                    }
                    else
                        pathOK = true;
                }
                while (!pathOK);
            }

            Properties.Settings.Default.Save();
            return true;
        }

        private bool RequestGamePath()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = String.Format("{0} ({1})|{1}|所有檔案 (*.*)|*.*", gameName, gameFileName),
                Title = "選取遊戲執行檔",
                InitialDirectory = Properties.Settings.Default.gamePath
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.gamePath = openFileDialog.FileName;
                return true;
            }
            else
                return false;
        }

        private static bool StartGame()
        {
            return StartProcess(Properties.Settings.Default.gamePath, "");
        }

        private static bool StartGame(string username, string otp)
        {
            return StartProcess(Properties.Settings.Default.gamePath, $"tw.login.maplestory.beanfun.com 8484 BeanFun {username} {otp}");
        }

        private static bool StartProcess(string path, string arg)
        {
            try
            {
                ProcessStartInfo psInfo = new()
                {
                    FileName = path,
                    Arguments = arg,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(path),
                };
                Process.Start(psInfo);
                return true;
            }
            catch
            {
                MessageBox.Show("啟動失敗，請嘗試手動以系統管理員身分啟動遊戲。", "");
                return false;
            }
        }
    }
}