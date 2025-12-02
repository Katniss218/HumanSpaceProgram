using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a state information about multiple resources.
    /// </summary>
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

        /// <summary>
        /// Linearly interpolates between two states.
        /// Result = From * (1 - t) + To * t.
        /// </summary>
        /// <param name="from">The start state.</param>
        /// <param name="to">The end state.</param>
        /// <param name="t">The interpolation factor (usually 0.0 to 1.0).</param>
        public static ISubstanceStateCollection Lerp( IReadonlySubstanceStateCollection from, IReadonlySubstanceStateCollection to, double t )
        {
            if( from == null )
                throw new ArgumentNullException( nameof( from ) );
            if( to == null )
                throw new ArgumentNullException( nameof( to ) );

            t = Math.Clamp( t, 0.0, 1.0 ); // Unclamped doesn't make sense, because we can't extrapolate substances that don't exist or have a negative amount of a substance.

            // Clone, scale, and add scaled from the other.
            ISubstanceStateCollection result = from.Clone();
            result.Scale( 1.0 - t );
            result.Add( to, t );

            return result;
        }

        /// <summary>
        /// Computes the average state of a collection of states.
        /// </summary>
        /// <param name="collections">The list of states to average.</param>
        /// <returns>A new collection where each substance mass is the average of the inputs.</returns>
        public static ISubstanceStateCollection Average( IEnumerable<IReadonlySubstanceStateCollection> collections )
        {
            if( collections == null )
                throw new ArgumentNullException( nameof( collections ) );

            using var enumerator = collections.GetEnumerator();

            if( !enumerator.MoveNext() )
                throw new ArgumentException( "Cannot average an empty sequence of collections.", nameof( collections ) );

            ISubstanceStateCollection result = enumerator.Current.Clone();
            long totalCount = 1;

            while( enumerator.MoveNext() )
            {
                totalCount++;
                result.Add( enumerator.Current );
            }

            result.Scale( 1.0 / totalCount );

            return result;
        }
    }
}