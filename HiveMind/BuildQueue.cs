using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class BuildQueue : IBuildQueue
    {
        private readonly BuildingManager _buildingManager;

        public BuildQueue(BuildingManager buildingManager)
        {
            _buildingManager = buildingManager;
        }

        public async Task Act(Observation currentObservation, ResponseData gameData)
        {
            if (currentObservation.PlayerCommon.FoodWorkers == 13)
            {
                await _buildingManager.Build(currentObservation, gameData, ConstantManager.SupplyDepot);
            }
        }
    }
}