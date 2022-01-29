using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        public class GameAccount
        {
            public string FriendlyName { get; set; }
            public string Username { get; set; }
            public string Sotp { get; set; }
            public string CreateTime { get; set; }
        }

        public class GameAccountResult : LoginResult
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
                return new GameAccountResult { Status = LoginStatus.Failed, Message = "介面與網路模組失去同步。請重開程式。" };

            rwLock.EnterWriteLock();
            try
            {
                HttpRequestMessage req;
                MatchCollection matches;

                req = new HttpRequestMessage(HttpMethod.Get, "https://tw.beanfun.com/game_zone/");
                req.Headers.Referrer = new Uri("https://tw.beanfun.com/");
                try { res = client.SendAsync(req).Result; }
                catch { return new GameAccountResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(game zone)" }; }
                if (!res.IsSuccessStatusCode)
                    return new GameAccountResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(game zone)" };

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx?service_code_and_region=610074_T9&web_token={account.webtoken}");
                req.Headers.Referrer = new Uri("https://tw.beanfun.com/game_zone/");
                try { res = client.SendAsync(req).Result; }
                catch { return new GameAccountResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(game accounts)" }; }
                if (!res.IsSuccessStatusCode)
                    return new GameAccountResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(game accounts)" };
                List<GameAccount> accounts = new();
                matches = Regex.Matches(res.Content.ReadAsStringAsync().Result, @"(?<!')<li class=""([^""]*)""[^<>]*><div id=""([^""]*)"" sn=""([^""]*)"" name=""([^""]*)""");
                foreach (Match match in matches)
                {
                    if (match.Groups[1].Value != "Stop")
                        accounts.Add(new GameAccount
                        {
                            FriendlyName = System.Web.HttpUtility.HtmlDecode(match.Groups[4].Value),
                            Username = match.Groups[2].Value,
                            Sotp = match.Groups[3].Value
                        });
                }
                return new GameAccountResult { Status = LoginStatus.Success, GameAccounts = accounts };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public class OTPResult : LoginResult
        {
            public string Username { get; set; }
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
         *   Success (retrieved OTP, username of the specified MapleStory acoount): 
         *     The OTP for the specified MapleStory account is retrieved.
         * </returns>
         */
        public OTPResult GetOTP(GameAccount gameAccount)
        {
            if (account == default(BeanfunAccount))
                return new OTPResult { Status = LoginStatus.Failed, Message = "介面與網路模組失去同步。請重開程式。" };

            rwLock.EnterWriteLock();
            try
            {
                HttpRequestMessage req;
                Dictionary<string, string> form;
                string body;
                Match match;

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=610074&service_region=T9&sotp={gameAccount.Sotp}&dt={GetDateTime(DateTimeType.OTP)}");
                req.Headers.Referrer = new Uri($"https://tw.beanfun.com/beanfun_block/game_zone/game_server_account_list.aspx?sc=610074&sr=T9&dt={GetDateTime(DateTimeType.Regular)}");
                try { res = client.SendAsync(req).Result; }
                catch { return new OTPResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(SN, CreateTime)" }; }
                if (!res.IsSuccessStatusCode)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(SN, CreateTime)" };
                body = res.Content.ReadAsStringAsync().Result;
                match = Regex.Match(body, @"key=([^""]*)""");
                if (match == Match.Empty)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "找不到 SN。" };
                string sn = match.Groups[1].Value;
                match = Regex.Match(body, @"ServiceAccountCreateTime: ""([^""]*)""");
                if (match == Match.Empty)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "找不到 CreateTime。" };
                string createTime = match.Groups[1].Value;

                req = new HttpRequestMessage(HttpMethod.Get, "https://tw.newlogin.beanfun.com/generic_handlers/get_cookies.ashx");
                req.Headers.Referrer = new Uri("https://tw.beanfun.com/");
                try { res = client.SendAsync(req).Result; }
                catch { return new OTPResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(SecretCode)" }; }
                if (!res.IsSuccessStatusCode)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(SecretCode)" };
                match = Regex.Match(res.Content.ReadAsStringAsync().Result, @"m_strSecretCode = '([^']*)'");
                if (match == Match.Empty)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "找不到 SecretCode。" };
                string secretCode = match.Groups[1].Value;

                req = new HttpRequestMessage(HttpMethod.Post, "https://tw.beanfun.com/beanfun_block/generic_handlers/record_service_start.ashx");
                req.Headers.Referrer = new Uri($"https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=610074&service_region=T9&sotp={gameAccount.Sotp}&dt={GetDateTime(DateTimeType.OTP)}");
                form = new Dictionary<string, string>
                {
                    { "service_code", "610074" },
                    { "service_region", "T9" },
                    { "service_account_id", gameAccount.Username },
                    { "sotp", gameAccount.Sotp },
                    { "service_account_display_name", gameAccount.FriendlyName },
                    { "service_account_create_time", createTime }
                };
                req.Content = new FormUrlEncodedContent(form);
                try { res = client.SendAsync(req).Result; }
                catch { return new OTPResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(record_service)" }; }
                if (!res.IsSuccessStatusCode)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(record_service)" };

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/generic_handlers/get_result.ashx?meth=GetResultByLongPolling&key={sn}&_={GetDateTime(DateTimeType.UNIX)}");
                req.Headers.Referrer = new Uri($"https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=610074&service_region=T9&sotp={gameAccount.Sotp}&dt={GetDateTime(DateTimeType.OTP)}");
                try { res = client.SendAsync(req).Result; }
                catch { return new OTPResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(get_result)" }; }
                if (!res.IsSuccessStatusCode)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(get_result)" };

                req = new HttpRequestMessage(HttpMethod.Get, $"https://tw.beanfun.com/beanfun_block/generic_handlers/get_webstart_otp.ashx?SN={sn}&WebToken={account.webtoken}&SecretCode={secretCode}&ppppp=0&ServiceCode=610074&ServiceRegion=T9&ServiceAccount={gameAccount.Username}&CreateTime={createTime.Replace(" ", "%20")}&d={GetDateTime(DateTimeType.System)}");
                req.Headers.Referrer = new Uri($"https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=610074&service_region=T9&sotp={gameAccount.Sotp}&dt={GetDateTime(DateTimeType.OTP)}");
                try { res = client.SendAsync(req).Result; }
                catch { return new OTPResult { Status = LoginStatus.ConnectionLost, Message = "連線中斷。(OTP)" }; }
                if (!res.IsSuccessStatusCode)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "不是成功的回應代碼。(OTP)" };
                body = res.Content.ReadAsStringAsync().Result;
                match = Regex.Match(body.Remove(0, 2), @"[^0-9A-F]");
                if (!body.StartsWith("1;") || match != Match.Empty)
                    return new OTPResult { Status = LoginStatus.Failed, Message = "不是預期的資料。(OTP)" };

                //DES (ECB)
                //ASCII: key, plain
                //HEX: cypher
                string key = body.Substring(2, 8);
                DES des = DES.Create();
                des.Key = Encoding.ASCII.GetBytes(key);
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;
                string cypher = body[10..];
                byte[] cypherBytes = new byte[cypher.Length / 2];
                for (int i = 0; i < cypherBytes.Length; ++i)
                    cypherBytes[i] = Convert.ToByte(cypher.Substring(i * 2, 2), 16);
                MemoryStream output = new();
                CryptoStream cryptoStream = new(output, des.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(cypherBytes, 0, cypherBytes.Length);
                byte[] plainBytes = new byte[10];
                output.Seek(0, SeekOrigin.Begin);
                output.Read(plainBytes, 0, plainBytes.Length);
                string plain = Encoding.ASCII.GetString(plainBytes);
                return new OTPResult { Status = LoginStatus.Success, Message = plain, Username = gameAccount.Username };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
    }
}
