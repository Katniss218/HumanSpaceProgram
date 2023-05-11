using KatnisssSpaceSimulator.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities
{
    public interface IIdentifiable
    {
        string ID { get; }
    }

    public static class IIdentifiable_Ex
    {
        public static bool IdentityEquals( this IIdentifiable lhs, IIdentifiable rhs )
        {
            if( lhs.ID != rhs.ID )
                return false;

            // maybe type checking later.

            return true;
        }
    }

    /// <summary>
    /// A container for a <see cref="BulkResource"/>.
    /// </summary>
    public class FBulkContainer : Functionality
    {
        [Serializable]
        public class FluidData : IIdentifiable
        {
            [field: SerializeField]
            public string ID { get; set; }

            [field: SerializeField]
            public string Name { get; set; }

            [field: SerializeField]
            public float Density { get; set; }
        }

        /// <summary>
        /// Represents an amount of specific bulk resources.
        /// </summary>
        [Serializable]
        public struct BulkContents
        {
            public float Volume { get; private set; }

            public int ResourceCount => _fluids.Count;

            [field: SerializeField]
            List<(FluidData fluid, float volume)> _fluids;

            public (FluidData fluid, float volume)[] GetResources()
            {
                return _fluids.ToArray();
            }

            public static BulkContents Empty => new BulkContents()
            {
                _fluids = new List<(FluidData fluid, float volume)>()
            };

            public void Add( BulkContents other, float volumeMultiplier = 1.0f )
            {
                List<(FluidData fluid, float volume)> newFluids = new();

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

        /// <summary>
        /// Defines how the properties of the contents of the container are calculated, at any point inside the container.
        /// </summary>
        public interface IShape
        {
            public struct SampleData
            {
                /// <summary>
                /// (Internal) pressure in the tank at the given point.
                /// </summary>
                public float Pressure;
                /// <summary>
                /// What will want to flow out of the hole and at which rate.
                /// </summary>
                public BulkContents[] Flow;
            }

            /// <summary>
            /// Calculates the pressure acting at any given point inside the container.
            /// </summary>
            /// <remarks>
            /// If possible, the pressure should be extrapolated, if the position falls out of bounds.
            /// </remarks>
            /// <param name="localPosition">The local position of the point to sample.</param>
            /// <param name="localAcceleration">The local acceleration vector, in [m/s^2].</param>
            /// <param name="contents">The contents of the tank to sample.</param>
            SampleData Sample( Vector3 localPosition, Vector3 localAcceleration, float holeCrossSection, FBulkContainer contents );
        }

        /// <summary>
        /// The total available volume of the container, in [m^3].
        /// </summary>
        [field: SerializeField]
        public float MaxVolume { get; set; }

        /// <summary>
        /// Determines the center position of the container.
        /// </summary>
        [field: SerializeField]
        public Transform VolumeTransform { get; set; }

#warning TODO - figure out something that is serializable (func?)
        [field: SerializeField]
        public IShape Shape { get; set; }

        [field: SerializeField]
        public BulkContents Contents { get; set; }

        /// <summary>
        /// Inflow minus outflow. positive = inflow, negative = outflow.
        /// </summary>
        [field: SerializeField]
        internal BulkContents TotalInflow { get; set; }

        /// <summary>
        /// Average velocity of the contents at each connection connected to this container, multiplied by the number of connections.
        /// </summary>
        [field: SerializeField]
        internal Vector3 TotalVelocity { get; set; }

        void FixedUpdate()
        {
            Contents.Add( TotalInflow, Time.fixedDeltaTime );
            //Volume += TotalInflow * Time.fixedDeltaTime;
            // TODO - update the mass too, because the fluid weighs something.
        }

        // flow between containers depends on many factors.
        // - acceleration on the container (ullage, only for liquids)
        // - pressure difference across the containers (both liquids and gasses).
        // - for mixed liquid + gas, gas will tend to accumulate on the opposite side to the liquid.
        // - for mixed liquid the densest will accumulate at the bottom.
        // - for mixed solid (gravel-like), orientation depends on where and when the resources came from. They don't tend to separate into layers based on density, unlike liquids and gasses.

        // - ullage purposes assume spherical tank??? maybe?
        //   - the orientation of the inlet could determine where on the sphere it is.

        // tanks could have an arbitrary number of connections depending on where the player puts the pipes?

        // the connections would be set up in the VAB / by the save loader.

        public override JToken Save()
        {
            throw new NotImplementedException();

            /*return new JObject()
            {
                { "Resources", this.Resources.ToString() }, // temp
                { "ConnectedTo", this.ConnectedTo.ToString() } // temp
            };*/
        }

        public override void Load( JToken data )
        {
            throw new NotImplementedException();

            //this.Resources = new Resource[] { }; // temp
            //this.ConnectedTo = new FBulkContainer[] { }; // temp
        }
    }
}