using System.Net;
using System.Net.Sockets;
using ArchTorrent.Core.PeerProtocol;
using ArchTorrent.Core.Torrents;
using ArchTorrent.Core.Trackers.UDPTrackerProtocol;

namespace ArchTorrent.Core.Trackers
{
    public class UdpTracker : Tracker
    {
        public int ListenPort;

        public UdpTracker(Torrent torrent, string announceUrl)
        {
            Torrent = torrent;
            AnnounceUrl = announceUrl;
            AnnounceURI = new Uri(announceUrl);
            CancelTokenSrc = new CancellationTokenSource();
            CancellationToken = CancelTokenSrc.Token;
        }

        public CancellationToken CancellationToken { get; set; }
        private CancellationTokenSource CancelTokenSrc { get; set; }

        /// <summary>
        /// returns an empty list (check result.Count == 0)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public override async Task<List<Peer>> TryGetPeers()
        {
            IPAddress ip = NetworkManager.GetIpFromUri(AnnounceURI);
            var networkManager = NetworkManager.instance;
            List<Peer> defaultRet = new();
            Logger.Log("Begin GetPeers", source: "UdpTracker");

            // connect to host
            Logger.Log($"Connecting to host {AnnounceURI}, host {AnnounceURI.DnsSafeHost}", source: "UdpTracker");
            
            ConnectRequest connectReq = new ConnectRequest();

            Logger.Log($"Sending connect request", source: "UdpTracker");
            
            if(!await networkManager.SendUDPAsync(AnnounceURI, connectReq.Serialize()))
            {
                Logger.Log($"Connection attempt to {AnnounceURI} failed, skipping tracker...");
                return defaultRet;
            }

            byte[]? conResData = await networkManager.WaitForResponseAsync(ip);

            //if (conResData == null) throw new InvalidDataException("Bytes not recieved from UDP request (connection request)");
            //if (conResData.Length < 15) throw new InvalidDataException($"Invalid amount of data recieved, expected: 16; got: {conResData.Length}");
            if (conResData == null)
            {
                Logger.Log($"Bytes not recieved from UDP request (connection request)");
                return defaultRet;
            }
            else if (conResData.Length < 15)
            {
                Logger.Log($"Invalid amount of data recieved, expected: 16; got: { conResData.Length }");
                return defaultRet;
            }

            Logger.Log($"Recieved connection response! parsing 16 bytes...", source: "UdpTracker");
            ConnectResponse connectRes = new ConnectResponse(conResData);

            if (!connectReq.CheckResponse(connectRes))
            {
                Logger.Log($"Invalid data returned");
                return defaultRet;
            }
            Logger.Log($"Recieved response and parsed correctly", source: "UdpTracker");

            AnnounceRequest announceReq = new AnnounceRequest(connectRes.connection_id, Torrent);
            Logger.Log("Built announce request, sending...");
          
            bool conResSent = await networkManager.SendUDPAsync(AnnounceURI ,announceReq.Serialize());
            if (!conResSent)
            { 
                Logger.Log("Unable to send connection response");
                return defaultRet;
            }

            conResData = await networkManager.WaitForResponseAsync(ip);
            // if (conResData == null) throw new InvalidDataException($"Bytes not recieved from UDP request (announce request)");
            if (conResData == null)
            {
                Logger.Log($"Bytes not recieved from UDP request (announce request)");
                return defaultRet;
            }

            Logger.Log($"Sent announce request... parsing bytes", source: "UdpTracker");

            if (conResData.Length < 20)
            {
                Logger.Log("Bad data recieved from announce request");
                return defaultRet;
            }

            AnnounceResponse announceResponse = new(conResData, Torrent.InfoHash);
            Peers = announceResponse.peers;

            Peers.ForEach(peer => Logger.Log($"PEER: {peer}", source: "TryGetPeers"));

            return announceResponse.peers;
        }

        /// <summary>
        /// Attempts to connect a socket to a URI, if a corresponding IP is found, returns true and can continue to go.
        /// </summary>
        private async Task<IPAddress?> FindUriIPAsync(Uri uri)
        {
            Logger.Log($"Start FindUriIPAsync", source: "FindUriIP");
            IPAddress[] addresses = Dns.GetHostAddresses(uri.Host);
            Socket sockv4 = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Exception lastex = null;
            foreach(IPAddress address in addresses)
            {
                try
                {
                    Logger.Log($"Trying to connect to: {address}:{uri.Port}...");
                    var cts = new CancellationTokenSource(500);
                    var token = cts.Token;
                    token.ThrowIfCancellationRequested();
                    await sockv4.ConnectAsync(address, uri.Port, token);
                    Logger.Log($"Found IP for host: {uri.Host}:{address}", Logger.LogLevel.INFO, source:"FindUriIP");
                    sockv4.Close();
                    return address;
                }
                catch (Exception ex)
                {
                    lastex = ex;
                }
            }
            return null;
        }

