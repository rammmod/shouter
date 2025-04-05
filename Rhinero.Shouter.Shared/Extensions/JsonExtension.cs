using System.Text;
using System.Text.Json;

namespace Rhinero.Shouter.Shared.Json
{
    public static class JsonExtension
    {
        private static readonly JsonSerializerOptions defaultSerializerSettings = new JsonSerializerOptions() { WriteIndented = true, PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
        public static string ToJson(this object data)
        {
            return JsonSerializer.Serialize(data, defaultSerializerSettings);
        }

        public static T FromJson<T>(this string data)
        {
            return JsonSerializer.Deserialize<T>(data, defaultSerializerSettings);
        }

        public static object FromJson(this string data, Type type)
        {
            return System.Text.Json.JsonSerializer.Deserialize(data, type, defaultSerializerSettings);

        }

        public static string FromASBankToJson(this string data)
        {
            data = data.Replace("\"", "\\\"");
            data = data.Replace("\'", "\\\'");
            data = data.Trim();

            var arr = data.Split("\r\n");

            StringBuilder strbld = new StringBuilder();

            foreach (string s in arr)
            {
                strbld.Append("\"" + s.Remove(s.IndexOf(':')) + "\": \"" + s.Substring(s.IndexOf(':') + 1) + "\",\n");
            }

            return "{\n" + strbld.ToString().Remove(strbld.ToString().LastIndexOf(',')) + "\n}";
        }

        /// <summary>
        /// This extension method can accept only POCO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string NormalizeJson<T>(this string data) where T : class
        {
            try
            {
                var dataArray = data.Split("\n");

                var propertyInfo = typeof(T).GetProperties();

                foreach (var property in propertyInfo)
                {
                    var typeCode = Type.GetTypeCode(property.PropertyType);
                    var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);

                    if (typeCode == TypeCode.Object)
                        typeCode = Type.GetTypeCode(underlyingType);

                    var propertyName = property.Name;

                    var oneLine = dataArray.Where(m => m.ToLower().Contains("\"" + propertyName.ToLower() + "\"")).FirstOrDefault();
                    var elementIndex = Array.IndexOf(dataArray, oneLine);

                    if (elementIndex != -1)
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Boolean:
                                oneLine = TransformToBoolean(oneLine);
                                break;
                            case TypeCode.Single:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                            case TypeCode.Int16:
                            case TypeCode.UInt32:
                            case TypeCode.Int32:
                            case TypeCode.Double:
                            case TypeCode.UInt64:
                            case TypeCode.Int64:
                            case TypeCode.Decimal:
                                oneLine = TransformToNumber(oneLine);
                                break;
                            case TypeCode.Char:
                            case TypeCode.String:
                                break;
                            case TypeCode.DateTime:
                                oneLine = TransformToDate(oneLine);
                                break;
                            default: break;
                        }

                        dataArray[elementIndex] = oneLine;
                    }
                }

                var strBuilder = new StringBuilder();

                foreach (var line in dataArray)
                {
                    strBuilder.Append(line.ToString());
                }

                return strBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            string TransformToBoolean(string oneLine)
            {
                var indexOfColon = oneLine.IndexOf(':') + 1;
                var firstPart = oneLine.Substring(0, indexOfColon);
                var secondPart = oneLine.Substring(indexOfColon);
                secondPart = secondPart.Replace("\"", "");

                if (secondPart.ToLower().Contains("0"))
                    secondPart = secondPart.Replace("0", "false");
                else if (secondPart.ToLower().Contains("1"))
                    secondPart = secondPart.Replace("1", "true");
                else if (secondPart.ToLower().Contains("false") || secondPart.ToLower().Contains("true"))
                    secondPart = secondPart.ToLower();
                else
                    throw new Exception("Error on method NormalizeJson");

                return firstPart + secondPart;
            }

            string TransformToNumber(string oneLine)
            {
                var indexOfColon = oneLine.IndexOf(':') + 1;
                var firstPart = oneLine.Substring(0, indexOfColon);
                var secondPart = oneLine.Substring(indexOfColon);
                secondPart = secondPart.Replace("\"", "");

                return firstPart + secondPart;
            }

            string TransformToDate(string oneLine)
            {
                var indexOfColon = oneLine.IndexOf(':') + 1;
                var firstPart = oneLine.Substring(0, indexOfColon);
                var secondPart = oneLine.Substring(indexOfColon);

                var indexOfQuotatitionMark = secondPart.IndexOf('"') + 1;

                secondPart = secondPart.Insert(indexOfQuotatitionMark + 6, "-");
                secondPart = secondPart.Insert(indexOfQuotatitionMark + 4, "-");

                return firstPart + secondPart;
            }
        }
    }
}
