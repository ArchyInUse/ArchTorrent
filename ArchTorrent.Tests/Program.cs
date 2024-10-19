// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

Console.WriteLine(GetFixedHost("www.google.com"));
Console.WriteLine(GetFixedHost("9.rargb.to"));
IPHostEntry hostInfo = Dns.GetHostEntry("9.rargb.to");
Console.WriteLine(hostInfo.AddressList[0].ToString());
Console.WriteLine(hostInfo.AddressList[0].ToString() == "127.0.0.1");
Console.Read();


private byte[] ConstructHandshake()
{
    // handshake: <pstrlen><pstr><reserved><info_hash><peer_id>

    // pstrlen: string length of<pstr>, as a single raw byte
    // pstr: string identifier of the protocol
    // reserved: eight(8) reserved bytes. All current implementations use all zeroes.
    // info hash
    // peer_id: 20 - byte string used as a unique ID for the client.
    // 
    // 
    // In version 1.0 of the BitTorrent protocol, pstrlen = 19, and pstr = "BitTorrent protocol".
    List<byte> data = new List<byte>();

    // pstrlen (constant)
    data.Add(19);

    // pstr
    var pstr = Encoding.ASCII.GetBytes("BitTorrent protocol");
    foreach (byte b in pstr) { data.Add(b); }

    // reserved
    for (int i = 0; i < 8; i++) { data.Add(0); }

    var hash = Encoding.ASCII.GetBytes(InfoHash);
    for (int i = 0; i < hash.Length; i++) { data.Add(hash[i]); }

    var peer_id = TrackerMessageHelpers.GenerateID();
    foreach (byte b in peer_id) { data.Add(b); }

    return data.ToArray();
}

string GetFixedHost(string AnnounceURI)
{
    Regex rx = new Regex(@"\w+\.\w+\.\w+");
    if (rx.IsMatch(AnnounceURI))
    {
        // removes everything behind . (+ 1 including the host
        return AnnounceURI.Remove(0, AnnounceURI.IndexOf('.') + 1);
    }

    return AnnounceURI;
}