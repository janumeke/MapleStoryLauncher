using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_PasswordInput()
        {
            accountInput.TextChanged += (sender, _) =>
            {
                if (!accountManager.Contains(accountInput.Text))
                    passwordInput.Text = "";
            };

            SyncEvents.AccountCreated_RestoreSettings += key =>
            {
                if (loggedIn)
                    accountManager.GetAccount(key).Password = rememberPwd.Checked ? passwordInput.Text : "";
            };

            SyncEvents.AccountLoading += key =>
            {
                passwordInput.Text = accountManager.GetAccount(key).Password;
            };

            SyncEvents.AccountClosing += (username, loggedIn) =>
            {
                if(loggedIn)
                    accountManager.GetAccount(username).Password = rememberPwd.Checked ? passwordInput.Text : "";
            };

            SyncEvents.LoggingIn += username =>
            {
                passwordInput.Enabled = false;

                if (activeAccount != null)
                    accountManager.GetAccount(username).Password = "";
            };

            SyncEvents.LoginFailed += () =>
            {
                passwordInput.Enabled = true;
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                passwordInput.Enabled = false;

                if (activeAccount != null)
                    if (rememberPwd.Checked)
                        accountManager.GetAccount(activeAccount).Password = passwordInput.Text;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                if (activeAccount != null && !auto)
                    accountManager.GetAccount(activeAccount).Password = "";

                passwordInput.Enabled = true;
            };
        }
    }
}
