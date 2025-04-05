using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Rhinero.Utils.Common.Http.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection source)
        {
            return source == null ? null : source.AllKeys.ToDictionary(k => k, k => source[k]);
        }

        public static string ToJson(this NameValueCollection source, JsonSerializerOptions options = null)
        {
            if (source == null) return null;
            var dict = source.ToDictionary();
            return JsonSerializer.Serialize(dict, options);
        }
    }
}
