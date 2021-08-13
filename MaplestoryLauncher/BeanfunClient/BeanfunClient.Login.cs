using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using System.ComponentModel;

namespace MaplestoryLauncher
{
    public partial class BeanfunClient : WebClient
    {
        public enum LoginMethod : int
        {
            Regular = 0,
            QRCode = 1
        };

        public void Login(string id, string pass, LoginMethod loginMethod)
        {
            try
            {
                string skey = null;
                string akey = null;
                switch (loginMethod)
                {
                    case LoginMethod.Regular:
                        skey = GetSessionKey();
                        akey = RegularLogin(id, pass, skey);
                        break;
                    case LoginMethod.QRCode:
                        skey = qrcode.skey;
                        akey = QRCodeLogin();
                        break;
                    default:
                        this.errmsg = "LoginNoMethod";
                        return;
                }
                if (akey == null)
                    return;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("SessionKey", skey);
                payload.Add("AuthKey", akey);
                Debug.WriteLine(skey);
                Debug.WriteLine(akey);
                string response = null;
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.beanfun.com/beanfun_block/bflogin/return.aspx", payload));
                Debug.WriteLine(response);
                response = this.DownloadString("https://tw.beanfun.com/" + this.ResponseHeaders["Location"]);
                Debug.WriteLine(response);
                Debug.WriteLine(this.ResponseHeaders);

                this.webtoken = this.GetCookie("bfWebToken");
                if (this.webtoken == "")
                { this.errmsg = "LoginNoWebtoken"; return; }

                GetAccounts(false);
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    this.errmsg = "網路連線錯誤，請檢查官方網站連線是否正常。" + e.Message;
                }
                else
                {
                    this.errmsg = "LoginUnknown\n\n" + e.Message + "\n" + e.StackTrace;
                }
                return;
            }
        }

        public void Logout()
        {
            string response = this.DownloadString("https://tw.beanfun.com/generic_handlers/remove_bflogin_session.ashx");
            //response = this.DownloadString("https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0");
            NameValueCollection payload = new NameValueCollection();
            payload.Add("web_token", "1");
            this.UploadValues("https://tw.newlogin.beanfun.com/generic_handlers/erase_token.ashx", payload);
        }

        public string GetSessionKey()
        {
            string response = this.DownloadString("https://tw.beanfun.com/beanfun_block/bflogin/default.aspx?service=999999_T0");
            //this.DownloadString(this.ResponseHeaders["Location"]);
            //this.DownloadString(this.ResponseHeaders["Location"]);
            //response = this.ResponseHeaders["Location"];
            response = this.ResponseUri.ToString();
            if (response == null)
            { this.errmsg = "LoginNoResponse"; return null; }
            Regex regex = new Regex("skey=(.*)&display");
            if (!regex.IsMatch(response))
            { this.errmsg = "LoginNoSkey"; return null; }
            return regex.Match(response).Groups[1].Value;
        }

