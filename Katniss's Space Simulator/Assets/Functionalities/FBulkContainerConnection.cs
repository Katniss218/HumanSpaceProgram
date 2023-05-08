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
    public class FBulkContainerConnection : Functionality
    {
        /// <summary>
        /// The container at one end of the connection.
        /// </summary>
        [field: SerializeField]
        public FBulkContainer End1 { get; set; }

        /// <summary>
        /// The container at the other end of the connection.
        /// </summary>
        [field: SerializeField]
        public FBulkContainer End2 { get; set; }

        /// <summary>
        /// Cross-sectional area of the connection (pipe) in [m^2].
        /// </summary>
        [field: SerializeField]
        public float CrossSectionArea { get; set; }

        // Flow from the last frame - positive => towards End2, negative => towards End1.
        // Since `flow out = flow in`, the magnitude of flow at both tanks due to this connection must be equal.

        float _currentFlow;
        float _currentVelocity;

        void SetFlow( float flow )
        {
            // Remove the previous flow.
            End1.TotalFlow += _currentFlow; // remove outflow (add the partial inflow from the other tank).
            End2.TotalFlow -= _currentFlow; // remove inflow.

            // Add the new flow.
            End1.TotalFlow -= flow;
            End2.TotalFlow += flow;

            // Cache.
            _currentFlow = flow;
        }

        public float GetMaximumFlowRate( float velocity )
        {
            // Area in [m^2] * velocity in [m/s] = volumetric flow rate in [m^3/s]
            return CrossSectionArea * velocity;
        }

        public float CalculateFlowRate( Vector3 externalAcceleration )
        {
            Vector3 direction = (End2.VolumeTransform.position - End1.VolumeTransform.position).normalized;

            // TODO - inlet direction based on inlet orientation.

#warning TODO - make it so acceleration accumulates velocity, as long as the fluid can flow. Assume some sort of drag in the fluid too.

            // Positive flows towards End2, negative flows towards End1, zero doesn't flow.
            float flowAcceleration = Vector3.Dot( externalAcceleration, direction );
            float flowAccelerationMagnitude = Mathf.Abs( flowAcceleration );

            if( flowAccelerationMagnitude < 0.001f ) // skip calculating in freefall. Later, we can do rebound and stuff.
            {
                return 0.0f;
            }

            // flow rate actually depends on velocity of the fluid.
            float flowRate = GetMaximumFlowRate( flowAcceleration );

            // need to clamp the flow rate based on the inlet conditions.
            // inlet depends on the sign of the acceleration.
            FBulkContainer inlet = flowAcceleration < 0 ? End2 : End1;
            FBulkContainer outlet = flowAcceleration < 0 ? End1 : End2;

            // Clamp based on how much can flow out of the inlet, and into the outlet.
            // Division by delta-time might be weird, but it makes sense, I promise.
            float inletAvailableFlowrate = (inlet.Volume / Time.fixedDeltaTime) * flowAccelerationMagnitude > flowRate ? flowRate : inlet.Volume / Time.fixedDeltaTime; // TODO - fluid velocity.
            float outletAvailableFlowrate = ((outlet.MaxVolume - outlet.Volume) / Time.fixedDeltaTime) * flowAccelerationMagnitude > flowRate ? flowRate : (outlet.MaxVolume - outlet.Volume) / Time.fixedDeltaTime; // TODO - fluid velocity.

            float actualFlowRate = Mathf.Min( flowRate, inletAvailableFlowrate, outletAvailableFlowrate );

            return actualFlowRate;
        }

        void FixedUpdate()
        {
            // reset the tanks' inflow from last frame (subtract what was set previously, so we don't have to zero it out, and update anything in any particular order).
            // as long as the order of scripts (container and connection) is consistent throughout the frames.


            // since flow sign is `towards end2`, we should calculate the dot between acceleration and direction from end1 to end2.

            Vector3 fluidAcceleration = Vector3.down; // acceleration due to external forces (gravity) minus the acceleration of the vessel.

            float newFlow = CalculateFlowRate( fluidAcceleration );

            SetFlow( newFlow );
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