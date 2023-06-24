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
        public void Controller_AccountInput()
        {
            this.Load += (sender, _) =>
            {
                this.ActiveControl = accountInput;

                accountInput.Items.Clear();
                accountInput.Items.AddRange(accountManager.GetListOfUsernames().ToArray());

                if (accountManager.Contains(""))
                    SyncEvents.LoadAccount("");
            };

            void OnLeave(object sender, EventArgs _)
            {
                if (loggedIn)
                    return;
                if (accountInput.Text == activeAccount)
                    return;

                if (activeAccount != null)
                    SyncEvents.CloseAccount(activeAccount, loggedIn);
                if (accountManager.Contains(accountInput.Text))
                    SyncEvents.LoadAccount(accountInput.Text);
            }

            accountInput.Leave += OnLeave;

            accountInput.SelectionChangeCommitted += (sender, _) =>
            {
                string selectedAccount = accountInput.SelectedItem?.ToString();
                if (selectedAccount == null)
                    return;
                if (selectedAccount == activeAccount)
                    return;
                if (loggedIn && selectedAccount == loggedInUsername)
                    return;

                bool loggedInBefore = loggedIn;
                if (loggedIn)
                {
                    //The current input in on accountInput, and AutoLogOut() will trigger accountInput.Leave before it is complete and cause race condition
                    accountInput.Leave -= OnLeave;
                    bool autoLoggedOut = AutoLogOut();
                    accountInput.Leave += OnLeave;
                    if (!autoLoggedOut)
                    {
                        accountInput.SelectedItem = accountInput.Text;
                        return;
                    }
                }
                if (activeAccount != null)
                    SyncEvents.CloseAccount(activeAccount, loggedInBefore);
                accountInput.Text = selectedAccount; //LoadAccount() will auto log in if needed, and accountInput.Text hasn't changed to accountInput.SelectedItem
                SyncEvents.LoadAccount(selectedAccount);
            };

            SyncEvents.AccountCreated += key =>
            {
                if (!loggedIn)
                    accountInput.Items.Add(key);
            };

            SyncEvents.AccountRemoved += key =>
            {
                if (!loggedIn)
                {
                    accountInput.Items.Remove(key);
                    accountInput.Text = key;
                }
            };

            SyncEvents.LoggingIn += username =>
            {
                accountInput.Enabled = false;
            };

            SyncEvents.LoginFailed += () =>
            {
                accountInput.Enabled = true;
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                accountInput.Enabled = true;

                if (!accountInput.Items.Contains(username))
                    accountInput.Items.Add(username);
                accountInput.SelectedItem = username;
                accountInput.DropDownStyle = ComboBoxStyle.DropDownList;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                accountInput.DropDownStyle = ComboBoxStyle.DropDown;
                if (!accountManager.Contains(username))
                    accountInput.Items.Remove(username);
                if(!auto)
                    accountInput.Text = username;
            };

            SyncEvents.OTPGetting += gameAccount =>
            {
                accountInput.Enabled = false;
            };

            SyncEvents.OTPGot += otp =>
            {
                accountInput.Enabled = true;
            };

            SyncEvents.GameLaunching += () =>
            {
                accountInput.Enabled = false;
            };

            SyncEvents.GameLaunched += () =>
            {
                accountInput.Enabled = true;
            };
        }
    }
}
