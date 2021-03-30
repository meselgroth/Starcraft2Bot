# Websockets
To control Sc2, Blizzard have built an API using gRPC over Websockets. Let's get comfortable with Websockets first.

Websockets is a communication protocol between 2 programs, one with an IP address or url called the server, the other the client that initiates the connection.

I find the [MDN Javascript page for websockets](https://developer.mozilla.org/en-US/docs/Web/API/WebSocket) an informative way to get started from a client perspective.

MDN even has a page to write a [C# websocket server console app](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server) using TcpListener from the System.Net.Sockets library.

For the server, I prefered microsoft's version of building the [websocket server on top of Asp.Net Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-3.1), as it comes with more library help out of the box, and usually you want to pair websockets with REST API endpoints.

In order to play around with a C# server having multiple clients in both C# and Javascript, I built my own ASP.Net Core websocket server and a C# console app client as well as a JS snippet for the browser.

## Server
If the server supports multiple connections from different clients it needs to remember these different connections.

Note the MS guide and my sandbox version put the websocket receiving on the middleware pipeline. This means every request is potentially a websocket request and gets dealt with accordingly.

For simplicity, my version only deals with websocket requests:

[Startup.cs](https://github.com/meselgroth/websockets-sandbox/blob/master/server/Startup.cs)
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseWebSockets();

    var socketManager = new SocketManager();

    app.Use(async (context, next) =>
    {
        Console.WriteLine($"IsWebSocketRequest: {context.WebSockets.IsWebSocketRequest}");

        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        stateManager.AddSocket(webSocket);
        
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult receivedMsg;
        do
        {
            receivedMsg = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var msgText = Encoding.UTF8.GetString(buffer, 0, receivedMsg.Count);
            Console.WriteLine($"Received message: {msgText}");

            var state = stateManager.ProcessCommand(msgText);

        } while (receivedMsg.CloseStatus == null);

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", CancellationToken.None);
    });
}
```

[SocketManager.cs](https://github.com/meselgroth/websockets-sandbox/blob/master/server/SocketManager.cs)
```csharp
public class SocketManager
    {
        private readonly ConcurrentBag<WebSocket> _webSockets = new ConcurrentBag<WebSocket>();
        private string state = string.Empty;

        public async Task<string> ProcessCommand(string msgText)
        {
            switch (msgText)
            {
                case "start":
                    state = "started";
                    break;
                case "stop":
                    state = "stopped";
                    break;
            }
            await UpdateAllSockets();
            return state;
        }

        private async Task UpdateAllSockets()
        {
            foreach (var socket in _webSockets)
            {
                await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(state)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public void AddSocket(WebSocket webSocket)
        {
            _webSockets.Add(webSocket);
        }
    }
```
[Diagram]

## Client
Console app to receive and display websocket messages, and ability to close gracefully.

[Program.cs](https://github.com/meselgroth/websockets-sandbox/blob/master/csharpclient/Program.cs)
```csharp
class Program
{
    private static ClientWebSocket webSocket;

    static async Task Main(string[] args)
    {
        webSocket = new ClientWebSocket();
        await webSocket.ConnectAsync(new Uri("ws://localhost:5000/"), CancellationToken.None);

        var receiverTask = Receiver();
        var input = string.Empty;
        do
        {
            Console.WriteLine("Enter message to send (x to close):");
            input = Console.ReadLine();

            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(input)), WebSocketMessageType.Text, true, CancellationToken.None);
        } while (input != "x");

        await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "ok", CancellationToken.None);
        await receiverTask; // Receive last messages and close gracefully
    }

    private static async Task Receiver()
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult receivedMsg;

        do
        {
            receivedMsg = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var msgText = Encoding.UTF8.GetString(buffer, 0, receivedMsg.Count);
            Console.WriteLine($"Received message: {msgText}, close status: {receivedMsg.CloseStatus}");

        } while (receivedMsg.CloseStatus == null);
    }
}
```

[Full websockets github repo, including javascript snippet.](https://github.com/meselgroth/websockets-sandbox)

## Sc2 Websockets
For my bot, the Starcraft 2 program is the websocket server.
When you run the game, you tell it to activate the websocket and what IP address & port the client will come from:
`SC2_x64.exe -listen 127.0.0.1 -port 5678`

Interestingly it only accepts one websocket connection, ie. only 1 client.

[Talk about chosen architecture (Read loop, Periodic send for observation, and instant send for new actions)]

## Make ReceiveMessageAsync more efficient
The example bot [SC2-CSharpe-Starterkit](https://github.com/NikEyX/SC2-CSharpe-Starterkit) I was using as a reference, initialised very large byte arrays to recieve the websocket messages `var receiveBuf = new byte[1024 * 1024]`, essentially 1Gb. This is a high memory requirement, so I decided to look into it.

A websocket message can vary in size, however this stackoverflow is a handy reference for what typical sizes "should" be: https://stackoverflow.com/a/41926694/2235675

Essentially a size under 65535 is an efficient size. The Sc2 Api goes over this size for certain messgages, so I set the buffer to be 65535 bytes long, with the ability to "grow" if the message goes over this size. 

The C# websocket API supports this, by telling you if you read all of the message or not `WebSocketReceiveResult.EndOfMessage` and allows you to read more of the message by calling `ClientWebSocket.ReceiveAsync` into a next ArraySegment of your buffer.
```csharp
public async Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken)
{
    var bytes = new byte[UInt16.MaxValue]; // Max size of Sc2Api message is unknown, however size of efficient websocket fits in 2 bytes
    var finished = false;
    var index = 0;
    while (!finished)
    {
        var result = await _webSocketWrapper.ReceiveAsync(new ArraySegment<byte>(bytes, index, UInt16.MaxValue), cancellationToken);
        if (result.MessageType != WebSocketMessageType.Binary)
            throw new Exception("Expected binary message type.");

        finished = result.EndOfMessage;
        if (!finished)
        {
            bytes = IncreaseByteArraySize(bytes);
        }
        index += result.Count;
    }
    return bytes[0..index];
}

private static byte[] IncreaseByteArraySize(byte[] bytes)
{
    var temp = bytes;
    bytes = new byte[bytes.Length + UInt16.MaxValue];
    Array.Copy(temp, 0, bytes, 0, temp.Length);
    return bytes;
}
```
I did this in a TDD fashion, writing a test for a normal message size and a test for a message size that goes over original buffer. I used a wrapper around the `ClientWebSocket` class to allow for it's methods to be mocked. However due to the nature of byte arrays being written to in-place (it's passed as a paramater and then written to), the test came out pretty ugly.
Have a look at your own risk:
[ConnectionServiceTests.cs](https://github.com/meselgroth/Starcraft2Bot/blob/master/HiveMindTest/ConnectionServiceTests.cs)

[Would love feedback or comments!](https://github.com/meselgroth/Starcraft2Bot/issues/new)
