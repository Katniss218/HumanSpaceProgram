using System.Collections.Generic;

namespace HSP.Vessels
{
    public interface IReadonlyVesselIsland
    {
        IReadOnlyList<VesselPart> Parts { get; }
    }
}