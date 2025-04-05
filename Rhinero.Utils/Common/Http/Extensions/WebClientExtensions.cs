using Rhinero.Utils.Common.Http.User;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace Rhinero.Utils.Common.Http.Extensions
{
    public static class WebClientExtensions
    {
        public static void AnalizeResponseHeadersForErrorCode(this WebClient client)
        {
            //WebHeaderCollection responseHeaders = client.ResponseHeaders;
        }

        public static IDictionary<string, string> GetHeadersDict(this WebClient client)
        {
            string[] headerKeys = client.ResponseHeaders.AllKeys;
            IDictionary<string, string> retDictionary = new Dictionary<string, string>();
            foreach (string key in headerKeys)
            {
                retDictionary.Add(key, client.ResponseHeaders[key]);
            }

            return retDictionary;
        }

        public static void SetCookie(this WebClient client)
        {
            try
            {
                IDictionary<string, string> headersDictionary = client.GetHeadersDict();
                ValidUserInfo.Cookie = headersDictionary.ContainsKey("Set-Cookie")? headersDictionary["Set-Cookie"]:null;
            }
            catch { }
        }
    }
}
