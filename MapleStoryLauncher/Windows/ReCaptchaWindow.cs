using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapleStoryLauncher
{
    public partial class ReCaptchaWindow : Form
    {
        private async Task InitWebView()
        {
            string userDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{typeof(MainWindow).Namespace}\\";
            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, userDataPath, null);
            await webView.EnsureCoreWebView2Async(env);
        }

        private Task initialization;

        public ReCaptchaWindow()
        {
            InitializeComponent();
            initialization = InitWebView();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Hide();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void webView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.NavigationCompleted += webView_NavigationCompleted;
        }

        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            webView.ExecuteScriptAsync("document.getElementsByClassName('form-row').forEach((e) => { e.style.display = 'none' });");
            webView.ExecuteScriptAsync("let wrap = document.getElementsByClassName('loginContent__wrap')[0]; wrap.style.margin = 0; wrap.style.padding = 0;");
            webView.ExecuteScriptAsync(
                "let recaptcha = document.getElementsByClassName('g-recaptcha')[0];" +
                "if(!recaptcha) window.chrome.webview.postMessage('');" +
                "else {" +
                "  let sitekey = recaptcha.getAttribute('data-sitekey');" +
                "  function recaptchaReady(token) { window.chrome.webview.postMessage(token); }" +
                "  grecaptcha.enterprise.reset(recaptcha, { 'sitekey': sitekey, 'callback': 'recaptchaReady' });" +
                "}"
            );
            webView.Visible = true;
        }

        private void webView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            lock (response_lock)
            {
                response = e.TryGetWebMessageAsString();
            }
            DialogResult = DialogResult.Continue;
            Hide();
        }

        public void SetCookies(CookieCollection cookies)
        {
            webView.CoreWebView2.CookieManager.DeleteAllCookies();
            foreach (Cookie cookie in cookies)
            {
                CoreWebView2Cookie wvCookie = webView.CoreWebView2.CookieManager.CreateCookieWithSystemNetCookie(cookie);
                webView.CoreWebView2.CookieManager.AddOrUpdateCookie(wvCookie);
            }
        }

        private readonly object address_lock = new();
        private string address;

        public void SetAddress(string url)
        {
            lock (address_lock)
            {
                address = url;
            }
        }

        private readonly object response_lock = new();
        private string response;

        public string GetResponse()
        {
            lock (response_lock)
            {
                return response;
            }
        }

        private async void ReCaptchaWindow_Shown(object sender, EventArgs e)
        {
            webView.Visible = false;
            lock (response_lock)
            {
                response = default;
            }
            await initialization;
            lock (address_lock)
            {
                webView.CoreWebView2.Navigate(address);
            }
        }
    }
}
