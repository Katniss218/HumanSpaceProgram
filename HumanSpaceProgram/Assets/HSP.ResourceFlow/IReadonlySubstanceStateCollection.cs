using System.Collections.Generic;

namespace HSP.ResourceFlow
{
    public interface IReadonlySubstanceStateCollection : IEnumerable<(ISubstance, double)>
    {
        bool IsEmpty(); 
        bool IsPure( out ISubstance dominantSubstance );

        int Count { get; }
        (ISubstance, double) this[int i] { get; }
        double this[ISubstance i] { get; }

        double GetMass();

        ISubstanceStateCollection Clone();

        bool Contains( ISubstance substance );
        bool TryGet( ISubstance substance, out double mass );
    }
}