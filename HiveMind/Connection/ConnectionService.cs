using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
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
            //Console.WriteLine($"Received response, Case:{response.ResponseCase}, Status{response.Status}");

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
            var bytes = new byte[UInt16.MaxValue]; // Max size of Sc2Api message is unknown, however size of efficient websocket fits in 2 bytes https://stackoverflow.com/a/14119129/2235675 which results in 65535 possible values
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
    }
}
