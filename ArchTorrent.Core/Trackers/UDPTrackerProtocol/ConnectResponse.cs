using System.Net;

namespace ArchTorrent.Core.Trackers.UDPTrackerProtocol
{

    public struct ConnectResponse
    {
        public Int32 action;
        public Int32 transaction_id;
        public Int64 connection_id;

        public ConnectResponse(byte[] resp)
        {
            action = resp.ReadBytes(0, 4).DecodeInt32();
            transaction_id = resp.ReadBytes(4, 4).DecodeInt32();
            connection_id = resp.ReadBytes(8, 8).DecodeInt64();
            Logger.Log($"Logging response headers (action:4, t_id: 4, c_id: 8: {resp.HexToString()}", source: "Connect Response Constructor");
        }
    }

}
