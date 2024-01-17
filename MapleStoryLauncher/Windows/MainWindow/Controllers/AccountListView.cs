using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    using ExtentionMethods;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

    public partial class MainWindow : Form
    {
        void AccountListView_EmboldenAutoSelectionOrNot(bool _switch)
        {
            if (autoSelectGameAccount != default)
                autoSelectGameAccount.Font = new Font(
                    autoSelectGameAccount.Font,
                    _switch ? FontStyle.Bold : FontStyle.Regular
                );
        }

        public void Controller_AccountListView()
        {
            this.Load += (sender, _) =>
            {
                this.Tip.SetToolTip(accountListView, "雙擊複製登入帳號");
            };

            SyncEvents.AccountCreated_RestoreSettings += key =>
            {
                if(loggedIn)
                    AccountListView_EmboldenAutoSelectionOrNot(autoSelect.Checked);
            };

            SyncEvents.AccountRemoved += key =>
            {
                if (loggedIn)
                    AccountListView_EmboldenAutoSelectionOrNot(false);
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                accountListView.Items.Clear();
                foreach (BeanfunBroker.GameAccount account in gameAccounts)
                {
                    string[] row = { account.friendlyName, account.username };
                    ListViewItem listViewItem = new(row);
                    accountListView.Items.Add(listViewItem);
                }

                //padding: 4 pixels
                //scroll width: 16 pixels
                if (accountListView.Items.Count > 5)
                    accountListView.Columns[1].Width = accountListView.Width - 4 - 16 - accountListView.Columns[0].Width;
                else
                    accountListView.Columns[1].Width = accountListView.Width - 4 - accountListView.Columns[0].Width;

                accountListView.TabStop = true;
            };

            SyncEvents.LoggedIn_Loaded += username =>
            {
                accountListView.SelectedItems.Clear();
                if (activeAccount != null)
                    if (autoSelect.Checked)
                    {
                        ListViewItem autoSelection = accountListView.FindItemWithSubItemTextExact(accountManager.GetAccount(activeAccount).Settings.autoSelectAccount);
                        if(autoSelection != null)
                            autoSelection.Selected = true;
                    }
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                if (activeAccount != null)
                    if (!autoSelect.Checked)
                        accountManager.GetAccount(activeAccount).Settings.autoSelectAccount = default;

                accountListView.TabStop = false;
            };

            autoSelect.CheckedChanged += (sender, _) =>
            {
                AccountListView_EmboldenAutoSelectionOrNot(autoSelect.Checked);
            };

            accountListView.DoubleClick += (sender, _) =>
            {
                if (accountListView.SelectedItems.Count == 1)
                {
                    try { Clipboard.SetText(gameAccounts[accountListView.SelectedIndices[0]].username); }
                    catch { }
                }
            };

            SyncEvents.OTPGetting += gameAccount =>
            {
                if (activeAccount != null)
                    if (autoSelect.Checked)
                        accountManager.GetAccount(activeAccount).Settings.autoSelectAccount = gameAccount;

                accountListView.Enabled = false;
            };

            SyncEvents.OTPGot += otp =>
            {
                accountListView.Enabled = true;
            };

            SyncEvents.GameLaunching += () =>
            {
                accountListView.Enabled = false;
            };

            SyncEvents.GameLaunched += () =>
            {
                accountListView.Enabled = true;
            };
        }
    }
}