        private string RegularLogin(string id, string pass, string skey)
        {
            try
            {
                string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + skey);
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoViewstate"; return null; }
                string viewstate = regex.Match(response).Groups[1].Value;

                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoEventvalidation"; return null; }
                string eventvalidation = regex.Match(response).Groups[1].Value;
                regex = new Regex("id=\"__VIEWSTATEGENERATOR\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoViewstateGenerator"; return null; }
                string viewstateGenerator = regex.Match(response).Groups[1].Value;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__VIEWSTATEGENERATOR", viewstateGenerator);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("t_AccountID", id);
                payload.Add("t_Password", pass);
                payload.Add("btn_login", "登入");

                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + skey, payload));

                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                { this.errmsg = "LoginNoAkey"; return null; }
                string akey = regex.Match(this.ResponseUri.ToString()).Groups[1].Value;

                return akey;
            }
            catch (Exception e)
            {
                this.errmsg = "LoginUnknown\n\n" + e.Message + "\n" + e.StackTrace;
                return null;
            }
        }

        public class QRCodeLoginInfo
        {
            public string skey;
            public string value;
            public string viewstate;
            public string eventvalidation;
            public Bitmap bitmap;
        }

        public QRCodeLoginInfo qrcode;

        private string QRCodeLogin()
        {
            try
            {
                string skey = qrcode.skey;

                this.Headers.Set("Referer", @"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey=" + skey);
                this.redirect = false;
                string response = Encoding.UTF8.GetString(this.DownloadData("https://tw.newlogin.beanfun.com/login/qr_step2.aspx?skey=" + skey));
                Debug.Write(response);
                this.redirect = true;

                Regex regex = new Regex("akey=(.*)&authkey");
                if (!regex.IsMatch(response))
                { this.errmsg = "AKeyParseFailed"; return null; }
                string akey = regex.Match(response).Groups[1].Value;

                regex = new Regex("authkey=(.*)&");
                if (!regex.IsMatch(response))
                { this.errmsg = "authkeyParseFailed"; return null; }
                string authkey = regex.Match(response).Groups[1].Value;
                Debug.WriteLine(authkey);
                this.DownloadString("https://tw.newlogin.beanfun.com/login/final_step.aspx?akey=" + akey + "&authkey=" + authkey + "&bfapp=1");
                return akey;
            }
            catch (Exception e)
            {
                this.errmsg = "LoginUnknown\n\n" + e.Message + "\n" + e.StackTrace;
                return null;
            }
        }

        public void GetQRCodeLoginInfo(string skey)
        {
            qrcode = null;

            string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey=" + skey);
            Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
            if (!regex.IsMatch(response))
            { this.errmsg = "LoginNoViewstate"; return; }
            string viewstate = regex.Match(response).Groups[1].Value;

            regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
            if (!regex.IsMatch(response))
            { this.errmsg = "LoginNoEventvalidation"; return; }
            string eventvalidation = regex.Match(response).Groups[1].Value;

            //Thread.Sleep(3000);

            string value;
            string strEncryptData;
            Stream stream;

            regex = new Regex("\\$\\(\"#theQrCodeImg\"\\).attr\\(\"src\", \"../(.*)\" \\+ obj.strEncryptData\\);");
            if (!regex.IsMatch(response))
            { this.errmsg = "LoginNoHash"; return; }
            value = regex.Match(response).Groups[1].Value;

            response = this.DownloadString($"https://tw.newlogin.beanfun.com/generic_handlers/get_qrcodeData.ashx?skey={skey}");
            //regex = new Regex("\"strEncryptData\": \"(.*)\"}");
            //if (!regex.IsMatch(response))
            //{ this.errmsg = "LoginIntResultError"; return null; }

            //strEncryptData = regex.Match(response).Groups[1].Value;
            var definitions = new { intResult = "", strResult = "", strEncryptData = "", strEncryptBCDOData = "" };
            var json = JsonConvert.DeserializeAnonymousType(response, definitions);
            strEncryptData = json.strEncryptData;

            stream = this.OpenRead($"https://tw.newlogin.beanfun.com/{value}{strEncryptData}");

            qrcode = new QRCodeLoginInfo();
            qrcode.skey = skey;
            qrcode.viewstate = viewstate;
            qrcode.eventvalidation = eventvalidation;
            qrcode.value = strEncryptData;
            qrcode.bitmap = new Bitmap(stream);
        }

        public enum QRCodeLoginState
        {
            Pending,
            Expired,
            Successful,
            Error
        }

        public QRCodeLoginState CheckQRCodeLoginStatus()
        {
            try
            {
                string skey = qrcode.skey;
                string result;
                this.Headers.Set("Referer", @"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey=" + skey);

                NameValueCollection payload = new NameValueCollection();
                payload.Add("status", qrcode.value);
                //Debug.WriteLine(qrcodeclass.value);

                string response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/generic_handlers/CheckLoginStatus.ashx", payload));
                JObject jsonData;
                try { jsonData = JObject.Parse(response); }
                catch { this.errmsg = "LoginJsonParseFailed"; return QRCodeLoginState.Error; }

                result = (string)jsonData["ResultMessage"];
                Debug.WriteLine(result);
                switch (result)
                {
                    case "Failed":
                        return QRCodeLoginState.Pending;
                    case "Token Expired":
                        return QRCodeLoginState.Expired;
                    case "Success":
                        return QRCodeLoginState.Successful;
                    default:
                        this.errmsg = response;
                        return QRCodeLoginState.Error;
                        
                }
            }
            catch (Exception e)
            {
                this.errmsg = "Network Error on QRCode checking login status\n\n" + e.Message + "\n" + e.StackTrace;
            }
            return QRCodeLoginState.Error;
        }
    }

}
