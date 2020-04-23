using System.Collections.Generic;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace bot
{
    public class WorkerManager : IWorkerManager
    {
        private readonly IConnectionService _connectionService;
        public const int Scv = 45;
        public static uint COMMAND_CENTER = 18;
        public static uint ORBITAL_COMMAND = 132;

        public WorkerManager(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task Manage(Observation currentObservation, ResponseData gameData)
        {
            if (currentObservation.PlayerCommon.FoodWorkers < 75) // Decision
            {
                var baseUnits = GetUnits(new HashSet<uint> { COMMAND_CENTER, ORBITAL_COMMAND }, currentObservation); // Base Manager
                if (baseUnits[0].Orders.Count > 0)
                {
                    return;
                }
                var requestAction = new RequestAction();
                var action = new Action();
                action.ActionRaw = new ActionRaw();
                action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
                action.ActionRaw.UnitCommand.AbilityId = (int)gameData.Units[Scv].AbilityId;
                requestAction.Actions.Add(action); // ActionService? Can send multiple actions in one request


                action.ActionRaw.UnitCommand.UnitTags.Add(baseUnits[0].Tag);  // Single command centre for now
                await _connectionService.SendRequestAsync(new Request { Action = requestAction }); // Queue a list desired prioritised actions, that trigger when possible (unit queue is close to finished and resources are sufficient)
            }
        }

        public static List<Unit> GetUnits(HashSet<uint> hashset, Observation currentObservation,
            Alliance alliance = Alliance.Self, bool onlyCompleted = false, bool onlyVisible = false)
        {
            //ideally this should be cached in the future and cleared at each new frame
            var units = new List<Unit>();
            foreach (var unit in currentObservation.RawData.Units)
                if (hashset.Contains(unit.UnitType) && unit.Alliance == alliance)
                {
                    if (onlyCompleted && unit.BuildProgress < 1)
                        continue;

                    if (onlyVisible && (unit.DisplayType != DisplayType.Visible))
                        continue;

                    units.Add(unit);
                }
            return units;
        }
    }
}