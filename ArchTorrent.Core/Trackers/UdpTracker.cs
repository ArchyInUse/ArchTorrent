﻿using System;
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
        public Uri AnnounceURI { get; set; }
        public Torrent Torrent { get; set; }

        public const int LISTENPORT = 6881;

        public List<Peer> Peers { get; set; } = new List<Peer>();

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
        public async Task<List<Peer>> TryGetPeers()
        {
            List<Peer> defaultRet = new();
            Logger.Log("Begin GetPeers", source: "UdpTracker");

            // connect to host
            Logger.Log($"Connecting to host {AnnounceURI}, host {AnnounceURI.DnsSafeHost}", source: "UdpTracker");
            
            // this should be in its own function
            ConnectRequest connectReq = new ConnectRequest();

            Logger.Log($"Sending connect request", source: "UdpTracker");
            byte[]? conResData = await ExecuteUdpRequest(AnnounceURI, connectReq.Serialize());

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
            ConnectResponse connectRes = ConnectResponse.Parse(conResData);

            if (!connectReq.CheckResponse(connectRes))
            {
                Logger.Log($"Invalid data returned");
                return defaultRet;
            }
            Logger.Log($"Recieved response and parsed correctly", source: "UdpTracker");

            AnnounceRequest announceReq = new AnnounceRequest(connectRes.connection_id, Torrent);
            Logger.Log("Built announce request, sending...");

            conResData = await ExecuteUdpRequest(AnnounceURI, announceReq.Serialize());

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

            AnnounceResponse announceResponse = new AnnounceResponse(conResData);

            Logger.Log($"Recieved bytes correctly, byte length: {conResData.Length}", source: "UdpTracker");
            Peers = announceResponse.peers;

            Peers.ForEach(peer => Logger.Log($"PEER: {peer}", source: "TryGetPeers"));

            return announceResponse.peers;
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
            IPEndPoint any = new(IPAddress.Any, LISTENPORT);

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
                    if (res.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(15)))
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
