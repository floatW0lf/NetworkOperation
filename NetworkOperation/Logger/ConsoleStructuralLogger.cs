using System;

namespace NetworkOperation.Logger
{
    public class ConsoleStructuralLogger : IStructuralLogger
    {
        public ConsoleStructuralLogger()
        {
        }

        public ConsoleStructuralLogger(string loggerName)
        {
            Name = loggerName;
        }
        
        ConsoleColor GetColorFromLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return ConsoleColor.White;
                case LogLevel.Info:
                    return ConsoleColor.Green;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public string Name { get; } = "None";
        public LogLevel MinLogLevel { get; set; } = LogLevel.Warning;
        
        public void Write(LogLevel level, string message)
        {
            if (level >= MinLogLevel)
            {
                Console.ForegroundColor = GetColorFromLogLevel(level);
                Console.WriteLine(message);
                Console.ResetColor();
            }
            
        }

        public void Write<T>(LogLevel level, string message, T arg)
        {
            if (level >= MinLogLevel)
            {
                Console.ForegroundColor = GetColorFromLogLevel(level);
                Console.WriteLine($"{message} {arg}");
                Console.ResetColor();
            }
        }

        public void Write<T, T1>(LogLevel level, string message, T arg, T1 arg1)
        {
            if (level >= MinLogLevel)
            {
                Console.ForegroundColor = GetColorFromLogLevel(level);
                Console.WriteLine($"{message} {arg} {arg1}");
                Console.ResetColor();
            }
        }

        public void Write(LogLevel level, string message, params object[] args)
        {
            if (level >= MinLogLevel)
            {
                Console.ForegroundColor = GetColorFromLogLevel(level);
                Console.WriteLine(message, args);
                Console.ResetColor();
            }
        }
    }
}