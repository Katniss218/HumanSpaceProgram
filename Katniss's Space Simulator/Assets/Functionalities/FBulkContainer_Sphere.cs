using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.ResourceFlowSystem;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities
{

    /// <summary>
    /// A container for a <see cref="Substance"/>.
    /// </summary>
    public class FBulkContainer_Sphere : MonoBehaviour, IResourceConsumer, IResourceProducer, IResourceContainer
    {
        /// <summary>
        /// Determines the center position of the container.
        /// </summary>
        [field: SerializeField]
        public Transform VolumeTransform { get; set; }

        /// <summary>
        /// The total available volume of the container, in [m^3].
        /// </summary>
        [field: SerializeField]
        public float MaxVolume { get; set; }

        [field: SerializeField]
        public float Radius { get; set; }

        [field: SerializeField]
        public SubstanceStateCollection Contents { get; set; } = SubstanceStateCollection.Empty;

        [field: SerializeField]
        public SubstanceStateCollection Inflow { get; set; }

        [field: SerializeField]
        public SubstanceStateCollection Outflow { get; set; }

        public void ClampIn( SubstanceStateCollection inflow )
        {
            float currentVol = this.Contents.GetVolume();
            float inflowVol = inflow.GetVolume();

            if( currentVol + inflowVol > MaxVolume )
            {
                inflow.SetVolume( currentVol - MaxVolume );
            }
        }

        public FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea )
        {
            float heightOfLiquid = SolveHeightOfTruncatedSphere( this.Contents.GetVolume() / this.MaxVolume ) * this.Radius;

            float distanceAlongAcceleration = Vector3.Dot( localPosition, localAcceleration.normalized );

            // Adjust the height of the fluid column based on the distance along the acceleration direction
            heightOfLiquid += distanceAlongAcceleration;
            heightOfLiquid -= this.Radius; // since both distance and height of truncated sphere already contain that.

            if( heightOfLiquid <= 0 )
            {
                return FluidState.Vacuum;
            }

            // pressure in [Pa]
            float pressure = localAcceleration.magnitude * this.Contents[0].Data.Density * heightOfLiquid;

            return new FluidState( pressure, 273.15f, 0.0f );
        }

        public (SubstanceStateCollection, FluidState) SampleFlow( Vector3 localPosition, Vector3 localAcceleration, float holeArea, FluidState opposingFluid )
        {
            if( this.Contents.SubstanceCount == 0 )
            {
                return (SubstanceStateCollection.Empty, FluidState.Vacuum);
            }

            FluidState state = Sample( localPosition, localAcceleration, holeArea );

            float relativePressure = state.Pressure - opposingFluid.Pressure;
            if( relativePressure <= 0 )
            {
                return (SubstanceStateCollection.Empty, FluidState.Vacuum);
            }

#warning TODO - mixing and stratification.
            SubstanceStateCollection flow = Contents.Clone();

            float newSignedVelocity = Mathf.Sign( relativePressure ) * Mathf.Sqrt( 2 * relativePressure / flow.GetAverageDensity() );
            float maximumVolumetricFlowrate = Mathf.Abs( FBulkContainerConnection.GetVolumetricFlowrate( holeArea, newSignedVelocity ) );

            flow.SetVolume( maximumVolumetricFlowrate );

            return (flow, new FluidState( relativePressure, state.Temperature, newSignedVelocity ));
        }

        void FixedUpdate()
        {
            Contract.Assert( Contents != null, $"[{nameof( FBulkContainer_Sphere )}.{nameof( Sample )}] '{nameof( Contents )}' can't be null." );

            Contents.Add( Outflow, -Time.fixedDeltaTime );
            Contents.Add( Inflow, Time.fixedDeltaTime );
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

        public JToken Save()
        {
            throw new NotImplementedException();

            /*return new JObject()
            {
                { "Resources", this.Resources.ToString() }, // temp
                { "ConnectedTo", this.ConnectedTo.ToString() } // temp
            };*/
        }

        public void Load( JToken data )
        {
            throw new NotImplementedException();

            //this.Resources = new Resource[] { }; // temp
            //this.ConnectedTo = new FBulkContainer[] { }; // temp
        }


        /// <summary>
        /// Calculates the height of a truncated unit sphere with the given volume as a [0..1] percentage of the unit sphere's volume.
        /// </summary>
        /// <returns>Value between 0 and 2.</returns>
        public static float SolveHeightOfTruncatedSphere( float volumePercent )
        {
            // https://math.stackexchange.com/questions/2364343/height-of-a-spherical-cap-from-volume-and-radius

            const float UnitSphereVolume = 4.18879020479f; // 4/3 * pi     -- radius=1
            const float TwoPi = 6.28318530718f;            // 2 * pi       -- radius=1
            const float Sqrt3 = 1.73205080757f;

            float Volume = UnitSphereVolume * volumePercent;

            float A = 1.0f - ((3.0f * Volume) / TwoPi); // A is a coefficient, [-1..1] for volumePercent in [0..1]
            float OneThirdArccosA = 0.333333333f * Mathf.Acos( A );
            float height = Sqrt3 * Mathf.Sin( OneThirdArccosA ) - Mathf.Cos( OneThirdArccosA ) + 1.0f;
            return height;
        }

    }
}