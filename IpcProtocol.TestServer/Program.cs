using IpcProtocol.Core;
using IpcProtocol.Core.Models;
using IpcProtocol.TestDomain;
using System;

namespace IpcProtocol.TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TestBase64Server();
            TestUtf8Server();

            Console.ReadLine();
        }

        static void TestBase64Server()
        {
            var protocol = new Protocol<Entity>(8022, 8021, ProtocolEncoding.Base64, new ProtocolEncryptor());
            protocol.Listen((e, g) =>
            {
                Console.WriteLine("Received: " + e.Command);
                protocol.Send(new Entity()
                {
                    Command = "test-callback",
                    Payload = null
                }, g);
            });
        }

        static void TestUtf8Server()
        {
            var protocol = new Protocol<Entity>(8024, 8023, ProtocolEncoding.UTF8, new ProtocolEncryptor());
            protocol.Listen((e, g) =>
            {
                Console.WriteLine("Received: " + e.Command);
                protocol.Send(new Entity()
                {
                    Command = "test-callback",
                    Payload = null
                }, g);
            });
        }
    }

    public class Entity
    {
        public string Command { get; set; }
        public object Payload { get; set; }
    }
}
