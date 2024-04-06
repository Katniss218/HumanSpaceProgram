using KSS.Core;
using KSS.Input;
using KSS.Core.ResourceFlowSystem;
using System;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization;
using KSS.Control;
using KSS.Control.Controls;

namespace KSS.Components
{
    [Serializable]
    public class FRocketEngine : MonoBehaviour, IResourceConsumer, IPersistsObjects, IPersistsData
    {
        const float g = 9.80665f;

        public float Thrust { get; private set; }

        /// <summary>
        /// The maximum thrust, in [N].
        /// </summary>
        [field: SerializeField]
        public float MaxThrust { get; set; } = 10000f;

        /// <summary>
        /// The specific impulse, in [s].
        /// </summary>
        [field: SerializeField]
        public float Isp { get; set; } = 100f; // TODO - curve based on atmospheric pressure.

        /// <summary>
        /// The maximum mass flow (when thrust = max thrust), in [kg/s].
        /// </summary>
        public float MaxMassFlow => MaxThrust / (Isp * g);

        /// <summary>
        /// The current throttle level, in [0..1].
        /// </summary>
        [field: SerializeField]
        public float Throttle { get; set; }

        [NamedControl( "Throttle", "Sets the current throttle level, [0..1]." )]
        public ControlleeInput<float> SetThrottle;
        private void SetThrottleListener( float value )
        {
            this.Throttle = value;
        }

        /// <summary>
        /// The thrust will be aplied in the Z+ (`forward`) direction of this transform.
        /// </summary>
        [field: SerializeField]
        public Transform ThrustTransform { get; set; }

        [field: SerializeField]
        public SubstanceStateCollection Inflow { get; private set; } = SubstanceStateCollection.Empty;

        private float GetThrust( float massFlow )
        {
            return (this.Isp * g) * massFlow * Throttle;
        }

        void Awake()
        {
            SetThrottle = new ControlleeInput<float>( SetThrottleListener );
        }

        void FixedUpdate()
        {
            this.Thrust = GetThrust( Inflow.GetMass() );

            if( this.Throttle <= 0.0f )
            {
                return;
            }

            Vessel vessel = this.transform.GetVessel();
            if( vessel != null )
            {
                vessel.PhysicsObject.AddForceAtPosition( this.ThrustTransform.forward * this.Thrust, this.ThrustTransform.position );
            }
        }

        public void ClampIn( SubstanceStateCollection inflow, float dt )
        {
            FlowUtils.ClampMaxMassFlow( inflow, Inflow.GetMass(), MaxMassFlow * Throttle );
        }

        public FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea )
        {
            return FluidState.Vacuum; // temp, inlet condition (possible backflow, etc).
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "set_throttle", s.GetID( SetThrottle ).GetData() },
            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "set_throttle", out var setThrottle ) )
            {
                SetThrottle = new( SetThrottleListener );
                l.SetObj( setThrottle.ToGuid(), SetThrottle );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            SetThrottle ??= new ControlleeInput<float>( SetThrottleListener );

            ret.AddAll( new SerializedObject()
            {
                { "max_thrust", this.MaxThrust },
                { "isp", this.Isp },
                { "throttle", this.Throttle },
                { "thrust_transform", s.WriteObjectReference( this.ThrustTransform ) },
                { "set_throttle", this.SetThrottle.GetData( s ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "max_thrust", out var maxThrust ) )
                this.MaxThrust = (float)maxThrust;
            if( data.TryGetValue( "isp", out var isp ) )
                this.Isp = (float)isp;
            if( data.TryGetValue( "throttle", out var throttle ) )
                this.Throttle = (float)throttle;
            if( data.TryGetValue( "thrust_transform", out var thrustTransform ) )
                this.ThrustTransform = (Transform)l.ReadObjectReference( thrustTransform );

            if( data.TryGetValue( "set_throttle", out var setThrottle ) )
                this.SetThrottle.SetData( setThrottle, l );
        }
    }
}