using System.Collections.Generic;

namespace HSP.ResourceFlow
{
    public interface IReadonlySubstanceStateCollection : IEnumerable<SubstanceState>
    {
        bool IsEmpty();
        int SubstanceCount { get; }
        SubstanceState this[int i] { get; }
        float GetVolume();
        float GetMass();
        float GetAverageDensity();
        int IndexOf( Substance substance );
        bool Contains( Substance substance );
        bool TryGet( Substance substance, out SubstanceState result );
    }
}