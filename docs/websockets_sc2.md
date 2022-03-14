# Sc2 Websocket Efficiencies
For my bot, the Starcraft 2 program is the websocket server.
When you run the game, you tell it to activate the websocket and what IP address & port the client will come from:
`SC2_x64.exe -listen 127.0.0.1 -port 5678`

Interestingly it only accepts one websocket connection, ie. only 1 client.

[Talk about chosen architecture (Read loop, Periodic send for observation, and instant send for new actions)]

## Make ReceiveMessageAsync more efficient
The example bot [SimonPrins/ExampleBot](https://github.com/SimonPrins/ExampleBot) I was using as a reference, initialised a byte array of size 1Mb to recieve the websocket messages `var receiveBuf = new byte[1024 * 1024]`. This is slightly inefficient, so I decided to look into it.

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

