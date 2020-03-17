using IntegrationTests.Contract;
using NetLibOperation;
using NetworkOperation.Core;
using NetworkOperation.Host;
using NetworkOperation.LiteNet.Host;

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
            if (request.RequestPayload.Count > 1)
            {
                var payload = ReadPayload<ExampleConnectPayload>(request);
                if (payload.Authorize == Auth && payload.Version == version)
                {
                    request.Accept(new []{ new SessionProperty("appid", payload.AppId)});
                    return;
                }
                request.Reject();
                return;
            }
            request.Accept(new SessionProperty[0]);
        }

        
    }
}