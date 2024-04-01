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
        /**
         * <summary>Get the link to a page where you get response from reCAPTCHA for login.</summary>
         * <remarks>You also need the cookies to make the link work.</remarks>
         * <param name="required">
         *     <para>Asks to or not to check the requirement of reChaptcha when passed in</para>
         *     <para>Whether reCaptcha is required when it's asked and the link to it is returned, undefined otherwise</para>
         * </param>
         * <returns>
         * Possible statuses (message):<list type="bullet">
         *     <item><description>Failed (error description):<list type="number">
         *         <item><description>This account is currently logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description):
         *         <para>Any connection failed or was cancelled.</para>
         *     </description></item>
         *     <item><description>Denied (description):
         *         <para>The IP is temporarily blocked.</para>
         *     </description></item>
         *     <item><description>Success (the link):
         *         <para>The link to reCAPTCHA is acquired.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public TransactionResult GetReCaptcha(ref bool required)
        {
            if (account?.webToken != null)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(reCAPTCHA)" };

            rwLock.EnterWriteLock();
            try
            {
                TransactionResult result;

                result = Step_GameZone();
                if (result.Status != TransactionResultStatus.Success)
                    return result;

                account = new();
                result = Step_LoginPage();
                if (result.Status != TransactionResultStatus.Success)
                {
                    account = default;
                    return result;
                }

                if (required)
                {
                    HtmlAgilityPack.HtmlDocument doc = new();
                    doc.LoadHtml(res.Message.Content.ReadAsStringAsync().Result);
                    if (doc.DocumentNode.SelectSingleNode("//*[@class='g-recaptcha']") == null)
                        required = false;
                    else
                        required = true;
                }

                return new TransactionResult { Status = TransactionResultStatus.Success, Message = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={account.skey}&clientID=undefined" };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /**
         * <summary>Login to beanfun by an account, a password and a reCAPTCHA response (if required).</summary>
         * <remarks>You need to call <c>GetReCaptcha()</c> whether reCAPTCHA is required or not.</remarks>
         * <returns>
         * Possible statuses (message):<list type="bullet">
         *     <item><description>Failed (error description):<list type="number">
         *         <item><description><c>GetReCaptcha()</c> hasn't been called.</description></item>
         *         <item><description>This account is currently logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description):
         *         <para>Any connection failed or was cancelled.</para>
         *     </description></item>
         *     <item><description>RequireAppAuthentication (none):
         *         <para>App authentication is required by beanfun.</para>
         *     </description></item>
         *     <item><description>Denied (reason given by beanfun):
         *         <para>Beanfun rejected this login for any reason.</para>
         *     </description></item>
         *     <item><description>Success (none):
         *         <para>The account is logged in.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public TransactionResult Login(string username, string password, string reCaptchaResponse = default)
        {
            if (account == default || account.webToken != default)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(一般登入)" };

            rwLock.EnterWriteLock();
            try
            {
                string url;
                string body;
                Match match;

                #region Get bfWebToken and akey
                url = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={account.skey}&clientID=undefined";
                Dictionary<string, string> form = new()
                {
                    { "__EVENTTARGET", "" },
                    { "__EVENTARGUMENT", "" },
                    { "__VIEWSTATE", account.VIEWSTATE },
                    { "__VIEWSTATEGENERATOR", account.VIEWSTATEGENERATOR },
                    { "__EVENTVALIDATION", account.EVENTVALIDATION },
                    { "t_AccountID", username },
                    { "t_Password", password },
                    { "btn_login", "登入" }
                };
                if (reCaptchaResponse != default)
                    form.Add("g-recaptcha-response", reCaptchaResponse);
                res = client.HttpPost(url, form, null, new Client.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                {
                    account = default;
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("bfWebToken, akey", res.Status) };
                }

                string requestUrl = res.Message.RequestMessage.RequestUri.ToString();
                body = res.Message.Content.ReadAsStringAsync().Result;

                //Check for errors
                if (requestUrl == url) //No redirection
                {
                    //Requiring app authenticaiotn
                    match = Regex.Match(body, @"pollRequest\(""bfAPPAutoLogin\.ashx"",""([^""]*)""");
                    if (match != Match.Empty)
                    {
                        account.lt = match.Groups[1].Value;
                        return new TransactionResult { Status = TransactionResultStatus.RequireAppAuthentication };
                    }
                    //Denied
                    match = Regex.Match(body, @"<div id=""pnlMsg"">.*<script type=""text\/javascript"">\$\(function\(\)\{MsgBox\.Show\('([^']*)'[^<]*<\/script>.*<\/div>", RegexOptions.Singleline);
                    if (match != Match.Empty)
                    {
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Denied, Message = match.Groups[1].Value.Replace("<br />", "\n") };
                    }
                }
                else if (requestUrl.Contains("https://tw.newlogin.beanfun.com/login/msg.aspx")) //Expired or error
                {
                    match = Regex.Match(body, @"<p id=""divMsg"">([^<]*)<\/p>");
                    if (match != Match.Empty)
                    {
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Failed, Message = match.Groups[1].Value.Replace("<br />", "\n") };
                    }
                }

                string bfwebtoken = client.GetCookieContainer().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"]?.Value;
                if (bfwebtoken == null)
                {
                    account = default;
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 bfWebToken。" };
                }
                account.webToken = bfwebtoken;

                match = Regex.Match(requestUrl, @"akey=([^&]*)");
                if (match == Match.Empty)
                {
                    account = default;
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 akey。" };
                }
                string akey = match.Groups[1].Value;
                #endregion

                TransactionResult result = Step_Return(akey);
                if (result.Status != TransactionResultStatus.Success)
                {
                    account = default;
                    return result;
                }

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
         * Possible statuses (message):<list type="bullet">
         *     <item><description>Failed (error description):<list type="number">
         *         <item><description><c>Login()</c> hasn't been called.</description></item>
         *         <item><description>This account is currently logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description):
         *         <para>Any connection failed or was cancelled. You don't need to call <c>Login()</c> again.</para>
         *     </description></item>
         *     <item><description>RequireAppAuthentication (none):
         *         <para>App authentication is pending. You don't need to call <c>Login()</c> again.</para>
         *     </description></item>
         *     <item><description>Expired (description):
         *         <para>The login session has expired.</para>
         *     </description></item>
         *     <item><description>Denied (reason given by beanfun):
         *         <para>The App denied this login.</para>
         *     </description></item>
         *     <item><description>Success (none):
         *         <para>The account is logged in.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public TransactionResult CheckAppAuthentication()
        {
            if (account?.lt == null || account.webToken != default)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(檢查 App 授權)" };

            rwLock.EnterWriteLock();
            try
            {
                string url;
                Match match;

                url = "https://tw.newlogin.beanfun.com/login/bfAPPAutoLogin.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "LT", account.lt }
                }, null, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status == Client.HttpResponseStatus.Unsuccessful)
                    account = default;
                if (res.Status != Client.HttpResponseStatus.Successful) //Unsuccessful or Disconnected
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("檢查 App 授權", res.Status) };

                CheckAppAuthResponse authResult = JsonConvert.DeserializeObject<CheckAppAuthResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (authResult == null)
                {
                    account = default;
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 App 授權)" };
                }
                switch (authResult.IntResult)
                {
                    case -3:
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Denied, Message = "登入要求已被 App 拒絕。" };
                    case 1:
                        return new TransactionResult { Status = TransactionResultStatus.RequireAppAuthentication };
                    case 2:
                        #region Get bfWebToken and akey
                        url = $"https://tw.newlogin.beanfun.com/login/{authResult.StrReslut}";
                        res = client.HttpGet(url, null, new Client.HandlerConfiguration
                        {
                            setReferrer = true,
                            saveResponseUrlAsReferrer = true
                        });
                        if (res.Status == Client.HttpResponseStatus.Unsuccessful)
                            account = default;
                        if (res.Status != Client.HttpResponseStatus.Successful) //Unsuccessful or Disconnected
                            return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("bfWebToken, akey", res.Status) };

                        string bfWebToken = client.GetCookieContainer().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"]?.Value;
                        if (bfWebToken == null)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 bfWebToken。" };
                        }
                        account.webToken = bfWebToken;

                        match = Regex.Match(res.Message.RequestMessage.RequestUri.ToString(), @"akey=([^&]*)");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 akey。" };
                        }
                        string akey = match.Groups[1].Value;
                        #endregion

                        TransactionResult result = Step_Return(akey);
                        if (result.Status != TransactionResultStatus.Success)
                        {
                            account = default;
                            return result;
                        }

                        return new TransactionResult { Status = TransactionResultStatus.Success };
                    default:
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Expired, Message = "登入階段過期或錯誤。(檢查 App 授權)" };
                }
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
         * Possible statuses (message, picture):<list type="bullet">
         *     <item><description>Failed (error description, none):<list type="number">
         *         <item><description>This account is currently logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description, none):
         *         <para>Any connection failed or was cancelled.</para>
         *     </description></item>
         *     <item><description>Success (none, QRCode): 
         *         <para>QRCode was gotten.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public QRCodeResult GetQRCode()
        {
            if (account?.webToken != null)
                return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(取得 QRCode)" };

            rwLock.EnterWriteLock();
            try
            {
                TransactionResult result;
                string url;
                Match match;

                result = Step_GameZone();
                if (result.Status != TransactionResultStatus.Success)
                    return new QRCodeResult { Status = result.Status, Message = result.Message };

                account = new();
                result = Step_LoginPage();
                if (result.Status != TransactionResultStatus.Success)
                {
                    account = default;
                    return new QRCodeResult { Status = result.Status, Message = result.Message };
                }

                #region Switch to QRCode
                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={account.skey}&display_mode=2";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "__EVENTTARGET", "__Page" },
                    { "__EVENTARGUMENT", "SwitchToLocalAreaQR" },
                    { "__VIEWSTATE", account.VIEWSTATE },
                    { "__VIEWSTATEGENERATOR", account.VIEWSTATEGENERATOR },
                    { "ddlAuthType", "1" }
                }, null, new Client.HandlerConfiguration
                {
                    setReferrer= true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                {
                    account = default;
                    return new QRCodeResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("登入頁面(QRCode)", res.Status) };
                }

                result = Step_GetIframes("QRCode page");
                if (result.Status != TransactionResultStatus.Success)
                {
                    account = default;
                    return new QRCodeResult { Status = result.Status, Message = result.Message };
                }

                result = Step_Loading();
                if (result.Status != TransactionResultStatus.Success)
                {
                    account = default;
                    return new QRCodeResult { Status = result.Status, Message = result.Message };
                }

                url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={account.skey}&display_mode=2&region=qr&_={GetTimestamp(TimestampType.UNIX)}";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                {
                    account = default;
                    return new QRCodeResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("loginform(QRCode)", res.Status) };
                }
                #endregion

                #region QRCode
                url = $"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={account.skey}&clientID=undefined";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                {
                    account = default;
                    return new QRCodeResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("qr_form", res.Status) };
                }

                match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"\$\(""#theQrCodeImg""\)\.attr\(""src"", ""([^""]*)""");
                if (match == Match.Empty)
                {
                    account = default;
                    return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "找不到 QRCode。" };
                }
                string baseUrl = $"https://tw.newlogin.beanfun.com/login/{match.Groups[1].Value}";

                url = $"https://tw.newlogin.beanfun.com/generic_handlers/get_qrcodeData.ashx?skey={account.skey}&startGame=&clientID=";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                {
                    account = default;
                    return new QRCodeResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("encryptData", res.Status) };
                }

                GetQRCodeResponse codeResult = JsonConvert.DeserializeObject<GetQRCodeResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (codeResult?.strEncryptData == default)
                {
                    account = default;
                    return new QRCodeResult { Status = TransactionResultStatus.Failed, Message = "找不到 encryptData。" };
                }
                account.encryptData = codeResult.strEncryptData;

                url = $"{baseUrl}{account.encryptData}";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    setReferrer= true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                {
                    account = default;
                    return new QRCodeResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("QRcode 圖片", res.Status) };
                }
                Bitmap qrPic = new(res.Message.Content.ReadAsStreamAsync().Result);
                #endregion

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
         * Possible statuses (message):<list type="bullet">
         *     <item><description>Failed (error description):<list type="number">
         *         <item><description><c>GetQRCode()</c> hasn't been called.</description></item>
         *         <item><description>This account is currently logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description):
         *         <para>Any connection failed or was cancelled. You don't need to call <c>GetQRCode()</c> again.</para>
         *     </description></item>
         *     <item><description>RequireQRCode (none):
         *         <para>QRCode is pending for login. You don't need to call <c>GetQRCode()</c> again.</para>
         *     </description></item>
         *     <item><description>Expired (description):
         *         <para>The QRcode has expired.</para>
         *     </description></item>
         *     <item><description>Success (none):
         *         <para>The account is logged in.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public TransactionResult CheckQRCode()
        {
            if (account?.encryptData == null || account.webToken != default)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(檢查 QRCode)" };

            rwLock.EnterWriteLock();
            try
            {
                string url;
                Match match;

                url = "https://tw.newlogin.beanfun.com/generic_handlers/CheckLoginStatus.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "status", account.encryptData }
                }, null, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status == Client.HttpResponseStatus.Unsuccessful)
                    account = default;
                if (res.Status != Client.HttpResponseStatus.Successful) //Unsuccessful or Disconnected
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("檢查 QRCode", res.Status) };
                
                CheckQRCodeResponse codeResult = JsonConvert.DeserializeObject<CheckQRCodeResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (codeResult == null)
                {
                    account = default;
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 QRCode)" };
                }
                switch (codeResult.Result)
                {
                    case 0:
                        if (codeResult.ResultMessage == "Failed")
                            return new TransactionResult { Status = TransactionResultStatus.RequireQRCode };
                        if (codeResult.ResultMessage == "Token Expired")
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Expired, Message = "QRCode 過期。(檢查 QRCode)" };
                        }
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(檢查 QRCode)" };
                    case 1:
                        url = $"https://tw.newlogin.beanfun.com/login/qr_step2.aspx?skey={account.skey}";
                        res = client.HttpGet(url, null, new Client.HandlerConfiguration
                        {
                            setReferrer = true,
                            saveResponseUrlAsReferrer = true
                        });
                        if (res.Status == Client.HttpResponseStatus.Unsuccessful)
                            account = default;
                        if (res.Status != Client.HttpResponseStatus.Successful) //Unsuccessful or Disconnected
                            return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("final_step", res.Status) };

                        match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"RedirectPage\("""",""([^""]*)""");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 final_step。" };
                        }
                        string final_step = match.Groups[1].Value;

                        #region Get bfWebToken and akey
                        url = $"https://tw.newlogin.beanfun.com/login/{final_step}";
                        res = client.HttpGet(url, null, new Client.HandlerConfiguration
                        {
                            setReferrer = true,
                            saveResponseUrlAsReferrer= true
                        });
                        if (res.Status == Client.HttpResponseStatus.Unsuccessful)
                            account = default;
                        if (res.Status != Client.HttpResponseStatus.Successful) //Unsuccessful or Disconnected
                            return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("bfWebToken, akey", res.Status) };

                        string bfWebToken = client.GetCookieContainer().GetCookies(new Uri("https://tw.beanfun.com/"))["bfWebToken"]?.Value;
                        if (bfWebToken == null)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 bfWebToken。" };
                        }
                        account.webToken = bfWebToken;

                        match = Regex.Match(res.Message.RequestMessage.RequestUri.ToString(), @"akey=([^&]*)");
                        if (match == Match.Empty)
                        {
                            account = default;
                            return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 akey。" };
                        }
                        string akey = match.Groups[1].Value;
                        #endregion

                        TransactionResult result = Step_Return(akey);
                        if(result.Status != TransactionResultStatus.Success)
                        {
                            account = default;
                            return result;
                        }

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
         * <summary>Log out the beanfun account.</summary>
         * <returns><list type="bullet">
         *     <item>
         *         <description>true:
         *             <para>The process finished completely.</para>
         *         </description>
         *     </item>
         *     <item>
         *         <description>false<list type="number">
         *             <item><description>The account is not logged in.</description></item>
         *             <item><description>Some steps of the process are not successful.</description></item>
         *         </list></description>
         *     </item>
         * </list></returns>
         */
        public bool Logout()
        {
            if (account?.webToken == null)
                return false;

            rwLock.EnterWriteLock();
            try
            {
                bool success = true;
                string url, referrer;

                url = "https://tw.beanfun.com/beanfun_block/bflogin/logout_confirm.aspx?service=999999_T0";
                referrer = "https://tw.beanfun.com/game_zone/";
                res = client.HttpGet(url, referrer, new Client.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true
                });
                if(res.Status != Client.HttpResponseStatus.Successful)
                    success = false;

                url = $"https://tw.beanfun.com/generic_handlers/remove_bflogin_session.ashx?d={GetTimestamp(TimestampType.Float)}";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    success = false;

                url = $"https://tw.beanfun.com/TW/data_provider/remove_bflogin_session.ashx?d={GetTimestamp(TimestampType.Float)}";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    success = false;

                url = "https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0";
                referrer = "https://tw.beanfun.com/";
                res = client.HttpGet(url, referrer, new Client.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    success = false;

                Step_GetIframes("登出");

                url = "https://tw.newlogin.beanfun.com/generic_handlers/erase_token.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "web_token", "1" }
                }, referrer, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    success = false;
                else
                {
                    GeneralResponse result = JsonConvert.DeserializeObject<GeneralResponse>(res.Message.Content.ReadAsStringAsync().Result);
                    if (result == null || result.intResult != 1)
                        success = false;
                }

                url = $"https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0&_={GetTimestamp(TimestampType.UNIX)}";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
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
