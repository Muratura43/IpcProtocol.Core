using System;
using System.Net.Sockets;
using System.Text;
using IpcProtocol.Domain;

namespace IpcProtocol.Core.Server
{
    internal class Base64IpcServer : BaseIpcServer
    {
        internal Base64IpcServer(int portNumber, IProtocolEncryptor encryptor = null) 
            : base(portNumber, encryptor)
        {
        }

        protected override void ProcessTcpRequest(Socket handler)
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
                            var jsonData = Convert.ToBase64String(buffer);

                            if (_encryptor != null)
                            {
                                jsonData = _encryptor.Decrypt(jsonData);
                            }

                            InvokeDataReceived(this, new IpcEventArgs(jsonData));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[ERROR] IpcServer process request: " + ex.ToString());
                InvokeDataReceived(this, new IpcEventArgs(ex.Message) { HasErrors = true });
            }
            finally
            {
                handler.Disconnect(true);
                handler.Close();
            }
        }
    }
}
