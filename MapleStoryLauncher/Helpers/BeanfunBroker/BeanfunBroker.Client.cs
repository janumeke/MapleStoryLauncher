using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class BeanfunBroker
    {
        private class Client
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
                    
                    string firefox = $"Mozilla/5.0 (Windows {os}; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0";
                    string chrome = $"Mozilla/5.0 (Windows {os}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
                    string edge = $"Mozilla/5.0 (Windows {os}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0";
                    string progID = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice")?.GetValue("ProgID")?.ToString();
                    if (progID is null)
                        userAgent = edge;
                    else if (progID.StartsWith("FirefoxURL"))
                        userAgent = firefox;
                    else if (progID == "ChromeHTML")
                        userAgent = chrome;
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

            public Client()
            {
                handler = new();
                client = new HttpClient(handler);
            }

            /**
             * <summary>Get the cookie container of the internal http client.</summary>
             */
            public CookieContainer GetCookieContainer()
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

            CancellationTokenSource cancellationSource = new();

            /**
             * <summary>Cancel any ongoing connection.</summary>
             */
            public void Cancel()
            {
                lock (cancellationSource)
                {
                    cancellationSource.Cancel();
                }
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

                lock (cancellationSource)
                {
                    if(cancellationSource.IsCancellationRequested)
                        cancellationSource = new();
                }
                HttpResponseMessage res;
                try { res = client.Send(req, cancellationSource.Token); }
                catch { return new HttpResponse { Status = HttpResponseStatus.Disconnected }; }
                if (!res.IsSuccessStatusCode)
                    return new HttpResponse { Status = HttpResponseStatus.Unsuccessful, Message = res };
                Debug.WriteLine($"GET: {url}");
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

                if(form != default)
                    req.Content = new FormUrlEncodedContent(form);

                lock (cancellationSource)
                {
                    if (cancellationSource.IsCancellationRequested)
                        cancellationSource = new();
                }
                HttpResponseMessage res;
                try { res = client.Send(req, cancellationSource.Token); }
                catch { return new HttpResponse { Status = HttpResponseStatus.Disconnected }; }
                if (!res.IsSuccessStatusCode)
                    return new HttpResponse { Status = HttpResponseStatus.Unsuccessful, Message = res };
                Debug.WriteLine($"POST: {url}");
                return new HttpResponse { Status = HttpResponseStatus.Successful, Message = res };
            }
        }
    }
}
