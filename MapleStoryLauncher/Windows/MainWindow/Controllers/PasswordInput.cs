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
                    accountManager.SavePassword(activeAccount, rememberPwd.Checked ? passwordInput.Text : "");
            };

            SyncEvents.AccountLoading += key =>
            {
                passwordInput.Text = accountManager.GetPassword(key);
            };

            SyncEvents.AccountClosing += (username, loggedIn) =>
            {
                if(loggedIn)
                    accountManager.SavePassword(username, rememberPwd.Checked ? passwordInput.Text : "");
            };

            SyncEvents.LoggingIn += username =>
            {
                passwordInput.Enabled = false;

                if (activeAccount != null)
                    accountManager.SavePassword(username, "");
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
                        accountManager.SavePassword(activeAccount, passwordInput.Text);
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                if (activeAccount != null)
                    if (!auto)
                        accountManager.SavePassword(activeAccount, "");

                passwordInput.Enabled = true;
            };
        }
    }
}
