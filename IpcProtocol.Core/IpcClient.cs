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

        private IProtocolEncryptor _encryptor;

        internal IpcClient(int portNumber, IProtocolEncryptor encryptor = null)
        {
            _portNumber = portNumber;
            _encryptor = encryptor;
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
                            byte[] dataToSend;

                            if (_encryptor != null)
                            {
                                serializedData = _encryptor.Encrypt(serializedData);

                                var bufferHead = Convert.FromBase64String(serializedData).Length.ToString().PadLeft(4, '0');
                                dataToSend = Convert.FromBase64String(bufferHead + serializedData);
                            }
                            else
                            {
                                dataToSend = Encoding.UTF8.GetBytes(serializedData);
                            }
                            
                            socket.Client.Send(dataToSend);
                            socket.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] IpcClient Send: {ex?.ToString()}");
                }
            });
        }
    }
}
