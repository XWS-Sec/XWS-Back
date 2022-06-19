using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Shared
{
    public class ProceedIfServiceIsAlive
    {
        private readonly string _host;
        private readonly int _port;

        public ProceedIfServiceIsAlive(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Check()
        {
            bool responseReceived = false;
            do
            {
                try
                {
                    using (var httpClient = new TcpClient())
                    {
                        httpClient.ConnectAsync(_host, _port).GetAwaiter().GetResult();
                        responseReceived = true;
                    }
                }
                catch (Exception e)
                {
                    Task.Delay(100).GetAwaiter().GetResult();
                }
            } while (!responseReceived);
        }
    }
}