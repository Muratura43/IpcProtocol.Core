using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IpcProtocol.Core
{
    internal class IpcServer
    {
        private readonly int _portNumber;
        private const int _bufferHeaderSize = 4;
        private Socket _server;

        private IProtocolEncryptor _encryptor;

        public event EventHandler<IpcEventArgs> OnDataReceived;

        internal IpcServer(int portNumber, IProtocolEncryptor encryptor = null)
        {
            _portNumber = portNumber;
            _encryptor = encryptor;
        }

        public bool Listen()
        {
            try
            {
                var localEp = new IPEndPoint(IPAddress.Loopback, _portNumber);

                _server = new Socket(localEp.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _server.Bind(localEp);
                _server.Listen(100);

                _server.BeginAccept(new AsyncCallback(OnTcpData), _server);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IpcServer Listen: {ex?.ToString()}");
                return false;
            }
        }

        private void OnTcpData(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            bool accepted = false;

            try
            {
                if (listener != null)
                {
                    Socket handler = listener.EndAccept(ar);
                    listener.BeginAccept(new AsyncCallback(OnTcpData), listener);
                    accepted = true;
                    Task.Factory.StartNew(() => { ProcessTcpRequest(handler); });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IpcServer OnTcpData: {ex?.ToString()}");
            }
            finally
            {
                if (accepted)
                {
                    listener.BeginAccept(new AsyncCallback(OnTcpData), listener);
                }
            }
        }

        private void ProcessTcpRequest(Socket handler)
        {
            try
            {
                byte[] bufferHeader = new byte[_bufferHeaderSize];
                if (ReceiveTcp(handler, bufferHeader, _bufferHeaderSize))
                {
                    string slen = Encoding.UTF8.GetString(bufferHeader);
                    int length = int.Parse(slen);

                    if (length > 0)
                    {
                        byte[] buffer = new byte[length];
                        if (ReceiveTcp(handler, buffer, length))
                        {
                            var jsonData = Encoding.UTF8.GetString(buffer);

                            if (_encryptor != null)
                            {
                                jsonData = _encryptor.Decrypt(jsonData);
                            }

                            OnDataReceived?.Invoke(this, new IpcEventArgs(jsonData));
                        }
                    }
                }
            }
            finally
            {
                handler.Disconnect(true);
                handler.Close();
            }
        }

        private bool ReceiveTcp(Socket handler, byte[] bufferHeader, int size,
            SocketFlags flags = SocketFlags.None, int timeout = 10000)
        {
            var asyncResult = handler.BeginReceive(bufferHeader, 0, size, flags, null, null);
            asyncResult.AsyncWaitHandle.WaitOne(timeout);
            return asyncResult.IsCompleted;
        }
    }
}
