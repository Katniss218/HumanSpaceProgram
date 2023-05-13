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

namespace KatnisssSpaceSimulator.Functionalities
{
    public class FBulkContainerConnection : Functionality
    {
        [Serializable]
        public class End
        {
            public FBulkContainer Container;
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
        FBulkContainer.BulkContents _flow = FBulkContainer.BulkContents.Empty;
        [field: SerializeField]
        float _velocity = 0;

        void UpdateContainers( FBulkContainer.BulkContents newFlow, float newVelocity )
        {
            // Remove the previous flow.
            End1.Container.TotalInflow.Add( _flow ); // remove outflow (add the partial inflow from the other tank).
            End1.Container.TotalVelocity += End1.Forward * _velocity;
            End2.Container.TotalInflow.Add( _flow, -1.0f ); // remove inflow.
            End2.Container.TotalVelocity -= End2.Forward * _velocity;

            // Add the new flow.
            End1.Container.TotalInflow.Add( newFlow, -1.0f );
            End1.Container.TotalVelocity -= End1.Forward * newVelocity;
            End2.Container.TotalInflow.Add( newFlow );
            End2.Container.TotalVelocity += End2.Forward * newVelocity;

            // Cache.
            _flow = newFlow;
            _velocity = newVelocity;
        }

        public float GetVolumetricFlowrate( float velocity )
        {
            // Area in [m^2] * velocity in [m/s] = volumetric flow rate in [m^3/s]
            return CrossSectionArea * velocity;
        }

        // in [kg/m^3]
        const float fluidDensity = 1100.0f;

        public (float end1Pressure, float end2Pressure) GetPressures( Vector3 fluidAccelerationWorldSpace )
        {
            return (1, 1);
            // inlets on the surface of unit sphere in [m].
           /* Vector3 end1PosUnit = (this.transform.TransformPoint( End1.Position ) - End1.Container.VolumeTransform.position).normalized;
            Vector3 end2PosUnit = (this.transform.TransformPoint( End2.Position ) - End2.Container.VolumeTransform.position).normalized;

            // fluidAccelerationWorldSpace is the orientation of the fluid column.

            float end1Height = SolveHeightOfTruncatedSphere( End1.Container.Contents.Volume / End1.Container.MaxVolume );
            float end2Height = SolveHeightOfTruncatedSphere( End2.Container.Contents.Volume / End2.Container.MaxVolume );

#warning TODO - Use the position of each connection along the acceleration axis to find height. Currently assumes feeding bottom-to-bottom.

#warning TODO - if the tanks are at different heights, and connected at the bottoms (relative to acceleration), the level should ultimately be equal for both.
            // doesn't really work with the "portal" connection model.
            // this can be solved by extending the height along the acceleration axis until it reaches the end with the smaller position.

#warning TODO - multiply height by tank dimensions and use its magnitude to get more accurate pressure.

            float end1Pressure = fluidAccelerationWorldSpace.magnitude * fluidDensity * end1Height;
            float end2Pressure = fluidAccelerationWorldSpace.magnitude * fluidDensity * end2Height;

            // in [Pa]
            return (end1Pressure, end2Pressure);*/
        }

        public (FBulkContainer.BulkContents flowrate, float velocity) CalculateFlowRate( Vector3 fluidAccelerationRelativeToContainer, float deltaTime )
        {
            if( CrossSectionArea <= 1e-6f )
            {
                return (FBulkContainer.BulkContents.Empty, 0.0f);
            }
            if( fluidAccelerationRelativeToContainer.magnitude < 0.001f )
            {
                return (FBulkContainer.BulkContents.Empty, 0.0f);
            }

            // In [Pa]
            (float end1Pressure, float end2Pressure) = GetPressures( fluidAccelerationRelativeToContainer );

            // In [Pa] ([N/m^2]). Positive flows towards End2, negative flows towards End1, zero doesn't flow.
            float relativePressure = end1Pressure - end2Pressure;
            float relativePressureMagnitude = Mathf.Abs( relativePressure );

            float newSignedVelocity = Mathf.Sign( relativePressure ) * Mathf.Sqrt( 2 * relativePressureMagnitude / fluidDensity ); // _velocity + flowAcceleration;
            float newVelocityMagnitude = Mathf.Abs( newSignedVelocity );

            if( newVelocityMagnitude < 0.001f ) // skip calculating in freefall. Later, we can do rebound and stuff.
            {
                return (FBulkContainer.BulkContents.Empty, 0.0f);
            }

            End inlet = relativePressure < 0 ? End2 : End1;
            End outlet = relativePressure < 0 ? End1 : End2;

            newSignedVelocity *= 0.95f; // friction.

            // Flow rate actually depends on velocity of the fluid.
            float newMaximumFlowrate = Mathf.Abs( GetVolumetricFlowrate( newSignedVelocity ) );

            // Clamp based on how much can flow out of the inlet, and into the outlet.
            // Division by delta-time is because the removed/added volume will be multiplied by it later.
            float inletVolumeDt = inlet.Container.Contents.Volume / deltaTime;
            float outletRemainingVolumeDt = (outlet.Container.MaxVolume - outlet.Container.Contents.Volume) / deltaTime;

            // Need to clamp the flow rate based on the inlet and outlet conditions.
            float inletAvailableFlowrate = (inletVolumeDt > newMaximumFlowrate) // unsigned
                ? newMaximumFlowrate
                : inletVolumeDt;

#warning TODO - needs to be split evenly across all of the outlets flowing into the given tank, to stop oscillations. Also, conserve the volumes.
            // also, even with one outlet and constant deltatime, this somehow overshoots the maximum volume.

            float outletAvailableFlowrate = (outletRemainingVolumeDt > newMaximumFlowrate) // unsigned
                ? newMaximumFlowrate
                : outletRemainingVolumeDt;

            inletAvailableFlowrate = Mathf.Max( 0.0f, inletAvailableFlowrate );
            outletAvailableFlowrate = Mathf.Max( 0.0f, outletAvailableFlowrate );

            float newFlowrate = Mathf.Min( newMaximumFlowrate, inletAvailableFlowrate, outletAvailableFlowrate );

            float newSignedFlowrate = Mathf.Sign( relativePressure ) * newFlowrate;

            // Use the clamped flow rate to calculate clamped velocity, to keep it in sync.
            newSignedVelocity = newSignedFlowrate / CrossSectionArea;

            return (FBulkContainer.BulkContents.Empty, newSignedVelocity);
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

            (FBulkContainer.BulkContents newFlowrate, float newVelocity) = CalculateFlowRate( sceneAcceleration, Time.fixedDeltaTime );

            UpdateContainers( newFlowrate, newVelocity );
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