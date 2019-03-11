namespace NetworkOperation
{
    public interface IRequestPlaceHolder<TRequest>
    {
        void Fill<T>(ref TRequest request, T operation);
    }
}