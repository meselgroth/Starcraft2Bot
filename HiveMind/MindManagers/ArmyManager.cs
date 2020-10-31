using System;
using System.Threading.Tasks;
using SC2APIProtocol;
using Action = SC2APIProtocol.Action;

namespace HiveMind
{
    public class ArmyManager : IArmyManager
    {
        private readonly IConnectionService _connectionService;
        private readonly IConstantManager _constantManager;
        private readonly IGameDataService _gameDataService;

        public ArmyManager(IConnectionService connectionService, IConstantManager constantManager, IGameDataService gameDataService)
        {
            _connectionService = connectionService;
            _constantManager = constantManager;
            _gameDataService = gameDataService;
        }

        public async Task AttackMove(Observation currentObservation)
        {
            var units = currentObservation.GetPlayerUnits(new[] { (uint)_constantManager.FirstArmyUnit });

            foreach (var unit in units)
            {
                await SendAttackRequest(unit, Game.ResponseGameInfo.StartRaw.StartLocations[0]);
            }
        }

        private async Task SendAttackRequest(Unit unit, Point2D point)
        {
            var action2 = new Action();
            action2.ActionRaw = new ActionRaw();
            action2.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action2.ActionRaw.UnitCommand.AbilityId = 23; // Attack abilitiy
            action2.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);
            action2.ActionRaw.UnitCommand.TargetWorldSpacePos = point;
            
            var requestAction = new RequestAction();
            requestAction.Actions.Add(action2);

            await _connectionService.SendRequestAsync(new Request { Action = requestAction });
        }
    }
}