namespace NetworkOperation.Core.Messages
{
    public interface IResponsePlaceHolder<in TRequest,TResponse> 
    {
        void Fill(ref TResponse response, TRequest request);
    }
}