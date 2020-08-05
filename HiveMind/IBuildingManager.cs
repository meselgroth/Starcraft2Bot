using SC2APIProtocol;
using System.Threading.Tasks;

namespace HiveMind
{
    public interface IBuildingManager
    {
        Task Build(Observation currentObservation, int unitType);
    }
}