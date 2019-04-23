using LiteNetLib;
using NetworkOperation;

namespace NetLibOperation
{
    public static class SessionExtensions
    {
        public static byte[] GetPayload(this Session session)
        {
            return (byte[]) session[SessionConstants.DisconnectBytesPayload];
        }

        public static DisconnectReason? GetReason(this Session session)
        {
            return session[SessionConstants.DisconnectReason] as DisconnectReason?;
        }
        
        internal static void FillDisconnectInfo(this Session session, DisconnectInfo disconnectInfo)
        {
            
            if (!disconnectInfo.AdditionalData.IsNull && disconnectInfo.AdditionalData.UserDataSize > 0)
            {
                var payload = new byte[disconnectInfo.AdditionalData.UserDataSize];
                disconnectInfo.AdditionalData.GetBytes(payload, disconnectInfo.AdditionalData.UserDataOffset,
                    disconnectInfo.AdditionalData.UserDataSize);
                session[SessionConstants.DisconnectBytesPayload] = payload;
            }
            
            session[SessionConstants.DisconnectReason] = disconnectInfo.Reason;
        }
    }
}