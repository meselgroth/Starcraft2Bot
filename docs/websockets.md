# Websockets (general)
To control Sc2, Blizzard have built an API using gRPC over Websockets. Let's get comfortable with Websockets first, building a dummy project, separate from the StarCraft bot. [Full repo here.](https://github.com/meselgroth/websockets-sandbox)

Websockets is a communication protocol between 2 programs, one with an IP address or url called the server, the other the client that initiates the connection.

I find the [MDN Javascript page for websockets](https://developer.mozilla.org/en-US/docs/Web/API/WebSocket) an informative way to get started from a client perspective.

MDN even has a page to write a [C# websocket server console app](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server) using TcpListener from the System.Net.Sockets library.

For the server, I prefered microsoft's version of building the [websocket server on top of Asp.Net Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-3.1), as it comes with more library help out of the box, and usually you want to pair websockets with REST API endpoints.

In order to play around with a C# server having multiple clients in both C# and Javascript, I built my own ASP.Net Core websocket server and a C# console app client as well as a JS snippet for the browser.

## Server
If the server supports multiple connections from different clients it needs to remember these different connections.

Note the MS guide and my sandbox version put the websocket receiving on the middleware pipeline. This means every request is potentially a websocket request and gets dealt with accordingly. For simplicity, my middleware only deals with websocket requests:

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

[Full websockets repo, including javascript snippet.](https://github.com/meselgroth/websockets-sandbox)

Next: [Sc2 Websocket Efficiencies](websockets_sc2.md)

[I would love feedback or comments!](https://github.com/meselgroth/Starcraft2Bot/issues/8)
