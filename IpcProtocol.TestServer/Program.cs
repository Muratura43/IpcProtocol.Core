﻿using IpcProtocol.Core;
using IpcProtocol.Core.Models;
using IpcProtocol.TestDomain;
using System;

namespace IpcProtocol.TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var protocol = new Protocol<Entity>(8022, 8021, ProtocolEncoding.Base64, new ProtocolEncryptor());

            // Listen test
            protocol.Listen((e, g) =>
            {
                Console.WriteLine("Received: " + e.Command);
                protocol.Send(new Entity()
                {
                    Command = "test-callback",
                    Payload = null
                }, g);
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
