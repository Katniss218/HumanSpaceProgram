using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// State information about multiple resources.
    /// </summary>
    [Serializable]
    public class SubstanceStateCollection : IEnumerable<SubstanceState>
    {
        const float EPSILON = 1e-3f;

        [field: SerializeField]
        List<SubstanceState> _substances;

        public int SubstanceCount => _substances?.Count ?? 0;

        public SubstanceState this[int i]
        {
            get => _substances[i];
            set => _substances[i] = value;
        }

        public static SubstanceStateCollection Empty => new SubstanceStateCollection();

        public SubstanceStateCollection()
        {
            _substances = new List<SubstanceState>();
        }

        public SubstanceStateCollection( IEnumerable<SubstanceState> substances )
        {
            if( substances is ICollection<SubstanceState> collection )
            {
                _substances = new List<SubstanceState>( collection.Count );
                foreach( var s in collection )
                {
                    _substances.Add( s );
                }
            }
            else
            {
                _substances = new List<SubstanceState>();
                if( substances != null )
                {
                    foreach( var s in substances )
                    {
                        _substances.Add( s );
                    }
                }
            }
        }

        public SubstanceStateCollection( params SubstanceState[] substances )
        {
            _substances = substances == null ? new List<SubstanceState>() : new List<SubstanceState>( substances );
        }

        public bool IsEmpty() => _substances == null || _substances.Count == 0;

        public float GetVolume()
        {
            if( IsEmpty() )
                return 0f;

            float volumeAcc = 0f;
            for( int i = 0; i < _substances.Count; i++ )
            {
                SubstanceState s = _substances[i];
                volumeAcc += s.MassAmount / s.Substance.Density;
            }

            return volumeAcc;
        }

        public float GetMass()
        {
            if( IsEmpty() )
                return 0f;

            float massAcc = 0f;
            for( int i = 0; i < _substances.Count; i++ )
                massAcc += _substances[i].MassAmount;

            return massAcc;
        }

        public float GetAverageDensity()
        {
            if( IsEmpty() )
                throw new InvalidOperationException( $"Can't compute average density for empty {nameof( SubstanceStateCollection )}" );

            float totalMass = 0f;
            float totalVolume = 0f;
            for( int i = 0; i < _substances.Count; i++ )
            {
                SubstanceState s = _substances[i];
                float mass = s.MassAmount;
                float density = s.Substance.Density;
                totalMass += mass;
                totalVolume += (mass / density);
            }

            if( totalVolume <= 0f )
                throw new InvalidOperationException( "Total volume is zero or negative." );

            return totalMass / totalVolume;
        }

        public void SetVolume( float volume )
        {
            if( IsEmpty() )
                throw new InvalidOperationException( $"Can't set volume for empty {nameof( SubstanceStateCollection )}" );

            float currentVolume = GetVolume();

            if( Mathf.Approximately( currentVolume, 0f ) )
                throw new InvalidOperationException( "Current volume is zero; cannot scale." );

            float scaleFactor = volume / currentVolume;
            for( int i = _substances.Count - 1; i >= 0; i-- )
            {
                SubstanceState s = _substances[i];
                float newMass = scaleFactor * s.MassAmount; // density unchanged -> mass scaled same as volumetric scaling

                if( Mathf.Abs( newMass ) <= EPSILON )
                    _substances.RemoveAt( i );
                else
                    _substances[i] = new SubstanceState( newMass, s.Substance );
            }
        }

        public void SetMass( float mass )
        {
            if( IsEmpty() )
                throw new InvalidOperationException( $"Can't set mass for empty {nameof( SubstanceStateCollection )}" );

            float currentMass = GetMass();
            if( Mathf.Approximately( currentMass, 0f ) )
                throw new InvalidOperationException( "Current mass is zero; cannot scale." );

            float scaleFactor = mass / currentMass;
            for( int i = _substances.Count - 1; i >= 0; i-- )
            {
                SubstanceState s = _substances[i];
                float newMass = scaleFactor * s.MassAmount;

                if( Mathf.Abs( newMass ) <= EPSILON )
                    _substances.RemoveAt( i );
                else
                    _substances[i] = new SubstanceState( newMass, s.Substance );
            }
        }

        public int IndexOf( Substance substance )
        {
            if( substance == null )
                return -1;

            for( int i = 0; i < _substances.Count; i++ )
            {
                if( ReferenceEquals( _substances[i].Substance, substance ) )
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Add/remove a single SubstanceState (dt scales the incoming mass).
        /// </summary>
        public void Add( SubstanceState s, float dt = 1.0f )
        {
            float delta = s.MassAmount * dt;
            if( Mathf.Abs( delta ) <= EPSILON )
                return;

            int idx = IndexOf( s.Substance );
            if( idx >= 0 )
            {
                float newAmount = _substances[idx].MassAmount + delta;
                if( Mathf.Abs( newAmount ) <= EPSILON )
                    _substances.RemoveAt( idx );
                else
                    _substances[idx] = new SubstanceState( newAmount, _substances[idx].Substance );
            }
            else
            {
                _substances.Add( new SubstanceState( delta, s.Substance ) );
            }
        }

        /// <summary>
        /// Add/remove all substances from other (dt scales the incoming mass).
        /// </summary>
        public void Add( SubstanceStateCollection other, float dt = 1.0f )
        {
            if( other == null )
                return;
            if( other.IsEmpty() )
                return;

            for( int i = 0; i < other._substances.Count; i++ )
            {
                SubstanceState s = other._substances[i];
                float delta = s.MassAmount * dt;
                if( Mathf.Abs( delta ) <= EPSILON )
                    continue;

                int idx = IndexOf( s.Substance );
                if( idx >= 0 )
                {
                    float newAmount = _substances[idx].MassAmount + delta;
                    if( Mathf.Abs( newAmount ) <= EPSILON )
                        _substances.RemoveAt( idx );
                    else
                        _substances[idx] = new SubstanceState( newAmount, _substances[idx].Substance );
                }
                else
                {
                    _substances.Add( new SubstanceState( delta, s.Substance ) );
                }
            }
        }

        public bool Contains( Substance substance ) => IndexOf( substance ) != -1;

        public bool TryGet( Substance substance, out SubstanceState result )
        {
            int idx = IndexOf( substance );
            if( idx >= 0 )
            {
                result = _substances[idx];
                return true;
            }
            result = default;
            return false;
        }

        public void Clear() => _substances.Clear();

        public void TrimExcess() => _substances.TrimExcess();

        public SubstanceStateCollection Clone()
        {
            List<SubstanceState> copy = new( _substances.Count );
            copy.AddRange( _substances );
            return new SubstanceStateCollection( copy );
        }

        public IEnumerator<SubstanceState> GetEnumerator() => _substances.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        [MapsInheritingFrom( typeof( SubstanceStateCollection ) )]
        public static SerializationMapping SubstanceStateCollectionMapping()
        {
            return new MemberwiseSerializationMapping<SubstanceStateCollection>()
                .WithMember( "substances", o => o._substances );
        }
    }
}