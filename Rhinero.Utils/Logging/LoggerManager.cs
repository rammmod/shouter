using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Rhinero.Utils.Logging
{
    public class LoggerManager// : ILoggerManager
    {
        private readonly ILogger _logger;

        public LoggerManager(string categoryName = "LoggerExtension")
        {
            _logger = new LoggerFactory().CreateLogger(categoryName);
        }

        public void LogDebug(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogDebug($" FilePath:{callingClassName} Method:{callingMethodName} LineNumber: {lineNumber} {message}");            
        }

        public void LogError(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogError($" FilePath:{callingClassName} Method:{callingMethodName} LineNumber: {lineNumber} {message}");            
        }

        public void LogInfo(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogInformation($" FilePath:{callingClassName} Method:{callingMethodName} LineNumber: {lineNumber} {message}");            
        }

        public void LogWarn(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogWarning($" FilePath:{callingClassName} Method:{callingMethodName} LineNumber: {lineNumber} {message}");            
        }
    }
}
