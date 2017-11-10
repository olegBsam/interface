using System.Net.Sockets;
using System.Threading;

namespace I7000Server
{
    class Server
    {
        TcpListener Listener;

        public Server(int port)
        {
            Listener = new TcpListener(System.Net.IPAddress.Any, port);
        }

        public void Start()
        {
            Listener.Start();
            while (true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(
                    Client.ClientThread), Listener.AcceptTcpClient());
            }
        }

        ~Server()
        {
            if (Listener != null)
                Listener.Stop();
        }
    }
}
