# NetworkOperation
This is simple RPC Framework.

## Features

- Fully asynchronous code
- Lightweight codebase
- Modular architecture
- Supported platform:
    - Win/Linux/Mac/Android/iOS
    - Unity 2018+
    - WebGL
- Production ready (Used by [Exile: Survival Games Online](https://play.google.com/store/apps/details?id=com.pgstudio.exile.survival))
- Easy to implement custom transport level

## Installation
### For client:
```
PM> Install-Package NetworkOperation.Infrastructure.Client.LiteNet
```
Or
```
dotnet add package NetworkOperation.Infrastructure.Client.LiteNet
```
### For Server:
```
PM> Install-Package NetworkOperation.Infrastructure.Host.LiteNet
```
Or
```
dotnet add package NetworkOperation.Infrastructure.Host.LiteNet
```

## Usage samples
### Shared
```csharp
[Operation(1000, Handle = Side.Server)]
public struct PlusOperation : IOperation<int>
{
    public int A { get; set; }
    public int B { get; set; }
}
[Operation(1001, Handle = Side.Client)]
public struct SendToClientMessage : IOperation<Empty>
{
    public string Message { get; set; }
}
```
### Client
```csharp

public class SendToClientMessageHandler : IHandler<SendToClientMessage, Empty, DefaultMessage>
{
    public async Task<OperationResult<Empty>> Handle(SendToClientMessage objectData, RequestContext<DefaultMessage> context, CancellationToken token)
    {
        Console.WriteLine(objectData.Message);
        return this.ReturnEmpty();
    }
}

//infrastructure code
var clientCollection = new ServiceCollection();
            
clientCollection
    .NetworkOperationClient<DefaultMessage, DefaultMessage>()
    .Serializer<MsgSerializer>()
    .Executor()
    .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
    .Dispatcher<ExpressionDispatcher<DefaultMessage,DefaultMessage>>()
    .RegisterHandlers(AppDomain.CurrentDomain.GetAssemblies())
    .UseLiteNet();

var client = clientCollection.BuildServiceProvider(false).GetService<IClient>();

//user code
await client.ConnectAsync("localhost:8888");
var result = await client.Executor.Execute<PlusOperation, int>(new PlusOperation(){A = 2, B = 40});
result = await client.Executor.Execute(new PlusOperation(){A = 2, B = 40}, t => t); //with automatic inference of the operation result type
if (result.Is(BuiltInOperationState.Success))
{
    Console.WriteLine($"Operation result is {result.Result}");
}
await client.DisconnectAsync();
```
### Server
```csharp
public class PlusOperationHandler : IHandler<PlusOperation, int, DefaultMessage>
{
    public async Task<OperationResult<int>> Handle(PlusOperation objectData, RequestContext<DefaultMessage> context, CancellationToken token)
    {
        var result = objectData.A + objectData.B;
        await Task.Delay(1000, token);
        return this.Return(result);
    }
}

//infrastructure code
var hostCollection = new ServiceCollection();
hostCollection.NetworkOperationHost<DefaultMessage, DefaultMessage>()
    .Executor()
    .Serializer<MsgSerializer>()
    .ConnectHandler<DefaultLiteSessionOpenHandler>()
    .Dispatcher<ExpressionDispatcher<DefaultMessage, DefaultMessage>>()
    .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
    .RegisterHandlers(AppDomain.CurrentDomain.GetAssemblies())
    .UseLiteNet();

var provider = hostCollection.BuildServiceProvider(false);
var hostedService = provider.GetService<IHostedService>();
var context = provider.GetService<IHostContext>();

//user code
await hostedService.StartAsync(CancellationToken.None);            
context.Sessions.SessionOpened += async session =>
{
    await context.Executor.Execute<SendToClientMessage, Empty>(new SendToClientMessage() { Message = "Hello world" }, new [] {session});
};

await hostedService.StopAsync(CancellationToken.None); // if need shutdown server
```
More samples in project IntegrationTests
