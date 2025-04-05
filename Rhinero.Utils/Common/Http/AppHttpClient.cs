using Rhinero.Utils.Common.Http.Extensions;
using Rhinero.Utils.Common.Http.User;
using Rhinero.Utils.Logging;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rhinero.Utils.Common.Http
{
    public class AppHttpClient
    {
        private readonly string _serviceHostUrl;
        private readonly string _token;
        private readonly NetworkCredential _credentials;
        
        private static readonly JsonSerializerOptions defaultSerializerSettings = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull};
        
        public string Cookie { get; set; }
        public NetworkCredential Credentials => _credentials;

        public AppHttpClient(string serviceHostUrl)
        {
            _serviceHostUrl = serviceHostUrl;
        }

        public AppHttpClient(string serviceHostUrl, string token)
        {
            _serviceHostUrl = serviceHostUrl;
            _token = token;
        }

        public AppHttpClient(string serviceHostUrl, NetworkCredential credentials)
        {
            _serviceHostUrl = serviceHostUrl;
            _credentials = credentials;
        }

        public async Task<T> GetAsync<T>(string url) where T : new()
        {
            string resp = await GetTaskAsync(url);

            T response = this.Deserialize<T>(resp);

            return response;
        }

        public async Task<T> GetAsync<T>(string url, NameValueCollection data) where T : new()
        {
            string q = String.Join("&", data.AllKeys.Select(a => a + "=" + Uri.EscapeDataString(data[a])));
            return await GetAsync<T>(url + "?" + q);
        }

        public T Get<T>(string url) where T : new()
        {

            string resp = Get(url);

            T response = this.Deserialize<T>(resp);

            return response;
        }

        public T Get<T>(string url, int timeout) where T : new()
        {
            string resp = Get(url, timeout);

            T response = this.Deserialize<T>(resp);

            return response;
        }

        public T Get<T>(string url, NameValueCollection data) where T : new()
        {
            string q = String.Join("&", data.AllKeys.Select(a => a + "=" + Uri.EscapeDataString(data[a])));
            return Get<T>(url + "?" + q);
        }
        
        public async Task<T> GetTaskAsync<T>(string url, int timeout) where T : new()
        {
            string resp = await GetTaskAsync(url, timeout);

            T response = this.Deserialize<T>(resp);

            return response;
        }

        private string Get(string url)
        {
            string resp = "";
            using (var client = new WebClient())
            {
                AddHeaders(client);
                PrepareUrl(ref url);
                try
                {
                    resp = client.DownloadString(url);
                    client.AnalizeResponseHeadersForErrorCode();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return resp;

        }
        
        private async Task<string> GetTaskAsync(string url)
        {
            string resp = "";

            using (var client = new WebClient())
            {
                AddHeaders(client);
                PrepareUrl(ref url);
                Uri address = new Uri(url);
                resp = await client.DownloadStringTaskAsync(address);
                client.AnalizeResponseHeadersForErrorCode();
            }

            return resp;
        }
        public async Task<string> GetTaskAsync(string url, int timeout)
        {
            string resp = "";
            PrepareUrl(ref url);
            Uri address = new Uri(url);

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                resp = await client.GetStringAsync(address);
            }

            return resp;
        }
        
        public string Get(string url, int timeout)
        {
            var timer = new Stopwatch();
            timer.Start();
            new LoggerManager(nameof(AppHttpClient)).LogInfo("Get Request time: " + timer.ElapsedMilliseconds.ToString() + " ms.");

            string resp = "";
            try
            {
                PrepareUrl(ref url);
                Uri address = new Uri(url);

                using (HttpClient client = new HttpClient())
                {
                    // client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                    resp = client.GetStringAsync(address).Result;
                }

                timer.Stop();
                new LoggerManager(nameof(AppHttpClient)).LogInfo("Get Response time: " + timer.ElapsedMilliseconds.ToString() + " ms.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                timer.Stop();
                new LoggerManager(nameof(AppHttpClient)).LogInfo("Get Exception time: " + timer.ElapsedMilliseconds.ToString() + " ms.");
            }

            return resp;
        }

        public T Post<T>(string url, NameValueCollection data) where T : new()
        {
            T response = new T();

            using (var client = new WebClient())
            {
                AddHeaders(client);
                PrepareUrl(ref url);

                byte[] responseBytes = client.UploadData(url, "POST", Encoding.Default.GetBytes(data.ToJson(defaultSerializerSettings)));
                response = this.Deserialize<T>(Encoding.Default.GetString(responseBytes));

                client.AnalizeResponseHeadersForErrorCode();
                client.SetCookie();
            }

            return response;
        }

        public async Task<T> PostAsync<T>(string url, NameValueCollection data) where T : new()
        {
            T response = new T();

            using (var client = new WebClient())
            {
                AddHeaders(client, "");
                PrepareUrl(ref url);

                byte[] responseBytes = await client.UploadValuesTaskAsync(url, "POST", data);
                //var d = JsonSerializer.Deserialize<dynamic>(responseBytes, defaultSerializerSettings);
                response = this.Deserialize<T>(Encoding.Default.GetString(responseBytes));

                client.AnalizeResponseHeadersForErrorCode();
                client.SetCookie();
            }

            return response;
        }

        public async Task<T> PostAsync<T>(string url, object data) where T : new()
        {
            T response = new T();

            using (var client = new WebClient())
            {
                AddHeaders(client);
                PrepareUrl(ref url);
                try
                {
                    Console.WriteLine($"Calling {url} with data {this.Serialize(data)}");
                    byte[] responseBytes = await client.UploadDataTaskAsync(url, "POST", Encoding.Default.GetBytes(this.Serialize(data)));                    
                    response = this.Deserialize<T>(Encoding.Default.GetString(responseBytes));
                }
                catch(WebException ex)
                {
                    using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string responseContent = r.ReadToEnd();
                        throw new Exception(responseContent, ex);
                    }
                }
                client.AnalizeResponseHeadersForErrorCode();
                client.SetCookie();
            }

            return response;
        }

        public async Task<string> PostAsync(string url, object data)
        {
            string response;

            using (var client = new WebClient())
            {
                AddHeaders(client);
                PrepareUrl(ref url);

                byte[] responseBytes = await client.UploadDataTaskAsync(url, "POST", Encoding.Default.GetBytes(this.Serialize(data)));
                response = Encoding.Default.GetString(responseBytes);

                client.AnalizeResponseHeadersForErrorCode();
                client.SetCookie();
            }

            return response;
        }

        public string Post(string url)
        {
            string response;
            using (var client = new WebClient())
            {
                AddHeaders(client);
                PrepareUrl(ref url);

                byte[] responseBytes = client.UploadData(url, "POST", Encoding.Default.GetBytes(string.Empty));
                response = responseBytes.ToString();

                client.AnalizeResponseHeadersForErrorCode();
                client.SetCookie();
            }

            return response;
        }

        private void AddHeaders(WebClient client, string contentType = "application/json")
        {
            if (_token != null)
            {
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + _token);
            }

            if (_credentials != null)
            {
                client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(_credentials.UserName + ":" + _credentials.Password)));
            }                        

            if (contentType.Length > 0)
            {
                client.Headers["Content-Type"] = contentType;
            }

            if (!string.IsNullOrEmpty(Cookie))
                client.Headers.Add("cookie", Cookie);

            client.Headers.Add(HttpRequestHeader.Cookie, ValidUserInfo.Cookie);

            client.Encoding = Encoding.UTF8;

            //client.Proxy = null;
        }

        private void PrepareUrl(ref string url)
        {
            try
            {
            var temp = new Uri(_serviceHostUrl + url);
            url = temp.ToString();
            }
            catch(Exception ex){
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
