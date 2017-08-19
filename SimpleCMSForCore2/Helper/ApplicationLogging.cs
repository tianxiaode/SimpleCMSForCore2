using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace SimpleCMSForCore2.Helper
{
    public class ApplicationLogging
    {
        private static ILoggerFactory _factory = null;

        public static void ConfigureLogger(ILoggerFactory factory)
        {
            factory.AddDebug(LogLevel.None).AddNLog();
            //factory.AddFile("logFileFromHelper.log"); //serilog file extension
        }

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_factory != null) return _factory;
                _factory = new LoggerFactory();
                ConfigureLogger(_factory);
                return _factory;
            }
            set => _factory = value;
        }
        public static ILogger CreateLogger() => LoggerFactory.CreateLogger("SimpleCMS");
    }
}
