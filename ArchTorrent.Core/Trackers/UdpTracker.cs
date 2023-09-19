using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BencodeNET.Parsing;

namespace ArchTorrent.Core.Trackers
{
    public class UdpTracker : Tracker
    {
        public string AnnounceUrl { get; set; }
        public Uri AnnounceURI { get => new Uri(AnnounceUrl, UriKind.Absolute); }
        public UdpTracker(string announceURL)
        {
            AnnounceUrl = announceURL;
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
            catch (SocketException se)
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
            UDPTrackerProtocol.ConnectRequest req = new UDPTrackerProtocol.ConnectRequest();

            { 
                int BytesSent = await sock.SendAsync(req.GetBytes(), SocketFlags.None, CancellationToken);

                if(BytesSent != 16)
                {
                    throw new Exception("Invalid request sent from client");
                }
            }

            byte[] responseBuffer = new byte[16];
            await sock.ReceiveAsync(responseBuffer, SocketFlags.None, CancellationToken);
            UDPTrackerProtocol.ConnectResponse connectResponse = UDPTrackerProtocol.ConnectResponse.Parse(responseBuffer);
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
