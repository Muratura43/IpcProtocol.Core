using IpcProtocol.Core.Models;
using IpcProtocol.Domain;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IpcProtocol.Core.Client
{
    internal abstract class BaseIpcClient<T> where T : new()
    {
        public int PortNumber { get; private set; }

        protected volatile object _sendLock = new object();
        protected IProtocolEncryptor _encryptor;

        internal BaseIpcClient(int portNumber, IProtocolEncryptor encryptor = null)
        {
            PortNumber = portNumber;
            _encryptor = encryptor;
        }

        public abstract Task Send(IpcEntity<T> data);
    }
}
