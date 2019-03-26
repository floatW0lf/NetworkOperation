namespace NetworkOperation.Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
    
    public interface IStructuralLogger
    {
        string Name { get; }
        LogLevel MinLogLevel { get; set; }
        void Write(LogLevel level, string message);
        void Write<T>(LogLevel level, string message, T arg);
        void Write<T,T1>(LogLevel level, string message, T arg, T1 arg1);
        void Write(LogLevel level, string message, params object[] args);
    }
}