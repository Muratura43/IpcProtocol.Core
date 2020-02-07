using IpcProtocol.Domain;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IpcProtocol.Core.Server
{
    internal abstract class BaseIpcServer
    {
        protected const int _bufferHeaderSize = 4;
        protected IProtocolEncryptor _encryptor;

        private readonly int _portNumber;
        private Socket _server;

        public event EventHandler<IpcEventArgs> OnDataReceived;

        internal BaseIpcServer(int portNumber, IProtocolEncryptor encryptor = null)
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
                Console.Error.WriteLine($"[ERROR] IpcServer Listen: {ex?.ToString()}");
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

                    Task.Factory.StartNew(() => 
                    { 
                        ProcessTcpRequest(handler); 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] IpcServer OnTcpData: {ex?.ToString()}");
            }
            finally
            {
                if (accepted)
                {
                    listener.BeginAccept(new AsyncCallback(OnTcpData), listener);
                }
            }
        }

        protected bool ReceiveTcp(Socket handler, byte[] bufferHeader, int size,
            SocketFlags flags = SocketFlags.None, int timeout = 10000)
        {
            var asyncResult = handler.BeginReceive(bufferHeader, 0, size, flags, null, null);
            asyncResult.AsyncWaitHandle.WaitOne(timeout);
            return asyncResult.IsCompleted;
        }
        
        protected void InvokeDataReceived(BaseIpcServer server, IpcEventArgs args)
        {
            if (!args.HasErrors)
            {
                OnDataReceived?.Invoke(server, args);
            }
        }

        protected abstract void ProcessTcpRequest(Socket handler);
    }
}
