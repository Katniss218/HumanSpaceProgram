//using HSP.ResourceFlow;
//using HSP.Time;
//using System;
//using System.Diagnostics.Contracts;
//using UnityEngine;
//using UnityPlus.Serialization;

//namespace HSP.Vanilla.Components
//{
//    /// <summary>
//    /// A container for a <see cref="Substance"/>.
//    /// </summary>
//    [Obsolete("Use ResourceContainer_FlowTank instead.")]
//    public class FBulkContainer_Sphere : MonoBehaviour
//    {
//        /// <summary>
//        /// Determines the center position of the container.
//        /// </summary>
//        [field: SerializeField]
//        public Transform VolumeTransform { get; set; }

//        /// <summary>
//        /// The total available volume of the container, in [m^3].
//        /// </summary>
//        [field: SerializeField]
//        public float MaxVolume { get; set; }

//        /// <summary>
//        /// The physical interior radius of the spherical container. Used for inlet/outlet pressure calculations only.
//        /// </summary>
//        [field: SerializeField]
//        public float Radius { get; set; }

//        [field: SerializeField]
//        public SubstanceStateCollection Contents { get; set; } = SubstanceStateCollection.Empty;

//        [field: SerializeField]
//        public SubstanceStateCollection Inflow { get; private set; } = SubstanceStateCollection.Empty;

//        [field: SerializeField]
//        public SubstanceStateCollection Outflow { get; private set; } = SubstanceStateCollection.Empty;

//        public void ClampIn( SubstanceStateCollection inflow, float dt )
//        {
//            float currentVol = this.Contents.GetVolume();
//            throw new NotImplementedException();
//           // FlowUtils.ClampMaxVolume( inflow, currentVol, MaxVolume, dt );
//        }

//        public FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea )
//        {
//            float heightOfLiquid = SolveHeightOfTruncatedSphere( this.Contents.GetVolume() / this.MaxVolume ) * this.Radius;

//            float distanceAlongAcceleration = Vector3.Dot( localPosition, localAcceleration.normalized );

//            // Adjust the height of the fluid column based on the distance along the acceleration direction
//            heightOfLiquid += distanceAlongAcceleration;
//            heightOfLiquid -= this.Radius; // since both distance and height of truncated sphere already contain that.

//            if( heightOfLiquid <= 0 )
//            {
//                return FluidState.Vacuum;
//            }

//            float pressure = FlowUtils.GetStaticPressure( this.Contents[0].Substance.Density, heightOfLiquid, localAcceleration.magnitude );

//            return new FluidState( pressure, 273.15f, 0.0f );
//        }

//        public (SubstanceStateCollection, FluidState) SampleFlow( Vector3 localPosition, Vector3 localAcceleration, float holeArea, float dt, FluidState opposingFluid )
//        {
//            if( this.Contents.Count == 0 )
//            {
//                return (SubstanceStateCollection.Empty, FluidState.Vacuum);
//            }

//            FluidState state = Sample( localPosition, localAcceleration, holeArea );

//            float relativePressure = state.Pressure - opposingFluid.Pressure;
//            if( relativePressure <= 0 )
//            {
//                return (SubstanceStateCollection.Empty, FluidState.Vacuum);
//            }

//#warning TODO - mixing and stratification.
//            SubstanceStateCollection flow = Contents.Clone();

//            // Toricelli's law `sqrt((2 * (P1 - P2)) / density)`.
//            // Pressure can be total dynamic pressure.
//            // P2 can be negative to create suction
//            float newSignedVelocity = Mathf.Sign( relativePressure ) * Mathf.Sqrt( 2 * relativePressure / flow.GetAverageDensity() );
//            float maximumVolumetricFlowrate = Mathf.Abs( FBulkConnection.GetVolumetricFlowrate( holeArea, newSignedVelocity ) );

//            flow.SetVolume( maximumVolumetricFlowrate );

//            float remainingFluidInTank = Contents.GetVolume();

//            if( (flow.GetVolume() * dt) > remainingFluidInTank )
//            {
//                flow.SetVolume( remainingFluidInTank / dt );
//            }

//            return (flow, new FluidState( relativePressure, state.Temperature, newSignedVelocity ));
//        }

//        public float Mass { get => Contents.GetMass(); }

//        public event IHasMass.MassChange OnAfterMassChanged;

//        void FixedUpdate()
//        {
//            Contract.Assert( Contents != null, $"[{nameof( FBulkContainer_Sphere )}.{nameof( Sample )}] '{nameof( Contents )}' can't be null." );

//            float oldMass = this.Mass;
//            Contents.Add( Outflow, -TimeManager.FixedDeltaTime );
//            Contents.Add( Inflow, TimeManager.FixedDeltaTime );
//            OnAfterMassChanged?.Invoke( this.Mass - oldMass );
//        }

//        [MapsInheritingFrom( typeof( FBulkContainer_Sphere ) )]
//        public static SerializationMapping FBulkContainer_SphereMapping()
//        {
//            return new MemberwiseSerializationMapping<FBulkContainer_Sphere>()
//                .WithMember( "volume_transform", ObjectContext.Ref, o => o.VolumeTransform )
//                .WithMember( "max_volume", o => o.MaxVolume )
//                .WithMember( "radius",  o => o.Radius )
//                .WithMember( "contents", o => o.Contents );
//        }

//        /// <summary>
//        /// Calculates the height of a truncated unit sphere with the given volume as a [0..1] percentage of the unit sphere's volume.
//        /// </summary>
//        /// <returns>Value between 0 and 2.</returns>
//        public static float SolveHeightOfTruncatedSphere( float volumePercent )
//        {
//            // https://math.stackexchange.com/questions/2364343/height-of-a-spherical-cap-from-volume-and-radius

//            if( volumePercent > 1.0f )
//                return 2.0f;
//            if( volumePercent < 0.0f )
//                return 0.0f;

//            const float UnitSphereVolume = 4.18879020479f; // 4/3 * pi     -- radius=1
//            const float TwoPi = 6.28318530718f;            // 2 * pi       -- radius=1
//            const float Sqrt3 = 1.73205080757f;

//            float Volume = UnitSphereVolume * volumePercent;

//            float A = 1.0f - ((3.0f * Volume) / TwoPi); // A is a coefficient, [-1..1] for volumePercent in [0..1]
//            float OneThirdArccosA = 0.333333333f * Mathf.Acos( A );
//            float height = Sqrt3 * Mathf.Sin( OneThirdArccosA ) - Mathf.Cos( OneThirdArccosA ) + 1.0f;
//            return height;
//        }
//    }
//}