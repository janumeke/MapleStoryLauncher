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
        readonly MainWindow mainWindow;

        public AppAuthWindow(MainWindow handle)
        {
            mainWindow = handle;
            InitializeComponent();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Hide();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private readonly object result_lock = new();
        private BeanfunBroker.TransactionResult result = default;

        public BeanfunBroker.TransactionResult GetResult()
        {
            lock (result_lock)
            {
                return result;
            }
        }

        private const int maxFailedTries = 3;
        private int failedTries = 0;

        private void checkAppAuthStatusTimer_Tick(object sender, EventArgs e)
        {
            BeanfunBroker.TransactionResult checkResult = mainWindow.beanfun.CheckAppAuthentication();
            switch (checkResult.Status)
            {
                case BeanfunBroker.TransactionResultStatus.RequireAppAuthentication:
                    failedTries = 0;
                    break;
                case BeanfunBroker.TransactionResultStatus.ConnectionLost:
                    if (++failedTries >= maxFailedTries)
                    {
                        lock (result_lock)
                        {
                            result = checkResult;
                        }
                        Hide();
                    }
                    break;
                default:
                    lock (result_lock)
                    {
                        result = checkResult;
                    }
                    Hide();
                    break;
            }
        }

        private void AppAuthWindow_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                lock (result_lock)
                {
                    result = default;
                }
                failedTries = 0;
                checkAppAuthStatusTimer.Enabled = true;
            }
            else
            {
                checkAppAuthStatusTimer.Enabled = false;
                mainWindow.beanfun.Cancel();
                /*lock (result_lock)
                {
                    if (result == default //manual closing while pending
                        || result.Status == BeanfunBroker.TransactionResultStatus.ConnectionLost)
                        mainWindow.beanfun.LocalLogout(); //Deadlock Warning: hold (result) and wait (MainWindow.beanfun)
                }*/
            }
        }
    }
}
