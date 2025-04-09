using Microsoft.Extensions.Logging;
using Rhinero.Shouter.Contracts;

namespace Rhinero.Shouter.Shared.Extensions
{
    internal static class LoggerExtension
    {
        public static void LogPublishError(this ILogger logger, string contractName, ShouterMessage message, Exception exception = null) =>
            logger.LogError(@$"Consume error.
                               Contract name: {contractName},
                               Payload: {message.Payload}
                               CorrelationId: {message.CorrelationId},
                               Error message: {exception?.Message},
                               Inner Exception: {exception?.InnerException?.Message},
                               DateTime: {DateTime.UtcNow} {DateTime.UtcNow.Millisecond} ms");
    }
}