        /// <summary>
        /// socket implementation
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<byte[]?> ExecuteUdpRequest3(Uri uri, byte[] message)
        {
            byte[]? data = null;
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (message == null) throw new ArgumentNullException(nameof(message));

            int port = await Torrent.PortList.AwaitOpenPort();

            using Socket sock = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {

                // get IP through DNS
                string p = uri.Host;

                Logger.Log($"sending message to {uri.Host}", source: "UdpRequest");

                IPAddress? externalIp = await FindUriIPAsync(uri);

                if (externalIp == null)
                {
                    return null;
                }

                IPEndPoint ep = new IPEndPoint(externalIp, port);
                EndPoint externalEp = new IPEndPoint(externalIp, uri.Port);

                sock.Bind(externalEp);
                
                Logger.Log($"Listening on : {ep}");
                await sock.ConnectAsync(externalEp);

                int numBytesSent = await sock.SendAsync(message, SocketFlags.None);
                Logger.Log($"Sent: {numBytesSent}", source: "UdpRequest");

                byte[] res = new byte[1024];
                //var res = udpClient.BeginReceive(null, null);
                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource(500);
                    var token = cts.Token;
                    token.ThrowIfCancellationRequested();

                    var numBytesReceived = await sock.ReceiveAsync(res, SocketFlags.None, cts.Token);
                    Array.Resize(ref res, numBytesReceived);
                }
                catch (OperationCanceledException)
                {
                    sock.Close();
                    res = null;
                }
                // begin recieve right after request
                if (res != null && res.Length < 1)
                {
                    Logger.Log($"Recieved message from endpoint, returning.", source: "UdpRequest");
                    data = null;
                }
                else if(res == null)
                {
                    Logger.Log($"No Bytes Recieved from UdpRequest", source: "UdpRequest");
                    // here the client just times out.
                }
                sock.Close();
            }
            catch (SocketException ex)
            {
                Torrent.PortList.SetPortUnused(port);
                sock.Close();
                Logger.Log($"Failed UDP tracker message to {uri} for torrent {Torrent.InfoHash}: {ex.Message}");
            }
            Torrent.PortList.SetPortUnused(port);
            return data;
        }

        /// <summary>
        /// UdpClient.ReceiveAsync(...) implementation
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<byte[]?> ExecuteUdpRequest2(Uri uri, byte[] message)
        {
            byte[]? data = null;
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (message == null) throw new ArgumentNullException(nameof(message));

            int port = await Torrent.PortList.AwaitOpenPort();

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);

            try
            {
                using UdpClient udpClient = new(ep);
                
                Logger.Log($"Listening on : {ep}");

                udpClient.Client.SendTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
                udpClient.Client.ReceiveTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

                Logger.Log($"sending message to {uri.Host}, returning.", source: "UdpRequest");

                int numBytesSent = await udpClient.SendAsync(message, message.Length, uri.Host, uri.Port);
                Logger.Log($"Sent: {numBytesSent}", source: "UdpRequest");

                UdpReceiveResult? res;
                //var res = udpClient.BeginReceive(null, null);
                try
                {
                    res = await udpClient.ReceiveAsync();
                }
                catch (TimeoutException)
                {
                    Logger.Log($"!!! Timeout Exception !!!");
                    res = null;
                }

                // begin recieve right after request
                if (res != null && res?.Buffer != null && res?.Buffer.Length > 0)
                {
                    Logger.Log($"Recieved message from endpoint, returning.", source: "UdpRequest");

                    data = res?.Buffer;
                }
                else
                {
                    Logger.Log($"No Bytes Recieved from UdpRequest", source: "UdpRequest");
                    // here the client just times out.
                }
            }
            catch (SocketException ex)
            {
                Logger.Log($"Failed UDP tracker message to {uri} for torrent {Torrent.InfoHash}: {ex.Message}");
            }
            Torrent.PortList.SetPortUnused(port);
            return data;
        }

        /// <summary>
        /// Executes a udp request with a given uri and data
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="message"></param>
        /// <returns>returns a byte array or null on failure or timeout.</returns>
        /// <exception cref="ArgumentNullException">if uri or message is null an ArgumentNull exception will be raised</exception>
        private async Task<byte[]?> ExecuteUdpRequest(Uri uri, byte[] message)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (message == null) throw new ArgumentNullException(nameof(message));

            byte[]? data = null;
            IPEndPoint any = new(IPAddress.Any, ListenPort);

            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.SendTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
                    udpClient.Client.ReceiveTimeout = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;

                    Logger.Log($"sending message to {uri.Host}, returning.", source: "UdpRequest");

                    int numBytesSent = await udpClient.SendAsync(message, message.Length, uri.Host, uri.Port);
                    Logger.Log($"Sent: {numBytesSent}", source: "UdpRequest");

                    var res = udpClient.BeginReceive(null, null);
                    //data = udpClient.EndReceive(res, ref any);
                    // begin recieve right after request
                    if (res.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5)))
                    {
                        Logger.Log($"Recieved message from endpoint, returning.", source: "UdpRequest");

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        data = udpClient.EndReceive(res, ref any);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    }
                    else
                    {
                        Logger.Log($"No Bytes Recieved from UdpRequest", source: "UdpRequest");
                        // here the client just times out.
                    }
                }
            }
            catch(SocketException ex)
            {
                Logger.Log($"Failed UDP tracker message to {uri} for torrent {Torrent.InfoHash}: {ex.Message}");
            }

            return data;
        }
    }
}
