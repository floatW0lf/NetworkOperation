using IntegrationTests.Contract;
using LiteNetLib;
using NetLibOperation;
using NetworkOperation;
using NetworkOperation.Factories;
using NetworkOperation.Host;

namespace IntegrationTests
{
    public class TestSessionRequestHandler : DefaultLiteSessionOpenHandler
    {
        public string Auth { get; set; } = "token";
        public string version = "1.1";
        
        public TestSessionRequestHandler(BaseSerializer serializer) : base(serializer)
        {
        }

        protected override void OnHandle(SessionRequest request)
        {
            var payload = ReadPayload<ExampleConnectPayload>(request);
            if (payload.Authorize == Auth && payload.Version == version)
            {
                request.Accept(new []{ new SessionProperty("appid", payload.AppId)});
                return;
            }
            
            request.Reject();
        }

        
    }
}