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
        readonly MainWindow MainWindow;

        public QRCodeWindow(MainWindow handle)
        {
            MainWindow = handle;
            InitializeComponent();
        }

        private BeanfunBroker.TransactionResult result = default;

        public BeanfunBroker.TransactionResult GetResult()
        {
            lock (this)
            {
                return result;
            }
        }

        private void QRCodeWindow_Shown(object sender, EventArgs e)
        {
            TopMost = true;
            Waiting();
            getQRCodeWorker.RunWorkerAsync();
        }

        private void getQRCodeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = MainWindow.beanfun.GetQRCode();
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
                        checkQRCodeStatusTimer.Enabled = true;
                    break;
                default:
                    lock (this)
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
            BeanfunBroker.TransactionResult checkResult = MainWindow.beanfun.CheckQRCode();
            switch (checkResult.Status)
            {
                case BeanfunBroker.TransactionResultStatus.RequireQRCode:
                    failedTries = 0;
                    break;
                case BeanfunBroker.TransactionResultStatus.Expired:
                    failedTries = 0;
                    checkQRCodeStatusTimer.Enabled = false;
                    Waiting();
                    if (Visible)
                        getQRCodeWorker.RunWorkerAsync();
                    break;
                case BeanfunBroker.TransactionResultStatus.ConnectionLost:
                    if (++failedTries >= maxFailedTries)
                    {
                        lock (this)
                        {
                            result = checkResult;
                        }
                        Close();
                    }
                    break;
                default:
                    lock (this)
                    {
                        result = checkResult;
                    }
                    Close();
                    break;
            }
        }

        private void QRCodeWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkQRCodeStatusTimer.Enabled = false;
            if (result == default || //manual closing while pending
                result.Status == BeanfunBroker.TransactionResultStatus.ConnectionLost)
                MainWindow.beanfun.LocalLogout();
        }

        private void Waiting()
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
    }
}
