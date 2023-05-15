using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.Physics;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities.ResourceFlowSystem
{
    public class FBulkContainerConnection : Functionality
    {
        [Serializable]
        public class End
        {
            public FBulkContainer_Sphere Container;
            public Vector3 Position;
            public Vector3 Forward; // nominally faces into the tank.
        }

        /// <summary>
        /// The container at one end of the connection.
        /// </summary>
        [field: SerializeField]
        public End End1 { get; set; } = new End();

        /// <summary>
        /// The container at the other end of the connection.
        /// </summary>
        [field: SerializeField]
        public End End2 { get; set; } = new End();

        /// <summary>
        /// Cross-sectional area of the connection (pipe) in [m^2].
        /// </summary>
        [field: SerializeField]
        public float CrossSectionArea { get; set; }

        // Flow from the last frame - positive => towards End2, negative => towards End1.
        // Since `flow out = flow in`, the magnitude of flow at both tanks due to this connection must be equal.

        // across the pipe (pipe is 0-width, so inlet and outlet are the same thing).
        [field: SerializeField]
        SubstanceStateMultiple _flow = null;

        void UpdateContainers( SubstanceStateMultiple newFlow )
        {
            if( !SubstanceStateMultiple.IsNoFlow( _flow ) )
            {
                // Remove the previous flow.
                End1.Container.Inflow.Add( _flow ); // remove outflow (add the partial inflow from the other tank).
                End2.Container.Inflow.Add( _flow, -1.0f ); // remove inflow.
            }

            if( !SubstanceStateMultiple.IsNoFlow( newFlow ) )
            {
                // Add the new flow.
                End1.Container.Inflow.Add( newFlow, -1.0f );
                End2.Container.Inflow.Add( newFlow );
            }

            // Cache.
            _flow = newFlow;
        }

        public float GetVolumetricFlowrate( float velocity )
        {
            // Area in [m^2] * velocity in [m/s] = volumetric flow rate in [m^3/s]
            return CrossSectionArea * velocity;
        }

        public SubstanceStateMultiple CalculateFlow( Vector3 fluidAccelerationSceneSpace, float deltaTime )
        {
            if( CrossSectionArea <= 1e-6f )
            {
                return SubstanceStateMultiple.NoFlow;
            }
            if( fluidAccelerationSceneSpace.magnitude < 0.01f )
            {
                return SubstanceStateMultiple.NoFlow;
            }

#warning  TODO - for actual flow, we need pressure at the bottom for both, whichever is lower.
            // species are going to be the same, regardless. pressure is different though

            SubstanceStateMultiple[] endSamples = new SubstanceStateMultiple[2];

#warning TODO - doesn't work with multiple fluids.
            endSamples[0] = End1.Container.Sample( End1.Position, End1.Container.VolumeTransform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            endSamples[1] = End2.Container.Sample( End2.Position, End2.Container.VolumeTransform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );

            if( endSamples[0] == null && endSamples[1] == null )
            {
#warning TODO - in reality, model flow in-out of the atmosphere/vacuum.
                return SubstanceStateMultiple.NoFlow;
            }

            float[] endPressures = new float[2] { endSamples[0]?.FluidState.Pressure ?? 0.0f, endSamples[1]?.FluidState.Pressure ?? 0.0f };

            // In [Pa] ([N/m^2]). Positive flows towards End2, negative flows towards End1, zero doesn't flow.
            float relativePressure = endPressures[0] - endPressures[1];
            float relativePressureMagnitude = Mathf.Abs( relativePressure );

            if( relativePressureMagnitude < 0.001f )
            {
                return SubstanceStateMultiple.NoFlow;
            }

            int inlet = relativePressure < 0 ? 1 : 0;
            int outlet = relativePressure < 0 ? 0 : 1;

            // need average density (weighed for amount) of the fluid exiting the hole.
            float newSignedVelocity = Mathf.Sign( relativePressure ) * Mathf.Sqrt( 2 * relativePressureMagnitude / endSamples[inlet].GetAverageDensity() );
            // float newVelocityMagnitude = Mathf.Abs( newSignedVelocity );

            End[] ends = new End[2] { End1, End2 };

            // newSignedVelocity *= 0.95f; // friction.

            // v2 = sqrt((2/ρ)(P1 - P2 + ρgh1))

            // Flow rate actually depends on velocity of the fluid.
            float maximumVolumetricFlowrate = Mathf.Abs( GetVolumetricFlowrate( newSignedVelocity ) );

            // Clamp based on how much can flow out of the inlet, and into the outlet.
            // Division by delta-time is because the removed/added volume will be multiplied by it later.
            float inletVolumeDt = ends[inlet].Container.Contents.GetVolume() / deltaTime;
            float outletRemainingVolumeDt = (ends[outlet].Container.MaxVolume - ends[outlet].Container.Contents.GetVolume()) / deltaTime;

            // Need to clamp the flow rate based on the inlet and outlet conditions.
            float inletAvailVolumetricFlowrate = (inletVolumeDt > maximumVolumetricFlowrate) // unsigned
                ? maximumVolumetricFlowrate
                : inletVolumeDt;

            float outletAvailVolumetricFlowrate = (outletRemainingVolumeDt > maximumVolumetricFlowrate) // unsigned
                ? maximumVolumetricFlowrate
                : outletRemainingVolumeDt;

            inletAvailVolumetricFlowrate = Mathf.Max( 0.0f, inletAvailVolumetricFlowrate );
            outletAvailVolumetricFlowrate = Mathf.Max( 0.0f, outletAvailVolumetricFlowrate );

            float totalVolumetricFlowrate = Mathf.Min( maximumVolumetricFlowrate, inletAvailVolumetricFlowrate, outletAvailVolumetricFlowrate );

            float flowSign = Mathf.Sign( relativePressure );
            //  float newSignedFlowrate = Mathf.Sign( relativePressure ) * newFlowrate;

            // Use the clamped flow rate to calculate clamped velocity, to keep it in sync.
            //  newSignedVelocity = newSignedFlowrate / CrossSectionArea;

            float totalMassAmount = endSamples[inlet].GetMass();

            // convert volumetric to mass flowrate.
            List<SubstanceState> flowrate = new List<SubstanceState>();
            foreach( var sbs in endSamples[inlet].GetSubstances() )
            {
                float volumetricFlowofFluidI = (sbs.MassAmount / totalMassAmount) * flowSign * totalVolumetricFlowrate;

                flowrate.Add( new SubstanceState( volumetricFlowofFluidI * sbs.Data.Density, sbs.Data ) );
            }

            return new SubstanceStateMultiple(
                new FluidState() { Pressure = 0.0f, Temperature = 275.15f, Velocity = Vector3.zero },
                flowrate );
        }

        void FixedUpdate()
        {
            Contract.Assert( End1 != null, "Both ends must be set." );
            Contract.Assert( End2 != null, "Both ends must be set." );
            // reset the tanks' inflow from last frame (subtract what was set previously, so we don't have to zero it out, and update anything in any particular order).
            // as long as the order of scripts (container and connection) is consistent throughout the frames.


            // since flow sign is `towards end2`, we should calculate the dot between acceleration and direction from end1 to end2.

            const double PlaceholderMass = 8;

            Part part = this.GetComponent<Part>();
            Vector3Dbl airfGravityForce = PhysicsUtils.GetGravityForce( PlaceholderMass, part.Vessel.AIRFPosition ); // Move airfposition to PhysicsObject maybe?
            Vector3Dbl airfAcceleration = airfGravityForce / PlaceholderMass;
            Vector3 sceneAcceleration = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformDirection( (Vector3)airfAcceleration );
            Vector3 vesselAcceleration = part.Vessel.PhysicsObject.Acceleration;
            // acceleration due to external forces (gravity) minus the acceleration of the vessel.
            sceneAcceleration -= vesselAcceleration;
#warning TODO - add angular acceleration to the mix.

            SubstanceStateMultiple flow = CalculateFlow( sceneAcceleration, Time.fixedDeltaTime );

            UpdateContainers( flow );
        }

        public override void Load( JToken data )
        {
            throw new NotImplementedException();
        }

        public override JToken Save()
        {
            throw new NotImplementedException();
        }
    }
}