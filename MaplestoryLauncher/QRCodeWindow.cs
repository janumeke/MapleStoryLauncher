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
        MainWindow MainWindow;

        public QRCodeWindow(MainWindow handle)
        {
            MainWindow = handle;
            InitializeComponent();
        }

        private void getQRCodeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MainWindow.bfClient = new BeanfunClient();
            MainWindow.bfClient.GetQRCodeLoginInfo(MainWindow.bfClient.GetSessionKey());
        }

        private void getQRCodeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (MainWindow.bfClient.qrcode == null)
                Error();
            else
            {
                progressReport.Visible = false;
                qrcodeDisplay.Image = MainWindow.bfClient.qrcode.bitmap;
                qrcodeDisplay.Visible = true;
                CheckQRCodeStatusTimer.Enabled = true;
            }
        }

        private int tries = 0;
        public BeanfunClient.QRCodeLoginState result = BeanfunClient.QRCodeLoginState.Pending;

        private void CheckQRCodeStatusTimer_Tick(object sender, EventArgs e)
        {
            BeanfunClient.QRCodeLoginState state = MainWindow.bfClient.CheckQRCodeLoginStatus();
            switch (state)
            {
                case BeanfunClient.QRCodeLoginState.Pending:
                    tries = 0;
                    break;
                case BeanfunClient.QRCodeLoginState.Expired:
                    tries = 0;
                    CheckQRCodeStatusTimer.Enabled = false;
                    Waiting();
                    getQRCodeWorker.RunWorkerAsync();
                    break;
                case BeanfunClient.QRCodeLoginState.Successful:
                    CheckQRCodeStatusTimer.Enabled = false;
                    result = BeanfunClient.QRCodeLoginState.Successful;
                    Close();
                    break;
                case BeanfunClient.QRCodeLoginState.Error:
                    if (++tries >= 3)
                    {
                        CheckQRCodeStatusTimer.Enabled = false;
                        Error();
                    }
                    break;
            }
        }

        private void Waiting()
        {
            qrcodeDisplay.Visible = false;
            progressReport.Text = "取得中...";
            CenterAlignLabel(progressReport);
            progressReport.Visible = true;
        }

        private void Error()
        {
            qrcodeDisplay.Visible = false;
            progressReport.Text = "取得失敗！\n請關閉視窗後再試一次。";
            CenterAlignLabel(progressReport);
            progressReport.Visible = true;
            result = BeanfunClient.QRCodeLoginState.Error;
        }

        private void CenterAlignLabel(Label label)
        {
            label.Location = new Point(((Size.Width - 10) - label.Size.Width) / 2, ((Size.Height - 29) - label.Size.Height) / 2);
        }

        private void QRCodeWindow_Shown(object sender, EventArgs e)
        {
            TopMost = true;
            Waiting();
            getQRCodeWorker.RunWorkerAsync();
        }

        private void QRCodeWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            CheckQRCodeStatusTimer.Enabled = false;
        }
    }
}
