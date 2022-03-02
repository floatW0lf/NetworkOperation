namespace NetworkOperation.Core.Messages
{
    public class NullRequestPlaceHolder<TMessage> : IRequestPlaceHolder<TMessage>
    {
        private NullRequestPlaceHolder(){}
        public static NullRequestPlaceHolder<TMessage> Instance = new NullRequestPlaceHolder<TMessage>();
        public void Fill<T>(ref TMessage request, T op){}
    }
}