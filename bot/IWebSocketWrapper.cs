using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public interface IWebSocketWrapper
    {
        Task ConnectWebSocket();
        Task SendRequestAsync(Request request);
    }
}