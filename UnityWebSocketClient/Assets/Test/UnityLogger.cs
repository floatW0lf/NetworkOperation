using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Test
{
    public class UnityLogger<T> : UnityLogger, ILogger<T>
    {
    }
    public class UnityLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Debug.Log(logLevel + " "+ formatter(state,exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }

    public class UnityLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new UnityLogger();
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }
    }
}