//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using System.Text;

//namespace Rhinero.Utils.Logging
//{
//    public class FunctionProfiler : IDisposable
//    {
//        // Flag: Has Dispose already been called?
//        bool disposed = false;

//        string message = "";
//        string callingMethodName = null;
//        string callingClassName = null;
//        int lineNumber = 0;
//        Stopwatch timer = new Stopwatch();

//        public static FunctionProfiler Create(string _message = "", [CallerMemberName] string _callingMethodName = null, [CallerFilePath] string _callingClassName = null, [CallerLineNumber] int _lineNumber = 0)
//        {
//            return new FunctionProfiler(_message, _callingMethodName, _callingClassName, _lineNumber);
//        }

//        public FunctionProfiler (string _message, [CallerMemberName] string _callingMethodName = null, [CallerFilePath] string _callingClassName = null, [CallerLineNumber] int _lineNumber = 0)
//        {
//            message = _message;

//            callingMethodName = _callingMethodName;
//            callingClassName = _callingClassName;
//            lineNumber = _lineNumber;
            
//            timer.Start();
//        }

//        // Public implementation of Dispose pattern callable by consumers.
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        // Protected implementation of Dispose pattern.
//        protected virtual void Dispose(bool disposing)
//        {
//            if (disposed)
//                return;

//            if (disposing)
//            {
//                // Free any other managed objects here.
//                //
//                //_locker.ExitUpgradeableReadLock();
//            }

//            timer.Stop();
//            new LoggerManager().LogInfo(message + ": took " + timer.ElapsedMilliseconds.ToString() + " ms.", callingMethodName, callingClassName, lineNumber);

//            disposed = true;
//        }

//        ~FunctionProfiler()
//        {
//            Dispose(false);
//        }        
//    }
//}
