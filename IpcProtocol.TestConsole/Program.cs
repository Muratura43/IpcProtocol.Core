using System;
using IpcProtocol.Core;

namespace IpcProtocol.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var protocol2 = new Protocol<Entity>(8021, 8023);
            protocol2.Send(new Entity()
            {
                Command = "test-send2",
                Payload = null
            }).SetCallback((e) =>
            {
                Console.WriteLine(e.Command);
            });

            var protocol3 = new Protocol<Entity>(8021, 8024);
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
