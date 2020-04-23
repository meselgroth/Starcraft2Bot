using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public interface IWorkerManager
    {
        Task Manage(Observation currentObservation, ResponseData gameData);
    }
}