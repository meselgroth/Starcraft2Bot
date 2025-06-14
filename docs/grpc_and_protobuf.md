# gRPC and Protocol Buffers

After building the [WebSocket connection](websockets_sc2.md) to SC2, we need a way to structure the data we send. Blizzard chose Protocol Buffers (Protobuf) for their SC2 API.

## What is Protocol Buffers?

Protocol Buffers is a binary serialization format developed by Google. Compared to JSON and XML, Protocol Buffers offers:

- Smaller message size (binary, not text)
- Faster serialization/deserialization
- Strictly typed messages
- Cross-language compatibility

In this project, we use Protocol Buffers over WebSockets, rather than traditional gRPC over HTTP/2. This approach follows Blizzard's SC2 API design.

## SC2 Protocol Structure

The SC2 API defines protobuf messages in `.proto` files. Key files include:

```
s2clientprotocol/
  sc2api.proto     - Main API messages, requests and responses
  common.proto     - Shared data structures
  data.proto       - Game unit and ability data
  raw.proto        - Raw game state
  ...
```

All messages use a single request/response pattern:

```protobuf
message Request {
  oneof request {
    RequestCreateGame create_game = 1;
    RequestJoinGame join_game = 2;
    RequestObservation observation = 10;
    // many more request types...
  }
}

message Response {
  oneof response {
    ResponseCreateGame create_game = 1;
    ResponseJoinGame join_game = 2;
    ResponseObservation observation = 10;
    // corresponding response types...
  }
  repeated string error = 98;
  optional Status status = 99;
}
```

Each request type has a specific corresponding response type.

## Protocol Buffers in C#

To use protobuf in C#:

1. Add the `Google.Protobuf` and `Grpc.Tools` packages to your project
2. Add proto files to your project with build action set to "Protobuf"

```xml
<ItemGroup>
  <Protobuf Include="s2clientprotocol\*.proto" GrpcServices="Client" />
</ItemGroup>
```

The build process generates C# classes from proto files automatically.

Example generated class usage:

```csharp
// Creating a request
var request = new Request
{
    Observation = new RequestObservation()
};

// Creating a more complex request
var createGameRequest = new Request
{
    CreateGame = new RequestCreateGame
    {
        LocalMap = new LocalMap
        {
            MapPath = "Maps/Simple64.SC2Map"
        },
        PlayerSetup =
        {
            new PlayerSetup
            {
                Type = PlayerType.Participant,
                Race = Race.Terran
            },
            new PlayerSetup
            {
                Type = PlayerType.Computer,
                Race = Race.Zerg,
                Difficulty = Difficulty.VeryEasy
            }
        }
    }
};
```

## Sending and Receiving Protobuf Messages

To send protobuf messages over WebSockets, we need to serialize them to binary:

```csharp
public async Task SendAsync(Request request, CancellationToken cancellationToken)
{
    var sendBuf = new byte[1024 * 1024];
    var outStream = new CodedOutputStream(sendBuf);
    request.WriteTo(outStream);

    await _clientSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, (int)outStream.Position),
        WebSocketMessageType.Binary, true, cancellationToken);
}
```

When receiving, we deserialize the binary data back to protobuf objects:

```csharp
public async Task<Response> ReceiveRequestAsync()
{
    var bytes = await ReceiveMessageAsync(cancellationToken.Token);
    var response = Response.Parser.ParseFrom(bytes);
    
    // Handle errors
    if (response.Error.Count > 0)
    {
        foreach (var error in response.Error)
        {
            Console.WriteLine(error);
        }
    }
    
    return response;
}
```

## Async Message Handling Pattern

The SC2 API uses an async communication pattern. For a responsive bot, we need to:
1. Send requests like `RequestStep` or `RequestAction`
2. Constantly receive observations in the background

The pattern looks like this:

```csharp
public async Task Run()
{
    // Initial setup
    await RequestGameInfo();
    
    // Start background receiver
    var receiverTask = Receiver();
    
    // Main game loop
    while (!_surrender)
    {
        // Request observations periodically
        await _connectionService.SendRequestAsync(new Request { Observation = new RequestObservation() });
        
        // Process game state, send actions
        // ...
        
        await Task.Delay(500);
    }
    
    // Wait for receiver to complete
    await receiverTask;
}

private async Task Receiver()
{
    while (_running)
    {
        var response = await _connectionService.ReceiveRequestAsync();
        ProcessResponse(response);
    }
}

private void ProcessResponse(Response response)
{
    switch (response.ResponseCase)
    {
        case Response.ResponseOneofCase.Observation:
            UpdateGameState(response.Observation);
            break;
        case Response.ResponseOneofCase.Step:
            // Handle step response
            break;
        // Handle other response types
    }
}
```

This pattern allows for:
- Responsive message handling
- Asynchronous game state updates
- Separate concerns for sending and receiving

## Example Usage: Game Start

Here's an example of starting a game:

```csharp
// Connect WebSocket
await _webSocketWrapper.ConnectWebSocket();

// Create game request
var createGameRequest = new Request
{
    CreateGame = new RequestCreateGame
    {
        LocalMap = new LocalMap { MapPath = "Maps/Simple64.SC2Map" },
        PlayerSetup =
        {
            new PlayerSetup { Type = PlayerType.Participant },
            new PlayerSetup 
            { 
                Type = PlayerType.Computer,
                Race = Race.Random,
                Difficulty = Difficulty.Medium
            }
        },
        RealTime = false
    }
};

// Send request
await _connectionService.SendRequestAsync(createGameRequest);

// Receive response
var createResponse = await _connectionService.ReceiveRequestAsync();
if (createResponse.Error.Count > 0)
{
    // Handle errors
    return;
}

// Join game
var joinRequest = new Request
{
    JoinGame = new RequestJoinGame
    {
        Race = Race.Terran,
        Options = new InterfaceOptions
        {
            Raw = true,
            Score = true
        }
    }
};

await _connectionService.SendRequestAsync(joinRequest);
var joinResponse = await _connectionService.ReceiveRequestAsync();

// Start main game loop
// ...
```

## Next Steps

Now that we understand how to structure and send data with Protocol Buffers, the next step is to understand [Basic Game Requests](basic_requests.md) needed to play the game.

[I would love feedback or comments!](https://github.com/meselgroth/Starcraft2Bot/issues/8)