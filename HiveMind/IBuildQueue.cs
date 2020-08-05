using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public interface IBuildQueue
    {
        Task Act(Observation currentObservation);
    }
}