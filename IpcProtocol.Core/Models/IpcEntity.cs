using System;

namespace IpcProtocol.Core.Models
{
    public class IpcEntity
    {
        public IpcHeader Header { get; set; }
        public string Command { get; set; }
        public object Payload { get; set; }

        public IpcEntity()
        {
            Header = new IpcHeader(Guid.NewGuid(), 0);
        }

        public IpcEntity(string command, object payload = null)
            : this()
        {
            Command = command;
        }

        public IpcEntity(string command, object payload, Guid callbackId, int port)
            : this(command, payload)
        {
            Header = new IpcHeader(callbackId, port);
        }
    }
}
