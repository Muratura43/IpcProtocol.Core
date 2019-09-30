using System;
using System.Collections.Generic;
using IpcProtocol.Core;

namespace IpcProtocol.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var protocol = new Protocol<Entity>(new List<int>() { 8021, 8023, 8024 }, 8022);

            // Listen test
            protocol.Listen((e) =>
            {
                Console.WriteLine("Received: " + e.Command);
            });

            // Send test
            protocol.Send(new Entity()
            {
                Command = "test-send",
                Payload = null
            }, 8021).SetCallback((e) =>
            {
                // Callback test
                Console.WriteLine("Callbacked: " + e.Command);
            });

            protocol.Send(new Entity()
            {
                Command = "test-send",
                Payload = null
            }, 8023);

            protocol.Send(new Entity()
            {
                Command = "test-send",
                Payload = null
            }, 8024);

            Console.ReadLine();
        }
    }

    public class Entity
    {
        public string Command { get; set; }
        public object Payload { get; set; }
    }
}
