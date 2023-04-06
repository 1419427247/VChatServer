using System.Net.Sockets;
using System.Text;
namespace VChatService.Net;

class Client
{

    public byte[] data = new byte[1024];
    public Socket socket;
    public bool authenticated = false;
    public Client(Socket socket)
    {
        this.socket = socket;
    }

    public override String ToString()
    {
        return socket.RemoteEndPoint!.ToString()!;
    }
}