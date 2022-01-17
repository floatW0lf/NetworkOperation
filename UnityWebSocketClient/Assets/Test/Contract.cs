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

    [Operation(2, Handle = Side.Client, WaitResponse = false)]
    [MessagePackObject]
    public struct ClientOp : IOperation<Empty>
    {
        [Key(0)] public string Message;
    }

    [Operation(3, Handle = Side.All, WaitResponse = true)]
    [MessagePackObject]
    public struct LargeDataOperation : IOperation<int>
    {
        [Key(0)] public byte[] Raw;
    }

    [MessagePackObject]
    public struct ConnectPayload : IConnectPayload
    {
        [Key(0)] public int Version;
    }

    [MessagePackObject]
    public struct DisconnectPayload : IDisconnectPayload
    {
        
    }
    
}