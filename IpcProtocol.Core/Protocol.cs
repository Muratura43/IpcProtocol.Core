using IpcProtocol.Core.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace IpcProtocol.Core
{
    public class Protocol
    {
        private bool _isListening = false;

        private readonly IpcClient _client;
        private readonly IpcServer _server;

        private Action<IpcEntity> _onMessageReceivedAction;

        public Protocol(int clientPort, int serverPort)
        {
            _client = new IpcClient(clientPort);
            _server = new IpcServer(serverPort);
        }

        public void Listen(Action<IpcEntity> onMessageReceived)
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

        public Task SendAsync(IpcEntity data)
        {
            return Task.Run(() =>
            {
                _client.Send(data);
            });
        }

        private void Server_OnDataReceived(object sender, IpcEventArgs e)
        {
            var entity = JsonConvert.DeserializeObject<IpcEntity>(e.JsonData);
            _onMessageReceivedAction?.Invoke(entity);
        }
    }
}
