using System.Threading;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public interface IConnectionService
    {
        Task SendRequestAsync(Request request);
        Task<Response> ReceiveRequestAsync();
        Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken);
    }
}