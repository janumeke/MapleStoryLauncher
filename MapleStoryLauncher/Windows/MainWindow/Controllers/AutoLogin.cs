using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_AutoLogin()
        {
            accountInput.TextChanged += (sender, _) =>
            {
                autoLogin.Enabled = accountManager.Contains(accountInput.Text);
            };

            accountInput.Leave += (sender, _) =>
            {
                if (!loggedIn && !accountManager.Contains(accountInput.Text))
                    autoLogin.Checked = false;
            };

            autoLogin.CheckedChanged += (sender, _) =>
            {
                if (autoLogin.Checked)
                    rememberPwd.Checked = true;
            };

            SyncEvents.AccountCreated += key =>
            {
                autoLogin.Enabled = true;
            };

            SyncEvents.AccountLoading += key =>
            {
                autoLogin.Checked = accountManager.GetSettings(key).autoLogin;

                autoLogin.Enabled = true;
            };

            SyncEvents.AccountClosing += (key, loggedIn) =>
            {
                if (loggedIn)
                    accountManager.GetSettings(key).autoLogin = autoLogin.Checked;
                else
                    accountManager.GetSettings(key).autoLogin = false;
            };

            SyncEvents.AccountClosed += (key, loggedIn) =>
            {
                autoLogin.Enabled = false;
            };
        }
    }
}
