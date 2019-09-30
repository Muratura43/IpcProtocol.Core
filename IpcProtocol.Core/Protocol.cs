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

        private readonly IpcServer _server;
        private readonly Dictionary<int, IpcClient<T>> _multiClients;

        private Action<T> _onMessageReceivedAction;

        private Dictionary<Guid, Action<T>> _callbacks;
        private volatile object _dictionaryLock = new object();

        private Protocol(int serverPort)
        {
            _serverPort = serverPort;
            _server = new IpcServer(serverPort);

            _multiClients = new Dictionary<int, IpcClient<T>>();
            _callbacks = new Dictionary<Guid, Action<T>>();
        }

        public Protocol(int clientPort, int serverPort) : this(serverPort)
        {
            _multiClients.Add(clientPort, new IpcClient<T>(clientPort));
            _clientPort = clientPort;
        }

        public Protocol(List<int> clientPorts, int serverPort) : this(serverPort)
        {
            foreach (var port in clientPorts)
            {
                var client = new IpcClient<T>(port);
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
            if (_multiClients.TryGetValue(port, out IpcClient<T> client) == true)
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
