using System;
using System.Collections.Generic;
using SC2APIProtocol;

namespace HiveMind
{
    public class ConstantManager : IConstantManager
    {
        private readonly Race _race;
        public const int SupplyDepot = 19;
        public const int Scv = 45;
        public const uint CommandCenter = 18;
        public const uint OrbitalCommand = 132;

        public ConstantManager(Race race)
        {
            _race = race;
        }

        public int WorkerUnitIndex
        {
            get
            {
                if (_race == Race.Terran)
                    return Scv;
                throw new NotImplementedException();
            }
        }

        public uint[] BaseTypeIds
        {
            get
            {
                if (_race == Race.Terran)
                    return new[] { CommandCenter, OrbitalCommand };
                throw new NotImplementedException();
            }
        }

        public int SupplyUnit
        {
            get
            {
                if (_race == Race.Terran)
                    return SupplyDepot;
                throw new NotImplementedException();
            }
        }
    }
}