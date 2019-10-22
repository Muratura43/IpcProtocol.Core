using IpcProtocol.Core.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IpcProtocol.Core
{
    internal class IpcClient<T> where T : new()
    {
        private readonly int _portNumber;
        private volatile object _sendLock = new object();

        internal IpcClient(int portNumber)
        {
            _portNumber = portNumber;
        }

        public Task Send(IpcEntity<T> data)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    lock (_sendLock)
                    {
                        using (TcpClient socket = new TcpClient())
                        {
                            socket.Connect(new IPEndPoint(IPAddress.Loopback, _portNumber));

                            string serializedData = JsonConvert.SerializeObject(data);
                            byte[] dataToSend = Encoding.UTF8.GetBytes(serializedData);
                            byte[] bufferSize = Encoding.UTF8.GetBytes(dataToSend.Length.ToString().PadLeft(4));

                            var result = new byte[bufferSize.Length + dataToSend.Length];
                            Array.Copy(bufferSize, result, bufferSize.Length);
                            Array.Copy(dataToSend, 0, result, bufferSize.Length, dataToSend.Length);

                            socket.Client.Send(result);
                            socket.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] IpcClient Send: {ex?.Message}");
                }
            });
        }
    }
}
