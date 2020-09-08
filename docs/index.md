# StarCraft 2 Bot
Here I will describe my learnings as I build an automated player of the PC game StarCraft 2 (aka Sc2).

These docs are not yet complete!

## 1 Websockets
To control Sc2, Blizzard have built an API using gRPC over Websockets. Let's get comfortable with Websockets first. [Server vs Client]

I find the [MDN Javascript page for websockets](https://developer.mozilla.org/en-US/docs/Web/API/WebSocket) an informative way to get started from the client perspective.

MDN even has a page to write a [C# websocket server console app](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server) using TcpListener from the System.Net.Sockets library.

I prefered building the websocket server on top of Asp.Net Core, as it comes with more library help out of the box, and usually you want to pair websockets with REST API endpoints.
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-3.1

I built my own version of this so I could play around with a C# server having multiple clients in both C# and Javascript.
https://github.com/meselgroth/websockets-sandbox

###Server
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

### Client
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
### Sc2 Websockets
For my bot, the Starcraft 2 program is the websocket server.
When you run the game, you tell it to activate the websocket and what IP address & port the client will come from:
`SC2_x64.exe -listen 127.0.0.1 -port 5678`

Interestingly it only accepts one websocket connection, ie. only 1 client.

[Talk about chosen architecture (Read loop, Periodic send for observation, and instant send for new actions)]

### Make ReceiveMessageAsync more efficient
The example bot I was using as a reference, initialised very large byte arrays to recieve the websocket messages `var receiveBuf = new byte[1024 * 1024]`, essentially 1Gb. This is a high memory requirement, so I decided to look into it.

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
I did this in a TDD fashion, writing a test for a normal message size and a message size that goes over original buffer. I used a wrapper around the `ClientWebSocket` class to allow for it's methods to be mocked. However due to the nature of byte arrays being written to in-place (it's passed as a paramater and then written to), the test came out pretty ugly.
Have a look at your own risk:
https://github.com/meselgroth/Starcraft2Bot/blob/master/HiveMindTest/ConnectionServiceTests.cs

## 2 gRPC
Protobuf files

## 3 Basic Requests

## 4 Making the Bot Map Aware

## 5 Processing Bitmaps
The SC2 Api returns bitmaps of what the map looks like. To understand how to process this, I looked at an existing example [Sc2Bot](http://safd). Essentially the critical method this one with a confusing looking bitwise operation.

```csharp
public static int GetDataValueBit(ImageData data, int x, int y)
{
    int pixelID = x + y * data.Size.X;
    int byteLocation = pixelID / 8;
    int bitLocation = pixelID % 8;
    int bit = data.Data[byteLocation] & (1 << (7 - bitLocation));
    return bit == 0 ? 0 : 1;
}
```
At first I just used this at face value: it returns 0 if a position(x,y) is white, 1 if black.
Then I decided to properly relearn bitwise operators (I started vaguely remembering, I had learned this in uni, It has to do with electronic circuits and the logic gates that can turn a following circuit on or off (bit output) after receiving on or off electric signal (bit input).)

There's plenty of info out there on what each operator does, I will just step through each line.

Let's start with regular dotnet types and recreate each line, in a format that will work in a unit test or Linqpad.

Imagine our Bitmap, each pixel is a bit:
>`0 0 0 0 0` 5 pixels width  
`0 0 0 2 0`  times 2 pixels height  

Stored in bytes
>` 0000 0000, 2000 0000` Note: we have 6 extra bits overhead  
Using a 0 based position of (x,y), our colour `2` is the 8th bit.

```csharp
// First byte is 0
var data = new byte[]{0,0};
// For 2nd byte, create new BitArray to represent 1 0 0 0 0 0 0 0
var bitArr = new System.Collections.BitArray(new bool[]{false,false,false,false,false,false,false,true});
// OR
Convert.ToByte("10000000", 2);

// Convert to Byte and place in second position in data
bitArr.CopyTo(data, 1); // data: [0]: 0, [1]: 128

var b = 1<<7; // 128
var bitcompare = data[1] & b; // returns 128. Although a byte is returned, Bitwise AND compares a single bit. So it is comparing the first bit of each byte and as they match it returns the number representation of the full byte of that single bit. 1 1 1 1 1 1 1 1 & 1 0 0 0 0 0 0 0 also returns 128
```

Let's return to our original method.  
Assuming the bitmap of pixels in our ImageData.Data:
>`0 0 0 0 0`  
`0 0 0 2 0`

When we call it for a position of `x=3`, `y=1`
```csharp
public static int GetDataValueBit(ImageData data, int x, int y)
{
    int pixelID = x + y * data.Size.X;  // 8 pixels
    int byteLocation = pixelID / 8;  // 1 (2nd byte): 1bit per pixel, 8bits per byte
    int bitLocation = pixelID % 8;  // 0 (1st bit)
    int bit = data.Data[byteLocation] & (1 << (7 - bitLocation)); // Test binary rep of byte(128=1 0 0 0 0 0 0 0) is same as binary rep of (1 left shift 7 bits to get to first bit 1 0 0 0 0 0 0 0, which = 1)
    return bit == 0 ? 0 : 1;
}