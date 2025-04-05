using Rhinero.Utils.Common.Http.User;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Rhinero.Utils.Common.Http.Extensions
{
    public static class HttpClientExtensions
    {
        public static void AnalyzeResponseHeadersForErrorCode(this HttpClient client)
        {
            //WebHeaderCollection responseHeaders = client.ResponseHeaders;
        }

        public static IDictionary<string, string> GetHeadersDict(this HttpResponseMessage response)
        {
            IDictionary<string, string> retDictionary = new Dictionary<string, string>();

            response.Headers.ToList().ForEach(m => retDictionary.Add(m.Key, m.Value.FirstOrDefault()));

            return retDictionary;
        }

        public static void SetCookie(this HttpResponseMessage response)
        {
            try
            {
                IDictionary<string, string> headersDictionary = response.GetHeadersDict();
                ValidUserInfo.Cookie = headersDictionary.ContainsKey("Set-Cookie")? headersDictionary["Set-Cookie"]:null;
            }
            catch { }
        }

        /// <summary>
        /// Converts NameValueCollection to query string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToQueryString(this NameValueCollection data)
        {
            return "?" + string.Join("&", data.AllKeys.Select(a => a + "=" + Uri.EscapeDataString(data[a])));
        }
    }
}
