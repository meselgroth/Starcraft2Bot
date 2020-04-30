using System;
using System.Threading.Tasks;
using SC2APIProtocol;
using Action = SC2APIProtocol.Action;

namespace HiveMind
{
    public class BuildingManager
    {
        private readonly IConnectionService _connectionService;
        private readonly IConstantManager _constantManager;
        private readonly IGameDataService _gameDataService;

        public BuildingManager(IConnectionService connectionService, IConstantManager constantManager, IGameDataService gameDataService)
        {
            _connectionService = connectionService;
            _constantManager = constantManager;
            _gameDataService = gameDataService;
        }

        public async Task Build(Observation currentObservation, int unitType,
            ResponseGameInfo responseGameInfo)
        {
            var workers = currentObservation.GetPlayerUnits(new[] { (uint)_constantManager.WorkerUnitIndex });
            var worker = workers[0]; // Use first selected worker for now

            //var buildingPlanner = new BasePlanner(responseGameInfo.);
            
            await SendBuildRequest(worker, unitType);
        }

        private async Task SendBuildRequest(Unit worker, int unitType)
        {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = _gameDataService.GetAbilityId(unitType);
                action.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D {X = worker.Pos.X, Y = worker.Pos.Y};

            var requestAction = new RequestAction();
            requestAction.Actions.Add(action);

            await _connectionService.SendRequestAsync(new Request {Action = requestAction});

            // Send worker back to minerals
        }
    }
}