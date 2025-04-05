using Rhinero.Utils.Common.Http.Extensions;
using Rhinero.Utils.Common.Http.User;
using Rhinero.Utils.Logging;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rhinero.Utils.Common.Http
{
    public class AppHttpClient2
    {
        private readonly string _serviceHostUrl;
        private readonly string _token;
        private readonly NetworkCredential _credentials;
        private readonly string _contentType;

        private static readonly JsonSerializerOptions defaultSerializerSettings = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        public string Cookie { get; set; }
        public NetworkCredential Credentials => _credentials;

        public AppHttpClient2(string serviceHostUrl, string contentType = "application/json")
        {
            _serviceHostUrl = serviceHostUrl;
            _contentType = contentType;
        }

        public AppHttpClient2(string serviceHostUrl, string token, string contentType = "application/json")
        {
            _serviceHostUrl = serviceHostUrl;
            _token = token;
            _contentType = contentType;
        }

        public AppHttpClient2(string serviceHostUrl, NetworkCredential credentials, string contentType = "application/json")
        {
            _serviceHostUrl = serviceHostUrl;
            _credentials = credentials;
            _contentType = contentType;
        }

        public async Task<T> GetAsync<T>(string url) where T : new()
        {
            string responseString = await GetTaskAsync(url);
            return this.Deserialize<T>(responseString);
        }

        public async Task<T> GetAsync<T>(string url, NameValueCollection data) where T : new()
        {
            return await GetAsync<T>(url + data.ToQueryString());
        }

        public T Get<T>(string url) where T : new()
        {
            string responseString = Get(url);
            return this.Deserialize<T>(responseString);
        }

        public T Get<T>(string url, int timeout) where T : new()
        {
            string responseString = Get(url, timeout);
            return this.Deserialize<T>(responseString);
        }

        public T Get<T>(string url, NameValueCollection data) where T : new()
        {
            return Get<T>(url + data.ToQueryString());
        }

        public async Task<T> GetTaskAsync<T>(string url, int timeout) where T : new()
        {
            string responseString = await GetTaskAsync(url, timeout);
            return this.Deserialize<T>(responseString);
        }

        private string Get(string url)
        {
            string responseString = default;

            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                using (var client = new HttpClient(handler))
                {
                    AddHeaders(client);
                    var uri = PrepareUri(url);

                    responseString = Encoding.UTF8.GetString(client.GetByteArrayAsync(uri).GetAwaiter().GetResult());
                    client.AnalyzeResponseHeadersForErrorCode();
                }
            }

            return responseString;
        }

        private async Task<string> GetTaskAsync(string url)
        {
            string responseString = default;

            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                using (var client = new HttpClient(handler))
                {
                    AddHeaders(client);
                    var uri = PrepareUri(url);

                    responseString = Encoding.UTF8.GetString(await client.GetByteArrayAsync(uri));
                    client.AnalyzeResponseHeadersForErrorCode();
                }
            }

            return responseString;
        }

        public async Task<string> GetTaskAsync(string url, int timeout)
        {
            var uri = PrepareUri(url);

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                return await client.GetStringAsync(uri);
            }
        }

        public string Get(string url, int timeout)
        {
            string responseString = default;

            var timer = new Stopwatch();
            timer.Start();
            new LoggerManager(nameof(AppHttpClient2)).LogInfo($"Get Request time: {timer.ElapsedMilliseconds} ms.");

            try
            {
                var uri = PrepareUri(url);

                using (HttpClient client = new HttpClient())
                {
                    // client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                    responseString = client.GetStringAsync(uri).GetAwaiter().GetResult();
                }

                timer.Stop();
                new LoggerManager(nameof(AppHttpClient2)).LogInfo($"Get Response time: {timer.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                timer.Stop();
                new LoggerManager(nameof(AppHttpClient2)).LogInfo($"Get Exception time: {timer.ElapsedMilliseconds} ms.");
            }

            return responseString;
        }

        public T Post<T>(string url, NameValueCollection data) where T : new()
        {
            T response = new T();

            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                using (var client = new HttpClient(handler))
                {
                    AddHeaders(client);
                    var uri = PrepareUri(url);

                    using (var request = new HttpRequestMessage())
                    {
                        request.Content = new StringContent(data.ToJson(defaultSerializerSettings), Encoding.UTF8, _contentType);
                        AddRequestHeaders(request);

                        var responseMessage = client.PostAsync(uri, request.Content).GetAwaiter().GetResult();
                        response = this.Deserialize<T>(responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                        client.AnalyzeResponseHeadersForErrorCode();
                        responseMessage.SetCookie();
                    }
                }
            }

            return response;
        }

        public async Task<T> PostAsync<T>(string url, NameValueCollection data) where T : new()
        {
            T response = new T();

            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                using (var client = new HttpClient(handler))
                {
                    AddHeaders(client);
                    var uri = PrepareUri(url);

                    using (var request = new HttpRequestMessage())
                    {
                        request.Content = new StringContent(uri.ToString() + data.ToQueryString(), Encoding.UTF8, _contentType);
                        AddRequestHeaders(request);

                        var responseMessage = await client.PostAsync(uri, request.Content);
                        response = this.Deserialize<T>(await responseMessage.Content.ReadAsStringAsync());

                        client.AnalyzeResponseHeadersForErrorCode();
                        responseMessage.SetCookie();
                    }
                }
            }

            return response;
        }

        public async Task<T> PostAsync<T>(string url, object data) where T : new()
        {
            T response = new T();

            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                using (var client = new HttpClient(handler))
                {
                    AddHeaders(client);
                    var uri = PrepareUri(url);

                    using (var request = new HttpRequestMessage())
                    {
                        HttpResponseMessage responseMessage;

                        try
                        {
                            await Console.Out.WriteLineAsync($"Calling {url} with data {this.Serialize(data)}");

                            request.Content = new ByteArrayContent(Encoding.Default.GetBytes(this.Serialize(data)));
                            AddRequestHeaders(request);

                            responseMessage = await client.PostAsync(uri, request.Content);
                            var a = await responseMessage.Content.ReadAsStringAsync();
                            response = this.Deserialize<T>(await responseMessage.Content.ReadAsStringAsync());
                        }
                        catch (WebException ex)
                        {
                            using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
                            {
                                string responseContent = r.ReadToEnd();
                                throw new Exception(responseContent, ex);
                            }
                        }

                        client.AnalyzeResponseHeadersForErrorCode();
                        responseMessage.SetCookie();
                    }
                }
            }

            return response;
        }

        public async Task<string> PostAsync(string url, object data)
        {
            string response = default;

            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                using (var client = new HttpClient(handler))
                {
                    AddHeaders(client);
                    var uri = PrepareUri(url);

                    using (var request = new HttpRequestMessage())
                    {
                        request.Content = new ByteArrayContent(Encoding.Default.GetBytes(this.Serialize(data)));
                        AddRequestHeaders(request);

                        var responseMessage = await client.PostAsync(uri, request.Content);
                        response = await responseMessage.Content.ReadAsStringAsync();

                        client.AnalyzeResponseHeadersForErrorCode();
                        responseMessage.SetCookie();
                    }
                }
            }

            return response;
        }

        public string Post(string url)
        {
            string response = default;

            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                using (var client = new HttpClient(handler))
                {
                    AddHeaders(client);
                    var uri = PrepareUri(url);

                    var responseMessage = client.PostAsync(uri, null).GetAwaiter().GetResult();
                    response = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    client.AnalyzeResponseHeadersForErrorCode();
                    responseMessage.SetCookie();
                }
            }

            return response;
        }

        private void AddHeaders(HttpClient client)
        {
            if (_token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            if (_credentials != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(_credentials.UserName + ":" + _credentials.Password)));
            }
        }

        private void AddRequestHeaders(HttpRequestMessage message)
        {
            if (_contentType.Length > 0)
            {
                message.Content.Headers.Add("Content-Type", _contentType);
            }

            if (!string.IsNullOrEmpty(Cookie))
                message.Content.Headers.Add("cookie", Cookie);

            if (ValidUserInfo.Cookie is not null)
                message.Content.Headers.Add(HttpRequestHeader.Cookie.ToString(), ValidUserInfo.Cookie);
        }

        private Uri PrepareUri(string url)
        {
            try
            {
                return new Uri(_serviceHostUrl + url);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + _serviceHostUrl + url);
            }

        }

        private string Serialize<T>(T data)
        {
            return JsonSerializer.Serialize<T>(data, defaultSerializerSettings);
        }

        private T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, defaultSerializerSettings);
        }
    }
}
