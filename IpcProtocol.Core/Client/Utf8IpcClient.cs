using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IpcProtocol.Core.Models;
using IpcProtocol.Domain;
using Newtonsoft.Json;

namespace IpcProtocol.Core.Client
{
    internal class Utf8IpcClient<T> : BaseIpcClient<T> where T : new()
    {
        internal Utf8IpcClient(int portNumber, IProtocolEncryptor encryptor = null) 
            : base(portNumber, encryptor)
        {
        }

        public override Task Send(IpcEntity<T> data)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    lock (_sendLock)
                    {
                        using (TcpClient socket = new TcpClient())
                        {
                            socket.Connect(new IPEndPoint(IPAddress.Loopback, PortNumber));

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
                    Console.Error.WriteLine($"[ERROR] IpcClient Send: {ex?.ToString()}");
                }
            });
        }
    }
}
