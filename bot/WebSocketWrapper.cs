using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using SC2APIProtocol;

namespace bot
{
    public class WebSocketWrapper : IWebSocketWrapper
    {
        private readonly ClientWebSocket _clientSocket;

        public WebSocketWrapper()
        {
            _clientSocket = new ClientWebSocket();
        }

        public async Task ConnectWebSocket()
        {
            Console.WriteLine("Connecting to port 5678");
            await _clientSocket.ConnectAsync(new Uri("ws://127.0.0.1:5678/sc2api"), CancellationToken.None);
            Console.WriteLine("Connected");
        }

        public async Task SendRequestAsync(Request request)
        {
            var sendBuf = new byte[1024 * 1024];
            var outStream = new CodedOutputStream(sendBuf);
            request.WriteTo(outStream);

            using var cancellationSource = new CancellationTokenSource();
            //cancellationSource.CancelAfter(2000);

            await _clientSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, (int)outStream.Position),
                WebSocketMessageType.Binary, true, cancellationSource.Token);
        }

        public async Task<Response> ReceiveRequestAsync()
        {
            var receiveBuf = new byte[1024 * 1024];
            var finished = false;
            var curPos = 0;
            while (!finished)
            {
                using var cancellationSource = new CancellationTokenSource();
                var left = receiveBuf.Length - curPos;
                if (left < 0)
                {
                    // No space left in the array, enlarge the array by doubling its size.
                    var temp = new byte[receiveBuf.Length * 2];
                    Array.Copy(receiveBuf, temp, receiveBuf.Length);
                    receiveBuf = temp;
                    left = receiveBuf.Length - curPos;
                }

                //cancellationSource.CancelAfter(5000);
                var result = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuf, curPos, left), cancellationSource.Token);
                if (result.MessageType != WebSocketMessageType.Binary)
                    throw new Exception("Expected Binary message type.");

                curPos += result.Count;
                finished = result.EndOfMessage;
            }

            var response = Response.Parser.ParseFrom(new MemoryStream(receiveBuf, 0, curPos));
            Console.WriteLine($"Received response, Case:{response.ResponseCase}, Status{response.Status}");

            if (response.Error.Count <= 0) return response;
            Console.WriteLine("Response errors:");
            foreach (var error in response.Error)
            {
                Console.WriteLine(error);
            }

            return response;
        }
    }
}