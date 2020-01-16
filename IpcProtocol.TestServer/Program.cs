using IpcProtocol.Core;
using IpcProtocol.Domain;
using System;
using System.Collections.Generic;

namespace IpcProtocol.TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var protocol = new Protocol<Entity>(new List<int>() { 8023, 8024 }, 8022, new ProtocolEncryptor());

            // Listen test
            protocol.Listen((e) =>
            {
                Console.WriteLine("Received: " + e.Command);
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
