using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http;
using Microsoft.Win32;

namespace MaplestoryLauncher
{
    /**
     * <summary>
     * <para>A BeanfunBroker instance can be associated with a beanfun account.</para>
     * <para>BeanfunBroker has reader-writer lock that may block when calling its public methods.</para>
     * </summary>
     */
    public partial class BeanfunBroker
    {
        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private class CustomHandler : DelegatingHandler
        {
            public CookieContainer CookieContainer { get; }

            private readonly string userAgent;

            public CustomHandler()
            {
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                InnerHandler = httpClientHandler;
                CookieContainer = httpClientHandler.CookieContainer;

                //Find OS, Browser and User-Agent
                string win7 = "NT 6.1", win8 = "NT 6.2", win8_1 = "NT 6.3", win10 = "NT 10.0";
                string os;
                if (Environment.OSVersion.Version.Major == 6)
                    switch (Environment.OSVersion.Version.Minor)
                    {
                        case 2:
                            os = win8;
                            break;
                        case 3:
                            os = win8_1;
                            break;
                        default:
                            os = win7;
                            break;
                    }
                else
                    os = win10;
                string firefox95 = $"Mozilla/5.0 (Windows {os}; Win64; x64; rv:95.0) Gecko/20100101 Firefox/95.0";
                string chrome97 = $"Mozilla/5.0 (Windows {os}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36";
                string edge97 = $"Mozilla/5.0 (Windows {os}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36 Edg/97.0.1072.69";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice"))
                {
                    if (key == null)
                        userAgent = edge97;
                    else
                    {
                        object progId = key.GetValue("ProgId");
                        if (progId == null)
                            userAgent = edge97;
                        else
                            switch (progId.ToString())
                            {
                                case "FirefoxURL":
                                    userAgent = firefox95;
                                    break;
                                case "ChromeHTML":
                                    userAgent = chrome97;
                                    break;
                                default:
                                    userAgent = edge97;
                                    break;
                            }
                    }
                }
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Add("User-Agent", userAgent);
                return base.SendAsync(request, cancellationToken);
            }
        }

        private HttpClient client = default;
        private CookieContainer cookies = default;

        public BeanfunBroker()
        {
            CustomHandler customHandler = new CustomHandler();
            client = new HttpClient(customHandler);
            cookies = customHandler.CookieContainer;
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

        private string GetDateTime(DateTimeType type)
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
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    TimeSpan diff = now.ToUniversalTime() - origin;
                    return diff.TotalMilliseconds.ToString("F0");
                case DateTimeType.System:
                    return Environment.TickCount.ToString();
            }
            return string.Empty;
        }
    }
}
