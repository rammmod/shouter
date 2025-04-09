using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhinero.Shouter.Shared.Extensions
{
    internal static class JsonExtension
    {
        private static readonly JsonSerializerOptions defaultSerializerSettings = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowTrailingCommas = false
        };

        public static string ToJson(this object data) =>
            JsonSerializer.Serialize(data, defaultSerializerSettings);

        public static T FromJson<T>(this string data) =>
            JsonSerializer.Deserialize<T>(data, defaultSerializerSettings);

        public static object FromJson(this string data, Type type) =>
            JsonSerializer.Deserialize(data, type, defaultSerializerSettings);
    }
}
