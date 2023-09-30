using KSS.Core;
using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using KSS.Core.ResourceFlowSystem;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    /// <summary>
    /// An object that connects two containers and calculates the resource flow between them.
    /// </summary>
    public class FBulkConnection : MonoBehaviour, IPersistent
    {
        /// <summary>
        /// Represents an inlet or outlet.
        /// </summary>
        [Serializable]
        public class Port
        {
            // idk if SerializeField works with interfaces. probably better to save over in json.
            [SerializeField]
            IResourceConsumer _objC;
            [SerializeField]
            IResourceProducer _objP;

            /// <summary>
            /// The position of the connection, in local space of the connected object.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The orientation of the connection, in local space of the connected object.
            /// </summary>
            /// <remarks>
            /// Nominally faces "inwards", into the container/tank.
            /// </remarks>
            public Vector3 Forward;

            public void ConnectTo<T>( T obj ) where T : IResourceProducer, IResourceConsumer
            {
                // This is kind of a hack because the other two methods are ambiguous when assigning something that's both a producer and a consumer.
                this._objC = obj;
                this._objP = obj;
            }

            public void ConnectTo( IResourceConsumer consumer )
            {
                this._objC = consumer;
                this._objP = consumer as IResourceProducer;
            }

            public void ConnectTo( IResourceProducer producer )
            {
                this._objC = producer as IResourceConsumer;
                this._objP = producer;
            }

            public (IResourceConsumer, IResourceProducer) ConnectedTo()
            {
                return (this._objC, this._objP);
            }
        }

        /// <summary>
        /// The container at one end of the connection.
        /// </summary>
        [field: SerializeField]
        public Port End1 { get; private set; } = new Port();

        /// <summary>
        /// The container at the other end of the connection.
        /// </summary>
        [field: SerializeField]
        public Port End2 { get; private set; } = new Port();

        /// <summary>
        /// Cross-sectional area of the connection (pipe) in [m^2].
        /// </summary>
        [field: SerializeField]
        public float CrossSectionArea { get; set; }

        // Flow from the last frame - positive => towards End2, negative => towards End1.
        // Since `flow out = flow in`, the magnitude of flow at both tanks due to this connection must be equal.

        // across the pipe (pipe is 0-width, so inlet and outlet are the same thing).
        SubstanceStateCollection _cacheFlow = null;
        int _cacheInlet;
        int _cacheOutlet;

        /// <summary>
        /// Returns the end by index. Useful when selecting the index by some criterion.
        /// </summary>
        public Port GetEnd( int index )
        {
            switch( index )
            {
                case 0: return End1;
                case 1: return End2;
            }
            throw new ArgumentOutOfRangeException( $"Index must be 0 or 1" );
        }

        private void TryResetFlowAcrossConnection()
        {
            if( _cacheFlow == null )
                return; // do not throw here without guarding for the first update, which will try reset a flow that's not flowing, before setting it to something.
                        // Possibly fixable by initializing cacheflow to empty instead of null, but then we also need to make sure that it's not left unset by some return-guard.

            var oldFlow = _cacheFlow;

            if( !SubstanceStateCollection.IsNullOrEmpty( oldFlow ) )
            {
                (_, var inletP) = GetEnd( _cacheInlet ).ConnectedTo();
                (var outletC, _) = GetEnd( _cacheOutlet ).ConnectedTo();
                inletP.Outflow.Add( oldFlow, -1.0f );
                outletC.Inflow.Add( oldFlow, -1.0f );
            }

            _cacheFlow = null;
        }

        private void SetFlowAcrossConnection( SubstanceStateCollection newFlow, int inlet, int outlet )
        {
            if( _cacheFlow != null )
                throw new InvalidOperationException( $"Can't set flow that's not been reset" );

            _cacheFlow = newFlow;
            _cacheInlet = inlet;
            _cacheOutlet = outlet;

            if( !SubstanceStateCollection.IsNullOrEmpty( newFlow ) )
            {
                (_, var inletP) = GetEnd( inlet ).ConnectedTo();
                (var outletC, _) = GetEnd( outlet ).ConnectedTo();
                inletP.Outflow.Add( newFlow );
                outletC.Inflow.Add( newFlow );
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

        public void FixedUpdate_Flow( Vector3 fluidAccelerationSceneSpace )
        {
            // this is important. remove the current flow before calculating the new one, like done here, because restrictions on flow may apply, and they don't know that this flow is already flowing.
            TryResetFlowAcrossConnection();

            const float MIN_AREA = 1e-6f;
            const float MIN_ACCELERATION = 0.01f;
            const float MIN_PRESSURE_DIFFERENCE = 0.001f;

            if( CrossSectionArea <= MIN_AREA )
            {
                return;
            }
            if( fluidAccelerationSceneSpace.magnitude < MIN_ACCELERATION )
            {
                return;
            }

#warning  TODO - for actual flow, we need pressure at the bottom for both, whichever is geometrically lower.
            // Imagine that tanks are connected.
            // The tanks should equalize in such a way that the pressure in both should be the same when both samples' position along the acceleration direction is the same.
            // - only valid if the pressure at both ends is greater than 0.

            // we can achieve that by increasing the height of the fluid column in the inlet tank so that it matches the outlet when projected along the acceleration direction.

            FluidState[] endSamples = new FluidState[2];
            IResourceConsumer[] endC = new IResourceConsumer[2];
            IResourceProducer[] endP = new IResourceProducer[2];

            (endC[0], endP[0]) = End1.ConnectedTo();
            (endC[1], endP[1]) = End2.ConnectedTo();

            if( endC[0] != null )
            {
                endSamples[0] = endC[0].Sample( End1.Position, endC[0].transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }
            else if( endP[0] != null )
            {
                endSamples[0] = endP[0].Sample( End1.Position, endP[0].transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }

            if( endC[1] != null )
            {
                endSamples[1] = endC[1].Sample( End2.Position, endC[1].transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }
            else if( endP[1] != null )
            {
                endSamples[1] = endP[1].Sample( End2.Position, endP[1].transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea );
            }

            float signedRelativePressure_Pa = endSamples[0].Pressure - endSamples[1].Pressure;

            if( Mathf.Abs( signedRelativePressure_Pa ) < MIN_PRESSURE_DIFFERENCE )
            {
                return;
            }

            (int inlet, int outlet) = GetInletAndOutletIndices( signedRelativePressure_Pa );

            Port inletEnd = this.GetEnd( inlet );

            IResourceProducer inletProducer = endP[inlet]; // inlet must have a producer, to produce the flow.
            IResourceConsumer outletConsumer = endC[outlet]; // outlet must have a consumer, to consume the flow.
            if( inletProducer == null || outletConsumer == null ) // fluid can't flow.
            {
                return;
            }
#warning TODO - fast rotations/collisions destabilize and break the flow, same mode as when the flow wasn't cleared before.
            (SubstanceStateCollection flow, _) = inletProducer.SampleFlow( inletEnd.Position, inletProducer.transform.InverseTransformVector( fluidAccelerationSceneSpace ), CrossSectionArea, Time.fixedDeltaTime, endSamples[outlet] );

            outletConsumer.ClampIn( flow, Time.fixedDeltaTime );

            SetFlowAcrossConnection( flow, inlet, outlet );
        }

        void FixedUpdate()
        {
            if( End1 == null || End2 == null )
            {
                throw new InvalidOperationException( $"Both ends must exist" );
            }

            Vessel vessel = this.transform.GetVessel();
            Vector3Dbl airfAcceleration = GravityUtils.GetGravityAcceleration( vessel.AIRFPosition );
            Vector3 sceneAcceleration = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformVector( (Vector3)airfAcceleration );
            Vector3 vesselAcceleration = vessel.PhysicsObject.Acceleration;

            // acceleration due to external forces (gravity) minus the acceleration of the vessel.
            sceneAcceleration -= vesselAcceleration;
#warning TODO - Each part should have its own acceleration (which includes centrifugal acceleration due to spinning vessel), that way if the vessel is spinning, it'll work correctly.

            FixedUpdate_Flow( sceneAcceleration );
        }

        public void SetData( ILoader l, SerializedData data )
        {
            throw new NotImplementedException();
        }

        public SerializedData GetData( ISaver s )
        {
            throw new NotImplementedException();
        }
    }
}