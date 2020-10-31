using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class BuildQueue : IBuildQueue
    {
        private readonly IBuildingManager _buildingManager;
        private readonly Queue<BuildQueueItem> _queue; // Queue of Action, WasTriggered
        private readonly IUnitBuilder _unitManager;
        private readonly IArmyManager _armyManager;
        private DateTime _attackTime = DateTime.MinValue;

        public BuildQueue(IBuildingManager buildingManager, IUnitBuilder unitManager, IArmyManager armyManager)
        {
            _buildingManager = buildingManager;
            _queue = new Queue<BuildQueueItem>();
            _unitManager = unitManager;
            _armyManager = armyManager;
        }

        public async Task Act(Observation currentObservation)
        {
            var constantManager = new ConstantManager(Race.Terran);
            var supplyUnit = constantManager.SupplyUnit;

            // Fill queue
            if (RequireSupply(currentObservation) && currentObservation.GetPlayerUnitsInProgress(supplyUnit).Count == 0
                && !_queue.Any(b => b.UnitType == supplyUnit))
            {
                Console.WriteLine("Queue supply");
                _queue.Enqueue(new BuildQueueItem
                {
                    Action = () => _buildingManager.Build(currentObservation, supplyUnit, 2, 2),
                    PreviousCount = currentObservation.GetPlayerUnits(supplyUnit, false).Count,
                    UnitType = supplyUnit
                });
            }
            if (currentObservation.GetPlayerUnits(supplyUnit).Count == 1 && currentObservation.GetPlayerUnits(constantManager.FirstArmyBuilding, false).Count < 3
                && !_queue.Any(b => b.UnitType == constantManager.FirstArmyBuilding))
            {
                Console.WriteLine("Queue baracks");

                _queue.Enqueue(new BuildQueueItem
                {
                    Action = () => _buildingManager.Build(currentObservation, constantManager.FirstArmyBuilding, 3, 3),
                    PreviousCount = currentObservation.GetPlayerUnits(constantManager.FirstArmyBuilding, false).Count,
                    UnitType = constantManager.FirstArmyBuilding
                });
            }
            if (currentObservation.GetPlayerUnits(constantManager.FirstArmyBuilding).Count > 0
                && !_queue.Any(b => b.UnitType == constantManager.FirstArmyUnit))
            {
                Console.WriteLine("Queue marine");

                _queue.Enqueue(new BuildQueueItem
                {
                    Action = () => _unitManager.BuildUnitIfEmptyQueue(currentObservation, constantManager.FirstArmyBuilding, constantManager.FirstArmyUnit),
                    PreviousCount = currentObservation.GetPlayerUnits(constantManager.FirstArmyUnit, false).Count,
                    UnitType = constantManager.FirstArmyUnit
                });
            }
            if (currentObservation.GetPlayerUnits(constantManager.FirstArmyUnit).Count >= 24
                && DateTime.Now.Subtract(_attackTime).TotalMinutes > 2)
            {
                Console.WriteLine("Attack move");

                await _armyManager.AttackMove(currentObservation);
                _attackTime = DateTime.Now;
            }

            // Trigger next build queue item, dequeue if previous trigger was successful or too delayed
            // Next trigger waits till Act is called again
            if (_queue.Count > 0)
            {
                var buildItem = _queue.Peek();
                if (!buildItem.Triggered)
                {
                    if (currentObservation.PlayerCommon.Minerals >= Game.ResponseData.Units.First(a => a.UnitId == buildItem.UnitType).MineralCost)
                    {
                        var actionAvailable = await buildItem.Action();
                        buildItem.ActTime = DateTime.Now;
                        buildItem.Triggered = true;
                        if (!actionAvailable)
                        {
                            _queue.Dequeue();
                        }
                    }
                }
                else
                {
                    // Check if previous trigger was successful
                    if (currentObservation.GetPlayerUnits(buildItem.UnitType, false).Count > buildItem.PreviousCount)
                    {
                        _queue.Dequeue();
                    }
                    // Previous trigger was delayed
                    else if (DateTime.Now.Subtract(buildItem.ActTime).TotalSeconds > 8)
                    {
                        Console.WriteLine($"BuildItem failed: {buildItem}");
                        _queue.Dequeue();
                    }
                }
            }
        }

        private static bool RequireSupply(Observation currentObservation)
        {
            return currentObservation.PlayerCommon.FoodCap - currentObservation.PlayerCommon.FoodUsed < 10;
        }
    }

    internal class Resource
    {
        public Resource(int minerals, int gas)
        {
            Minerals = minerals;
            Gas = gas;
        }

        public int Minerals { get; set; }
        public int Gas { get; set; }
    }

    public class BuildQueueItem
    {
        public Func<Task<bool>> Action { get; set; }
        public bool Triggered { get; set; }
        public int PreviousCount { get; internal set; }
        public int UnitType { get; internal set; }
        public DateTime ActTime { get; internal set; }

        public override string ToString()
        {
            return $"UnitType: {UnitType}, ActTime: {ActTime}, PreviousCount: {PreviousCount}";
        }
    }
}