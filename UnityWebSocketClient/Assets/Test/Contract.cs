using MessagePack;
using NetworkOperation.Core;

namespace WebGL.WebSockets.Tests
{
    [Operation(0, Handle = Side.Server)]
    [MessagePackObject]
    public struct TestOp : IOperation<int>
    {
        [Key(0)] public int A;
        [Key(1)] public int B;
    }
    
    [Operation(1, Handle = Side.Server)]
    [MessagePackObject]
    public struct TestOp2 : IOperation<string>
    {
        [Key(0)] public string Message;
    }

    [Operation(2, Handle = Side.Client)]
    public struct ClientOp : IOperation<Empty>
    {
        [Key(0)] public string Message;
    }

    [MessagePackObject]
    public struct ConnectPayload : IConnectPayload
    {
        
    }

    [MessagePackObject]
    public struct DisconnectPayload : IDisconnectPayload
    {
        
    }
    
}