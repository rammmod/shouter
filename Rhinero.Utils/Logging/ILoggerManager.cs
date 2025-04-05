//using System;
//using System.Runtime.CompilerServices;

//namespace Rhinero.Utils.Logging
//{   
//    public abstract class ILoggerManager
//    {        
//        //protected static IEasyLogger logger = Log4NetService.Instance.GetLogger(Log4NetService.Instance.GetType());
      
//        public abstract void LogInfo(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0);
//        public abstract void LogWarn(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0);
//        public abstract void LogDebug(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0);
//        public abstract void LogError(string message, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0);

//        public static void Info(string ex, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0)
//        {
//            logger.InfoFormat($" FilePath:{callingClassName} Method:{callingMethodName} LineNumber: {lineNumber} {ex}");
//        }

//        public static void Write(string ex, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null, [CallerLineNumber] int lineNumber = 0)
//        {
//            logger.ErrorFormat($" FilePath:{callingClassName} Method:{callingMethodName} LineNumber: {lineNumber} {ex}");
//        }

//        public static void Write(Exception ex, [CallerMemberName] string callingMethodName = null, [CallerFilePath] string callingClassName = null,[CallerLineNumber] int lineNumber = 0)
//        {
//            logger.ErrorFormat($" FilePath:{callingClassName} Method:{callingMethodName} LineNumber: {lineNumber} {ex}");           
//        }       
//    }
//}
