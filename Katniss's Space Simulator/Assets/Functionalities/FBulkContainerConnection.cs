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
        float _flowrate = 0;
        [field: SerializeField]
        float _velocity = 0;

        void UpdateContainers( float newFlowrate, float newVelocity )
        {
            // Remove the previous flow.
            End1.Container.TotalInflow += _flowrate; // remove outflow (add the partial inflow from the other tank).
            End1.Container.TotalVelocity += End1.Forward * _velocity;
            End2.Container.TotalInflow -= _flowrate; // remove inflow.
            End2.Container.TotalVelocity -= End2.Forward * _velocity;

            // Add the new flow.
            End1.Container.TotalInflow -= newFlowrate;
            End1.Container.TotalVelocity -= End1.Forward * newVelocity;
            End2.Container.TotalInflow += newFlowrate;
            End2.Container.TotalVelocity += End2.Forward * newVelocity;

            // Cache.
            _flowrate = newFlowrate;
            _velocity = newVelocity;
        }

        public float GetMaximumFlowRate( float velocity )
        {
            // Area in [m^2] * velocity in [m/s] = volumetric flow rate in [m^3/s]
            return CrossSectionArea * velocity;
        }

        public (float flowrate, float velocity) CalculateFlowRate( Vector3 fluidAccelerationRelativeToContainer, float deltaTime )
        {
            if( CrossSectionArea <= 1e-6f )
            {
                return (0.0f, 0.0f);
            }
            if( fluidAccelerationRelativeToContainer.magnitude < 0.001f )
            {
                return (0.0f, 0.0f);
            }

            Vector3 direction = (End2.Container.VolumeTransform.position - End1.Container.VolumeTransform.position).normalized;

            // TODO - inlet direction based on inlet orientation.

            // Positive flows towards End2, negative flows towards End1, zero doesn't flow.
            float flowAcceleration = Vector3.Dot( fluidAccelerationRelativeToContainer, direction );
            float flowAccelerationMagnitude = Mathf.Abs( flowAcceleration );

            float newSignedVelocity = _velocity + flowAcceleration;
            float newVelocityMagnitude = Mathf.Abs( newSignedVelocity );

            if( newVelocityMagnitude < 0.001f ) // skip calculating in freefall. Later, we can do rebound and stuff.
            {
                return (0.0f, 0.0f);
            }

            if( fluidAccelerationRelativeToContainer.magnitude < 1f )
            {

            }

            // Inlet depends on the sign of the acceleration.
            End inlet = flowAcceleration < 0 ? End2 : End1;
            End outlet = flowAcceleration < 0 ? End1 : End2;

            newSignedVelocity *= 0.95f; // friction.

            // Flow rate actually depends on velocity of the fluid.
            float newMaximumFlowrate = Mathf.Abs( GetMaximumFlowRate( newSignedVelocity ) );

            // Clamp based on how much can flow out of the inlet, and into the outlet.
            // Division by delta-time is because the removed/added volume will be multiplied by it later.
            float inletVolumeDt = inlet.Container.Volume / deltaTime;
            float outletRemainingVolumeDt = (outlet.Container.MaxVolume - outlet.Container.Volume) / deltaTime;

            // Need to clamp the flow rate based on the inlet and outlet conditions.
            float inletAvailableFlowrate = (inletVolumeDt * flowAccelerationMagnitude > newMaximumFlowrate) // unsigned
                ? newMaximumFlowrate
                : inletVolumeDt;

#warning TODO - needs to be split evenly across all of the outlets flowing into the given tank, to stop oscillations. Also, conserve the volumes.
            // also, even with one outlet and constant deltatime, this somehow overshoots the maximum volume.

            float outletAvailableFlowrate = (outletRemainingVolumeDt * flowAccelerationMagnitude > newMaximumFlowrate) // unsigned
                ? newMaximumFlowrate
                : outletRemainingVolumeDt;
#warning TODO - something (acceleration?) breaks when the vessel crashes into the ground.
            // also the inflow didn't match outflow, somehow.

            inletAvailableFlowrate = Mathf.Max( 0.0f, inletAvailableFlowrate );
            outletAvailableFlowrate = Mathf.Max( 0.0f, outletAvailableFlowrate );

            float newFlowrate = Mathf.Min( newMaximumFlowrate, inletAvailableFlowrate, outletAvailableFlowrate );

            float newSignedFlowrate = Mathf.Sign( flowAcceleration ) * newFlowrate;

            // Use the clamped flow rate to calculate clamped velocity, to keep it in sync.
            newSignedVelocity = newSignedFlowrate / CrossSectionArea;

            return (newSignedFlowrate, newSignedVelocity);
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
            sceneAcceleration -= vesselAcceleration; // acceleration = gravity minus whatever the container is doing.

            Debug.Log( vesselAcceleration );
            // acceleration due to external forces (gravity) minus the acceleration of the vessel.
            Vector3 fluidAcceleration = sceneAcceleration;

            (float newFlowrate, float newVelocity) = CalculateFlowRate( fluidAcceleration, Time.fixedDeltaTime );

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