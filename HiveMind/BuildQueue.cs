using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace HiveMind
{
    public class BuildQueue : IBuildQueue
    {
        private readonly IBuildingManager _buildingManager;
        private readonly Queue<BuildQueueItem> _queue; // Queue of Action, WasTriggered

        public BuildQueue(IBuildingManager buildingManager)
        {
            _buildingManager = buildingManager;
            _queue = new Queue<BuildQueueItem>();
        }

        public async Task Act(Observation currentObservation)
        {
            var supplyUnit = new ConstantManager(Race.Terran).SupplyUnit;

            // Fill queue
            if (currentObservation.PlayerCommon.FoodWorkers == 13)
            {
                // don't queue if already queued
                _queue.Enqueue(new BuildQueueItem
                {
                    Action = () => _buildingManager.Build(currentObservation, supplyUnit),
                    Triggered = false
                });
            }

            // Trigger next build queue item, dequeue if previous trigger was successful.
            // Next trigger waits till Act is called again
            // TODO: trigger after dequeue
            if (_queue.Count > 0)
            {
                var buildItem = _queue.Peek();
                if (buildItem.Triggered)
                {
                    // Previous trigger was successful, TODO: Store success query in queue build item
                    if (currentObservation.GetPlayerUnits(new[] { (uint)supplyUnit }, false).Count == 1)
                    {
                        _queue.Dequeue();
                    }
                    else
                    {
                        // Trigger again
                        await buildItem.Action();
                    }
                }
                else
                {
                    await buildItem.Action();
                    buildItem.Triggered = true;
                }
            }
        }
    }

    public class BuildQueueItem
    {
        public Func<Task> Action { get; set; }
        public bool Triggered { get; set; }
    }
}