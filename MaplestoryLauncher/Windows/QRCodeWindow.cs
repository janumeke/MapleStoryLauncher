using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaplestoryLauncher
{
    public partial class QRCodeWindow : Form
    {
        readonly MainWindow MainWindow;

        public QRCodeWindow(MainWindow handle)
        {
            MainWindow = handle;
            InitializeComponent();
        }

        public BeanfunBroker.LoginResult result = default;

        private void getQRCodeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = MainWindow.beanfun.GetQRCode();
        }

        private void QRCodeWindow_Shown(object sender, EventArgs e)
        {
            TopMost = true;
            Waiting();
            getQRCodeWorker.RunWorkerAsync();
        }

        private void getQRCodeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BeanfunBroker.QRLoginResult result = (BeanfunBroker.QRLoginResult)e.Result;
            switch (result.Status)
            {
                case BeanfunBroker.LoginStatus.Success:
                    progressReport.Visible = false;
                    qrcodeDisplay.Image = result.Picture;
                    qrcodeDisplay.Visible = true;
                    FormBorderStyle = FormBorderStyle.Sizable;
                    checkQRCodeStatusTimer.Enabled = true;
                    break;
                default:
                    this.result = result;
                    Close();
                    break;
            }
        }

        private int failedTries = 0;

        private void checkQRCodeStatusTimer_Tick(object sender, EventArgs e)
        {
            BeanfunBroker.LoginResult checkResult = MainWindow.beanfun.CheckQRCode();
            switch (checkResult.Status)
            {
                case BeanfunBroker.LoginStatus.RequireQRCode:
                    failedTries = 0;
                    break;
                case BeanfunBroker.LoginStatus.Expired:
                    failedTries = 0;
                    checkQRCodeStatusTimer.Enabled = false;
                    Waiting();
                    getQRCodeWorker.RunWorkerAsync();
                    break;
                case BeanfunBroker.LoginStatus.ConnectionLost:
                    if (++failedTries >= 3)
                    {
                        result = checkResult;
                        checkQRCodeStatusTimer.Enabled = false;
                        Close();
                    }
                    break;
                default:
                    checkQRCodeStatusTimer.Enabled = false;
                    result = checkResult;
                    Close();
                    break;
            }
        }

        private void QRCodeWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkQRCodeStatusTimer.Enabled = false; //for manual closing
            if (result == default(BeanfunBroker.LoginResult) || //manual closing while pending
                result.Status == BeanfunBroker.LoginStatus.ConnectionLost)
                MainWindow.beanfun.LocalLogout();
        }

        private void Waiting()
        {
            qrcodeDisplay.Visible = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            progressReport.Text = "取得中...";
            CenterAlignLabel(progressReport);
            progressReport.Visible = true;
        }

        private void Error()
        {
            qrcodeDisplay.Visible = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            progressReport.Text = "取得失敗！\n請關閉視窗後再試一次。";
            CenterAlignLabel(progressReport);
            progressReport.Visible = true;
        }

        private void CenterAlignLabel(Label label)
        {
            label.Location = new Point(((Size.Width - 10) - label.Size.Width) / 2, ((Size.Height - 29) - label.Size.Height) / 2);
        }
    }
}
