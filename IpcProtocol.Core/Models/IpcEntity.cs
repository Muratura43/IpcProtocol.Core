using System;

namespace IpcProtocol.Core.Models
{
    public class IpcEntity<T> where T : new()
    {
        public IpcHeader Header { get; set; }
        public T Entity { get; set; }

        public IpcEntity()
        {
            Header = new IpcHeader(Guid.NewGuid(), 0);
        }

        public IpcEntity(T entity)
            : this()
        {
            Entity = entity;
        }

        public IpcEntity(T entity, Guid callbackId, int port)
            : this(entity)
        {
            Header = new IpcHeader(callbackId, port);
        }
    }
}
