using System;
using IpcProtocol.Core;

namespace IpcProtocol.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var protocol = new Protocol<Entity>(8021, 8022);

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
            }).SetCallback((e) =>
            {
                // Callback test
                Console.WriteLine("Callbacked: " + e.Command);
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
