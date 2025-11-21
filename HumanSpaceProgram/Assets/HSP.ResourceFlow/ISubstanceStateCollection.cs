namespace HSP.ResourceFlow
{
    public interface ISubstanceStateCollection : IReadonlySubstanceStateCollection
    {
        void Add( ISubstance s, double mass, double dt = 1.0 );
        void Add( IReadonlySubstanceStateCollection s, double dt = 1.0 );
        void Clear();
    }
}