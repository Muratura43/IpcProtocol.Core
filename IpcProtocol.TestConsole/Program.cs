using System;
using IpcProtocol.Core;
using IpcProtocol.Core.Models;
using IpcProtocol.TestDomain;

namespace IpcProtocol.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TestBase64Client();
            //TestUtf8Client();

            Console.ReadLine();
        }

        static void TestBase64Client()
        {
            var protocol = new Protocol<Entity>(8021, 8022, ProtocolEncoding.Base64, new ProtocolEncryptor());
            protocol.Listen((e, g) =>
            {
                Console.WriteLine(e.Command);
            });
            protocol.Send(new Entity()
            {
                Command = "test-send",
                Payload = null
            }).SetCallback((e) =>
            {
                Console.WriteLine("Callback: " + e.Command);
            });
        }

        static void TestUtf8Client()
        {
            var protocol = new Protocol<Entity>(8023, 8024, ProtocolEncoding.UTF8, new ProtocolEncryptor());
            protocol.Listen((e, g) =>
            {
                Console.WriteLine(e.Command);
            });
            protocol.Send(new Entity()
            {
                Command = "test-send",
                Payload = null
            }).SetCallback((e) =>
            {
                Console.WriteLine("Callback: " + e.Command);
            });
        }
    }

    public class Entity
    {
        public string Command { get; set; }
        public object Payload { get; set; }
    }
}
