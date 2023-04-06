using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VChatService.Net;
class TCPServer
{
    private Socket server_socket;
    private Dictionary<string, Client> client_dictionary = new();
    public TCPServer()
    {
        server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server_socket.Bind(IPEndPoint.Parse("127.0.0.1:9492"));
    }

    public void Start()
    {
        server_socket.Listen();
        server_socket.BeginAccept(OnAccept, null);
    }

    public void OnAccept(IAsyncResult async_result)
    {
        Client client = new Client(server_socket.EndAccept(async_result));
        server_socket.BeginAccept(OnAccept, null);

        System.Console.WriteLine(client.socket.RemoteEndPoint!.ToString()! + " connected.");

        client_dictionary.Add(client.socket.RemoteEndPoint!.ToString()!, client);
        client.socket.BeginReceive(client.data, 0, client.data.Length, SocketFlags.None, OnReceive, client);
    }

    public void OnReceive(IAsyncResult async_result)
    {
        Client client = (Client)async_result.AsyncState!;
        int received;
        try
        {
            received = client.socket.EndReceive(async_result);
        }
        catch (SocketException)
        {
            client_dictionary.Remove(client.socket.RemoteEndPoint!.ToString()!);
            client.socket.Close();
            System.Console.WriteLine(client.socket.RemoteEndPoint!.ToString()! + " disconnected.");
            return;
        }

        if (received == 0)
        {
            client_dictionary.Remove(client.socket.RemoteEndPoint!.ToString()!);
            client.socket.Close();
            System.Console.WriteLine(client.socket.RemoteEndPoint!.ToString()! + " disconnected.");
            return;
        }

        string text = Encoding.UTF8.GetString(client.data, 0, received);
        System.Console.WriteLine("Received: " + text);
        client.socket.BeginReceive(client.data, 0, client.data.Length, SocketFlags.None, OnReceive, client);
    }

    public void Send(Client client, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        client.socket.Send(data);
    }
}