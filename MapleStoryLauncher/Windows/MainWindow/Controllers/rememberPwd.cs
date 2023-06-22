using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_RememberPwd()
        {
            accountInput.TextChanged += (sender, _) =>
            {
                rememberPwd.Enabled = accountManager.Contains(accountInput.Text);
            };

            accountInput.Leave += (sender, _) =>
            {
                if (!loggedIn && !accountManager.Contains(accountInput.Text))
                    rememberPwd.Checked = false;
            };

            rememberPwd.CheckedChanged += (sender, _) =>
            {
                if(!rememberPwd.Checked)
                    autoLogin.Checked = false;
            };

            SyncEvents.AccountCreated += key =>
            {
                rememberPwd.Enabled = true;
            };

            SyncEvents.AccountLoading += key =>
            {
                rememberPwd.Checked = accountManager.GetSettings(key).rememberPassword;

                rememberPwd.Enabled = true;
            };

            SyncEvents.AccountClosing += (key, loggedIn) =>
            {
                accountManager.GetSettings(key).rememberPassword = rememberPwd.Checked;
            };

            SyncEvents.AccountClosed += (key, loggedIn) =>
            {
                rememberPwd.Enabled = false;
            };
        }
    }
}
