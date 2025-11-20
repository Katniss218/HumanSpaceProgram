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
    public sealed class SubstanceStateCollection : ISubstanceStateCollection, IEnumerable<ISubstance>, IEnumerable<(ISubstance, double)>
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

        public bool IsEmpty() => _count == 0;

        public double GetVolume()
        {
            if( IsEmpty() )
                return 0;

            double volumeAcc = 0;
            for( int i = 0; i < _count; i++ )
            {
                volumeAcc += _masses[i] / _species[i].GetDensity(,);
            }

            return volumeAcc;
        }

        public double GetMass()
        {
            return _totalMass;
        }

        public void SetMass( float mass )
        {
            if( IsEmpty() )
                throw new InvalidOperationException( $"Can't set mass for empty {nameof( SubstanceStateCollection )}" );

            if( _totalMass == 0 )
                throw new InvalidOperationException( "Current mass is zero; cannot scale." );

            double scaleFactor = mass / _totalMass;

            for( int i = 0; i < _count; /* increment inside */)
            {
                _masses[i] *= scaleFactor;

                if( Math.Abs( _masses[i] ) <= EPSILON )
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
            if( _species.Length >= capacity )
                return;

            int newCapacity = Math.Max( capacity, _species.Length * 2 );
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
        public void Add( ISubstance substance, double mass, double dt = 1.0 )
        {
            if( substance == null )
                return;

            double delta = mass * dt;
            if( Math.Abs( delta ) <= EPSILON )
                return;

            int i = IndexOf( substance );
            if( i >= 0 )
            {
                _totalMass += delta;
                double newMass = _masses[i] + delta;
                // Remove element i, compact by moving last into i.
                if( Math.Abs( newMass ) <= EPSILON )
                {
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

                _masses[i] = newMass;
            }
            else
            {
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
        public void Add( IReadonlySubstanceStateCollection other, double dt = 1.0f )
        {
            if( other == null || other.IsEmpty() )
                return;

            for( int i = 0; i < other.Count; i++ )
            {
                (ISubstance s, double mass) = other[i];
                Add( s, mass, dt );
            }
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
            SubstanceStateCollection copy = new SubstanceStateCollection();
            copy.EnsureCapacity( _count );

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


        // setialization stuff:

        private (Substance, double)[] GetSubstances()
        {
            throw new NotImplementedException();
        }

        private void SetSubstances( (Substance, double)[] substances )
        {
            throw new NotImplementedException();
        }

        [MapsInheritingFrom( typeof( SubstanceStateCollection ) )]
        public static SerializationMapping SubstanceStateCollectionMapping()
        {
            return new MemberwiseSerializationMapping<SubstanceStateCollection>()
                .WithMember( "substances", o => o.GetSubstances(), ( o, value ) => o.SetSubstances( value ) );
        }
    }
}