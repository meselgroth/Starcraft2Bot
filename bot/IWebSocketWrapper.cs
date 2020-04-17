using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public interface IWebSocketWrapper
    {
        Task SendAsync(Request request, CancellationToken cancellationToken);
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> arraySegment, CancellationToken cancellationToken);
    }
}