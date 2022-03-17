using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Win32;

namespace MapleStoryLauncher
{
    /**
     * <summary>
     * <para>A BeanfunBroker instance can be associated with a beanfun account.</para>
     * <para>BeanfunBroker has reader-writer lock that may block when calling its public methods.</para>
     * </summary>
     */
    public partial class BeanfunBroker
    {
        private readonly ReaderWriterLockSlim rwLock = new();

        private class CustomHandler : DelegatingHandler
        {
            public CookieContainer CookieContainer { get; }

            private readonly string userAgent;

            public CustomHandler()
            {
                HttpClientHandler httpClientHandler = new();
                InnerHandler = httpClientHandler;
                CookieContainer = httpClientHandler.CookieContainer;

                //Find OS, Browser and User-Agent
                string win7 = "NT 6.1", win8 = "NT 6.2", win8_1 = "NT 6.3", win10 = "NT 10.0";
                string os;
                if (Environment.OSVersion.Version.Major == 6)
                    os = Environment.OSVersion.Version.Minor switch
                    {
                        2 => win8,
                        3 => win8_1,
                        _ => win7,
                    };
                else
                    os = win10;
                string firefox = $"Mozilla/5.0 (Windows {os}; Win64; x64; rv:98.0) Gecko/20100101 Firefox/98.0";
                string chrome = $"Mozilla/5.0 (Windows {os}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36";
                string edge = $"Mozilla/5.0 (Windows {os}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36 Edg/99.0.1150.36";
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice");
                if (key != null)
                {
                    object progId = key.GetValue("ProgId");
                    if (progId != null)
                        userAgent = progId.ToString() switch
                        {
                            "FirefoxURL" => firefox,
                            "ChromeHTML" => chrome,
                            _ => edge,
                        };
                    else
                        userAgent = edge;
                }
                else
                    userAgent = edge;
            }

            public bool SaveNextRequestUrlAsFurtherReferrer { get; set; } = false;

            public bool SetNextRequestReferrer { get; set; } = false;

            private Uri referrer = default;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Add("User-Agent", userAgent);
                if (SetNextRequestReferrer)
                {
                    request.Headers.Referrer = referrer;
                    SetNextRequestReferrer = false;
                }
                if (SaveNextRequestUrlAsFurtherReferrer)
                {
                    referrer = request.RequestUri;
                    SaveNextRequestUrlAsFurtherReferrer = false;
                }
                return base.SendAsync(request, cancellationToken);
            }
        }

        private HttpClient client = default;
        private CustomHandler handler = default;

        public BeanfunBroker()
        {
            handler = new();
            client = new HttpClient(handler);
        }

        private class BeanfunAccount
        {
            public string skey = default;
            public string lt = default; //for app authentication
            public string encryptdata = default; //for QRCode
            public string webtoken = default;
        }

        private BeanfunAccount account = default;

        HttpResponseMessage res = default;

        /**
         * <summary>Get the last response from the internal http client.</summary>
         */
        public HttpResponseMessage GetLastResponse()
        {
            rwLock.EnterReadLock();
            try
            {
                return res;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        /**
         * <summary>Clear all cookies of the internal http client.</summary>
         */
        public void ClearCookies()
        {
            handler = new();
            client = new HttpClient(handler);
        }

        private enum DateTimeType
        {
            Skey,
            Ping,
            Logout,
            OTP,
            Regular,
            UNIX,
            System
        }

        private static string GetDateTime(DateTimeType type)
        {
            DateTime now = DateTime.Now;
            switch (type)
            {
                case DateTimeType.Skey:
                case DateTimeType.Ping:
                case DateTimeType.Logout:
                    return now.ToString("yyyyMMddHHmmss.fff");
                case DateTimeType.OTP:
                    return (now.Year - 1900).ToString() + (now.Month - 1).ToString() + now.ToString("dHmsfff");
                case DateTimeType.Regular:
                    return now.ToString("yyyyMMddHHmmss");
                case DateTimeType.UNIX:
                    DateTime origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    TimeSpan diff = now.ToUniversalTime() - origin;
                    return diff.TotalMilliseconds.ToString("F0");
                case DateTimeType.System:
                    return Environment.TickCount.ToString();
            }
            return string.Empty;
        }
    }
}
