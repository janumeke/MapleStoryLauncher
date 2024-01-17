using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_AutoLaunch()
        {
            SyncEvents.AccountCreated += key =>
            {
                autoLaunch.Enabled = true;
            };

            SyncEvents.AccountRemoved += key =>
            {
                autoLaunch.Enabled = false;
            };

            SyncEvents.AccountLoading += key =>
            {
                autoLaunch.Enabled = true;
            };

            SyncEvents.AccountClosing += (key, loggedIn) =>
            {
                autoLaunch.Enabled = false;
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                if (activeAccount != null)
                    autoLaunch.Checked = accountManager.GetAccount(activeAccount).Settings.autoLaunch;
                else
                    autoLaunch.Checked = false;

                autoLaunch.TabStop = true;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                if (activeAccount != null)
                    accountManager.GetAccount(activeAccount).Settings.autoLaunch = autoLaunch.Checked;

                autoLaunch.TabStop = false;
            };


            void UpdateText()
            {
                if (autoSelect.Checked && autoSelectGameAccount != default)
                    autoLaunch.Text = "自動啟動遊戲並登入";
                else
                    autoLaunch.Text = "自動啟動遊戲";
            }

            Model_AutoSelectGameAccountChanged += () =>
            {
                UpdateText();
            };

            autoSelect.CheckedChanged += (sender, _) =>
            {
                UpdateText();
            };
        }
    }
}
