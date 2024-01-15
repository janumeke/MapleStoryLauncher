using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MapleStoryLauncher
{
    /**
     * <summary>A BeanfunBroker instance can be associated with a beanfun account.</summary>
     * <remarks>BeanfunBroker has reader-writer lock that may block when calling its public methods.</remarks>
     */
    public partial class BeanfunBroker
    {
        private readonly ReaderWriterLockSlim rwLock = new();

        private readonly BeanfunClient client = new();

        private BeanfunClient.HttpResponse res;

        /**
         * <summary>Get the last response from the internal http client.</summary>
         */
        public HttpResponseMessage GetLastResponse()
        {
            rwLock.EnterReadLock();
            try
            {
                return res.Message;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        /**
         * <summary>Get all cookies of the internal http client.</summary>
         */
        public CookieCollection GetAllCookies()
        {
            return client.GetCookieContainer().GetAllCookies();
        }

        /**
         * <summary>Cancel any ongoing connection.</summary>
         */
        public void Cancel()
        {
            client.Cancel();
        }

        private class BeanfunAccount
        {
            public string skey;  //for app authentication and QRCode
            public string VIEWSTATE;
            public string VIEWSTATEGENERATOR;
            public string EVENTVALIDATION;
            public string lt; //for app authentication
            public string encryptData; //for QRCode
            public string webToken;
        }

        private BeanfunAccount account;

        public enum TransactionResultStatus
        {
            Success,
            Failed,
            Cancelled,
            ConnectionLost,
            Expired,
            Denied,
            RequireAppAuthentication,
            RequireQRCode,
            LoggedOutByBeanfun,
        }

        public class TransactionResult
        {
            public TransactionResultStatus Status { get; set; }
            public string Message { get; set; }
        }

        private static TransactionResultStatus ConvertStatus(BeanfunClient.HttpResponseStatus status)
        {
            return status switch
            {
                BeanfunClient.HttpResponseStatus.Successful => TransactionResultStatus.Success,
                BeanfunClient.HttpResponseStatus.Disconnected => TransactionResultStatus.ConnectionLost,
                _ => TransactionResultStatus.Failed,
            };
        }

        private static string MakeTransactionMessage(string label, BeanfunClient.HttpResponseStatus status)
        {
            return status switch
            {
                BeanfunClient.HttpResponseStatus.Unsuccessful => $"不是成功的回應代碼。({label})",
                BeanfunClient.HttpResponseStatus.Disconnected => $"連線中斷。({label})",
                _ => "",
            };
        }

        private class GeneralResponse
        {
            public int intResult { get; set; }
        }

        private class GetPointsResponse
        {
            public int ResultCode { get; set; }
            public string ResultDesc { get; set; }
            public int RemainPoint { get; set; }
        }

        /**
         * <summary>Get the remaining points of the beanfun account.</summary>
         * <returns><list type="bullet">
         *     <item><description>-1 if<list type="number">
         *         <item><description>This beanfun account is currently not logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *         <item><description>Any connection failed or was cancelled.</description></item>
         *     </list></description></item>
         *     <item><description>points if successful.</description></item>
         * </list></returns>
         */
        public int GetRemainingPoints()
        {
            if (account?.webToken == null)
                return -1;

            rwLock.EnterWriteLock();
            try
            {
                string url, referrer;
                Match match;

                url = $"https://tw.beanfun.com/beanfun_block/generic_handlers/get_remain_point.ashx?webtoken=1&noCacheIE={GetTimestamp(TimestampType.Float)}";
                referrer = "https://tw.beanfun.com/game_zone/";
                res = client.HttpGet(url, referrer);
                if (res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return -1;

                match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"(\{[^\{\}]*\})");
                if (match == Match.Empty)
                    return -1;

                GetPointsResponse result = JsonConvert.DeserializeObject<GetPointsResponse>(match.Groups[1].Value);
                if (result == null)
                    return -1;
                return result.ResultCode switch
                {
                    0 => result.RemainPoint,
                    _ => -1
                };
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
         * Possible statuses (message):<list type="bullet">
         *     <item><description>Failed (error description):<list type="number">
         *         <item><description>This beanfun account is currently not logged in.</description></item>
         *         <item><description>Requests to beanfun returns a non-success http code.</description></item>
         *         <item><description>Returned messages don't have the expected contents.</description></item>
         *     </list></description></item>
         *     <item><description>ConnectionLost (description):
         *         <para>Any connection failed or was cancelled.</para>
         *     </description></item>
         *     <item><description>LoggedOutByBeanfun (none):
         *         <para>The account is logged out.</para>
         *     </description></item>
         *     <item><description>Success (username of this beanfun account):
         *         <para>The account is still logged in.</para>
         *     </description></item>
         * </list>
         * </returns>
         */
        public TransactionResult Ping()
        {
            if (account?.webToken == null)
                return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "登入狀態不允許此操作。(ping)" };

            rwLock.EnterWriteLock();
            try
            {
                string url, referrer;
                Match match;

                url = $"https://tw.beanfun.com/beanfun_block/generic_handlers/echo_token.ashx?webtoken=1&noCacheIE={GetTimestamp(TimestampType.Float)}";
                referrer = "https://tw.beanfun.com/game_zone";
                res = client.HttpGet(url, referrer);
                if(res.Status != BeanfunClient.HttpResponseStatus.Successful)
                    return new TransactionResult { Status = ConvertStatus(res.Status), Message = MakeTransactionMessage("ping", res.Status) };

                match = Regex.Match(res.Message.Content.ReadAsStringAsync().Result, @"(\{[^\{\}]*\})");
                if (match == Match.Empty)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(ping)" };

                PingResponse result = JsonConvert.DeserializeObject<PingResponse>(match.Groups[1].Value);
                if (result == null)
                    return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(ping)" };
                switch (result.ResultCode)
                {
                    case 0:
                        account = default;
                        return new TransactionResult { Status = TransactionResultStatus.LoggedOutByBeanfun };
                    case 1:
                        return new TransactionResult { Status = TransactionResultStatus.Success, Message = result.MainAccountID };
                    default:
                        return new TransactionResult { Status = TransactionResultStatus.Failed, Message = "不是預期的結果。(ping)" };
                };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private enum TimestampType
        {
            OTP,
            Regular,
            Float,
            UNIX,
            UNIX_Random,
            System
        }

        Random rng = new();

        private string GetTimestamp(TimestampType type)
        {
            DateTime now = DateTime.Now;
            switch (type)
            {
                case TimestampType.OTP:
                    return (now.Year - 1900).ToString() + (now.Month - 1).ToString() + now.ToString("dHmsfff");
                case TimestampType.Regular:
                    return now.ToString("yyyyMMddHHmmss");
                case TimestampType.Float:
                    return now.ToString("yyyyMMddHHmmss.fff");
                case TimestampType.UNIX:
                    DateTime origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    TimeSpan diff = now.ToUniversalTime() - origin;
                    return diff.TotalMilliseconds.ToString("F0");
                case TimestampType.UNIX_Random:
                    return GetTimestamp(TimestampType.UNIX) + rng.Next(9999).ToString();
                case TimestampType.System:
                    return Environment.TickCount.ToString();
            }
            return string.Empty;
        }
    }
}
