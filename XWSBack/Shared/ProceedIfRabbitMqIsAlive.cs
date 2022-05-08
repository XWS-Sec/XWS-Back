using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Shared
{
    public class ProceedIfRabbitMqIsAlive : IHostedService
    {
        private string _host;

        public ProceedIfRabbitMqIsAlive(string host)
        {
            _host = host;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            do
            {
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(_host, 5672);
                    }
                }
                catch (Exception e)
                {
                    await Task.Delay(1000);
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}