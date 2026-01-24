using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// A pooled, disposable, read-only wrapper around a SubstanceStateCollection.
    /// Intended for temporary results from simulation methods to avoid GC allocations.
    /// MUST be disposed via a 'using' block.
    /// </summary>
    public sealed class PooledReadonlySubstanceStateCollection : ISampledSubstanceStateCollection
    {
        private readonly SubstanceStateCollection _collection = new SubstanceStateCollection();
        private static readonly Stack<PooledReadonlySubstanceStateCollection> _pool = new Stack<PooledReadonlySubstanceStateCollection>();

        private PooledReadonlySubstanceStateCollection() { }

        /// <summary>
        /// Acquires a cleared PooledReadonlySubstanceState from the object pool.
        /// </summary>
        public static PooledReadonlySubstanceStateCollection Get()
        {
            lock( _pool )
            {
                if( _pool.Count > 0 )
                {
                    return _pool.Pop();
                }
            }
            return new PooledReadonlySubstanceStateCollection();
        }

        /// <summary>
        /// Returns this instance to the object pool.
        /// </summary>
        public void Dispose()
        {
            _collection.Clear();
            lock( _pool )
            {
                _pool.Push( this );
            }
        }

        /// <summary>
        /// Adds the specified mass of a substance to this collection.
        /// Intended for use by producers before returning the collection.
        /// </summary>
        public void Add( ISubstance s, double mass, double scale = 1.0 ) => _collection.Add( s, mass, scale );
        public void Add( IReadonlySubstanceStateCollection s, double scale = 1.0 ) => _collection.Add( s, scale );
        public void Scale( double scale ) => _collection.Scale( scale );

        // --- IReadonlySubstanceStateCollection Implementation ---

        public int Count => _collection.Count;

        public (ISubstance s, double mass) this[int i] => _collection[i];

        public double this[ISubstance s] => _collection[s];

        public bool IsEmpty() => _collection.IsEmpty();

        public bool IsPure( out ISubstance dominantSubstance ) => _collection.IsPure( out dominantSubstance );

        public double GetMass() => _collection.GetMass();

        /// <summary>
        /// Creates a new, non-pooled, persistent clone of the contents.
        /// </summary>
        public ISubstanceStateCollection Clone() => _collection.Clone();

        public bool Contains( ISubstance substance ) => _collection.Contains( substance );

        public bool TryGet( ISubstance substance, out double mass ) => _collection.TryGet( substance, out mass );

        public IEnumerator<(ISubstance, double)> GetEnumerator() => ((IReadonlySubstanceStateCollection)_collection).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}