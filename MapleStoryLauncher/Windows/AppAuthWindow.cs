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

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if(keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private object result_lock = new();
        private BeanfunBroker.TransactionResult result = default;

        public BeanfunBroker.TransactionResult GetResult()
        {
            lock (result_lock)
            {
                return result;
            }
        }

        private void AppAuthWindow_Shown(object sender, EventArgs e)
        {
            lock (result_lock)
            {
                result = default;
            }
            lock (checkAppAuthStatusTimer)
            {
                failedTries = 0;
                checkAppAuthStatusTimer.Enabled = true;
            }
        }

        private const int maxFailedTries = 3;
        private int failedTries = 0;

        private void checkAppAuthStatusTimer_Tick(object sender, EventArgs e)
        {
            if (!checkAppAuthStatusTimer.Enabled)
                return;

            lock (checkAppAuthStatusTimer)
            {
                BeanfunBroker.TransactionResult checkResult = MainWindow.beanfun.CheckAppAuthentication();
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
                            Close(); //Deadlock Warning: hold (checkAppAuthStatusTimer) and wait (result, MainWindow.beanfun)
                        }
                        break;
                    default:
                        lock (result_lock)
                        {
                            result = checkResult;
                        }
                        Close(); //Deadlock Warning: hold (checkAppAuthStatusTimer) and wait (result, MainWindow.beanfun)
                        break;
                }
            }
        }

        private void AppAuthWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkAppAuthStatusTimer.Enabled = false;
            lock (result_lock)
            {
                if (result == default //manual closing while pending
                    || result.Status == BeanfunBroker.TransactionResultStatus.ConnectionLost)
                    MainWindow.beanfun.LocalLogout(); //Deadlock Warning: hold (result) and wait (MainWindow.beanfun)
            }
        }
    }
}
