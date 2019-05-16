namespace NetworkOperation.Logger
{
    public interface ILoggerFactory
    {
        IStructuralLogger Create(string name);
    }
}