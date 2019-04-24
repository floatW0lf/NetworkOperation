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
            if (!disconnectInfo.AdditionalData.IsNull && disconnectInfo.AdditionalData.AvailableBytes > 0)
            {
                session[SessionConstants.DisconnectBytesPayload] = disconnectInfo.AdditionalData.GetRemainingBytes();
            }
            session[SessionConstants.DisconnectReason] = disconnectInfo.Reason;
        }
    }
}