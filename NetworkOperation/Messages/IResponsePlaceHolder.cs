namespace NetworkOperation
{
    public interface IResponsePlaceHolder<in TRequest,TResponse> 
    {
        void Fill(ref TResponse response, TRequest request);
    }
}