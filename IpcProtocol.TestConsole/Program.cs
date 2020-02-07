using System;
using IpcProtocol.Core;
using IpcProtocol.Core.Models;
using IpcProtocol.Domain;

namespace IpcProtocol.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var protocol2 = new Protocol<Entity>(8021, 8022, ProtocolEncoding.Base64, new ProtocolEncryptor());
            protocol2.Listen((e, g) =>
            {
                Console.WriteLine(e.Command);
            });
            protocol2.Send(new Entity()
            {
                Command = "test-send2",
                Payload = null
            }).SetCallback((e) =>
            {
                Console.WriteLine("Callback: " + e.Command);
            });

            Console.ReadLine();
        }
    }

    public class Entity
    {
        public string Command { get; set; }
        public object Payload { get; set; }
    }
}
