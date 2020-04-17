using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using SC2APIProtocol;

namespace bot
{
    public class ConnectionService : IConnectionService
    {
        private readonly IWebSocketWrapper _webSocketWrapper;

        public ConnectionService(IWebSocketWrapper webSocketWrapper)
        {
            _webSocketWrapper = webSocketWrapper;
        }

        public async Task SendRequestAsync(Request request)
        {
            using var cancellationSource = new CancellationTokenSource();
            //cancellationSource.CancelAfter(2000);

            await _webSocketWrapper.SendAsync(request, cancellationSource.Token);
        }

        public async Task<Response> ReceiveRequestAsync()
        {
            using var cancellationSource = new CancellationTokenSource();
            //cancellationSource.CancelAfter(5000);

            var bytes = await ReceiveMessageAsync(cancellationSource.Token);


            var response = Response.Parser.ParseFrom(bytes);
            Console.WriteLine($"Received response, Case:{response.ResponseCase}, Status{response.Status}");

            if (response.Error.Count <= 0) return response;
            Console.WriteLine("Response errors:");
            foreach (var error in response.Error)
            {
                Console.WriteLine(error);
            }

            return response;
        }

        public async Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            var receiveBuf = new byte[1024 * 1024];
            var finished = false;
            var curPos = 0;
            while (!finished)
            {
                var left = receiveBuf.Length - curPos;
                if (left < 0)
                {
                    // No space left in the array, enlarge the array by doubling its size.
                    var temp = new byte[receiveBuf.Length * 2];
                    Array.Copy(receiveBuf, temp, receiveBuf.Length);
                    receiveBuf = temp;
                    left = receiveBuf.Length - curPos;
                }

                var result = await _webSocketWrapper.ReceiveAsync(new ArraySegment<byte>(receiveBuf, curPos, left), cancellationToken);
                if (result.MessageType != WebSocketMessageType.Binary)
                    throw new Exception("Expected binary message type.");

                curPos += result.Count;
                finished = result.EndOfMessage;
            }

            return new ArraySegment<byte>(receiveBuf, 0, curPos).ToArray();
        }
    }
}