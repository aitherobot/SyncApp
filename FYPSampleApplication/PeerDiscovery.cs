using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FYPSampleApplication;

public class PeerDiscovery
{
    private const int Port = 41234; // any unused port
    private const string DiscoverMessage = "SYNC_DISCOVER";
    private const string ResponseMessage = "SYNC_RESPONSE";

    private readonly List<string> _discoveredPeers = new ();
    public IReadOnlyList<string> DiscoveredPeers => _discoveredPeers;

    public void StartBroadcasting()
    {
        // sends UDP broadcast message
        var client = new UdpClient();
        client.EnableBroadcast = true;
        var endpoint = new IPEndPoint(IPAddress.Broadcast, Port);
        byte[] message = Encoding.UTF8.GetBytes(DiscoverMessage);
        client.Send(message, message.Length, endpoint);
        Console.WriteLine($"[DEBUG] Broadcast sent.");
    }

    public void StartListening()
    {
        // listens for UDP messages and responds to discovery requests
        
        Task.Run(() =>
        {
            var client = new UdpClient(AddressFamily.InterNetwork);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            

            while (true)
            {
                byte[] data = client.Receive(ref endpoint);
                string message = Encoding.UTF8.GetString(data);

                Console.WriteLine($"[DEBUG] Received message: '{message}' from '{endpoint.Address}'");

                if (message == DiscoverMessage)
                {
                    // convert the response message to a byte array
                    byte[] response = Encoding.UTF8.GetBytes(ResponseMessage);
                    var replyEndpoint = new IPEndPoint(endpoint.Address, Port);
                    // send the response back to the sender's endpoint
                    client.Send(response, response.Length, replyEndpoint);
                } 
                else if (message == ResponseMessage)
                {
                    string senderIP = endpoint.Address.ToString();
                    if (senderIP != GetLocalIPAddress() && !_discoveredPeers.Contains(endpoint.Address.ToString()))
                        _discoveredPeers.Add(senderIP);
                }
            }
        });
    }

    private static string GetLocalIPAddress()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect("8.8.8.8", 65530);
        var endpoint = socket.LocalEndPoint as IPEndPoint;
        return endpoint!.Address.ToString();
    }
}