using SC2APIProtocol;
using System.Threading.Tasks;

namespace HiveMind
{
    public interface IBuildingManager
    {
        Task<bool> Build(Observation currentObservation, int unitType, int width, int height);
    }
}