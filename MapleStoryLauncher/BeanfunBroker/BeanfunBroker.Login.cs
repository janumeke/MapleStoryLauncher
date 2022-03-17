using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        public enum LoginStatus
        {
            Success,
            RequireAppAuthentication,
            RequireQRCode,
            Expired,
            Denied,
            Failed,
            ConnectionLost,
            LoginFirst,
        }

        public class LoginResult
        {
            public LoginStatus Status { get; set; }
            public string Message { get; set; }
        }

        private LoginResult GetSkey()
        {
            HttpRequestMessage req;
            Match match;

            req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/beanfun_block/bflogin/default.aspx?service=999999_T0&dt={GetDateTime(DateTimeType.Skey)}&url=https://tw.beanfun.com/");
            try { res = client.SendAsync(req).Result; }
            catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(skey)" }; }
            if (!res.IsSuccessStatusCode)
                return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(skey)" };
            match = Regex.Match(res.Content.ReadAsStringAsync().Result, @"strSessionKey = ""([^""]*)""");
            if (match == Match.Empty)
                return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 skey。" };
            return new LoginResult { Status = LoginStatus.Success, Message = match.Groups[1].Value };
        }

        /**
         * <summary>Login to beanfun by an account and a password.</summary>
         * <returns>
         * Possible statuses (message):
         *   Failed (error description):
         *     1. This account is currently logged in.
         *     2. Requests to beanfun returns a non-success http code.
         *     3. Returned messages don't have the expected contents.
         *   ConnectionLost (description):
         *     Any connection failed.
         *   RequireAppAuthentication (none):
         *     App authentication is required by beanfun. An incomplete account has been created.(See '<see cref="LocalLogout"/>'.)
         *   Denied (reason given by beanfun):
         *     Beanfun rejected this login for any reason.
         *   Success (none): 
         *     The account is logged in.
         * </returns>
         */
        public LoginResult Login(string username, string password)
        {
            if (account != default(BeanfunAccount))
                return new LoginResult { Status = LoginStatus.Failed, Message = "登入狀態不允許此操作。(一般登入)" };

            rwLock.EnterWriteLock();
            try
            {
                HttpRequestMessage req;
                Dictionary<string, string> form;
                string body;
                Match match;

                string skey;
                LoginResult getskey = GetSkey();
                if (getskey.Status == LoginStatus.Success)
                    skey = getskey.Message;
                else
                    return getskey;

                #region Login page(Regular)
                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/checkin_step2.aspx?skey={skey}&display_mode=2");
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(登入頁面(一般))" }; }
                if (!res.IsSuccessStatusCode)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(登入頁面(一般))" };
                #endregion

                #region Get viewstate, viewstateGenerator and eventvalidation
                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={skey}&clientID=undefined");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/checkin_step2.aspx?skey={skey}&display_mode=2");
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(viewstate, viewstateGenerator, eventvalidation)" }; }
                if (!res.IsSuccessStatusCode)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(viewstate, viewstateGenerator, eventvalidation)" };
                body = res.Content.ReadAsStringAsync().Result;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 viewstate。" };
                string viewstate = match.Groups[1].Value;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__VIEWSTATEGENERATOR"" id=""__VIEWSTATEGENERATOR"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 viewstategenerator。" };
                string viewstateGenerator = match.Groups[1].Value;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__EVENTVALIDATION"" id=""__EVENTVALIDATION"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 eventvalidation。" };
                string eventvalidation = match.Groups[1].Value;
                #endregion

                #region Get akey and writeUrl
                req = new HttpRequestMessage(HttpMethod.Post, $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={skey}&clientID=undefined");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={skey}&clientID=undefined");
                form = new Dictionary<string, string>
                {
                    { "__EVENTTARGET", "" },
                    { "__EVENTARGUMENT", "" },
                    { "__VIEWSTATE", viewstate },
                    { "__VIEWSTATEGENERATOR", viewstateGenerator },
                    { "__EVENTVALIDATION", eventvalidation },
                    { "t_AccountID", username },
                    { "t_Password", password },
                    { "btn_login", "登入" }
                };
                req.Content = new FormUrlEncodedContent(form);
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(akey, writeUrl)" }; }
                if (!res.IsSuccessStatusCode)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(akey, writeUrl)" };
                body = res.Content.ReadAsStringAsync().Result;
                //Require app authenticaiotn
                match = Regex.Match(body, @"pollRequest\(""bfAPPAutoLogin\.ashx"",""([^""]*)"",""[^""]*""\);");
                if (match != Match.Empty)
                {
                    account = new BeanfunAccount
                    {
                        skey = skey,
                        lt = match.Groups[1].Value
                    };
                    return new LoginResult { Status = LoginStatus.RequireAppAuthentication };
                }
                //Check for errors
                match = Regex.Match(body, @"<div id=""pnlMsg"">.*<script type=""text\/javascript"">\$\(function\(\)\{MsgBox\.Show\('(.*)'\);\}\);<\/script>.*<\/div>", RegexOptions.Singleline);
                if (match != Match.Empty)
                {
                    ClearCookies();
                    return new LoginResult { Status = LoginStatus.Denied, Message = match.Groups[1].Value.Replace("<br />", "\n") };
                }
                //Retrieve info
                match = Regex.Match(body, @"AuthKey.value = ""([^""]*)"";");
                if (match == Match.Empty)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 akey。" };
                string akey = match.Groups[1].Value;
                match = Regex.Match(body, @"var strWriteUrl = ""([^""]*)"";");
                if (match == Match.Empty)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 writeUrl。" };
                string writeUrl = match.Groups[1].Value;
                #endregion

                #region Writecookie
                req = new HttpRequestMessage(HttpMethod.Get, writeUrl);
                req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/");
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(writecookie)" }; }
                if (!res.IsSuccessStatusCode)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(writecookie)" };
                #endregion

                #region Get bfwebtoken
                req = new HttpRequestMessage(HttpMethod.Post, $"https://tw.beanfun.com/beanfun_block/bflogin/return.aspx");
                req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/");
                form = new Dictionary<string, string>
                {
                    { "SessionKey", skey },
                    { "AuthKey", akey },
                    { "ServiceCode", "" },
                    { "ServiceRegion", "" },
                    { "ServiceAccountSN", "0" }
                };
                req.Content = new FormUrlEncodedContent(form);
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(bfWebToken)" }; }
                if (!res.IsSuccessStatusCode)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(bfWebToken)" };
                if (cookies.GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"] == default(Cookie))
                    return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 bfWebToken。" };
                string bfwebtoken = cookies.GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"].Value;
                #endregion

                account = new BeanfunAccount
                {
                    webtoken = bfwebtoken
                };
                return new LoginResult { Status = LoginStatus.Success };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private class CheckAppAuthResponse
        {
            public int IntResult = default;
            public string StrReslut = default; //this name is decided by the protocol
        }

        /**
         * <summary>Check if the app authentication is passed.</summary>
         * <returns>
         * Possible statuses (message):
         *   Failed (error description):
         *     1. 'Login()' hasn't been called.
         *     2. Requests to beanfun returns a non-success http code.
         *     3. Returned messages don't have the expected contents.
         *   ConnectionLost (description):
         *     Any connection failed. The incomplete account is not cleared.(See '<see cref="LocalLogout"/>'.)
         *   RequireAppAuthentication (none):
         *     App authentication is pending. The incomplete account is not cleared.(See '<see cref="LocalLogout"/>'.)
         *   Expired (description):
         *     The login session has expired.
         *   Denied (reason given by beanfun):
         *     The App denied this login.
         *   Success (none): 
         *     The account is logged in.
         * </returns>
         */
        public LoginResult CheckAppAuthentication()
        {
            if (account == default(BeanfunAccount))
                return new LoginResult { Status = LoginStatus.Failed, Message = "登入狀態不允許此操作。(檢查App授權)" };

            rwLock.EnterWriteLock();
            try
            {
                HttpRequestMessage req;
                Dictionary<string, string> form;
                string body;
                Match match;

                req = new HttpRequestMessage(HttpMethod.Post, "https://tw.newlogin.beanfun.com/login/bfAPPAutoLogin.ashx");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={account.skey}&clientID=undefined");
                form = new Dictionary<string, string>
                {
                    { "LT", account.lt }
                };
                req.Content = new FormUrlEncodedContent(form);
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(檢查App授權)" }; }
                if (!res.IsSuccessStatusCode)
                {
                    account = default;
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(檢查App授權)" };
                }
                CheckAppAuthResponse result = JsonConvert.DeserializeObject<CheckAppAuthResponse>(res.Content.ReadAsStringAsync().Result);
                switch (result.IntResult)
                {
                    case -3:
                        account = default;
                        return new LoginResult { Status = LoginStatus.Denied, Message = "登入要求已被 App 拒絕。" };
                    case -1:
                        account = default;
                        return new LoginResult { Status = LoginStatus.Expired, Message = "登入階段過期。(檢查App授權)" };
                    case 1:
                        return new LoginResult { Status = LoginStatus.RequireAppAuthentication };
                    case 2:
                        #region Get akey and writeUrl
                        req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/login/{result.StrReslut}");
                        req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={account.skey}&clientID=undefined");
                        try { res = client.SendAsync(req).Result; }
                        catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(akey, writeUrl)" }; }
                        if (!res.IsSuccessStatusCode)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(akey, writeUrl)" };
                        }
                        body = res.Content.ReadAsStringAsync().Result;
                        match = Regex.Match(body, @"AuthKey.value = ""([^""]*)"";");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 akey。" };
                        }
                        string akey = match.Groups[1].Value;
                        match = Regex.Match(body, @"var strWriteUrl = ""([^""]*)"";");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 writeUrl。" };
                        }
                        string writeUrl = match.Groups[1].Value;
                        #endregion

                        #region Writecookie
                        req = new HttpRequestMessage(HttpMethod.Get, writeUrl);
                        req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/");
                        try { res = client.SendAsync(req).Result; }
                        catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(writecookie)" }; }
                        if (!res.IsSuccessStatusCode)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(writecookie)" };
                        }
                        #endregion

                        #region Get bfwebtoken
                        req = new HttpRequestMessage(HttpMethod.Post, $"https://tw.beanfun.com/beanfun_block/bflogin/return.aspx");
                        req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/");
                        form = new Dictionary<string, string>
                        {
                            { "SessionKey", account.skey },
                            { "AuthKey", akey },
                            { "ServiceCode", "" },
                            { "ServiceRegion", "" },
                            { "ServiceAccountSN", "0" }
                        };
                        req.Content = new FormUrlEncodedContent(form);
                        try { res = client.SendAsync(req).Result; }
                        catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(bfWebToken)" }; }
                        if (!res.IsSuccessStatusCode)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(bfWebToken)" };
                        }
                        if (cookies.GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"] == default(Cookie))
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 bfWebToken。" };
                        }
                        string bfwebtoken = cookies.GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"].Value;
                        #endregion

                        account.webtoken = bfwebtoken;
                        return new LoginResult { Status = LoginStatus.Success };
                }
                account = default;
                return new LoginResult { Status = LoginStatus.Failed, Message = "不是預期的資料。(檢查App授權)" };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public class QRLoginResult : LoginResult
        {
            public Bitmap Picture { get; set; }
        }

        private class GetQRCodeResponse
        {
            public int intResult = default;
            public string strResult = default;
            public string strEncryptData = default;
            public string strEncryptBCDOData = default;
        }

        /**
         * <summary>Get a QRCode for login.</summary>
         * <returns>
         * Possible statuses (message, picture):
         *   Failed (error description, none):
         *     1. This account is currently logged in.
         *     2. Requests to beanfun returns a non-success http code.
         *     3. Returned messages don't have the expected contents.
         *   ConnectionLost (description, none):
         *     Any connection failed.
         *   Success (none, QRCode): 
         *     QRCode was gotten. An incomplete account has been created.(See '<see cref="LocalLogout"/>'.)
         * </returns>
         */
        public QRLoginResult GetQRCode()
        {
            if (account != default(BeanfunAccount))
                return new QRLoginResult { Status = LoginStatus.Failed, Message = "登入狀態不允許此操作。(取得 QRCode)" };

            rwLock.EnterWriteLock();
            try
            {
                HttpRequestMessage req;
                Match match;

                string skey;
                LoginResult getskey = GetSkey();
                if (getskey.Status == LoginStatus.Success)
                    skey = getskey.Message;
                else
                    return new QRLoginResult { Status = getskey.Status, Message = getskey.Message };

                #region Login page(QRCode)
                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2&region=qr");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2");
                try { res = client.SendAsync(req).Result; }
                catch { return new QRLoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(登入頁面(QRCode))" }; }
                if (!res.IsSuccessStatusCode)
                    return new QRLoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(登入頁面(QRCode))" };
                #endregion

                #region Get qrhandler
                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={skey}&clientID=undefined");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2&region=qr");
                try { res = client.SendAsync(req).Result; }
                catch { return new QRLoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(qrhandler)" }; }
                if (!res.IsSuccessStatusCode)
                    return new QRLoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(qrhandler)" };
                match = Regex.Match(res.Content.ReadAsStringAsync().Result, @"\$\(""#theQrCodeImg""\)\.attr\(""src"", ""\.\.\/([^""]*)""");
                if (match == Match.Empty)
                    return new QRLoginResult { Status = LoginStatus.Failed, Message = "找不到 qrhandler。" };
                string qrhandler = match.Groups[1].Value;
                #endregion

                #region Get encryptdata
                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/generic_handlers/get_qrcodeData.ashx?skey={skey}&startGame=&clientID=");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={skey}&clientID=undefined");
                try { res = client.SendAsync(req).Result; }
                catch { return new QRLoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(encryptdata)" }; }
                if (!res.IsSuccessStatusCode)
                    return new QRLoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(encryptdata)" };
                GetQRCodeResponse qrdata = JsonConvert.DeserializeObject<GetQRCodeResponse>(res.Content.ReadAsStringAsync().Result);
                string encryptdata;
                if (qrdata.strEncryptData != default)
                    encryptdata = qrdata.strEncryptData;
                else
                    return new QRLoginResult { Status = LoginStatus.Failed, Message = "找不到 encryptdata。" };
                #endregion

                #region Get QRCode picture
                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/{qrhandler}{encryptdata}");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={skey}&clientID=undefined");
                try { res = client.SendAsync(req).Result; }
                catch { return new QRLoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(QRcode圖片)" }; }
                if (!res.IsSuccessStatusCode)
                    return new QRLoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(QRcode圖片)" };
                Bitmap qrPic = new(res.Content.ReadAsStreamAsync().Result);
                #endregion

                account = new BeanfunAccount
                {
                    skey = skey,
                    encryptdata = encryptdata
                };
                return new QRLoginResult { Status = LoginStatus.Success, Picture = qrPic };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private class CheckQRCodeResponse
        {
            public int Result = default;
            public string ResultMessage = default;
        }

        /**
         * <summary>Check if the QRCode has been used to login.</summary>
         * <returns>
         * Possible statuses (message):
         *   Failed (error description):
         *     1. 'GetQRCode()' hasn't been called.
         *     2. Requests to beanfun returns a non-success http code.
         *     3. Returned messages don't have the expected contents.
         *   ConnectionLost (description):
         *     Any connection failed. The incomplete account is not cleared.(See '<see cref="LocalLogout"/>'.)
         *   RequireQRCode (none):
         *     QRCode is pending for login. The incomplete account is not cleared.(See '<see cref="LocalLogout"/>'.)
         *   Expired (description):
         *     The QRcode has expired.
         *   Success (none): 
         *     The account is logged in.
         * </returns>
         */
        public LoginResult CheckQRCode()
        {
            if (account == default(BeanfunAccount))
                return new LoginResult { Status = LoginStatus.Failed, Message = "登入狀態不允許此操作。(檢查 QRCode)" };

            rwLock.EnterWriteLock();
            try
            {
                HttpRequestMessage req;
                Dictionary<string, string> form;
                string body;
                Match match;

                req = new HttpRequestMessage(HttpMethod.Post, "https://tw.newlogin.beanfun.com/generic_handlers/CheckLoginStatus.ashx");
                req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={account.skey}&clientID=undefined");
                form = new Dictionary<string, string>
                {
                    { "status", account.encryptdata }
                };
                req.Content = new FormUrlEncodedContent(form);
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(檢查 QRCode)" }; }
                if (!res.IsSuccessStatusCode)
                {
                    account = default;
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(檢查 QRCode)" };
                }
                CheckQRCodeResponse result = JsonConvert.DeserializeObject<CheckQRCodeResponse>(res.Content.ReadAsStringAsync().Result);
                switch (result.Result)
                {
                    case 0:
                        if (result.ResultMessage == "Failed")
                            return new LoginResult { Status = LoginStatus.RequireQRCode };
                        if (result.ResultMessage == "Token Expired")
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Expired, Message = "QRCode 過期。(檢查 QRCode)" };
                        }
                        account = default;
                        return new LoginResult { Status = LoginStatus.Failed, Message = "不是預期的資料。(檢查 QRCode)" };
                    case 1:
                        #region Get akey, finalstep
                        req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/login/qr_step2.aspx?skey={account.skey}");
                        req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={account.skey}&clientID=undefined");
                        try { res = client.SendAsync(req).Result; }
                        catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(akey, finalstep)" }; }
                        if (!res.IsSuccessStatusCode)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(akey, finalstep)" };
                        }
                        body = res.Content.ReadAsStringAsync().Result;
                        match = Regex.Match(body, @"RedirectPage\("""",""\.\/([^""]*)""\)");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 final_step。" };
                        }
                        string finalstep = match.Groups[1].Value;
                        match = Regex.Match(finalstep, @"akey=([^&]*)&");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 akey。" };
                        }
                        string akey = match.Groups[1].Value;
                        #endregion

                        #region Get writeUrl
                        req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/login/{finalstep}");
                        req.Headers.Referrer = new Uri($"https://tw.newlogin.beanfun.com/login/qr_step2.aspx?skey={account.skey}");
                        try { res = client.SendAsync(req).Result; }
                        catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(writeUrl)" }; }
                        if (!res.IsSuccessStatusCode)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(writeUrl)" };
                        }
                        match = Regex.Match(res.Content.ReadAsStringAsync().Result, @"var strWriteUrl = ""([^""]*)"";");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 writeUrl。" };
                        }
                        string writeUrl = match.Groups[1].Value;
                        #endregion

                        #region Writecookie
                        req = new HttpRequestMessage(HttpMethod.Get, writeUrl);
                        req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/");
                        try { res = client.SendAsync(req).Result; }
                        catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(Writecookie)" }; }
                        if (!res.IsSuccessStatusCode)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(Writecookie)" };
                        }
                        #endregion

                        #region Get bfwebtoken
                        req = new HttpRequestMessage(HttpMethod.Post, $"https://tw.beanfun.com/beanfun_block/bflogin/return.aspx");
                        req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/");
                        form = new Dictionary<string, string>
                        {
                            { "SessionKey", account.skey },
                            { "AuthKey", akey },
                            { "ServiceCode", "" },
                            { "ServiceRegion", "" },
                            { "ServiceAccountSN", "0" }
                        };
                        req.Content = new FormUrlEncodedContent(form);
                        try { res = client.SendAsync(req).Result; }
                        catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(bfWebToken)" }; }
                        if (!res.IsSuccessStatusCode)
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(bfWebToken)" };
                        }
                        if (cookies.GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"] == default(Cookie))
                        {
                            account = default;
                            return new LoginResult { Status = LoginStatus.Failed, Message = "找不到 bfWebToken。" };
                        }
                        string bfwebtoken = cookies.GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"].Value;
                        #endregion

                        account.webtoken = bfwebtoken;
                        return new LoginResult { Status = LoginStatus.Success };
                }
                account = default;
                return new LoginResult { Status = LoginStatus.Failed, Message = "不是預期的資料。(檢查 QRCode)" };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /**
         * <summary>
         * <para>Remove internal beanfun account.</para>
         * 
         * After 'Login()' returns status 'RequireAppAuthentication' or 'GetQRCode()' is called, an incomplete beanfun account is created, but hasn't been logged in.
         * If 'CheckAppAuthentication()' or 'CheckQRCode()' returns a non-fatal status: 'ConnectionLost' or 'RequireAppAuthentication'/'RequireQRCode'(Pending),
         * or is not called at all, you need to call this function before logging in again.
         * </summary>
         */
        public void LocalLogout()
        {
            rwLock.EnterWriteLock();
            try
            {
                account = default;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /**
         * <summary>Log out the beanfun account.</summary>
         * <returns>
         * true: The process is finished completely.
         * false: Some steps of the process are not successful.
         * </returns>
         */
        public bool Logout()
        {
            if (account == default(BeanfunAccount))
                return false;

            rwLock.EnterWriteLock();
            try
            {
                bool result = true;
                HttpRequestMessage req;

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/generic_handlers/remove_bflogin_session.ashx?d={GetDateTime(DateTimeType.Logout)}");
                req.Headers.Referrer = new Uri("https://tw.beanfun.com/beanfun_block/bflogin/logout_confirm.aspx?service=999999_T0");
                try { res = client.SendAsync(req).Result; }
                catch { result = false; }
                if (res != default && !res.IsSuccessStatusCode)
                    result = false;

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/TW/data_provider/remove_bflogin_session.ashx?d={GetDateTime(DateTimeType.Logout)}");
                req.Headers.Referrer = new Uri("https://tw.beanfun.com/beanfun_block/bflogin/logout_confirm.aspx?service=999999_T0");
                try { res = client.SendAsync(req).Result; }
                catch { result = false; }
                if (res != default && !res.IsSuccessStatusCode)
                    result = false;

                req = new HttpRequestMessage(HttpMethod.Get, "https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0");
                req.Headers.Referrer = new Uri("https://tw.beanfun.com/");
                try { res = client.SendAsync(req).Result; }
                catch { result = false; }
                if (res != default && !res.IsSuccessStatusCode)
                    result = false;

                req = new HttpRequestMessage(HttpMethod.Get, "https://tw.newlogin.beanfun.com/generic_handlers/erase_token.ashx");
                req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0");
                try { res = client.SendAsync(req).Result; }
                catch { result = false; }
                if (res != default && !res.IsSuccessStatusCode)
                    result = false;

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0&_={GetDateTime(DateTimeType.UNIX)}");
                req.Headers.Referrer = new Uri("https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0");
                try { res = client.SendAsync(req).Result; }
                catch { result = false; }
                if (res != default && !res.IsSuccessStatusCode)
                    result = false;

                if (result)
                    account = default;

                return result;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private class PingResponse
        {
            public int ResultCode { get; set; }
            public string ResultDesc { get; set; }
            public string MainAccountID { get; set; }
        }

        /**
         * <summary>Check the login status of beanfun account.</summary>
         * <returns>
         * Possible statuses (message):
         *   Failed (error description):
         *     1. This beanfun account is currently not logged in.
         *     2. Requests to beanfun returns a non-success http code.
         *     3. Returned messages don't have the expected contents.
         *   ConnectionLost (description):
         *     Any connection failed.
         *   LoginFirst (none):
         *     The account is logged out.
         *   Success (username of this beanfun account): 
         *     The account is still logged in.
         * </returns>
         */
        public LoginResult Ping()
        {
            if (account == default(BeanfunAccount))
                return new LoginResult { Status = LoginStatus.Failed, Message = "登入狀態不允許此操作。(Ping)" };

            rwLock.EnterWriteLock();
            try
            {
                HttpRequestMessage req;
                Match match;

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/beanfun_block/generic_handlers/echo_token.ashx?webtoken=1&noCacheIE={GetDateTime(DateTimeType.Ping)}");
                req.Headers.Referrer = new Uri("https://tw.beanfun.com/");
                try { res = client.SendAsync(req).Result; }
                catch { return new LoginResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(ping)" }; }
                if (!res.IsSuccessStatusCode)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(ping)" };
                match = Regex.Match(res.Content.ReadAsStringAsync().Result, @"(\{[^\{\}]*\})");
                if (match == Match.Empty)
                    return new LoginResult { Status = LoginStatus.Failed, Message = "不是預期的結果。(ping)" };
                PingResponse result = JsonConvert.DeserializeObject<PingResponse>(match.Groups[1].Value);
                return result.ResultCode switch
                {
                    0 => new LoginResult { Status = LoginStatus.LoginFirst },
                    1 => new LoginResult { Status = LoginStatus.Success, Message = result.MainAccountID },
                    _ => new LoginResult { Status = LoginStatus.Failed, Message = "不是預期的結果。(ping)" },
                };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
    }
}
