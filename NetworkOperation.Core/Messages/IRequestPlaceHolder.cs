namespace NetworkOperation.Core.Messages
{
    public interface IRequestPlaceHolder<TRequest>
    {
        void Fill<T>(ref TRequest request, T operation);
    }
}