using System;
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
            cancellationSource.CancelAfter(5000);

            await _clientSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, (int)outStream.Position),
                WebSocketMessageType.Binary, true, cancellationSource.Token);
        }
    }
}