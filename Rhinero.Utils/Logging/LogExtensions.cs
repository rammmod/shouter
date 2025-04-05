using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Rhinero.Utils.Logging
{
    public static class LogExtensions
    {
        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() { WriteIndented = true};

        public static void LogInformation(this ILogger logger, object obj)
        {
            logger.LogInformation(JsonSerializer.Serialize(obj, _jsonOptions), null);
        }        
    }
}
