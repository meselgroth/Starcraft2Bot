using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public interface IWebSocketWrapper
    {
        Task SendAsync(Request request, CancellationToken cancellationToken);
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> arraySegment, CancellationToken cancellationToken);
    }
}