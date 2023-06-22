using ExtentionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        private AccountManager accountManager;

        private string activeAccount = null;
        private bool loggedIn = false;
        private string loggedInUsername = null;

        private List<BeanfunBroker.GameAccount> gameAccounts = default;
        private ListViewItem autoSelectGameAccount = default;

        private event SyncEvents.EventHandler_Void Model_AutoSelectGameAccountChanged;

        public void Model()
        {
            string savePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{typeof(MainWindow).Namespace}\\UserData";
            try { accountManager = new AccountManager(savePath); }
            catch
            {
                MessageBox.Show("無法存取使用者資料檔案，請確認其沒有被其他程式鎖定。");
                Environment.Exit(0);
            }

            SyncEvents.AccountCreated += key =>
            {
                accountManager.AddAccount(key);
                activeAccount = key;
            };

            SyncEvents.AccountCreated_RestoreSettings += key =>
            {
                if (loggedIn)
                    accountManager.GetSettings(key).autoSelectAccount = autoSelectGameAccount?.SubItems[0].Text;
            };

            SyncEvents.AccountRemoved += key =>
            {
                accountManager.RemoveAccount(key);
            };

            SyncEvents.AccountLoaded += key =>
            {
                activeAccount = key;
            };

            SyncEvents.AccountClosed += (key, loggedIn) =>
            {
                accountManager.SaveToFile();
                activeAccount = null;
            };

            SyncEvents.LoggedIn_Loaded += username =>
            {
                if (activeAccount != null)
                {
                    autoSelectGameAccount = accountListView.FindItemWithSubItemTextExact(accountManager.GetSettings(activeAccount).autoSelectAccount);
                    AccountListView_EmboldenAutoSelectionOrNot(true);
                }
                else
                    autoSelectGameAccount = default;

                loggedIn = true;
                loggedInUsername = username;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                loggedIn = false;
                loggedInUsername = null;
            };

            SyncEvents.OTPGetting += gameAccount =>
            {
                if(activeAccount != null)
                {
                    if (autoSelect.Checked)
                    {
                        AccountListView_EmboldenAutoSelectionOrNot(false);
                        autoSelectGameAccount = accountListView.FindItemWithSubItemTextExact(gameAccount);
                        Model_AutoSelectGameAccountChanged?.Invoke();
                        AccountListView_EmboldenAutoSelectionOrNot(true);
                    }
                }
            };
        }
    }
}
