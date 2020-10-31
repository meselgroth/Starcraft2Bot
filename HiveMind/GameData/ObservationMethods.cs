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
        public static List<Unit> GetPlayerUnits(this Observation currentObservation,
            int unitTypeId,
            bool onlyCompleted = true)
        {
            return currentObservation.RawData.Units
                .Where(unit => unitTypeId == unit.UnitType
                               && unit.Alliance == Alliance.Self
                               // ReSharper disable once CompareOfFloatsByEqualityOperator
                               && (!onlyCompleted || unit.BuildProgress == 1)).ToList();
        }
        public static List<Unit> GetPlayerUnitsInProgress(this Observation currentObservation,
            int unitTypeId)
        {
            return currentObservation.RawData.Units
                .Where(unit => unitTypeId == unit.UnitType
                               && unit.Alliance == Alliance.Self
                               && (unit.BuildProgress < 1)).ToList();
        }
        
        public static List<Unit> GetMainBaseMineral(this Observation currentObservation,
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