using System;
using IpcProtocol.Core;
using IpcProtocol.Domain;

namespace IpcProtocol.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var protocol2 = new Protocol<Entity>(8021, 8023, new ProtocolEncryptor());

            protocol2.Listen((e) =>
            {
                Console.WriteLine("Received: " + e.Command);
            });

            protocol2.Send(new Entity()
            {
                Command = "test-send2",
                Payload = null
            }).SetCallback((e) =>
            {
                Console.WriteLine(e.Command);
            });

            var protocol3 = new Protocol<Entity>(8021, 8024, new ProtocolEncryptor());
            protocol2.Send(new Entity()
            {
                Command = "asdf",
                Payload = null
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
