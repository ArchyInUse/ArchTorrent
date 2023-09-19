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
            response.action = BitConverter.ToInt32(action);
            response.transaction_id = BitConverter.ToInt32(t_id);
            response.connection_id = BitConverter.ToInt64(c_id);

            return response;
        }
    }

}
