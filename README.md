# IpcProtocol.Core
Provides simple interprocess communication, through TCP.

## Getting Started
In C#:
```
// Create a new instance of the client/server Protocol (Entity represents your object type to send)
var protocol = new Protocol<Entity>(clientPort, serverPort, encodingEnum, encryption: optional);

// A listen example
protocol.Listen((e, g) =>
{
    Console.WriteLine("Received: " + e.Command);

    // In order to send a callback, you must provide the callbackId ('g' parameter)
    protocol.Send(new Entity()
    {
        Command = "test-callback",
        Payload = null
    }, g);
});

// A send example
protocol.Send(new Entity()
{
    Command = "test-send",
    Payload = null
}).SetCallback((e) =>
{
    // Set a callback message to be sent to the original caller
    Console.WriteLine("Callback: " + e.Command);
});
```

## Note:
- In order to integrate with the Node.js solution [ipc-protocol](https://github.com/Muratura43/ipc-protocol), you need to use the encodingEnum: ProtocolEncoding.UTF8
- In order to integrate with the dot net core solution, you need to use the encodingEnum: ProtocolEncoding.Base64
- The encryption parameter is an optional interface that cand be passed in order to encrypt your messaged (the implementation must be provided in your solution)