using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class WorkerManager : IWorkerManager
    {
        private readonly IConnectionService _connectionService;
        private readonly IConstantManager _constantManager;

        public WorkerManager(IConnectionService connectionService, IConstantManager constantManager)
        {
            _connectionService = connectionService;
            _constantManager = constantManager;
        }

        public async Task Manage(Observation currentObservation, ResponseData gameData)
        {
            if (currentObservation.PlayerCommon.FoodWorkers < 75) // Decision
            {
                var baseUnits = currentObservation.GetPlayerUnits(_constantManager.BaseTypeIds); // Base Manager
                if (baseUnits[0].Orders.Count > 0) // Single command centre for now
                {
                    return;
                }
                var action = new Action();
                action.ActionRaw = new ActionRaw();
                action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
                action.ActionRaw.UnitCommand.AbilityId = (int)gameData.Units[_constantManager.WorkerUnitIndex].AbilityId;
                action.ActionRaw.UnitCommand.UnitTags.Add(baseUnits[0].Tag); // Single command centre for now
                var requestAction = new RequestAction();
                requestAction.Actions.Add(action); // ActionService? Can send multiple actions in one request
                
                await _connectionService.SendRequestAsync(new Request { Action = requestAction }); // Queue a list desired prioritised actions, that trigger when possible (unit queue is close to finished and resources are sufficient)
            }
        }
    }
}