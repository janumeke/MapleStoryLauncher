using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        private TransactionResult Step_GameZone(bool isRedirected = false)
        {
            bool isLoggedIn = account?.webToken != null;
            string url, referrer;
            TransactionResult result;

            if(!isRedirected)
            {
                url = "https://tw.beanfun.com/game_zone/";
                res = client.HttpGet(url, null, new Client.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true,
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("game_zone", res.Status) };
            }

            result = Step_GetIframes("game_zone");
            if (result.Status != TransactionResultStatus.Success)
                return result;

            if (!isLoggedIn)
            {
                url = "https://tw.beanfun.com/generic_handlers/gamezone.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "strFunction", "getPromotions"},
                    { "strSubtype", "ALL"},
                }, null, new Client.HandlerConfiguration
                {
                    setReferrer = true,
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("gamezone", res.Status) };
            }
            else
            {
                url = "https://tw.beanfun.com/generic_handlers/gamezone.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "strFunction", "getOpenedServices"},
                    { "webtoken", "1"},
                }, null, new Client.HandlerConfiguration
                {
                    setReferrer = true,
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("gamezone 1", res.Status) };

                url = "https://gamaad.beanfun.com/api/BFWebCommon/ShowcasePlayPhone";
                referrer = "https://tw.beanfun.com/";
                res = client.HttpPost(url, null, referrer);
                if (res.Status != Client.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("ShowcasePlayPhone", res.Status) };

                url = "https://tw.beanfun.com/generic_handlers/gamezone.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "strFunction", "getPromotions"},
                    { "strSubtype", "-1"},
                }, null, new Client.HandlerConfiguration
                {
                    setReferrer = true,
                });
                if (res.Status != Client.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("gamezone 2", res.Status) };

                if (isRedirected)
                {
                    url = "https://tw.beanfun.com/scripts/floatbox/graphics/loader_iframe_custom.html";
                    res = client.HttpGet(url, null, new Client.HandlerConfiguration
                    {
                        setReferrer = true,
                    });
                    if (res.Status != Client.HttpResponseStatus.Successful)
                        return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("loader_iframe_custom", res.Status) };

                    url = "https://tw.beanfun.com/bfweb/NEW2/showcase_playphone.aspx";
                    res = client.HttpGet(url, null, new Client.HandlerConfiguration
                    {
                        setReferrer = true,
                    });
                    if (res.Status != Client.HttpResponseStatus.Successful)
                        return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("showcase_playphone", res.Status) };
                }
            }
            return new TransactionResult { Status = TransactionResultStatus.Success };
        }

        private TransactionResult Step_Loading()
        {
            string url = $"https://tw.newlogin.beanfun.com/login/loading.htm?{GetTimestamp(TimestampType.UNIX_Random)}";
            res = client.HttpGet(url, null, new Client.HandlerConfiguration
            {
                setReferrer = true
            });
            if (res.Status != Client.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("loading", res.Status) };
            return new TransactionResult { Status = TransactionResultStatus.Success };
        }

        /**
         * <remarks>This assumes this.account is created.</remarks>
         */
        private TransactionResult Step_LoginPage()
        {
            string url;
            Match match;
            TransactionResult result;

            url = $"https://tw.beanfun.com/beanfun_block/bflogin/default.aspx?service=999999_T0&dt={GetTimestamp(TimestampType.Float)}&url=https%3A//tw.beanfun.com/game_zone/";
            res = client.HttpGet(url, null, new Client.HandlerConfiguration
            {
                setReferrer= true,
                saveResponseUrlAsReferrer = true,
            });
            if (res.Status != Client.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("Login page", res.Status) };

            match = Regex.Match(res.Message.RequestMessage.RequestUri.ToString(), "skey=([^&]*)");
            if (match == Match.Empty)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 skey。" };
            account.skey = match.Groups[1].Value;

            result = Step_GetIframes("Login page");
            if (result.Status != TransactionResultStatus.Success)
                return result;

            url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={account.skey}&display_mode=2";
            res = client.HttpGet(url, null, new Client.HandlerConfiguration
            {
                setReferrer = true,
                saveResponseUrlAsReferrer = true,
            });
            if (res.Status != Client.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("loginform", res.Status) };

            result = Step_GetIframes("loginform");
            if (result.Status != TransactionResultStatus.Success)
                return result;

            for(int i = 1; i <= 2; ++i)
            {
                result = Step_Loading();
                if (result.Status != TransactionResultStatus.Success)
                    return result;
            }

            url = $"https://tw.newlogin.beanfun.com/loginform.aspx?skey={account.skey}&display_mode=2&_={GetTimestamp(TimestampType.UNIX)}";
            res = client.HttpGet(url, null, new Client.HandlerConfiguration
            {
                setReferrer = true,
            });
            if (res.Status != Client.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("loginform 2", res.Status) };

            url = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={account.skey}&clientID=undefined";
            res = client.HttpGet(url, null, new Client.HandlerConfiguration
            {
                setReferrer = true,
                saveResponseUrlAsReferrer = true,
            });
            if (res.Status != Client.HttpResponseStatus.Successful)
                return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("id-pass_form", res.Status) };

            string body = res.Message.Content.ReadAsStringAsync().Result;

            match = Regex.Match(body, @"id=""__VIEWSTATE"" value=""([^""]*)""");
            if (match == Match.Empty)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 viewState。" };
            account.VIEWSTATE = match.Groups[1].Value;

            match = Regex.Match(body, @"id=""__VIEWSTATEGENERATOR"" value=""([^""]*)""");
            if (match == Match.Empty)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 viewStateGenerator。" };
            account.VIEWSTATEGENERATOR = match.Groups[1].Value;

            match = Regex.Match(body, @"id=""__EVENTVALIDATION"" value=""([^""]*)""");
            if (match == Match.Empty)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "找不到 eventValidation。" };
            account.EVENTVALIDATION = match.Groups[1].Value;

            return new TransactionResult { Status = TransactionResultStatus.Success };
        }

        private TransactionResult Step_Return(string akey)
        {
            string url = "https://tw.beanfun.com/beanfun_block/bflogin/return.aspx";
            string referrer = "https://tw.newlogin.beanfun.com/";
            res = client.HttpPost(url, new Dictionary<string, string>
            {
                { "SessionKey", account.skey },
                { "AuthKey", akey },
                { "ServiceCode", "" },
                { "ServiceRegion", "" },
                { "ServiceAccountSN", "0" },
            }, referrer, new Client.HandlerConfiguration
            {
                saveResponseUrlAsReferrer = true,
            });

            TransactionResult result = Step_GameZone(true);
            if(result.Status != TransactionResultStatus.Success)
                return result;

            return new TransactionResult { Status = TransactionResultStatus.Success };
        }

        /**
         * <summary>Send HTTP GET to the sources of all iframes that have src atrribute in the domain beanfun.com.</summary>
         * <param name="resMsg">The response message to parse HTML from or this.res.Message if not specified.</param>
         */
        private TransactionResult Step_GetIframes(string label, HttpResponseMessage resMsg = default)
        {
            HttpResponseMessage msg = resMsg != default ? resMsg : res.Message;
            Uri baseUri = new(msg.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority));

            HtmlDocument doc = new();
            doc.LoadHtml(msg.Content.ReadAsStringAsync().Result);
            HtmlNodeCollection iframes = doc.DocumentNode.SelectNodes("//iframe[@src]");
            if(iframes != null)
                foreach (HtmlNode iframe in iframes)
                {
                    try
                    {
                        Uri src = new(iframe.Attributes["src"].Value, UriKind.RelativeOrAbsolute);
                        if (!src.IsAbsoluteUri)
                            src = new(baseUri, src);
                        if (src.IsWellFormedOriginalString() && src.Host.EndsWith("beanfun.com"))
                        {
                            res = client.HttpGet(src.OriginalString, null, new Client.HandlerConfiguration
                            {
                                setReferrer = true,
                                saveResponseUrlAsReferrer = false,
                            });
                            if (res.Status != Client.HttpResponseStatus.Successful)
                                return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage($"{label} iframes", res.Status) };
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        continue;
                    }
                }
            return new TransactionResult { Status = TransactionResultStatus.Success };
        }
    }
}
