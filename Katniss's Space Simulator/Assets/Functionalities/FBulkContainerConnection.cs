using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.Physics;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
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
    public class FBulkContainerConnection : Functionality
    {
        [Serializable]
        public class End
        {
            public object O;

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

        public End GetEnd( int index )
        {
            switch( index )
            {
                case 0: return End1;
                case 1: return End2;
            }
            throw new ArgumentOutOfRangeException( $"Index must be 0 or 1" );
        }

        /// <summary>
        /// Cross-sectional area of the connection (pipe) in [m^2].
        /// </summary>
        [field: SerializeField]
        public float CrossSectionArea { get; set; }

        // Flow from the last frame - positive => towards End2, negative => towards End1.
        // Since `flow out = flow in`, the magnitude of flow at both tanks due to this connection must be equal.

        // across the pipe (pipe is 0-width, so inlet and outlet are the same thing).
        [field: SerializeField]
        SubstanceStateCollection _flow = null;

        int _cacheInlet;
        int _cacheOutlet;

        private void UpdateContainers( SubstanceStateCollection newFlow, int inlet, int outlet )
        {
            var oldFlow = _flow;

            if( !SubstanceStateCollection.IsNullOrEmpty( oldFlow ) )
            {
                ((IResourceProducer)GetEnd( _cacheInlet ).O).Outflow.Add( oldFlow, -1.0f );
                ((IResourceConsumer)GetEnd( _cacheOutlet ).O).Inflow.Add( oldFlow, -1.0f );
            }

            // Cache.
            _flow = newFlow;
            _cacheInlet = inlet;
            _cacheOutlet = outlet;

            if( !SubstanceStateCollection.IsNullOrEmpty( newFlow ) )
            {
                ((IResourceProducer)GetEnd( inlet ).O).Outflow.Add( newFlow );
                ((IResourceConsumer)GetEnd( outlet ).O).Inflow.Add( newFlow );
            }
        }

        public static (int inlet, int outlet) GetInletAndOutletIndices( float sign )
        {
            int inlet = sign < 0 ? 1 : 0;
            int outlet = sign < 0 ? 0 : 1;
            return (inlet, outlet);
        }

        public static float GetVolumetricFlowrate( float area, float velocity )
        {
            // Area in [m^2] * velocity in [m/s] = volumetric flow rate in [m^3/s]
            return area * velocity;
        }

        public void FixedUpdate_Flow( Vector3 fluidAccelerationSceneSpace, float deltaTime )
        {
            const float MIN_AREA = 1e-6f;
            const float MIN_ACCELERATION = 0.01f;
            const float MIN_PRESSURE_DIFFERENCE = 0.001f;

            if( CrossSectionArea <= MIN_AREA )
            {
                UpdateContainers( SubstanceStateCollection.Empty, _cacheInlet, _cacheOutlet );
                return;
            }
            if( fluidAccelerationSceneSpace.magnitude < MIN_ACCELERATION )
            {
                UpdateContainers( SubstanceStateCollection.Empty, _cacheInlet, _cacheOutlet );
                return;
            }

#warning  TODO - for actual flow, we need pressure at the bottom for both, whichever is lower.
            // Imagine that tanks are connected.
            // The tanks should equalize in such a way that the pressure in both should be the same when both samples' position along the acceleration direction is the same.
            // - only valid if the pressure at both ends is greater than 0.

            // we can achieve that by increasing the height of the fluid column in the inlet tank so that it matches the outlet when projected along the acceleration direction.

            FluidState[] endSamples = new FluidState[2];

            if( End1.O is IResourceConsumer c1 )
            {
                endSamples[0] = c1.Sample( End1.Position, c1.transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }
            else if( End1.O is IResourceProducer p1 )
            {
                endSamples[0] = p1.Sample( End1.Position, p1.transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }
            if( End2.O is IResourceConsumer c2 )
            {
                endSamples[1] = c2.Sample( End2.Position, c2.transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }
            else if( End2.O is IResourceProducer p2 )
            {
                endSamples[1] = p2.Sample( End2.Position, p2.transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }

            // In [Pa] ([N/m^2]). Positive flows towards End2, negative flows towards End1, zero doesn't flow.
            float relativePressure = endSamples[0].Pressure - endSamples[1].Pressure;
            float relativePressureMagnitude = Mathf.Abs( relativePressure );

            if( relativePressureMagnitude < MIN_PRESSURE_DIFFERENCE )
            {
                UpdateContainers( SubstanceStateCollection.Empty, _cacheInlet, _cacheOutlet );
                return;
            }

            (int inlet, int outlet) = GetInletAndOutletIndices( relativePressure );

            End inletEnd = this.GetEnd( inlet );
            End outletEnd = this.GetEnd( outlet );

            IResourceProducer inletProducer = inletEnd.O as IResourceProducer; // inlet must have a producer, to produce the flow.
            IResourceConsumer outletConsumer = outletEnd.O as IResourceConsumer; // outlet must have a consumer, to consume the flow.
            if( inletProducer == null || outletConsumer == null ) // fluid can't flow.
            {
                UpdateContainers( SubstanceStateCollection.Empty, _cacheInlet, _cacheOutlet );
                return;
            }

            (SubstanceStateCollection flow, _) = inletProducer.SampleFlow( inletEnd.Position, inletProducer.transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea, endSamples[outlet] );

            outletConsumer.ClampIn( flow );

            UpdateContainers( flow, inlet, outlet );
        }

        void FixedUpdate()
        {
            if( End1 == null || End2 == null )
            {
                throw new InvalidOperationException( $"Both ends must exist" );
            }

            const double PlaceholderMass = 8;

            Part part = this.GetComponent<Part>();
            Vector3Dbl airfGravityForce = PhysicsUtils.GetGravityForce( PlaceholderMass, part.Vessel.AIRFPosition ); // Move airfposition to PhysicsObject maybe?
            Vector3Dbl airfAcceleration = airfGravityForce / PlaceholderMass;
            Vector3 sceneAcceleration = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformVector( (Vector3)airfAcceleration );
            Vector3 vesselAcceleration = part.Vessel.PhysicsObject.Acceleration;
            // acceleration due to external forces (gravity) minus the acceleration of the vessel.
            sceneAcceleration -= vesselAcceleration;
#warning TODO - add angular acceleration to the mix.

            FixedUpdate_Flow( sceneAcceleration, Time.fixedDeltaTime );

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