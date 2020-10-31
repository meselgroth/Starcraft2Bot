using SC2APIProtocol;
using System.Threading.Tasks;

namespace HiveMind
{
    public interface IArmyManager
    {
        Task AttackMove(Observation currentObservation);
    }
}