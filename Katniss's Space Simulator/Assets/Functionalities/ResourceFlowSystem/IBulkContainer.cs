using KatnisssSpaceSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities.ResourceFlowSystem
{
    /// <summary>
    /// State information about an unspecified fluid at a given point.
    /// </summary>
    [Serializable]
    public struct FluidState
    {
        [field: SerializeField]
        public float Pressure { get; set; }
        [field: SerializeField]
        public float Temperature { get; set; }
        [field: SerializeField]
        public Vector3 Velocity { get; set; }
    }


    /// <summary>
    /// State information about a single resource.
    /// </summary>
    [Serializable]
    public struct SubstanceState
    {
        /// <summary>
        /// Amount of the resource, tracked using mass, in [kg].
        /// </summary>
        [field: SerializeField]
        public float MassAmount { get; private set; } // Could also be in moles hahaha.

        /// <summary>
        /// The physical/chemical data about the specific resource.
        /// </summary>
        [field: SerializeField]
        public Substance Data { get; private set; }

        public SubstanceState( float massAmount, Substance resource )
        {
            this.MassAmount = massAmount;
            this.Data = resource;
        }
    }


    /// <summary>
    /// Contains state information about the fluid, and what resources it carries.
    /// </summary>
    [Serializable]
    public class SubstanceStateMultiple
    {
        [field: SerializeField]
        public FluidState FluidState { get; }

        [field: SerializeField]
        List<SubstanceState> _substances; // different substances in the fluid.

        public int SubstanceCount => _substances.Count;

        /// <summary>
        /// Returns a <see cref="SubstanceStateMultiple"/> that represents no flow. Nominally <see cref="null"/>.
        /// </summary>
        public static SubstanceStateMultiple NoFlow => null;

        public static bool IsNoFlow( SubstanceStateMultiple sbs )
        {
            return sbs == null || sbs._substances == null; // substances should never be null, but unity inspector forces them to be null... Retarded bullshit...
        }

        public SubstanceStateMultiple( FluidState fluidState, IEnumerable<SubstanceState> substances )
        {
            if( substances == null )
            {
                throw new ArgumentNullException( nameof( substances ), $"Substances must not be null. Use a null {nameof( SubstanceStateMultiple )} if there is no flow." );
            }

            this.FluidState = fluidState;
            this._substances = substances.ToList();
            if( _substances == null || _substances.Count == 0 )
            {
                throw new ArgumentNullException( nameof( substances ), $"Substances must not be zero length. Use a null {nameof( SubstanceStateMultiple )} if there is no flow." );
            }
        }

        public SubstanceState[] GetSubstances()
        {
            return _substances.ToArray();
        }

        public float GetVolume()
        {
            return _substances.Sum( s => s.MassAmount / s.Data.Density );
        }

        public float GetMass()
        {
            return _substances.Sum( s => s.MassAmount );
        }

        public float GetAverageDensity()
        {
            if( _substances.Count == 0 )
            {
                throw new InvalidOperationException( $"There must be at least one substance in order to compute the average density." );
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

        public void Add( SubstanceStateMultiple other, float dt = 1.0f )
        {
            if( other == null || other._substances == null )
            {
                // `other._substances == null` should never happen, but the inspector forces it to be null, and I hate it.
                throw new ArgumentException( $"The substances to add must be set", nameof( other ) );
            }

            // ADDS OR REMOVES RESOURCES (and updates thermodynamical characteristics).


            // calculates the final stable state.
            // add resources together
            // average temperature based on physical characteristics.
            // combine velocities based on physical formulas.
#warning TODO - deleting fluids too? needs fixing the unstable flow first though, we can't let it get to negatives because it's gonna be permanently lost.


            Dictionary<string, int> substanceIndexMap = new();

            // Create a dictionary to store the index of each substance in the _substances list
            for( int i = 0; i < _substances.Count; i++ )
            {
                substanceIndexMap[_substances[i].Data.ID] = i;
            }

            for( int j = 0; j < other._substances.Count; j++ )
            {
                string substanceId = other._substances[j].Data.ID;

                if( substanceIndexMap.ContainsKey( substanceId ) )
                {
                    int index = substanceIndexMap[substanceId];
                    float amountDelta = other._substances[j].MassAmount * dt;

                    this._substances[index] = new SubstanceState( this._substances[index].MassAmount + amountDelta, this._substances[index].Data );
                }
                else
                {
                    // Substance not present in this container yet, add it to newSubstances
                    _substances.Add( new SubstanceState( other._substances[j].MassAmount * dt, other._substances[j].Data ) );
                }
            }


            return;


            List<SubstanceState> newSubstances = new();

            for( int i = 0; i < _substances.Count; i++ )
            {
                for( int j = 0; j < other._substances.Count; j++ )
                {
                    if( this._substances[i].Data.IdentityEquals( other._substances[j].Data ) )
                    {
                        float amountDelta = other._substances[j].MassAmount * dt;

                        this._substances[i] = new SubstanceState( this._substances[i].MassAmount + amountDelta, this._substances[i].Data );
                        break;
                    }
                    // Fluid not present in this container yet.
                    if( j == other._substances.Count - 1 )
                    {
                        newSubstances.Add( new SubstanceState( other._substances[j].MassAmount * dt, other._substances[j].Data ) );
                    }
                }
            }

            /*if( newFluids.Count == 0 )
            {
                return;
            }*/
#warning TODO - doesn't add fluids correctly.
            foreach( var substance in newSubstances )
            {
                float amountDelta = substance.MassAmount * dt;
                _substances.Add( new SubstanceState( amountDelta, substance.Data ) );
            }
        }
    }

    public interface IBulkContainer
    {
        [Serializable]
        public class ResourceData : IIdentifiable
        {
            [field: SerializeField]
            public string ID { get; set; }

            [field: SerializeField]
            public string Name { get; set; }

            [field: SerializeField]
            public float Density { get; set; }
        }

        /// <summary>
        /// Determines the center position of the container.
        /// </summary>
        Transform VolumeTransform { get; }

        /// <summary>
        /// The total available volume of the container, in [m^3].
        /// </summary>
        float MaxVolume { get; }

        SubstanceStateMultiple Contents { get; }

        /// <summary>
        /// Inflow minus outflow. positive = inflow, negative = outflow.
        /// </summary>
        SubstanceStateMultiple Inflow { get; set; }

        /// <summary>
        /// Calculates the pressure acting at any given point inside the container, as well as what species will want to `flow` out of the container.
        /// </summary>
        /// <remarks>
        /// If possible, the pressure should be extrapolated, if the position falls out of bounds.
        /// </remarks>
        /// <param name="localPosition">The local position of the point to sample, in [m].</param>
        /// <param name="localAcceleration">The local acceleration vector, in [m/s^2].</param>
        /// <param name="holeArea">The area of the hole, in [m^2].</param>
        public SubstanceStateMultiple Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea );
    }
}