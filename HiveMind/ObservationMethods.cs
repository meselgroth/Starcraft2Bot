using System.Collections.Generic;
using System.Linq;
using SC2APIProtocol;

namespace HiveMind
{
    public static class ObservationMethods
    {
        public static List<Unit> GetPlayerUnits(this Observation currentObservation,
            uint[] unitTypeIds,
            bool onlyCompleted = true)
        {
            return currentObservation.RawData.Units
                .Where(unit => unitTypeIds.Contains(unit.UnitType) 
                               && unit.Alliance == Alliance.Self 
                               // ReSharper disable once CompareOfFloatsByEqualityOperator
                               && (!onlyCompleted || unit.BuildProgress == 1)).ToList();
        }
    }
}