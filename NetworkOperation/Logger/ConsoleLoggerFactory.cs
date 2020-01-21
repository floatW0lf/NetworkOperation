namespace NetworkOperation.Logger
{
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        public IStructuralLogger Create(string name)
        {
            return new ConsoleStructuralLogger(name);
        }
    }
}