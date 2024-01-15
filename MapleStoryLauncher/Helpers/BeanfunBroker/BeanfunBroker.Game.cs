using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Newtonsoft.Json;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Web;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        public class GameAccount
        {
            public string friendlyName;
            public string username;
            public string sn;
        }

        public class GameAccountResult : TransactionResult
        {
            public List<GameAccount> GameAccounts { get; set; }
        }

        /**
         * <summary>Get all loginable MapleStory accounts associated with this beanfun account.</summary>
         * <returns>
         * Possible statuses (message, gameAccounts):<list type="bullet">
         *     <item><description>Failed (error description, none):<list type="number">
         *         <item><description>This beanfun account is currently not logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description, none):
         *         <para>Any connection failed or was cancelled.</para>
         *     </description></item>
         *     <item><description>Success (none, MapleStory accounts): 
         *         <para>All loginable MapleStory accounts are retrieved.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public GameAccountResult GetGameAccounts()
        {
            if (account?.webToken == null)
                return new GameAccountResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(取得遊戲帳號)" };

            rwLock.EnterWriteLock();
            try
            {
                string url, referrer;

                url = "https://tw.beanfun.com/scripts/floatbox/graphics/loader_iframe_custom.html";
                referrer = "https://tw.beanfun.com/game_zone/";
                res = client.HttpGet(url, referrer);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new GameAccountResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("loader_iframe_custom", res.Status) };

                url = $"https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D610074_T9&web_token={account.webToken}";
                referrer = "https://tw.beanfun.com/game_zone/";
                res = client.HttpGet(url, referrer, new BeanfunClient.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true,
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new GameAccountResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("遊戲帳號", res.Status) };

                List<GameAccount> accounts = new();
                HtmlDocument doc = new();
                doc.LoadHtml(res.Message.Content.ReadAsStringAsync().Result);
                HtmlNodeCollection lis = doc.DocumentNode.SelectNodes("//ul[@id='ulServiceAccountList']/li");
                if(lis != null)
                    foreach(HtmlNode li in lis)
                    {
                        if (li.Attributes["class"].Value.Contains("Stop"))
                            continue;
                        HtmlNode div = li.FirstChild;
                        accounts.Add(new GameAccount
                        {
                            friendlyName = HttpUtility.HtmlDecode(div.Attributes["name"].Value),
                            username = div.Attributes["id"].Value,
                            sn = div.Attributes["sn"].Value,
                        });
                    }

                return new GameAccountResult { Status = TransactionResultStatus.Success, GameAccounts = accounts };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public class OTPResult : TransactionResult
        {
            public string Username { get; set; }
            public string ArgPrefix { get; set; } //Prefix of startup argument for auto-login
        }

        /**
         * <summary>Get the OTP for the specified MapleStory account.</summary>
         * <returns>
         * Possible statuses (message, username):<list type="bullet">
         *     <item><description>Failed (error description, none):<list type="number">
         *         <item><description>This beanfun account is currently not logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description, none):
         *         <para>Any connection failed or was cancelled.</para>
         *     </description></item>
         *     <item><description>Denied (reason given by beanfun):
         *         <para>Beanfun rejected the request for any reason.</para>
         *     </description></item>
         *     <item><description>Success (retrieved OTP, username of the specified MapleStory acoount):
         *         <para>The OTP for the specified MapleStory account is retrieved.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public OTPResult GetOTP(GameAccount gameAccount)
        {
            if (account?.webToken == null)
                return new OTPResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(取得 OTP)" };

            rwLock.EnterWriteLock();
            try
            {
                string url, referrer;
                string body;
                Match match;
                GeneralResponse result;

                url = $"https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D610074_T9&web_token={account.webToken}";
                referrer = "https://tw.beanfun.com/game_zone/";
                res = client.HttpGet(url, referrer, new BeanfunClient.HandlerConfiguration
                {
                    saveResponseUrlAsReferrer = true,
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("auth", res.Status) };

                #region createTime, SN and query
                url = $"https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=610074&service_region=T9&sotp={gameAccount.sn}&dt={GetTimestamp(TimestampType.OTP)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("createTime, SN", res.Status) };

                body = res.Message.Content.ReadAsStringAsync().Result;

                match = Regex.Match(body, @"ServiceAccountCreateTime: ""([^""]*)""");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到 createTime。" };
                string createTime = match.Groups[1].Value;
                match = Regex.Match(body, @"GetResultByLongPolling&key=([^""]*)");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到 SN。" };
                string resultKey = match.Groups[1].Value;
                match = Regex.Match(body, @"MyAccountData.ServiceAccountCreateTime \+ ""&([^=]*)=([^""]*)"";");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到 query。" };
                string qK = match.Groups[1].Value;
                string qV = match.Groups[2].Value;
                #endregion

                #region secretCode
                url = "https://tw.newlogin.beanfun.com/generic_handlers/get_cookies.ashx";
                referrer = "https://tw.beanfun.com/";
                res = client.HttpGet(url, referrer);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("secretCode", res.Status) };

                match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"var m_strSecretCode = '([^']*)'");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到 secretCode。" };
                string secretCode = match.Groups[1].Value;
                #endregion

                #region record_start
                url = "https://tw.beanfun.com/beanfun_block/generic_handlers/record_service_start.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "service_code", "610074" },
                    { "service_region", "T9" },
                    { "service_account_id", gameAccount.username },
                    { "sotp", gameAccount.sn },
                    { "service_account_display_name", gameAccount.friendlyName },
                    { "service_account_create_time", createTime },
                    { qK, qV }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("record_start", res.Status) };

                result = JsonConvert.DeserializeObject<GeneralResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (result == null || result.intResult != 1)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(record_start)" };
                #endregion

                #region WebStart
                url = "https://tw.beanfun.com/generic_handlers/CheckVersion.ashx";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("CheckVersion", res.Status) };

                url = $"https://tw.beanfun.com/generic_handlers/adapter.ashx?cmd=01003&service_code=610074&service_region=T9&d={GetTimestamp(TimestampType.System)}";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("adapter 1", res.Status) };

                match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"exe=([\S]*) ([^%]*) %s %s");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到啟動參數。" };
                string argPrefix = match.Groups[2].Value;

                url = $"https://tw.beanfun.com/generic_handlers/adapter.ashx?cmd=06002&sn={resultKey}&result=1&d={GetTimestamp(TimestampType.System)}";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("adapter 2", res.Status) };
                #endregion

                #region OTP
                url = $"https://tw.beanfun.com/beanfun_block/generic_handlers/get_webstart_otp.ashx?SN={resultKey}&WebToken={account.webToken}&SecretCode={secretCode}&ppppp=0&ServiceCode=610074&ServiceRegion=T9&ServiceAccount={gameAccount.username}&CreateTime={createTime.Replace(" ", "%20")}&d={GetTimestamp(TimestampType.System)}";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("OTP", res.Status) };

                body = res.Message.Content.ReadAsStringAsync().Result;
                if (body.StartsWith("0;"))
                    return new OTPResult { Status = TransactionResultStatus.Denied, Message = body.Remove(0, 2) };
                match = Regex.Match(body.Remove(0, 2), @"[^0-9A-F]");
                if (!body.StartsWith("1;") || match != Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "不是預期的資料。(OTP)" };
                string key = body.Substring(2, 8);
                string cypher = body[10..];
                string otp = DESDecrypt(cypher, key);
                #endregion

                #region get_result
                url = $"https://tw.beanfun.com/generic_handlers/get_result.ashx?meth=GetResultByLongPolling&key={resultKey}&_={GetTimestamp(TimestampType.UNIX)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("get_result", res.Status) };

                result = JsonConvert.DeserializeObject<GeneralResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (result == null || result.intResult != 1)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(get_result)" };
                #endregion

                return new OTPResult { Status = TransactionResultStatus.Success, Message = otp, Username = gameAccount.username, ArgPrefix = argPrefix };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        //DES (ECB)
        //HEX: cypher
        //ASCII: key, plain
        private static string DESDecrypt(string cypher, string key)
        {
            DES des = DES.Create();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.None;
            des.Key = Encoding.ASCII.GetBytes(key);

            MemoryStream output = new();
            CryptoStream cryptoStream = new(output, des.CreateDecryptor(), CryptoStreamMode.Write);
            byte[] cypherBytes = new byte[cypher.Length / 2];
            for (int i = 0; i < cypherBytes.Length; ++i)
                cypherBytes[i] = Convert.ToByte(cypher.Substring(i * 2, 2), 16);
            cryptoStream.Write(cypherBytes, 0, cypherBytes.Length);

            byte[] plainBytes = new byte[output.Length];
            output.Seek(0, SeekOrigin.Begin);
            output.Read(plainBytes, 0, plainBytes.Length);
            return Encoding.ASCII.GetString(plainBytes).Trim('\0');
        }
    }
}
