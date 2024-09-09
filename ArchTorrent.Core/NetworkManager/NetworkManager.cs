using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core
{
    /// <summary>
    /// This class is going to manage network listening, each request is going to *have* to have an identifier
    /// good enough for the response data to correspond to the request data.
    /// 
    /// For example - UDPTrackers - will be able to corelate a response and a request based on the transaction ID
    /// 
    /// A tracker and or peer should be able to use this class to subscribe to messages recieved.
    /// 
    /// Subscriptions are based on the IP *address* (NOT endpoint) that we send the tracker or peer protocol
    /// messages to. Because the torrent
    /// </summary>
    public class NetworkManager
    {

        /// <summary>
        /// The low and high range limits for port listening are the ports the network manager will use for
        /// sockets, it recommended to leave these unchanged
        /// note: port 6890 will be used for the HTTP client as to not interfere with the UDP trackers.
        /// </summary>
        public int HighRangeLimit = 6889;
        /// <summary>
        /// The low and high range limits for port listening are the ports the network manager will use for
        /// sockets, it recommended to leave these unchanged.
        /// </summary>
        public int LowRangeLimit = 6881;

        /// <summary>
        /// The network manager uses sockets which cannot be initialized on the same port
        /// therefore only one instance should exist at once, if this value is set to true and another
        /// instance is initialized, the program should stop.
        /// </summary>
        private static bool InstanceExists = false;
        public static NetworkManager instance;

        /// <summary>
        /// Timeout for each request until it returns automatically.
        /// This value is in milliseconds (1000ms = 1s)
        /// Defaults to 1 second.
        /// </summary>
        public static int Timeout = 1000;

        /// <summary>
        /// Buffer sizes for the incoming traffick must be set at some interval, the bitTorrent specification
        /// specifies 32KB as the correct answer to this, it is not recommended to change this value.
        /// Important to note, this number is in bytes (as in- the size of a byte[]).
        /// </summary>
        public static int BufferSize = 32768;

        private Dictionary<Socket, byte[]> buffers;
        private Dictionary<IPAddress, Action<byte[]>> MessageActions;
        private Dictionary<int, bool> OccupiedPorts;
        
        private List<Socket> sockets;
        
        public NetworkManager()
        {
            if (InstanceExists) Kill("An instance of NetworkManager already exists.");

            sockets = new List<Socket>();
            buffers = new Dictionary<Socket, byte[]>();
            MessageActions = new Dictionary<IPAddress, Action<byte[]>>();
            OccupiedPorts = new Dictionary<int, bool>();

            // UDP & TCP sockets cannot operate on the same port, therefor we need to open them with unknown type and protocol
            for(int i = LowRangeLimit; i <= HighRangeLimit; i++)
            {
                OccupiedPorts[i] = false;
                var s = new Socket(AddressFamily.InterNetwork, SocketType.Unknown, ProtocolType.Unknown);
                s.Bind(new IPEndPoint(IPAddress.Any, i));
                s.BeginAccept(ConnectionAcceptedCallback, s);
                sockets.Add(s);
            }

            InstanceExists = true;
            instance = this;
        }

        #region Response
        private void ConnectionAcceptedCallback(IAsyncResult ar)
        {
            Socket? s = ar.AsyncState as Socket;
            if (s == null)
            {
                Logger.Log($"Async state is null in connection accepted callback.", source: "NetworkManager");
                return;
            }
            buffers.Add(s, new byte[BufferSize]);
            int port = ((s.LocalEndPoint as IPEndPoint)?.Port) ?? -1;
            if (port == -1) Kill("LocalEndPoint is null in ConnectionAcceptedCallback.");
            OccupiedPorts[port] = true;
            s.BeginReceive(buffers[s], 0, BufferSize, SocketFlags.None, MessageRecievedCallback, s);
        }

        private void MessageRecievedCallback(IAsyncResult ar)
        {
            Socket? s = ar.AsyncState as Socket;
            if(s == null)
            {
                Logger.Log("Async state is null in message recieved callback.", source: "NetworkManager");
                return;
            }
            // not null for sure as a null check is handled beforehand.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            IPAddress ip = (s.RemoteEndPoint as IPEndPoint).Address;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            if (MessageActions.Keys.Contains(ip))
            {
                int bytesRecieved = s.EndReceive(ar);
                byte[] r = buffers[s];
                Array.Resize(ref r, bytesRecieved);
                byte[] results = new byte[bytesRecieved];
                Array.Copy(r, results, bytesRecieved);
                

                // reset buffers and socket
                buffers[s] = new byte[BufferSize];
                int port = ((s.LocalEndPoint as IPEndPoint)?.Port) ?? -1;
                if (port == -1) Kill("Socket LocalEP is null in MessageReceivedCallback.");
                s.Close();
                int index = sockets.IndexOf(s);

                var newSock = new Socket(AddressFamily.InterNetwork, SocketType.Unknown, ProtocolType.Unknown);
                newSock.Bind(new IPEndPoint(IPAddress.Any, port));
                sockets[index] = newSock;
                newSock.BeginAccept(ConnectionAcceptedCallback, newSock);

                MessageActions[ip].Invoke(results);
                MessageActions.Remove(ip);
                return;
            }
        }
        
        /// <summary>
        /// Subscribe to a message recieved with a callback handle (not async, does not account for time)
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="callback"></param>
        public void Subscribe(IPAddress ip, Action<byte[]> callback)
        {
            MessageActions.Add(ip, callback);
        }

        /// <summary>
        /// Waits for a response from an IP address and returns the bytes, if NetworkManager.Timeout ms have passed, returns an empty array.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<byte[]> WaitForResponseAsync(IPAddress ip)
        {
            byte[] result = null;
            CancellationTokenSource ct = new CancellationTokenSource(Timeout);
            MessageActions.Add(ip, (byte[] bytes) =>
            {
                result = bytes;
                ct.Cancel();
            });
            await Task.Delay(TimeSpan.FromSeconds(Timeout), ct.Token);
            if (result == null) return Array.Empty<byte>();
            return result;
        }


        private void Kill(string message)
        {
            // TODO: Proper error handling is needed here, the (generic) Exception is intended for now.
            Logger.Log($"[CRITICAL KILL] {message}", source: "NetworkManager");
            sockets.ForEach(s => s.Close());
            throw new Exception("CRITICAL KILL");
        }

        public static IPAddress GetIpFromUri(Uri uri) => Dns.GetHostAddresses(uri.Host)[0];

        /// <summary>
        /// Gets the last unoccupied port (between the range of ports)
        /// </summary>
        /// <returns></returns>
        public int GetUnoccupiedPort()
        {
            if (OccupiedPorts.All(x => x.Value)) return -1;

            // unlike UDP, the HTTP services require a port as a parameter of the GET request
            // it is best to leave the last port unused so the port can always be used for HTTP requests.
            // TODO: test this in action using UI (WPF) to see which ports are usually picked by UDP services
            return OccupiedPorts.Last(x => !x.Value).Key;
        }

        #endregion

        #region Requests

        public async Task<bool> SendUDPAsync(Uri uri, byte[] data)
        {
            using UdpClient udpClient = new UdpClient();
            var bytesSent = await udpClient.SendAsync(data, data.Length, uri.Host, uri.Port);
            return bytesSent != 0;
        }

        #endregion
    }
}
