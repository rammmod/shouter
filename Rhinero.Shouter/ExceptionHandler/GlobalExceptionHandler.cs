using Microsoft.AspNetCore.Diagnostics;

namespace Rhinero.Shouter.ExceptionHandler
{
    internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError("General exception occurred: {@exception}", exception);

            return await Task.FromResult(true);
        }
    }

}
