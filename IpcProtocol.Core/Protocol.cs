using IpcProtocol.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IpcProtocol.Core
{
    public class Protocol<T> where T : new()
    {
        private bool _isListening = false;
        private int _clientPort;
        private int _serverPort;

        private readonly IpcClient<T> _client;
        private readonly IpcServer _server;

        private Action<T> _onMessageReceivedAction;

        private Dictionary<Guid, Action<T>> _callbacks;
        private volatile object _dictionaryLock = new object();

        public Protocol(int clientPort, int serverPort)
        {
            _clientPort = clientPort;
            _serverPort = serverPort;

            _client = new IpcClient<T>(clientPort);
            _server = new IpcServer(serverPort);

            _callbacks = new Dictionary<Guid, Action<T>>();
        }

        public void Listen(Action<T> onMessageReceived)
        {
            _onMessageReceivedAction += onMessageReceived;
            _server.OnDataReceived += Server_OnDataReceived;

            if (_isListening == false)
            {
                _isListening = _server.Listen();
            }

            if (_isListening == true)
            {
                Console.WriteLine("Started listening...");
            }
        }

        public IpcCallback<T> Send(T data)
        {
            var request = new IpcEntity<T>(data, Guid.NewGuid(), _clientPort);

            _client.Send(request);
            return new IpcCallback<T>(this, request.Header.CallbackId);
        }

        internal void AddCallback(IpcCallback<T> callback)
        {
            lock (_dictionaryLock)
            {
                _callbacks.TryAdd(callback.Id, callback.CallbackAction);
            }
        }

        private void Server_OnDataReceived(object sender, IpcEventArgs e)
        {
            var entity = JsonConvert.DeserializeObject<IpcEntity<T>>(e.JsonData);

            // See if a callback is assigned to this request and call that
            if (_callbacks.TryGetValue(entity.Header.CallbackId, out Action<T> cb))
            {
                cb?.Invoke(entity.Entity);

                lock (_dictionaryLock)
                {
                    _callbacks.Remove(entity.Header.CallbackId);
                }
            }
            else
            {
                // If no specific callback exists, call the generic action
                _onMessageReceivedAction?.Invoke(entity.Entity);
            }
        }
    }
}
