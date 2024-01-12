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
        public ReCaptchaWindow()
        {
            InitializeComponent();
            webView.EnsureCoreWebView2Async();
        }

        private void webView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.NavigationCompleted += webView_NavigationCompleted;
        }

        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            webView.ExecuteScriptAsync("document.getElementsByClassName('form-row').forEach((e) => { e.style.display = 'none' });");
            webView.ExecuteScriptAsync("document.getElementsByClassName('loginContent__wrap')[0].style.margin = 0;");
            webView.ExecuteScriptAsync("document.getElementsByClassName('loginContent__wrap')[0].style.padding = 0;");
            webView.ExecuteScriptAsync(
                "let reC = document.getElementsByClassName('g-recaptcha')[0];" +
                "if(!reC) window.chrome.webview.postMessage('NULL');" +
                "else{" +
                " setInterval(() => {" +
                "  if(grecaptcha.getResponse() != '')" +
                "   window.chrome.webview.postMessage(grecaptcha.getResponse());" +
                " }, 1000);" +
                "}");
            webView.Visible = true;
        }

        private void webView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            lock (result_lock)
            {
                result = e.TryGetWebMessageAsString();
            }
            Close();
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

        private readonly object address_lock = new();
        private string address;

        public void SetAddress(string url)
        {
            lock (address_lock)
            {
                address = url;
            }
        }

        private readonly object result_lock = new();
        private string result;

        public string GetResult()
        {
            lock (result_lock)
            {
                return result;
            }
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

        private void ReCaptchaWindow_Shown(object sender, EventArgs e)
        {
            webView.Visible = false;
            lock (result_lock)
            {
                result = default;
            }
            lock (address_lock)
            {
                webView.CoreWebView2.Navigate(address);
            }
        }
    }
}
