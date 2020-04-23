using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public interface IWorkerManager
    {
        Task Manage(Observation currentObservation, ResponseData gameData);
    }
}