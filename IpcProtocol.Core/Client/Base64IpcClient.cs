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
    internal class Base64IpcClient<T> : BaseIpcClient<T> where T : new()
    {
        internal Base64IpcClient(int portNumber, IProtocolEncryptor encryptor = null) 
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

                                byte[] a = Convert.FromBase64String(serializedData);
                                string size = a.Length.ToString().PadLeft(4, '0');
                                byte[] sizeByte = Encoding.UTF8.GetBytes(size);

                                var z = new byte[a.Length + sizeByte.Length];
                                sizeByte.CopyTo(z, 0);
                                a.CopyTo(z, sizeByte.Length);

                                dataToSend = z;
                            }
                            else
                            {
                                dataToSend = Convert.FromBase64String(serializedData);
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
