using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapleStoryLauncher
{
    public partial class AppAuthWindow : Form
    {
        readonly MainWindow MainWindow;

        public AppAuthWindow(MainWindow handle)
        {
            MainWindow = handle;
            InitializeComponent();
        }

        public BeanfunBroker.LoginResult result = default;

        private int failedTries = 0;

        private void checkAppAuthStatusTimer_Tick(object sender, EventArgs e)
        {
            BeanfunBroker.LoginResult checkResult = MainWindow.beanfun.CheckAppAuthentication();
            switch (checkResult.Status)
            {
                case BeanfunBroker.LoginStatus.RequireAppAuthentication:
                    failedTries = 0;
                    break;
                case BeanfunBroker.LoginStatus.ConnectionLost:
                    if (++failedTries >= 3)
                    {
                        checkAppAuthStatusTimer.Enabled = false;
                        result = checkResult;
                        Close();
                    }
                    break;
                default:
                    checkAppAuthStatusTimer.Enabled = false;
                    result = checkResult;
                    Close();
                    break;
            }
        }

        private void AppAuthWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkAppAuthStatusTimer.Enabled = false; //for manual closing
            if (result == default(BeanfunBroker.LoginResult) || //manual closing while pending
                result.Status == BeanfunBroker.LoginStatus.ConnectionLost)
                MainWindow.beanfun.LocalLogout();
        }
    }
}
