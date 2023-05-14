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
    /// Represents an amount of specific bulk resources.
    /// </summary>
    [Serializable]
    public struct BulkContents
    {
        public float Volume { get; private set; }

        public int ResourceCount => _fluids.Count;

        [field: SerializeField]
        List<(IBulkContainer.ResourceData fluid, float volume)> _fluids;

        public (IBulkContainer.ResourceData fluid, float volume)[] GetResources()
        {
            return _fluids.ToArray();
        }

        public static BulkContents Empty => new BulkContents()
        {
            _fluids = new List<(IBulkContainer.ResourceData fluid, float volume)>()
        };

        public void Add( BulkContents other, float volumeMultiplier = 1.0f )
        {
            List<(IBulkContainer.ResourceData fluid, float volume)> newFluids = new();

            foreach( var (fluid, volume) in other._fluids )
            {
                for( int i = 0; i < _fluids.Count; i++ )
                {
                    if( this._fluids[i].fluid.IdentityEquals( fluid ) )
                    {
#warning TODO - deleting fluids too? needs fixing the unstable flow first though, we can't let it get to negatives because it's gonna be permanently lost.
                        float vol = volume * volumeMultiplier;

                        this._fluids[i] = (_fluids[i].fluid, _fluids[i].volume + vol);
                        this.Volume += vol;
                        break;
                    }
                    // Fluid not present in this container yet.
                    if( i == _fluids.Count - 1 )
                    {
                        newFluids.Add( (fluid, volume * volumeMultiplier) );
                    }
                }
            }

            if( newFluids.Count == 0 )
            {
                return;
            }

            foreach( var (fluid, volume) in newFluids )
            {
                float vol = volume * volumeMultiplier;
                _fluids.Add( (fluid, vol) );

                this.Volume += vol;
            }
        }
    }

    public struct BulkSampleData
    {
        /// <summary>
        /// (Internal) pressure in the tank at the given point, in [Pa].
        /// </summary>
        public float Pressure;
        /// <summary>
        /// What will want to flow out of the hole and at which rate.
        /// </summary>
        public BulkContents[] Flow;
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

        BulkContents Contents { get; }

        /// <summary>
        /// Inflow minus outflow. positive = inflow, negative = outflow.
        /// </summary>
        BulkContents TotalInflow { get; set; }

        /// <summary>
        /// Average velocity of the contents at each connection connected to this container, multiplied by the number of connections.
        /// </summary>
        Vector3 TotalVelocity { get; set; }

        /// <summary>
        /// Calculates the pressure acting at any given point inside the container, as well as what species will want to `flow` out of the container.
        /// </summary>
        /// <remarks>
        /// If possible, the pressure should be extrapolated, if the position falls out of bounds.
        /// </remarks>
        /// <param name="localPosition">The local position of the point to sample, in [m].</param>
        /// <param name="localAcceleration">The local acceleration vector, in [m/s^2].</param>
        /// <param name="holeArea">The area of the hole, in [m^2].</param>
        public BulkSampleData Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea );
    }
}