using System.Threading;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public interface IConnectionService
    {
        Task SendRequestAsync(Request request);
        Task<Response> ReceiveRequestAsync();
        Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken);
    }
}