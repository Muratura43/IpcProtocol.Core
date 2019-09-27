using IpcProtocol.Core.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace IpcProtocol.Core
{
    public class Protocol<T> where T : new()
    {
        private bool _isListening = false;

        private readonly IpcClient<T> _client;
        private readonly IpcServer _server;

        private Action<IpcEntity<T>> _onMessageReceivedAction;

        public Protocol(int clientPort, int serverPort)
        {
            _client = new IpcClient<T>(clientPort);
            _server = new IpcServer(serverPort);
        }

        public void Listen(Action<IpcEntity<T>> onMessageReceived)
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

        public Task SendAsync(IpcEntity<T> data)
        {
            return Task.Run(() =>
            {
                _client.Send(data);
            });
        }

        private void Server_OnDataReceived(object sender, IpcEventArgs e)
        {
            var entity = JsonConvert.DeserializeObject<IpcEntity<T>>(e.JsonData);
            _onMessageReceivedAction?.Invoke(entity);
        }
    }
}
