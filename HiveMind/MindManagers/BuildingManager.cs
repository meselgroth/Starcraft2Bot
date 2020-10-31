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

        public async Task<bool> Build(Observation currentObservation, int unitType, int width, int height)
        {
            var workers = currentObservation.GetPlayerUnits(new[] { (uint)_constantManager.WorkerUnitIndex });
            var worker = workers[0]; // Use first selected worker for now

            var mapManager = Game.MapManager;
            var point = mapManager.GetAvailableDiamondInMainBase(width, height);

            var countOfUnitType = currentObservation.GetPlayerUnits(unitType, false).Count;

            await SendBuildRequest(worker, unitType, point);

            return true;

            //var response = await _connectionService.ReceiveRequestAsync();

            // Check if Build was successful and update MapManager
            // TODO: Move this to BuildQueue
            //if (response != null && response.HasAction && response.Action.Result.Contains(ActionResult.Success))
            //{

            //    if (currentObservation.GetPlayerUnits(unitType, false).Count > countOfUnitType)
            //    {
            //    }
            //}
        }

        private async Task SendBuildRequest(Unit worker, int unitType, Point2D point)
        {
            var requestDebug = new RequestDebug();
            DebugDraw debugDraw = new DebugDraw();
            debugDraw.Boxes.Add(new DebugBox
            {
                Min = new Point { X = point.X - 1, Y = point.Y - 1, Z = 14 },  // Z??
                Max = new Point { X = point.X + 1, Y = point.Y + 1, Z = 11 },
                Color = new Color { R = 180, G = 255, B = 255 }
            });
            requestDebug.Debug.Add(new DebugCommand { Draw = debugDraw });
            await _connectionService.SendRequestAsync(new Request { Debug = requestDebug });

            // move there first
            var action2 = new Action();
            action2.ActionRaw = new ActionRaw();
            action2.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action2.ActionRaw.UnitCommand.AbilityId = 19; // Move abilitiy
            action2.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            action2.ActionRaw.UnitCommand.TargetWorldSpacePos = point;

            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = _gameDataService.GetAbilityId(unitType);
            action.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = point;

            // move back to mining
            var backToMining = new Action();
            backToMining.ActionRaw = new ActionRaw();
            backToMining.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            backToMining.ActionRaw.UnitCommand.QueueCommand = true;
            backToMining.ActionRaw.UnitCommand.AbilityId = 295; // Gather ability TODO: (scv specific!)
            backToMining.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            backToMining.ActionRaw.UnitCommand.TargetUnitTag = Game.MapManager.GetMineralInMainBase();
                        
            var requestAction = new RequestAction();
            //requestAction.Actions.Add(action2);
            requestAction.Actions.Add(action);
            requestAction.Actions.Add(backToMining);

            await _connectionService.SendRequestAsync(new Request { Action = requestAction });
        }
    }
}