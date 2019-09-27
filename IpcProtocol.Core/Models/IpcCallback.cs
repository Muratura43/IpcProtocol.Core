using System;

namespace IpcProtocol.Core.Models
{
    public class IpcCallback<T> where T : new()
    {
        public Action<T> CallbackAction { get; private set; }
        public Guid Id { get; private set; }

        private Protocol<T> _protocol;

        internal IpcCallback(Protocol<T> protocol, Guid id)
        {
            _protocol = protocol;
            Id = id;
        }

        public void SetCallback(Action<T> callbackAction)
        {
            CallbackAction = callbackAction;
            _protocol.AddCallback(this);
        }
    }
}
