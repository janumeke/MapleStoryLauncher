using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow
    {
        const int initialWindowHeight = 223;
        const int loggedInHeight = 482;

        private void Controller_MainWindow()
        {
            this.Load += (sender, e) =>
            {
                this.Size = new Size(this.Size.Width, initialWindowHeight);

                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                this.Text += $" v{version.Major}.{version.Minor}";
                if (version.Build != 0)
                    this.Text += $".{version.Build}";
            };

            FormClosing += (sender, e) =>
            {
                beanfun.Cancel();

                bool loggedInBefore = loggedIn;
                if (!AutoLogOut())
                    e.Cancel = true;
                else if (activeAccount != null)
                    SyncEvents.CloseAccount(activeAccount, loggedInBefore);
            };

            SyncEvents.LoggingIn += username =>
            {
                this.UseWaitCursor = true;
            };

            SyncEvents.LoginFailed += () =>
            {
                this.UseWaitCursor = false;
            };

            SyncEvents.LoggedIn_Loading += username =>
            {
                this.Size = new Size(this.Size.Width, loggedInHeight);
                this.UseWaitCursor = false;
            };

            SyncEvents.LoggedOut += (username, auto) =>
            {
                this.Size = new Size(this.Size.Width, initialWindowHeight);
            };
        }
    }
}
