using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.ResourceFlowSystem
{
    /// <summary>
    /// Contains state information about multiple resources, and methods to combine them.
    /// </summary>
    [Serializable]
    public class SubstanceStateCollection
    {
        [field: SerializeField]
        List<SubstanceState> _substances = null; // different substances in the fluid.

        public int SubstanceCount => _substances?.Count ?? 0;

        public SubstanceState this[int i]
        {
            get
            {
                return _substances[i];
            }
            set
            {
                _substances[i] = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="SubstanceStateCollection"/> that represents no flow. Nominally <see cref="null"/>.
        /// </summary>
        public static SubstanceStateCollection Empty => new SubstanceStateCollection( null );

        public SubstanceStateCollection( IEnumerable<SubstanceState> substances )
        {
            if( substances != null )
            {
                this._substances = substances.ToList();
            }
        }

        /// <summary>
        /// Checks whether or not the substance collection is empty.
        /// </summary>
        public bool IsEmpty()
        {
            return _substances == null || SubstanceCount == 0;
        }

        /// <summary>
        /// Checks whether or not the specified substance collection is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty( SubstanceStateCollection sbs )
        {
            return sbs == null || sbs.IsEmpty();
        }

        /// <summary>
        /// Calculates the total volume occupied by the resources.
        /// </summary>
        public float GetVolume()
        {
            if( IsEmpty() )
            {
                return 0.0f;
            }

            // for compressibles, this needs to know their pressure.
            return _substances.Sum( s => s.MassAmount / s.Data.Density );
        }

        /// <summary>
        /// Sets the total volume to the specified amount, preserving the volumetric ratio of the contents.
        /// </summary>
        public void SetVolume( float volume )
        {
            if( IsEmpty() )
            {
                throw new InvalidOperationException( $"Can't set volume for a {nameof( SubstanceStateCollection )} that is empty." );
            }

            // for compressibles, this needs to know their pressure.
            float currentVolume = GetVolume();
            float scalingFactor = volume / currentVolume;

            for( int i = 0; i < _substances.Count; i++ )
            {
                float newMassAmount = scalingFactor * (_substances[i].MassAmount / _substances[i].Data.Density);
                this._substances[i] = new SubstanceState( this._substances[i], newMassAmount );
            }
        }

        public float GetMass()
        {
            if( IsEmpty() )
            {
                return 0.0f;
            }

            return _substances.Sum( s => s.MassAmount );
        }

        public void SetMass( float mass )
        {
            if( IsEmpty() )
            {
                throw new InvalidOperationException( $"Can't set volume for a {nameof( SubstanceStateCollection )} that is empty." );
            }

            // for compressibles, this needs to know their pressure.
            float currentMass = GetMass();
            float scalingFactor = mass / currentMass;

            for( int i = 0; i < _substances.Count; i++ )
            {
                float newMassAmount = scalingFactor * _substances[i].MassAmount;
                this._substances[i] = new SubstanceState( this._substances[i], newMassAmount );
            }
        }
        // set mass possibly too (preserving mass ratio of contents).

        public float GetAverageDensity()
        {
            if( IsEmpty() )
            {
                throw new InvalidOperationException( $"Can't compute the average density for a {nameof( SubstanceStateCollection )} that is empty." );
            }

            float totalMass = 0;
            float weightedSum = 0;

            foreach( var substance in _substances )
            {
                totalMass += substance.MassAmount;
                weightedSum += substance.MassAmount * substance.Data.Density; // idk if this is right.
            }

            return weightedSum / totalMass;
        }

        public void Add( SubstanceStateCollection other, float dt = 1.0f )
        {
            if( IsNullOrEmpty( other ) )
            {
                return;
            }

            // ADDS OR REMOVES RESOURCES.

            Dictionary<string, int> substanceIndexMap = new();

            // Create a dictionary to store the index of each substance in the _substances list
            for( int i = 0; i < _substances.Count; i++ )
            {
                substanceIndexMap[_substances[i].Data.ID] = i;
            }

            for( int i = 0; i < other._substances.Count; i++ )
            {
                string substanceId = other._substances[i].Data.ID;

                if( substanceIndexMap.ContainsKey( substanceId ) )
                {
                    int index = substanceIndexMap[substanceId];
                    float amountDelta = other._substances[i].MassAmount * dt;
                    float newAmount = this._substances[index].MassAmount + amountDelta;

                    if( newAmount <= 0 )
                    {
                        // Remove the entry from the _substances list
                        _substances.RemoveAt( index );
                        substanceIndexMap.Remove( substanceId );
                    }
                    else
                    {
                        this._substances[index] = new SubstanceState( newAmount, this._substances[index].Data );
                    }
                }
                else
                {
                    // Substance not present in this container yet, add it to newSubstances
                    _substances.Add( new SubstanceState( other._substances[i], other._substances[i].MassAmount * dt ) );
                }
            }
        }

        public void Multiply( float amountMult )
        {
            if( IsEmpty() )
            {
                return;
            }

            for( int i = 0; i < _substances.Count; i++ )
            {
                this._substances[i] = new SubstanceState( this._substances[i], this._substances[i].MassAmount * amountMult );
            }
        }

        public SubstanceStateCollection Clone()
        {
            return new SubstanceStateCollection( _substances?.ToArray() );
        }
    }
}