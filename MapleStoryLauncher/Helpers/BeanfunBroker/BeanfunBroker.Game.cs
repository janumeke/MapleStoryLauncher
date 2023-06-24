using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        private TransactionResult Step_GamePage()
        {
            string url = "https://tw.beanfun.com/game_zone/";
            string referrer = "https://tw.beanfun.com/";
            res = client.HttpGet(url, referrer, new BeanfunClient.HandlerConfiguration
            {
                saveResponseUrlAsReferrer = true
            });
            if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                return new GameAccountResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("game_zone", res.Status) };

            TransactionResult loader = Step_Loader();
            if (loader.Status != TransactionResultStatus.Success)
                return loader;

            return new TransactionResult { Status = TransactionResultStatus.Success };
        }

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
         * Possible statuses (message, gameAccounts):
         *   Failed (error description, none):
         *     1. This beanfun account is currently not logged in.
         *     2. Requests to beanfun returns a non-success http code.
         *     3. Returned messages don't have the expected contents.
         *   ConnectionLost (description, none):
         *     Any connection failed.
         *   Success (none, MapleStory accounts): 
         *     All loginable MapleStory accounts are retrieved.
         * </returns>
         */
        public GameAccountResult GetGameAccounts()
        {
            if (account == default(BeanfunAccount))
                return new GameAccountResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(取得遊戲帳號)" };

            rwLock.EnterWriteLock();
            try
            {
                string url;
                MatchCollection matches;

                TransactionResult gamepage = Step_GamePage();
                if (gamepage.Status != TransactionResultStatus.Success)
                    return new GameAccountResult { Status = gamepage.Status, Message = gamepage.Message };

                #region handlers
                url = "https://tw.beanfun.com/generic_handlers/gamezone.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "strFunction", "getOpenedServices" },
                    { "webtoken", "1" }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new GameAccountResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("handler 1", res.Status) };

                
                GeneralResponse result = JsonConvert.DeserializeObject<GeneralResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (result == null || (result.intResult != 0 && result.intResult != 1))
                    return new GameAccountResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(handler 1)" };

                url = "https://tw.beanfun.com/generic_handlers/gamezone.ashx";
                res = client.HttpPost(url, new Dictionary<string, string>
                {
                    { "strFunction", "getPromotions" },
                    { "strSubtype", "-1" }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new GameAccountResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("handler 2", res.Status) };
                #endregion

                #region Game accounts
                url = $"https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx?service_code_and_region=610074_T9&web_token={account.webToken}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new GameAccountResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("遊戲帳號", res.Status) };

                List<GameAccount> accounts = new();
                matches = Regex.Matches(res.Message.Content.ReadAsStringAsync().Result, @"(?<!')<li class=""([^""]*)""[^<>]*><div id=""([^""]*)"" sn=""([^""]*)"" name=""([^""]*)""");
                foreach (Match match in matches)
                {
                    if (match.Groups[1].Value != "Stop")
                        accounts.Add(new GameAccount
                        {
                            friendlyName = System.Web.HttpUtility.HtmlDecode(match.Groups[4].Value),
                            username = match.Groups[2].Value,
                            sn = match.Groups[3].Value,
                        });
                }
                #endregion

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
         * Possible statuses (message, username):
         *   Failed (error description, none):
         *     1. This beanfun account is currently not logged in.
         *     2. Requests to beanfun returns a non-success http code.
         *     3. Returned messages don't have the expected contents.
         *   ConnectionLost (description, none):
         *     Any connection failed.
         *   Denied (reason given by beanfun):
         *     Beanfun rejected the request for any reason.
         *   Success (retrieved OTP, username of the specified MapleStory acoount): 
         *     The OTP for the specified MapleStory account is retrieved.
         * </returns>
         */
        public OTPResult GetOTP(GameAccount gameAccount)
        {
            if (account == default(BeanfunAccount))
                return new OTPResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(取得 OTP)" };

            rwLock.EnterWriteLock();
            try
            {
                string url, referrer;
                string body;
                Match match;
                GeneralResponse result;

                #region createTime, SN and session
                url = $"https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=610074&service_region=T9&sotp={gameAccount.sn}&dt={GetDateTime(DateTimeType.OTP)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true,
                    saveResponseUrlAsReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("createTime, SN", res.Status) };

                body = res.Message.Content.ReadAsStringAsync().Result;
                match = Regex.Match(body, @"ServiceAccountCreateTime: ""([^""]*)""");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到 createTime。" };
                string createTime = match.Groups[1].Value;
                match = Regex.Match(body, @"GetResultByLongPolling&key=([^""]*)");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到 SN。" };
                string sn = match.Groups[1].Value;
                match = Regex.Match(body, @"MyAccountData.ServiceAccountCreateTime \+ ""&([^""]*)"";");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到 session。" };
                string session = match.Groups[1].Value;
                if (session.Split('=').Length != 2)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(session)" };
                #endregion

                #region Params
                url = "https://tw.beanfun.com/beanfun_block/scripts/BeanFunBlockParams.ashx";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("Params", res.Status) };
                #endregion

                #region secretCode
                url = "https://tw.newlogin.beanfun.com/generic_handlers/get_cookies.ashx";
                referrer = "https://tw.beanfun.com/";
                res = client.HttpGet(url, referrer);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("secretCode", res.Status) };

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
                    { session.Split('=')[0], session.Split('=')[1] }
                }, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("record_start", res.Status) };

                result = JsonConvert.DeserializeObject<GeneralResponse>(res.Message.Content.ReadAsStringAsync().Result);
                if (result == null || result.intResult != 1)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(record_start)" };
                #endregion

                #region WebStart
                url = "https://tw.beanfun.com/generic_handlers/CheckVersion.ashx";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("CheckVersion", res.Status) };

                url = $"https://tw.beanfun.com/generic_handlers/adapter.ashx?cmd=01003&service_code=610074&service_region=T9&d={GetDateTime(DateTimeType.System)}";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("adapter 1", res.Status) };

                body = res.Message.Content.ReadAsStringAsync().Result;
                match = Regex.Match(body, @"exe=([\S]*) ([^%]*) %s %s");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到啟動參數。" };
                //string executableName = match.Groups[1].Value;
                string argPrefix = match.Groups[2].Value;
                /*match = Regex.Match(body, @"dir_reg=(.*)");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到登錄檔路徑。" };
                string regPath = match.Groups[1].Value;
                match = Regex.Match(body, @"dir_value_name=(.*)");
                if (match == Match.Empty)
                    return new OTPResult { Status = TransactionResultStatus.Failed, Message = "找不到登錄檔鍵名。" };
                string regKey = match.Groups[1].Value;*/

                url = $"https://tw.beanfun.com/generic_handlers/adapter.ashx?cmd=06002&sn={sn}&result=1&d={GetDateTime(DateTimeType.System)}";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("adapter 2", res.Status) };
                #endregion

                #region OTP
                url = $"https://tw.beanfun.com/beanfun_block/generic_handlers/get_webstart_otp.ashx?SN={sn}&WebToken={account.webToken}&SecretCode={secretCode}&ppppp=0&ServiceCode=610074&ServiceRegion=T9&ServiceAccount={gameAccount.username}&CreateTime={createTime.Replace(" ", "%20")}&d={GetDateTime(DateTimeType.System)}";
                res = client.HttpGet(url);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("OTP", res.Status) };

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
                url = $"https://tw.beanfun.com/generic_handlers/get_result.ashx?meth=GetResultByLongPolling&key={sn}&_={GetDateTime(DateTimeType.UNIX)}";
                res = client.HttpGet(url, null, new BeanfunClient.HandlerConfiguration
                {
                    setReferrer = true
                });
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new OTPResult { Status = ConvertToTransactionStatus(res.Status), Message = GetTransactionMessage("get_result", res.Status) };

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
