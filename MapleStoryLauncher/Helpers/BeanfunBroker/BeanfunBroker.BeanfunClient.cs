using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        private class BeanfunClient
        {
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

                public bool SaveNextResponseUrlAsFurtherReferrer { get; set; } = false;

                public bool SetNextRequestReferrer { get; set; } = false;

                private Uri savedReferrer = default;

                protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    request.Headers.Add("User-Agent", userAgent);
                    if (SetNextRequestReferrer)
                    {
                        request.Headers.Referrer = savedReferrer;
                        SetNextRequestReferrer = false;
                    }
                    HttpResponseMessage response = base.Send(request, cancellationToken);
                    if (SaveNextResponseUrlAsFurtherReferrer)
                    {
                        savedReferrer = response.RequestMessage.RequestUri;
                        SaveNextResponseUrlAsFurtherReferrer = false;
                    }
                    return response;
                }
            }

            private CustomHandler handler = default;
            private HttpClient client = default;

            public BeanfunClient()
            {
                handler = new();
                client = new HttpClient(handler);
            }

            /**
             * <summary>Get all cookies of the internal http client.</summary>
             */
            public CookieContainer GetCookies()
            {
                return handler.CookieContainer;
            }

            /**
             * <summary>Clear all cookies of the internal http client.</summary>
             */
            public void ClearCookies()
            {
                handler = new();
                client = new HttpClient(handler);
            }

            public enum HttpResponseStatus
            {
                Successful,
                Unsuccessful,
                Disconnected
            }

            public class HttpResponse
            {
                public HttpResponseStatus Status { get; set; }
                public HttpResponseMessage Message { get; set; }
            }

            public class HandlerConfiguration
            {
                public bool setReferrer = false;
                public bool saveResponseUrlAsReferrer = false;
            }

            public HttpResponse HttpGet(string url, string referrer = default, HandlerConfiguration handlerConfiguration = default)
            {
                HttpRequestMessage req = new(HttpMethod.Get, url);
                if (referrer != default)
                    req.Headers.Referrer = new Uri(referrer);

                if (handlerConfiguration != default)
                {
                    handler.SetNextRequestReferrer = handlerConfiguration.setReferrer;
                    handler.SaveNextResponseUrlAsFurtherReferrer = handlerConfiguration.saveResponseUrlAsReferrer;
                }

                HttpResponseMessage res;
                try { res = client.Send(req); }
                catch { return new HttpResponse { Status = HttpResponseStatus.Disconnected }; }
                if (!res.IsSuccessStatusCode)
                    return new HttpResponse { Status = HttpResponseStatus.Unsuccessful, Message = res };
                return new HttpResponse { Status = HttpResponseStatus.Successful, Message = res };
            }

            public HttpResponse HttpPost(string url, Dictionary<string, string> form, string referrer = default, HandlerConfiguration handlerConfiguration = default)
            {
                HttpRequestMessage req = new(HttpMethod.Post, url);
                if (referrer != default)
                    req.Headers.Referrer = new Uri(referrer);

                if (handlerConfiguration != default)
                {
                    handler.SetNextRequestReferrer = handlerConfiguration.setReferrer;
                    handler.SaveNextResponseUrlAsFurtherReferrer = handlerConfiguration.saveResponseUrlAsReferrer;
                }

                req.Content = new FormUrlEncodedContent(form);

                HttpResponseMessage res;
                try { res = client.Send(req); }
                catch { return new HttpResponse { Status = HttpResponseStatus.Disconnected }; }
                if (!res.IsSuccessStatusCode)
                    return new HttpResponse { Status = HttpResponseStatus.Unsuccessful, Message = res };
                return new HttpResponse { Status = HttpResponseStatus.Successful, Message = res };
            }
        }
    }
}
