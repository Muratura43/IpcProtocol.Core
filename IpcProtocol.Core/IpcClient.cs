using IpcProtocol.Core.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IpcProtocol.Core
{
    internal class IpcClient
    {
        private readonly int _portNumber;

        internal IpcClient(int portNumber)
        {
            _portNumber = portNumber;
        }

        public Task Send(IpcEntity data)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    using (TcpClient socket = new TcpClient())
                    {
                        socket.Connect(new IPEndPoint(IPAddress.Loopback, _portNumber));
                        var dataToSend = ProcessDataForSending(data);
                        socket.Client.Send(dataToSend);

                        socket.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] IpcClient Send: {ex?.Message}");
                }
            });
        }

        private byte[] ProcessDataForSending(object data)
        {
            string serializedData = JsonConvert.SerializeObject(data);
            byte[] processedData = Encoding.UTF8.GetBytes(serializedData);

            return processedData;
        }
    }
}
