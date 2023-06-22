using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_AutoSelect()
        {
            SyncEvents.AccountCreated += key =>
            {
                autoSelect.Enabled = true;
            };

            SyncEvents.AccountRemoved += key =>
            {
                autoSelect.Enabled = false;
            };

            SyncEvents.AccountLoading += key =>
            {
                autoSelect.Enabled = true;
            };

            SyncEvents.AccountClosing += (key, loggedIn) =>
            {
                autoSelect.Enabled = false;
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                if (activeAccount != null)
                    autoSelect.Checked = accountManager.GetSettings(activeAccount).autoSelect;

                autoSelect.TabStop = true;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                if (activeAccount != null)
                    accountManager.GetSettings(activeAccount).autoSelect = autoSelect.Checked;

                autoSelect.TabStop = false;
            };
        }
    }
}
