using System;
using System.Reflection;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Infrastructure.Client;
using NetworkOperation.Infrastructure.Client.WebSockets;
using NetworkOperations.Dispatching;
using Test;
using UnityEngine;

namespace WebGL.WebSockets.Tests
{
    public class WebSocketTest : MonoBehaviour
    {
        private ServiceProvider _serviceProvider;
        private IClient _client;

        // Start is called before the first frame update
        void Start()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<ILoggerFactory, UnityLoggerFactory>();
            collection.AddSingleton(typeof(ILogger<>), typeof(UnityLogger<>));
            
            collection.NetworkOperationClient<DefaultMessage, DefaultMessage>()
                .Executor()
                .RegisterStatusCodes()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
                .RegisterHandlers(new []{typeof(WebSocketTest).Assembly})
                .Serializer<MsgSerializer>()
                .UseWebSockets()
                .Dispatcher<PreGeneratedDispatcher<DefaultMessage,DefaultMessage>>()
                ;

            _serviceProvider = collection.BuildServiceProvider(false);
            _client = _serviceProvider.GetService<IClient>();

        }

        // Update is called once per frame
        async void Update()
        {
            if (_client?.Current == ClientState.Connected)
            {
                return;
            }
            await _client.ConnectAsync(new Uri("ws://localhost:7777"), new ConnectPayload());
            await Task.Delay(100);
        }
    }
}

