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
        LogLevel MinLogLevel { get; }
        void Write(LogLevel level, string message);
#if !AOT
        void Write<T>(LogLevel level, string message, T arg);
        void Write<T,T1>(LogLevel level, string message, T arg, T1 arg1);
#endif
        void Write(LogLevel level, string message, params object[] args);
    }
}