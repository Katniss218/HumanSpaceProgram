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

        /// <summary>
        /// Calculates the height of a truncated unit sphere with the given volume as a [0..1] percentage of the unit sphere's volume.
        /// </summary>
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

        // in [kg/m^3]
        const float fluidDensity = 1100.0f;

        public (float end1Pressure, float end2Pressure) GetPressures( Vector3 fluidAccelerationWorldSpace )
        {
            // inlets on the surface of unit sphere in [m].
            Vector3 end1PosUnit = (this.transform.TransformPoint( End1.Position ) - End1.Container.VolumeTransform.position).normalized;
            Vector3 end2PosUnit = (this.transform.TransformPoint( End2.Position ) - End2.Container.VolumeTransform.position).normalized;

            // fluidAccelerationWorldSpace is the orientation of the fluid column.

            // find the pressure at each point.
            // find the fluid height, assuming the tank is a unit sphere and that the fluid is settled towards the acceleration direction.

            float end1Height = SolveHeightOfTruncatedSphere( End1.Container.Volume / End1.Container.MaxVolume );
            float end2Height = SolveHeightOfTruncatedSphere( End2.Container.Volume / End2.Container.MaxVolume );

#warning TODO - Use the position of each connection along the acceleration axis to find height. Currently assumes feeding bottom-to-bottom.

#warning TODO - if the tanks are at different heights, and connected at the bottoms (relative to acceleration), the level should ultimately be equal for both.

#warning TODO - multiply height by tank dimensions and use its magnitude to get more accurate pressure.

            float end1Pressure = fluidAccelerationWorldSpace.magnitude * fluidDensity * end1Height;
            float end2Pressure = fluidAccelerationWorldSpace.magnitude * fluidDensity * end2Height;

            // in [Pa]
            return (end1Pressure, end2Pressure);
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

            // In [Pa]
            (float end1Pressure, float end2Pressure) = GetPressures( fluidAccelerationRelativeToContainer );

            // In [Pa] ([N/m^2]). Positive flows towards End2, negative flows towards End1, zero doesn't flow.
            float relativePressure = end1Pressure - end2Pressure;
            float relativePressureMagnitude = Mathf.Abs( relativePressure );
            //float flowAcceleration = relativePressure / fluidDensity;
            //float flowAccelerationMagnitude = Mathf.Abs( flowAcceleration );

            float newSignedVelocity = Mathf.Sign(relativePressure) * Mathf.Sqrt( 2 * relativePressureMagnitude / fluidDensity ); // _velocity + flowAcceleration;
            float newVelocityMagnitude = Mathf.Abs( newSignedVelocity );

            if( newVelocityMagnitude < 0.001f ) // skip calculating in freefall. Later, we can do rebound and stuff.
            {
                return (0.0f, 0.0f);
            }

            // Inlet depends on the sign of the acceleration.
            End inlet = relativePressure < 0 ? End2 : End1;
            End outlet = relativePressure < 0 ? End1 : End2;

            newSignedVelocity *= 0.95f; // friction.

            // Flow rate actually depends on velocity of the fluid.
            float newMaximumFlowrate = Mathf.Abs( GetMaximumFlowRate( newSignedVelocity ) );

            // Clamp based on how much can flow out of the inlet, and into the outlet.
            // Division by delta-time is because the removed/added volume will be multiplied by it later.
            float inletVolumeDt = inlet.Container.Volume / deltaTime;
            float outletRemainingVolumeDt = (outlet.Container.MaxVolume - outlet.Container.Volume) / deltaTime;

            // Need to clamp the flow rate based on the inlet and outlet conditions.
            float inletAvailableFlowrate = (inletVolumeDt > newMaximumFlowrate) // unsigned
                ? newMaximumFlowrate
                : inletVolumeDt;

#warning TODO - needs to be split evenly across all of the outlets flowing into the given tank, to stop oscillations. Also, conserve the volumes.
            // also, even with one outlet and constant deltatime, this somehow overshoots the maximum volume.

            float outletAvailableFlowrate = (outletRemainingVolumeDt > newMaximumFlowrate) // unsigned
                ? newMaximumFlowrate
                : outletRemainingVolumeDt;
#warning TODO - something (acceleration?) breaks when the vessel crashes into the ground.
            // also the inflow didn't match outflow, somehow.

            inletAvailableFlowrate = Mathf.Max( 0.0f, inletAvailableFlowrate );
            outletAvailableFlowrate = Mathf.Max( 0.0f, outletAvailableFlowrate );

            float newFlowrate = Mathf.Min( newMaximumFlowrate, inletAvailableFlowrate, outletAvailableFlowrate );

            float newSignedFlowrate = Mathf.Sign( relativePressure ) * newFlowrate;

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