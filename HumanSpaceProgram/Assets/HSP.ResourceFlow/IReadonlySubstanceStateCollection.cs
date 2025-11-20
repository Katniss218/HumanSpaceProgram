using System.Collections.Generic;

namespace HSP.ResourceFlow
{
    public interface IReadonlySubstanceStateCollection : IEnumerable<ISubstance>, IEnumerable<(ISubstance, double)>
    {
        bool IsEmpty();
        int Count { get; }
        (ISubstance, double) this[int i] { get; }
        double this[ISubstance i] { get; }

        double GetMass();

        ISubstanceStateCollection Clone();

        bool Contains( ISubstance substance );
        bool TryGet( ISubstance substance, out double mass );
    }

    public interface ISubstanceStateCollection : IReadonlySubstanceStateCollection
    {
        void Add( ISubstance s, double mass, double dt );
        void Add( IReadonlySubstanceStateCollection s, double dt );
        void Clear();
    }
}