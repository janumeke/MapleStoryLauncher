using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_AddRemoveAccount()
        {
            void UpdateImage()
            {
                if (accountManager.Contains(accountInput.Text))
                    AddRemoveAccount.Image = Properties.Resources.minus;
                else
                    AddRemoveAccount.Image = Properties.Resources.plus;
            }

            this.Load += (sender, _) =>
            {
                UpdateImage();
            };

            accountInput.TextChanged += (sender, _) =>
            {
                UpdateImage();
            };

            AddRemoveAccount.Click += (sender, _) =>
            {
                string account = accountInput.Text;

                if (accountManager.Contains(account))
                {
                    if (activeAccount == account)
                        SyncEvents.CloseAccount(activeAccount, loggedIn);
                    SyncEvents.RemoveAccount(account);
                }
                else
                    SyncEvents.CreateAccountAndRestoreSettings(account);
                accountManager.SaveToFile();
            };

            SyncEvents.AccountCreated += key =>
            {
                UpdateImage();
            };

            SyncEvents.AccountRemoved += key =>
            {
                UpdateImage();
            };
        }
    }
}
