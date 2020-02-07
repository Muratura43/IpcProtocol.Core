using IpcProtocol.Core.Client;
using IpcProtocol.Core.Models;
using IpcProtocol.Core.Server;
using IpcProtocol.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IpcProtocol.Core
{
    public class Protocol<T> where T : new()
    {
        #region Fields
        private bool _isListening = false;

        private IProtocolEncryptor _encryptor;

        private readonly BaseIpcServer _server;
        private readonly BaseIpcClient<T> _client;

        private OnMessageReceved _onMessageReceivedAction;

        private Dictionary<Guid, Action<T>> _callbacks;
        private volatile object _dictionaryLock = new object();
        #endregion

        private Protocol(int serverPort, IProtocolEncryptor encryptor = null)
        {
            _encryptor = encryptor;
            _server = new BaseIpcServer(serverPort, _encryptor);
            _callbacks = new Dictionary<Guid, Action<T>>();
        }

        public Protocol(int clientPort, int serverPort, IProtocolEncryptor encryptor = null)
            : this(serverPort, encryptor)
        {
            _client = new BaseIpcClient<T>(clientPort, _encryptor);
        }

        public void Listen(OnMessageReceved onMessageReceived)
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
            var request = new IpcEntity<T>(data, Guid.NewGuid(), _client.PortNumber);
            _client.Send(request);

            return new IpcCallback<T>(this, request.Header.CallbackId);
        }

        public void Send(T data, Guid callbackId)
        {
            var request = new IpcEntity<T>(data, callbackId, _client.PortNumber);
            _client.Send(request);
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
                _onMessageReceivedAction?.Invoke(entity.Entity, entity.Header.CallbackId);
            }
        }

        public delegate void OnMessageReceved(T data, Guid callbackId);
    }
}
