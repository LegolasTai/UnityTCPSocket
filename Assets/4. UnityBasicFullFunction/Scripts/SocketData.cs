using System.Net.Sockets;
using System.IO;

namespace UnityBasicFullFunction
{
    [System.Serializable]
    public class SocketData
    {
        public Socket Socket;
        public NetworkStream Ns;
        public StreamReader Sr;
        public StreamWriter Sw;

        public SocketData(Socket socket)
        {
            Socket = socket;
            Ns = new NetworkStream(socket);
            Sr = new StreamReader(Ns, true);
            Sw = new StreamWriter(Ns);
        }
    }
}

