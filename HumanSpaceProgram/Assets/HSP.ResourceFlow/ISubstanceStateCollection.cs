namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a mutable state information about multiple resources.
    /// </summary>
    public interface ISubstanceStateCollection : IReadonlySubstanceStateCollection
    {
        new double this[ISubstance s] { get; set; }

        void Add( ISubstance s, double mass, double scale = 1.0 );
        void Add( IReadonlySubstanceStateCollection s, double scale = 1.0 );
        void Set( IReadonlySubstanceStateCollection s, double scale = 1.0 );
        void Scale( double factor );
        void Clear();
    }
}