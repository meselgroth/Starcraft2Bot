using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class UnitBuilder : IUnitBuilder
    {
        private readonly IConnectionService _connectionService;
        private readonly IConstantManager _constantManager;
        private readonly IGameDataService _gameDataService;

        public UnitBuilder(IConnectionService connectionService, IConstantManager constantManager,
            IGameDataService gameDataService)
        {
            _connectionService = connectionService;
            _constantManager = constantManager;
            _gameDataService = gameDataService;
        }

        public async Task<bool> BuildWorkerIfEmptyQueue(Observation currentObservation)
        {
            if (currentObservation.PlayerCommon.FoodWorkers < 18) // Decision
            {
                var baseUnits = currentObservation.GetPlayerUnits(_constantManager.BaseTypeIds); // Base Manager
                // Single command centre for now
                // Check queue is empty
                if (baseUnits[0].Orders.Count == 0)
                {
                    await SendBuildRequest(baseUnits[0], _constantManager.WorkerUnitIndex);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> BuildUnitIfEmptyQueue(Observation currentObservation, int buildingType, int unitType)
        {
            foreach (var building in currentObservation.GetPlayerUnits(buildingType))
            {
                if (building.Orders.Count == 0)
                {
                    await SendBuildRequest(building, unitType);
                    return true;
                }
            }
            return false;
        }

        private async Task SendBuildRequest(Unit buildingUnit, int unitType)
        {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = _gameDataService.GetAbilityId(unitType);
            action.ActionRaw.UnitCommand.UnitTags.Add(buildingUnit.Tag);
            var requestAction = new RequestAction();
            requestAction.Actions.Add(action); // ActionService? Can send multiple actions in one request

            await _connectionService.SendRequestAsync(new Request { Action = requestAction }); // Queue a list desired prioritised actions, that trigger when possible (unit queue is close to finished and resources are sufficient)
        }
    }
}