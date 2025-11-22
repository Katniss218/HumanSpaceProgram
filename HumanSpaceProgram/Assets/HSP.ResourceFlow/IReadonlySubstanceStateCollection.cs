using System.Collections.Generic;

namespace HSP.ResourceFlow
{
    public interface IReadonlySubstanceStateCollection : IEnumerable<(ISubstance, double)>
    {
        /// <summary>
        /// Checks if the collection is empty (contains no substances).
        /// </summary>
        bool IsEmpty(); 
        /// <summary>
        /// Checks if the collection contains only one substance, and outputs that substance.
        /// </summary>
        /// <param name="dominantSubstance">The only substance that exists in the collection.</param>
        bool IsPure( out ISubstance dominantSubstance );

        /// <summary>
        /// Gets the number of distinct substances in the collection.
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Gets the substance and its mass at the given index.
        /// </summary>
        (ISubstance s, double mass) this[int i] { get; }
        /// <summary>
        /// Gets the mass of the given substance in the collection.
        /// </summary>
        double this[ISubstance s] { get; }

        /// <summary>
        /// Gets the total mass of all substances in the collection.
        /// </summary>
        double GetMass();

        /// <summary>
        /// Returns a new clone of the substance state collection.
        /// </summary>
        ISubstanceStateCollection Clone();

        /// <summary>
        /// Checks if the collection contains the given substance.
        /// </summary>
        bool Contains( ISubstance substance );

        /// <summary>
        /// Tries to get the mass of the given substance in the collection.
        /// </summary>
        /// <param name="substance">The substance to find.</param>
        /// <param name="mass">The amount of the substance in the collection, in [kg].</param>
        /// <returns>True if the substance exists in the collection.</returns>
        bool TryGet( ISubstance substance, out double mass );
    }
}