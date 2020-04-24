using System.Collections.Generic;

namespace HiveMind
{
    public interface IConstantManager
    {
        int WorkerUnitIndex { get; }
        uint[] BaseTypeIds { get; }
        int SupplyUnit { get; }
    }
}