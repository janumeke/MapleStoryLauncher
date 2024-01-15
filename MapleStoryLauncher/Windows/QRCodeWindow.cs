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
    public partial class QRCodeWindow : Form
    {
        readonly MainWindow mainWindow;

        public QRCodeWindow(MainWindow handle)
        {
            mainWindow = handle;
            InitializeComponent();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private readonly object result_lock = new();
        private BeanfunBroker.TransactionResult result;

        public BeanfunBroker.TransactionResult GetResult()
        {
            lock (result_lock)
            {
                return result;
            }
        }

        private void QRCodeWindow_Shown(object sender, EventArgs e)
        {
            lock (result_lock)
            {
                result = default;
            }
            ShowWaitMessage();
            if (!getQRCodeWorker.IsBusy)
                getQRCodeWorker.RunWorkerAsync();
        }

        private void getQRCodeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = mainWindow.beanfun.GetQRCode();
        }

        private void getQRCodeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BeanfunBroker.QRCodeResult getResult = (BeanfunBroker.QRCodeResult)e.Result;
            switch (getResult.Status)
            {
                case BeanfunBroker.TransactionResultStatus.Success:
                    labelProgress.Visible = false;
                    qrcodeDisplay.Image = getResult.Picture;
                    qrcodeDisplay.Visible = true;
                    FormBorderStyle = FormBorderStyle.Sizable;
                    if (Visible)
                    {
                        failedTries = 0;
                        checkQRCodeStatusTimer.Enabled = true;
                    }
                    break;
                default:
                    lock (result_lock)
                    {
                        result = getResult;
                    }
                    Close();
                    break;
            }
        }

        private const int maxFailedTries = 3;
        private int failedTries = 0;

        private void checkQRCodeStatusTimer_Tick(object sender, EventArgs e)
        {
            BeanfunBroker.TransactionResult checkResult = mainWindow.beanfun.CheckQRCode();
            switch (checkResult.Status)
            {
                case BeanfunBroker.TransactionResultStatus.RequireQRCode:
                    failedTries = 0;
                    break;
                case BeanfunBroker.TransactionResultStatus.Expired:
                    checkQRCodeStatusTimer.Enabled = false;
                    ShowWaitMessage();
                    getQRCodeWorker.RunWorkerAsync();
                    break;
                case BeanfunBroker.TransactionResultStatus.ConnectionLost:
                    if (++failedTries >= maxFailedTries)
                    {
                        lock (result_lock)
                        {
                            result = checkResult;
                        }
                        Close();
                    }
                    break;
                default:
                    lock (result_lock)
                    {
                        result = checkResult;
                    }
                    Close();
                    break;
            }
        }

        private void ShowWaitMessage()
        {
            qrcodeDisplay.Visible = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            labelProgress.Text = "取得中...";
            labelProgress.Location = new Point(
                qrcodeDisplay.Location.X + (qrcodeDisplay.Width - labelProgress.Size.Width) / 2,
                qrcodeDisplay.Location.Y + (qrcodeDisplay.Height - labelProgress.Size.Height) / 2
            );
            labelProgress.Visible = true;
        }

        private void QRCodeWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkQRCodeStatusTimer.Enabled = false;
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
