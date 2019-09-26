using System;

namespace IpcProtocol.Core.Models
{
    public class IpcHeader
    {
        public Guid CallbackId { get; set; }
        public int Port { get; set; }

        public IpcHeader(Guid callbackId, int port)
        {
            CallbackId = callbackId;
            Port = port;
        }
    }
}
