using System;
using System.Threading.Tasks;
using SC2APIProtocol;
using Action = SC2APIProtocol.Action;

namespace HiveMind
{
    public class BuildingManager : IBuildingManager
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

        public async Task Build(Observation currentObservation, int unitType)
        {
            var workers = currentObservation.GetPlayerUnits(new[] { (uint)_constantManager.WorkerUnitIndex });
            var worker = workers[0]; // Use first selected worker for now

            var mapGrid = new MapGrid(Game.ResponseGameInfo.StartRaw.PlacementGrid, Game.ResponseGameInfo.StartRaw.PathingGrid,
                Game.ResponseGameInfo.StartRaw.PlayableArea);

            var point = mapGrid.GetAvailableMainBaseDiamond();

            await SendBuildRequest(worker, unitType, point);
        }

        private async Task SendBuildRequest(Unit worker, int unitType, Point2D point)
        {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = _gameDataService.GetAbilityId(unitType);
            action.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = point;

            var requestAction = new RequestAction();
            requestAction.Actions.Add(action);

            await _connectionService.SendRequestAsync(new Request { Action = requestAction });

            // Send worker back to minerals
        }
    }
}