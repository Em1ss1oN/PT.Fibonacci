using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace PT.Fibonacci
{
    public static class Log4Logger
    {
        private static ILog _log; 
        
        public static ILog Log => _log ?? (_log = InitLogger());

        public static ILog InitLogger()
        {
            XmlConfigurator.Configure();
            return LogManager.GetLogger("Logger");
        }
    }

    public interface ILogger
    {
        void Info(object message);
        void Debug(object message);
        void Warning(object message);
        void Error(object message, Exception exception);
        void Fatal(object message, Exception exception);
    }

    public class Logger : ILogger
    {
        private static Logger _instance;

        public static ILogger Instance => _instance ?? (_instance = new Logger());

        private Logger()
        {
        }

        public void Info(object message)
        {
            Log4Logger.Log.Info(message);
        }

        public void Debug(object message)
        {
            Log4Logger.Log.Debug(message);
        }

        public void Warning(object message)
        {
            Log4Logger.Log.Warn(message);
        }

        public void Error(object message, Exception exception)
        {
            Log4Logger.Log.Error(message, exception);
        }

        public void Fatal(object message, Exception exception)
        {
            Log4Logger.Log.Fatal(message, exception);
        }
    }
}
