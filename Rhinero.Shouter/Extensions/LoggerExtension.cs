using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Rhinero.Shouter.Extensions
{
    internal static class LoggerExtension
    {
        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

        public static void LogConsumeError(this ILogger logger, string contractName, Guid correlationId, Exception exception = null) =>
            logger.LogError(@$"Consume error.
                               Contract name: {contractName},
                               CorrelationId: {correlationId},
                               Error message: {exception?.Message},
                               Inner Exception: {exception?.InnerException?.Message},
                               DateTime: {DateTime.Now} {DateTime.Now.Millisecond} ms");
    }
}