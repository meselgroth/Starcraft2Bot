using System;
using System.Collections.Generic;
using SC2APIProtocol;

namespace HiveMind
{
    public class ConstantManager : IConstantManager
    {
        private readonly Race _race;

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

        public uint[] GetBaseTypeIds
        {
            get
            {
                if (_race == Race.Terran)
                    return new [] { CommandCenter, OrbitalCommand };
                throw new NotImplementedException();
            }
        }

        public const int Scv = 45;
        public const uint CommandCenter = 18;
        public const uint OrbitalCommand = 132;
    }
}