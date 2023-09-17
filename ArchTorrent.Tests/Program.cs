// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text.RegularExpressions;

Console.WriteLine(GetFixedHost("www.google.com"));
Console.WriteLine(GetFixedHost("9.rargb.to"));
IPHostEntry hostInfo = Dns.GetHostEntry("9.rargb.to");
Console.WriteLine(hostInfo.AddressList[0].ToString());
Console.WriteLine(hostInfo.AddressList[0].ToString() == "127.0.0.1");
Console.Read();


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