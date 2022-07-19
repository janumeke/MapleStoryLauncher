using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Web;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        private TransactionResult Step_FrontPage()
        {
            string url = "https://tw.beanfun.com/";
            string referrer = "https://tw.beanfun.com/";
            res = client.HttpGet(url, referrer, new BeanfunClient.HandlerConfiguration
            {
                saveResponseUrlAsReferrer = true
            });
            if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("首頁", res.Status) };

            TransactionResult loader = Step_Loader();
            if (loader.Status != TransactionResultStatus.Success)
                return loader;

            return new TransactionResult { Status = TransactionResultStatus.Success };
        }

        private TransactionResult Step_GetSkey()
        {
            string url, referrer;
            Match match;

            TransactionResult frontpage = Step_FrontPage();
            if (frontpage.Status != TransactionResultStatus.Success)
                return frontpage;

            #region skey
            url = $"https://tw.beanfun.com/beanfun_block/bflogin/default.aspx?service=999999_T0&dt={GetDateTime(DateTimeType.Skey)}&url=https://tw.beanfun.com/";
            res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
            {
                setReferrer = true,
                saveResponseUrlAsReferrer = true
            });
            if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("skey", res.Status) };

            match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"strSessionKey = ""([^""]*)""");
            if (match == Match.Empty)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 skey。" };
            string skey = match.Groups[1].Value;
            #endregion

            #region Params
            url = "https://tw.beanfun.com/beanfun_block/scripts/BeanFunBlockParams.ashx";
            referrer = "https://tw.newlogin.beanfun.com/";
            res = client.HttpGet(url, referrer);
            if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("Params", res.Status) };
            #endregion

            #region checking_step2
            url = $"https://tw.newlogin.beanfun.com/checkin_step2.aspx?skey={skey}&display_mode=2&_={GetDateTime(DateTimeType.UNIX)}";
            res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
            {
                setReferrer = true
            });
            if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("checking_step2", res.Status) };
            #endregion

            return new TransactionResult { Status = TransactionResultStatus.Success, Message = skey };
        }

        private TransactionResult Step_Final(string writeUrl, string skey, string akey)
        {
            string url, referrer;

            #region writecookie
            url = $"{writeUrl}&_={GetDateTime(DateTimeType.UNIX)}";
            referrer = "https://tw.newlogin.beanfun.com/";
            res = client.HttpGet(url, referrer);
            if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("writecookie", res.Status) };
            #endregion

            #region return
            url = "https://tw.beanfun.com/beanfun_block/bflogin/return.aspx";
            referrer = "https://tw.newlogin.beanfun.com/";
            res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "SessionKey", skey },
                    { "AuthKey", akey },
                    { "ServiceCode", "" },
                    { "ServiceRegion", "" },
                    { "ServiceAccountSN", "0" }
                }, referrer, new BeanfunClient.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true
                });
            if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("return", res.Status) };
            #endregion

            TransactionResult loader = Step_Loader();
            if (loader.Status != TransactionResultStatus.Success)
                return loader;

            return new TransactionResult { Status = TransactionResultStatus.Success };
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
        public TransactionResult Login(string username, string password)
        {
            if (account != default(BeanfunAccount))
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(一般登入)" };

            rwLock.EnterWriteLock();
            try
            {
                string skey;
                TransactionResult getskey = Step_GetSkey();
                if (getskey.Status != TransactionResultStatus.Success)
                    return getskey;
                else
                    skey = getskey.Message;

                string url;
                string body;
                Match match;

                #region Login page(Regular)
                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer= true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("登入頁面(一般)", res.Status) };
                #endregion

                #region loginform
                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2&_={GetDateTime(DateTimeType.UNIX)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("loginform", res.Status) };
                #endregion

                #region Get viewState, viewStateGenerator and eventValidation
                url = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={skey}&clientID=undefined";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("viewState, viewStateGenerator, eventValidation", res.Status) };

                body = res.Message.Content.ReadAsStringAsync().Result;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 viewState。" };
                string viewState = match.Groups[1].Value;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__VIEWSTATEGENERATOR"" id=""__VIEWSTATEGENERATOR"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 viewStateGenerator。" };
                string viewStateGenerator = match.Groups[1].Value;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__EVENTVALIDATION"" id=""__EVENTVALIDATION"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 eventValidation。" };
                string eventValidation = match.Groups[1].Value;

                MatchCollection matches = Regex.Matches(body, @"<script src=""\/WebResource\.axd\?([^""]*)"" type=""text\/javascript""><\/script>");
                foreach(Match m in matches)
                {
                    url = $"https://tw.newlogin.beanfun.com/WebResource.axd?{HttpUtility.HtmlDecode(m.Groups[1].Value)}";
                    res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                    {
                        setReferrer = true
                    });
                    if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                        return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("WebResource", res.Status) };
                }
                #endregion

                #region Get bfWebToken, writeUrl and akey
                url = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={skey}&clientID=undefined";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "__EVENTTARGET", "" },
                    { "__EVENTARGUMENT", "" },
                    { "__VIEWSTATE", viewState },
                    { "__VIEWSTATEGENERATOR", viewStateGenerator },
                    { "__EVENTVALIDATION", eventValidation },
                    { "t_AccountID", username },
                    { "t_Password", password },
                    { "btn_login", "登入" }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("bfWebToken, writeUrl, akey", res.Status) };

                body = res.Message.Content.ReadAsStringAsync().Result;
                //Check for errors
                if(res.Message.RequestMessage.RequestUri.ToString() == url) //No redirection
                {
                    //Requiring app authenticaiotn
                    match = Regex.Match(body, @"pollRequest\(""bfAPPAutoLogin\.ashx"",""([^""]*)"",""[^""]*""\);");
                    if (match != Match.Empty)
                    {
                        account = new BeanfunAccount
                        {
                            skey = skey,
                            lt = match.Groups[1].Value
                        };
                        return new TransactionResult { Status = TransactionResultStatus.RequireAppAuthentication };
                    }
                    //Denied
                    match = Regex.Match(body, @"<div id=""pnlMsg"">.*<script type=""text\/javascript"">\$\(function\(\)\{MsgBox\.Show\('([^']*)'\);\}\);<\/script>.*<\/div>", RegexOptions.Singleline);
                    if (match != Match.Empty)
                    {
                        client.ClearCookies();
                        return new TransactionResult { Status = TransactionResultStatus.Denied, Message = match.Groups[1].Value.Replace("<br />", "\n") };
                    }
                }
                //Retrieve info
                if (client.GetCookies().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"] == default)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 bfWebToken。" };
                string bfwebtoken = client.GetCookies().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"].Value;
                match = Regex.Match(body, @"var strWriteUrl = ""([^""]*)"";");
                if (match == Match.Empty)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 writeUrl。" };
                string writeUrl = match.Groups[1].Value;
                match = Regex.Match(body, @"AuthKey.value = ""([^""]*)"";");
                if (match == Match.Empty)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 akey。" };
                string akey = match.Groups[1].Value;
                #endregion

                TransactionResult final = Step_Final(writeUrl, skey, akey);
                if (final.Status != TransactionResultStatus.Success)
                    return final;

                account = new BeanfunAccount
                {
                    webToken = bfwebtoken
                };
                return new TransactionResult { Status = TransactionResultStatus.Success };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private class CheckAppAuthResponse
        {
            public int IntResult { get; set; }
            public string StrReslut { get; set; } //this name is decided by the protocol
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
        public TransactionResult CheckAppAuthentication()
        {
            if (account == default(BeanfunAccount))
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(檢查 App 授權)" };

            rwLock.EnterWriteLock();
            try
            {
                string url;
                string body;
                Match match;

                url = "https://tw.newlogin.beanfun.com/login/bfAPPAutoLogin.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "LT", account.lt }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status == BeanfunClient.HttpResponseStatus.Unsuccessful)
                    account = default;
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("檢查 App 授權", res.Status) };

                CheckAppAuthResponse result = JsonConvert.DeserializeObject<CheckAppAuthResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (result == null)
                {
                    account = default;
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 App 授權)" };
                }
                switch (result.IntResult)
                {
                    case -3:
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Denied, Message = "登入要求已被 App 拒絕。" };
                    case -1:
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Expired, Message = "登入階段過期。(檢查 App 授權)" };
                    case 1:
                        return new TransactionResult { Status = TransactionResultStatus.RequireAppAuthentication };
                    case 2:
                        #region Get bfWebToken, writeUrl and akey
                        url = $"https://tw.newlogin.beanfun.com/login/{result.StrReslut}";
                        res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                        {
                            setReferrer = true,
                            saveResponseUrlAsReferrer = true
                        });
                        if (res.Status == BeanfunClient.HttpResponseStatus.Unsuccessful)
                            account = default;
                        if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                            return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("bfWebToken, writeUrl, akey", res.Status) };

                        if (client.GetCookies().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"] == default)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 bfWebToken。" };
                        }
                        string bfwebtoken = client.GetCookies().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"].Value;
                        body = res.Message.Content.ReadAsStringAsync().Result;
                        match = Regex.Match(body, @"var strWriteUrl = ""([^""]*)"";");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 writeUrl。" };
                        }
                        string writeUrl = match.Groups[1].Value;
                        match = Regex.Match(body, @"AuthKey.value = ""([^""]*)"";");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 akey。" };
                        }
                        string akey = match.Groups[1].Value;
                        #endregion

                        TransactionResult final = Step_Final(writeUrl, account.skey, akey);
                        if (final.Status != TransactionResultStatus.Success)
                            return final;

                        account.webToken = bfwebtoken;
                        return new TransactionResult { Status = TransactionResultStatus.Success };
                }
                account = default;
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 App 授權)" };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public class QRCodeResult : TransactionResult
        {
            public Bitmap Picture { get; set; }
        }

        private class GetQRCodeResponse
        {
            public int intResult { get; set; }
            public string strResult { get; set; }
            public string strEncryptData { get; set; }
            public string strEncryptBCDOData { get; set; }
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
        public QRCodeResult GetQRCode()
        {
            if (account != default(BeanfunAccount))
                return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(取得 QRCode)" };

            rwLock.EnterWriteLock();
            try
            {
                string url;
                string body;
                Match match;

                string skey;
                TransactionResult getskey = Step_GetSkey();
                if (getskey.Status != TransactionResultStatus.Success)
                    return new QRCodeResult { Status = getskey.Status, Message = getskey.Message };
                else
                    skey = getskey.Message;

                #region Get viewState, viewStateGenerator
                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("viewState, viewStateGenerator", res.Status) };

                body = res.Message.Content.ReadAsStringAsync().Result;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "找不到 viewState。" };
                string viewState = match.Groups[1].Value;
                match = Regex.Match(body, @"<input type=""hidden"" name=""__VIEWSTATEGENERATOR"" id=""__VIEWSTATEGENERATOR"" value=""([^""]*)"" />");
                if (match == Match.Empty)
                    return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "找不到 viewStateGenerator。" };
                string viewStateGenerator = match.Groups[1].Value;
                #endregion

                #region loginform
                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2&_={GetDateTime(DateTimeType.UNIX)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("loginform", res.Status) };
                #endregion

                #region Login page(Regular)
                url = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={skey}&clientID=undefined";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("登入頁面(一般)", res.Status) };

                MatchCollection matches = Regex.Matches(body, @"<script src=""\/WebResource\.axd\?([^""]*)"" type=""text\/javascript""><\/script>");
                foreach (Match m in matches)
                {
                    url = $"https://tw.newlogin.beanfun.com/WebResource.axd?{HttpUtility.HtmlDecode(m.Groups[1].Value)}";
                    res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                    {
                        setReferrer = true
                    });
                    if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                        return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("WebResource", res.Status) };
                }
                #endregion

                #region Login page(QRCode)
                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "__EVENTTARGET", "__Page" },
                    { "__EVENTARGUMENT", "SwitchToLocalAreaQR" },
                    { "__VIEWSTATE", viewState },
                    { "__VIEWSTATEGENERATOR", viewStateGenerator },
                    { "ddlAuthType", "1" }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer= true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("登入頁面(QRCode)", res.Status) };
                #endregion

                #region loginform(qr)
                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={skey}&display_mode=2&region=qr&_={GetDateTime(DateTimeType.UNIX)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("loginform(qr)", res.Status) };
                #endregion

                #region baseUrl
                url = $"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={skey}&clientID=undefined";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("baseUrl", res.Status) };

                match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"\$\(""#theQrCodeImg""\)\.attr\(""src"", ""([^""]*)""[^\)]*\);");
                if (match == Match.Empty)
                    return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "找不到 baseUrl。" };
                string baseUrl = $"https://tw.newlogin.beanfun.com/login/{match.Groups[1].Value}";
                #endregion

                #region encryptData
                url = $"https://tw.newlogin.beanfun.com/generic_handlers/get_qrcodeData.ashx?skey={skey}&startGame=&clientID=";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("encryptData", res.Status) };

                GetQRCodeResponse result = JsonConvert.DeserializeObject<GetQRCodeResponse>(res.Message.Content.ReadAsStringAsync().Result);
                string encryptData;
                if (result == null || result.strEncryptData == default)
                    return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "找不到 encryptData。" };
                else
                    encryptData = result.strEncryptData;
                #endregion

                #region QRCode picture
                url = $"{baseUrl}{encryptData}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer= true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new QRCodeResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("QRcode 圖片", res.Status) };

                Bitmap qrPic = new(res.Message.Content.ReadAsStreamAsync().Result);
                #endregion

                account = new BeanfunAccount
                {
                    skey = skey,
                    encryptData = encryptData
                };
                return new QRCodeResult { Status = TransactionResultStatus.Success, Picture = qrPic };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private class CheckQRCodeResponse
        {
            public int Result { get; set; }
            public string ResultMessage { get; set; }
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
        public TransactionResult CheckQRCode()
        {
            if (account == default(BeanfunAccount))
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(檢查 QRCode)" };

            rwLock.EnterWriteLock();
            try
            {
                string url;
                string body;
                Match match;

                url = "https://tw.newlogin.beanfun.com/generic_handlers/CheckLoginStatus.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "status", account.encryptData }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status == BeanfunClient.HttpResponseStatus.Unsuccessful)
                    account = default;
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("檢查 QRCode", res.Status) };
                
                CheckQRCodeResponse result = JsonConvert.DeserializeObject<CheckQRCodeResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (result == null)
                {
                    account = default;
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 QRCode)" };
                }
                switch (result.Result)
                {
                    case 0:
                        if (result.ResultMessage == "Failed")
                            return new TransactionResult { Status = TransactionResultStatus.RequireQRCode };
                        if (result.ResultMessage == "Token Expired")
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Expired, Message = "QRCode 過期。(檢查 QRCode)" };
                        }
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 QRCode)" };
                    case 1:
                        #region final_step
                        url = $"https://tw.newlogin.beanfun.com/login/qr_step2.aspx?skey={account.skey}";
                        res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                        {
                            setReferrer = true,
                            saveResponseUrlAsReferrer = true
                        });
                        if (res.Status == BeanfunClient.HttpResponseStatus.Unsuccessful)
                            account = default;
                        if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                            return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("final_step", res.Status) };

                        match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"RedirectPage\("""",""([^""]*)""\)");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 final_step。" };
                        }
                        string final_step = match.Groups[1].Value;
                        #endregion

                        #region Get bfWebToken, writeUrl and akey
                        url = $"https://tw.newlogin.beanfun.com/login/{final_step}";
                        res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                        {
                            setReferrer = true,
                            saveResponseUrlAsReferrer= true
                        });
                        if (res.Status == BeanfunClient.HttpResponseStatus.Unsuccessful)
                            account = default;
                        if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                            return new TransactionResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("bfWebToken, writeUrl, akey", res.Status) };

                        if (client.GetCookies().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"] == default)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 bfWebToken。" };
                        }
                        string bfwebtoken = client.GetCookies().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"].Value;
                        body = res.Message.Content.ReadAsStringAsync().Result;
                        match = Regex.Match(body, @"var strWriteUrl = ""([^""]*)"";");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 writeUrl。" };
                        }
                        string writeUrl = match.Groups[1].Value;
                        match = Regex.Match(body, @"AuthKey.value = ""([^""]*)"";");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 akey。" };
                        }
                        string akey = match.Groups[1].Value;
                        #endregion

                        TransactionResult final = Step_Final(writeUrl, account.skey, akey);
                        if (final.Status != TransactionResultStatus.Success)
                            return final;

                        account.webToken = bfwebtoken;
                        return new TransactionResult { Status = TransactionResultStatus.Success };
                }
                account = default;
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 QRCode)" };
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
         * true: The process finished completely.
         * false:
         *   1. The account is not logged in.
         *   2. Some steps of the process are not successful.
         * </returns>
         */
        public bool Logout()
        {
            if (account == default(BeanfunAccount))
                return false;

            rwLock.EnterWriteLock();
            try
            {
                bool success = true;
                string url, referrer;

                url = "https://tw.beanfun.com/beanfun_block/bflogin/logout_confirm.aspx?service=999999_T0";
                referrer = "https://tw.beanfun.com/game_zones";
                res = client.HttpGet(url, referrer, new BeanfunClient.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true
                });
                if(res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    success = false;

                url = $"https://tw.beanfun.com/generic_handlers/remove_bflogin_session.ashx?d={GetDateTime(DateTimeType.Logout)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    success = false;

                url = $"https://tw.beanfun.com/TW/data_provider/remove_bflogin_session.ashx?d={GetDateTime(DateTimeType.Logout)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    success = false;

                url = "https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0";
                referrer = "https://tw.beanfun.com/";
                res = client.HttpGet(url, referrer, new BeanfunClient.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    success = false;

                url = "https://tw.beanfun.com/beanfun_block/scripts/BeanFunBlockParams.ashx";
                referrer = "https://tw.newlogin.beanfun.com/";
                res = client.HttpGet(url, referrer);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    success = false;

                url = "https://tw.newlogin.beanfun.com/generic_handlers/erase_token.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "web_token", "1" }
                }, referrer, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    success = false;
                else
                {
                    GeneralResponse result = JsonConvert.DeserializeObject<GeneralResponse>(res.Message.Content.ReadAsStringAsync().Result);
                    if (result == null ||
                        (result.intResult != 0 && result.intResult != 1 && result.intResult != 2))
                        success = false;
                }

                url = $"https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0&_={GetDateTime(DateTimeType.UNIX)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    success = false;

                if (success)
                    account = default;

                return success;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
    }
}
