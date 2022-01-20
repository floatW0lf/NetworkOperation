using NetworkOperation.Core;
using NetworkOperation.Core.Factories;

namespace NetworkOperation.WebSockets.Host
{
    public class CollectionFactory : IFactory<NoneConnectionCollection, MutableSessionCollection>
    {
        public MutableSessionCollection Create(NoneConnectionCollection arg)
        {
            return new WebSocketCollection();
        }
    }
    public struct NoneConnectionCollection { }
}