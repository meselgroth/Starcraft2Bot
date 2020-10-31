using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public interface IUnitBuilder
    {
        Task<bool> BuildWorkerIfEmptyQueue(Observation currentObservation);
        Task<bool> BuildUnitIfEmptyQueue(Observation currentObservation, int buildingType, int unitType);
    }
}