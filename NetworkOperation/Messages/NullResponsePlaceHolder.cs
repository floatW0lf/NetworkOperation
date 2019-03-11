namespace NetworkOperation
{
    public class NullResponsePlaceHolder<TRequest,TResponse> : IResponsePlaceHolder<TRequest,TResponse>
    {
        public static readonly NullResponsePlaceHolder<TRequest,TResponse> Instance = new NullResponsePlaceHolder<TRequest, TResponse>();
        private NullResponsePlaceHolder()
        {
        } 
        public void Fill(ref TResponse response, TRequest request){}
    }
}