using System.Net;

namespace ArchTorrent.Core.Trackers.UDPTrackerProtocol
{

    public struct ConnectResponse
    {
        public Int32 action;
        public Int32 transaction_id;
        public Int64 connection_id;

        public static ConnectResponse Parse(byte[] resp)
        {
            ConnectResponse response = new ConnectResponse();
            byte[] action = resp.ReadBytes(0, 4);
            byte[] t_id = resp.ReadBytes(4, 4);
            byte[] c_id = resp.ReadBytes(8, 8);

            response.action = action.DecodeInt32();
            response.transaction_id = t_id.DecodeInt32();
            response.connection_id = c_id.DecodeInt64();

            return response;
        }
    }

}
