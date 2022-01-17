using System;
using System.Linq;
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
        public string ConnectionUri { get; set; }

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
            Debug.Log("Start ws client");

        }

        public async void Connect()
        {
            if (_client?.Current == ClientState.Connected)
            {
                return;
            }
            Debug.Log($"Connect to {ConnectionUri}");
            await _client.ConnectAsync(new Uri(ConnectionUri, UriKind.Absolute), new ConnectPayload(){Version = 100});
            Debug.Log($"State is {_client.Current}");
            //await _client.Executor.Execute(new TestOp2 {Message = "heloo"}, p => p);
            var large = Enumerable.Repeat((byte) 125, 64000).ToArray();
            await _client.Executor.Execute(new LargeDataOperation() {Raw = large}, p => p);
        }

        public async void Disconnect()
        {
           await _client.DisconnectAsync();
        }
    }
}

