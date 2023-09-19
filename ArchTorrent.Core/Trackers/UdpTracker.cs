using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchTorrent.Core.Torrents;
using ArchTorrent.Core.Trackers.UDPTrackerProtocol;
using BencodeNET.Parsing;

namespace ArchTorrent.Core.Trackers
{
    public class UdpTracker : Tracker
    {
        public string AnnounceUrl { get; set; }
        public Uri AnnounceURI { get => new Uri(AnnounceUrl, UriKind.Absolute); }
        public Torrent Torrent { get; set; }

        public List<Peer> Peers { get; set; } = new List<Peer>();

        public UdpTracker(Torrent torrent)
        {
            Torrent = torrent;
            AnnounceUrl = torrent.AnnounceURL;
            CancelTokenSrc = new CancellationTokenSource();
            CancellationToken = CancelTokenSrc.Token;
        }

        public CancellationToken CancellationToken { get; set; }
        private CancellationTokenSource CancelTokenSrc { get; set; }

        public async Task GetPeers()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPHostEntry hostInfo;
            try
            {
                hostInfo = Dns.GetHostEntry(AnnounceURI.DnsSafeHost);
            }
            catch (SocketException)
            {
                Logger.Log($"Tried sending udp message to {AnnounceURI.DnsSafeHost}, sockException occured.");
                Logger.Log($"Trying fixed host option...");
                hostInfo = Dns.GetHostEntry(GetFixedHost(AnnounceURI.DnsSafeHost));
            }
            IPAddress ipAddr = hostInfo.AddressList[0];
            int port = AnnounceURI.Port;

            // connect to host
            await sock.ConnectAsync(ipAddr, port);

            // this should be in its own function
            ConnectRequest connectReq = new ConnectRequest();

            { 
                int BytesSent = await sock.SendAsync(connectReq.GetBytes(), SocketFlags.None, CancellationToken);

                if(BytesSent != ConnectRequest.BYTE_COUNT)
                {
                    throw new Exception("Invalid connect request sent from client");
                }
            }

            byte[] responseBuffer = new byte[16];
            await sock.ReceiveAsync(responseBuffer, SocketFlags.None, CancellationToken);
            ConnectResponse connectRes = ConnectResponse.Parse(responseBuffer);

            if (!connectReq.CheckResponse(connectRes)) throw new Exception("Invalid data returned");

            AnnounceRequest announceReq = new AnnounceRequest(connectRes.connection_id, Torrent);

            {
                int bytesSent = await sock.SendAsync(announceReq.Serialize(), SocketFlags.None, CancellationToken);

                if(bytesSent != AnnounceRequest.BYTE_COUNT)
                {
                    throw new Exception("Invalid announce request sent from client");
                }
            }

            responseBuffer = new byte[1024];
            int bytesRes = await sock.ReceiveAsync(responseBuffer, SocketFlags.None, CancellationToken);

            if(bytesRes < 20)
            {
                throw new Exception("Bad data recieved from announce request");
            }

            AnnounceResponse announceResponse = new AnnounceResponse(responseBuffer.Take(bytesRes).ToArray());

            Peers = announceResponse.peers;
        }

        /// <summary>
        /// Removes the beginning part of a host (www.google.com -> google.com)
        /// </summary>
        private string GetFixedHost(string announceUri)
        {
            // matches 1.2.3 and removes 1.
            Regex rx = new Regex(@"\w+\.\w+\.\w+");
            if(rx.IsMatch(announceUri))
            {
                // removes everything behind . (+ 1 including the host
                return announceUri.Remove(0, announceUri.IndexOf('.') + 1);
            }

            return announceUri;
        }

    }
}
