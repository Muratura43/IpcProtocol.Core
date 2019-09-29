# IpcProtocol.Core
Provides simple interprocess communication, through TCP.

## Getting Started
In C#:
```
// Create a new instance of the client/server Protocol (Entity represents your object type to send)
var protocol = new Protocol<Entity>(clientPort, serverPort);

// A listen example
protocol.Listen((e) =>
{
    Console.WriteLine("Received: " + e.Command);
});

// A send example
protocol.Send(new Entity()
{
    Command = "test-send",
    Payload = null
}).SetCallback((e) =>
{
    // Example for setting the callback action for a send request
    Console.WriteLine("Callbacked: " + e.Command);
});
```

## Note:
This solution integrates perfectly with the Node.js solution [ipc-protocol](https://github.com/Muratura43/ipc-protocol)