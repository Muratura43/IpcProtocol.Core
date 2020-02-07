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
        private bool _isListening = false;
        private int _clientPort;

        private IProtocolEncryptor _encryptor;

        private readonly BaseIpcServer _server;
        private readonly Dictionary<int, BaseIpcClient<T>> _multiClients;

        private Action<T> _onMessageReceivedAction;

        private Dictionary<Guid, Action<T>> _callbacks;
        private volatile object _dictionaryLock = new object();

        private Protocol(int serverPort, IProtocolEncryptor encryptor = null)
        {
            _encryptor = encryptor;

            _server = new BaseIpcServer(serverPort, _encryptor);

            _multiClients = new Dictionary<int, BaseIpcClient<T>>();
            _callbacks = new Dictionary<Guid, Action<T>>();
        }

        public Protocol(int clientPort, int serverPort, IProtocolEncryptor encryptor = null) 
            : this(serverPort, encryptor)
        {
            _multiClients.Add(clientPort, new BaseIpcClient<T>(clientPort, _encryptor));
            _clientPort = clientPort;
        }

        public Protocol(List<int> clientPorts, int serverPort, IProtocolEncryptor encryptor = null) 
            : this(serverPort, encryptor)
        {
            foreach (var port in clientPorts)
            {
                var client = new BaseIpcClient<T>(port, _encryptor);
                _multiClients.Add(port, client);

                _clientPort = port;
            }
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
            if (_multiClients.Count == 1)
            {
                var client = _multiClients[_clientPort];
                var request = new IpcEntity<T>(data, Guid.NewGuid(), _clientPort);
                client.Send(request);

                return new IpcCallback<T>(this, request.Header.CallbackId);
            }
            else
            {
                throw new InvalidOperationException("Multiple clients defined. Pass the port parameter to decide which client to use.");
            }
        }

        public IpcCallback<T> Send(T data, int port)
        {
            if (_multiClients.TryGetValue(port, out BaseIpcClient<T> client) == true)
            {
                var request = new IpcEntity<T>(data, Guid.NewGuid(), port);
                client.Send(request);

                return new IpcCallback<T>(this, request.Header.CallbackId);
            }
            else
            {
                throw new KeyNotFoundException("Port not found in clients list.");
            }
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
