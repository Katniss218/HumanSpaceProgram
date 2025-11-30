using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// State information about multiple resources.
    /// </summary>
    [Serializable]
    public sealed class SubstanceStateCollection : ISubstanceStateCollection
    {
        const float EPSILON = 1e-3f;

        ISubstance[] _species;
        double[] _masses;
        double _totalMass;
        int _count;

        public int Count => _count;

        public (ISubstance, double) this[int i]
        {
            get => (_species[i], _masses[i]);
        }

        public double this[ISubstance substance]
        {
            get
            {
                for( int i = 0; i < _count; i++ )
                {
                    if( ReferenceEquals( _species[i], substance ) )
                        return _masses[i];
                }
                return 0.0;
            }
            set
            {
                int i;
                double delta = value;
                for( i = 0; i < _count; i++ )
                {
                    if( ReferenceEquals( _species[i], substance ) )
                    {
                        delta -= _masses[i];
                        double newMass = value;
                        // Remove element i, compact by moving last into i.
                        if( newMass <= EPSILON )
                        {
                            _totalMass -= _masses[i];
                            int last = _count - 1;
                            if( i != last )
                            {
                                _species[i] = _species[last];
                                _masses[i] = _masses[last];
                            }
                            _species[last] = null;
                            _masses[last] = 0.0;
                            _count--;
                            return;
                        }

                        _totalMass += delta;
                        _masses[i] = newMass;
                        return;
                    }
                }

                if( delta <= EPSILON )
                    return;
                // Add new substance
                i = IndexOfEmpty();
                EnsureCapacity( i );
                _species[i] = substance;
                _masses[i] = value;
                _count++;
                _totalMass += delta;
            }
        }

        public static SubstanceStateCollection Empty => new SubstanceStateCollection();

        public SubstanceStateCollection()
        {
            // Possible optimization: most mixtures will have only a few components, store a few inline fields (maybe 2), and the array on top of that.
            _species = new ISubstance[4];
            _masses = new double[4];
            _count = 0;
            _totalMass = 0;
        }
        public SubstanceStateCollection( int capacity )
        {
            if( capacity <= 0 )
                throw new ArgumentOutOfRangeException( nameof( capacity ), "Capacity must be greater than zero." );

            _species = new ISubstance[capacity];
            _masses = new double[capacity];
            _count = 0;
            _totalMass = 0;
        }

        public bool IsEmpty() => _count == 0;

        public bool IsPure( out ISubstance dominantSubstance )
        {
            if( _count == 1 )
            {
                dominantSubstance = _species[0];
                return true;
            }

            dominantSubstance = null;
            return false;
        }

        public double GetMass()
        {
            return _totalMass;
        }

        public void SetMass( float mass )
        {
            if( mass < 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( mass ), "Mass cannot be negative." );
            }

            if( IsEmpty() )
                throw new InvalidOperationException( $"Can't set mass for empty {nameof( SubstanceStateCollection )}" );

            if( _totalMass == 0 )
                throw new InvalidOperationException( "Current mass is zero, cannot scale." );

            double scaleFactor = mass / _totalMass;

            for( int i = 0; i < _count; /* increment inside */)
            {
                _masses[i] *= scaleFactor;

                if( _masses[i] <= EPSILON )
                {
                    // Remove entry i by replacing with last entry.
                    int last = _count - 1;
                    if( i != last )
                    {
                        _species[i] = _species[last];
                        _masses[i] = _masses[last];
                    }
                    _species[last] = null;
                    _masses[last] = 0.0;
                    _count--;
                    // Do not increment i, because we've just copied new element into i.
                }
                else
                {
                    i++;
                }
            }
            _totalMass = mass;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int IndexOf( ISubstance substance )
        {
            if( substance == null )
                return -1;

            for( int i = 0; i < _count; i++ )
            {
                if( ReferenceEquals( _species[i], substance ) )
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int IndexOfEmpty()
        {
            for( int i = 0; i < _count; i++ )
            {
                if( _species[i] == null )
                {
                    return i;
                }
            }

            return _count;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void EnsureCapacity( int capacity )
        {
            if( _species.Length > capacity )
                return;

            int newCapacity = Math.Max( capacity + 1, _species.Length * 2 );
            Array.Resize( ref _species, newCapacity );
            Array.Resize( ref _masses, newCapacity );
        }

        public void TrimExcess()
        {
            int newCapacity = Math.Max( 4, _count );
            Array.Resize( ref _species, newCapacity );
            Array.Resize( ref _masses, newCapacity );
        }

        public bool Contains( ISubstance substance ) => IndexOf( substance ) != -1;

        public bool TryGet( ISubstance substance, out double mass )
        {
            int i = IndexOf( substance );
            if( i >= 0 )
            {
                mass = _masses[i];
                return true;
            }

            mass = 0.0;
            return false;
        }

        /// <summary>
        /// Add/remove a single SubstanceState (dt scales the incoming mass).
        /// </summary>
        public void Add( ISubstance substance, double mass, double scale = 1.0 )
        {
            if( substance == null )
                return;

            double delta = mass * scale;
            if( Math.Abs( delta ) <= EPSILON )
                return;

            int i = IndexOf( substance );
            if( i >= 0 )
            {
                double newMass = _masses[i] + delta;
                // Remove element i, compact by moving last into i.
                if( newMass <= EPSILON )
                {
                    _totalMass -= _masses[i];
                    int last = _count - 1;
                    if( i != last )
                    {
                        _species[i] = _species[last];
                        _masses[i] = _masses[last];
                    }
                    _species[last] = null;
                    _masses[last] = 0.0;
                    _count--;
                    return;
                }

                _totalMass += delta;
                _masses[i] = newMass;
            }
            else
            {
                if( delta <= EPSILON )
                    return;
                // Add new substance
                i = IndexOfEmpty();
                EnsureCapacity( i );
                _species[i] = substance;
                _masses[i] = delta;
                _count++;
                _totalMass += delta;
            }
        }

        /// <summary>
        /// Add/remove all substances from other (dt scales the incoming mass).
        /// </summary>
        public void Add( IReadonlySubstanceStateCollection other, double scale = 1.0 )
        {
            if( other == null || other.IsEmpty() )
                return;

            for( int i = 0; i < other.Count; i++ )
            {
                (ISubstance s, double mass) = other[i];
                Add( s, mass, scale );
            }
        }

        /// <summary>
        /// Overwrites the state of this collection with the state of the other collection.
        /// Optimized to minimize allocations.
        /// </summary>
        public void Set( IReadonlySubstanceStateCollection other, double scale )
        {
            if( other == null || other.IsEmpty() )
            {
                Clear();
                return;
            }

            int otherCount = other.Count;
            EnsureCapacity( otherCount );

            // Overwrite existing slots
            for( int i = 0; i < otherCount; i++ )
            {
                (ISubstance s, double m) = other[i];
                _species[i] = s;
                _masses[i] = m * scale;
            }

            // Clear any remaining slots if we had more items previously
            if( _count > otherCount )
            {
                Array.Clear( _species, otherCount, _count - otherCount );
                Array.Clear( _masses, otherCount, _count - otherCount );
            }

            _count = otherCount;
            _totalMass = other.GetMass();
        }

        public void Scale( double scale )
        {
            if( Math.Abs( scale ) <= EPSILON )
            {
                Clear();
                return;
            }

            for( int i = 0; i < _count; i++ )
            {
                _masses[i] *= scale;
            }
            _totalMass *= scale;
        }

        public void Clear()
        {
            Array.Clear( _species, 0, _species.Length );
            Array.Clear( _masses, 0, _masses.Length );
            _count = 0;
            _totalMass = 0.0;
        }

        public ISubstanceStateCollection Clone()
        {
            SubstanceStateCollection copy = new SubstanceStateCollection( _masses.Length );
            Array.Copy( _species, copy._species, _count );
            Array.Copy( _masses, copy._masses, _count );
            copy._count = _count;
            copy._totalMass = _totalMass;

            return copy;
        }

        public IEnumerator<ISubstance> GetEnumerator()
        {
            for( int i = 0; i < _count; i++ )
            {
                var s = _species[i];
                if( s != null )
                    yield return s;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<(ISubstance, double)> IEnumerable<(ISubstance, double)>.GetEnumerator()
        {
            for( int i = 0; i < _count; i++ )
            {
                ISubstance s = _species[i];
                if( s != null )
                    yield return (s, _masses[i]);
            }
        }

        // Serialization stuff:

        private (Substance, double)[] GetSubstances()
        {
            if( _count == 0 )
                return Array.Empty<(Substance, double)>();

            var result = new (Substance, double)[_count];
            for( int i = 0; i < _count; i++ )
            {
                // Note: We cast ISubstance to Substance here. 
                // This assumes that only concrete 'Substance' types are used in this collection.
                result[i] = ((Substance)_species[i], _masses[i]);
            }
            return result;
        }

        private void SetSubstances( (Substance, double)[] substances )
        {
            Clear();

            if( substances == null || substances.Length == 0 )
                return;

            EnsureCapacity( substances.Length );

            double calculatedTotalMass = 0;
            int validCount = 0;

            for( int i = 0; i < substances.Length; i++ )
            {
                var (s, m) = substances[i];
                if( s != null && m > EPSILON )
                {
                    _species[validCount] = s;
                    _masses[validCount] = m;
                    calculatedTotalMass += m;
                    validCount++;
                }
            }

            _count = validCount;
            _totalMass = calculatedTotalMass;
        }

        [MapsInheritingFrom( typeof( SubstanceStateCollection ) )]
        public static SerializationMapping SubstanceStateCollectionMapping()
        {
            return new MemberwiseSerializationMapping<SubstanceStateCollection>()
                .WithMember( "substances", o => o.GetSubstances(), ( o, value ) => o.SetSubstances( value ) );
        }
    }
}