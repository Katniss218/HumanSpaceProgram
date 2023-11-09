using KSS.Core;
using KSS.Core.ResourceFlowSystem;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    [Serializable]
    public class FRocketEngine : MonoBehaviour, IResourceConsumer, IPersistent
    {
        const float g = 9.80665f;

        float _currentThrust;

        /// <summary>
        /// The maximum thrust of the engine, in [N].
        /// </summary>
        [field: SerializeField]
        public float MaxThrust { get; set; } = 10000f;

        /// <summary>
        /// Specific impulse of the engine, in [s].
        /// </summary>
        [field: SerializeField]
        public float Isp { get; set; } = 100f; // TODO - curve based on atmospheric pressure.

        /// <summary>
        /// Maximum mass flow (at max thrust), in [kg/s]
        /// </summary>
        public float MaxMassFlow => MaxThrust / (Isp * g);

        [field: SerializeField]
        public float Throttle { get; set; }

        /// <summary>
        /// Defines which way the engine thrusts (thrust is applied in its `forward` (Z+) direction).
        /// </summary>
        [field: SerializeField]
        public Transform ThrustTransform { get; set; }

        [field: SerializeField]
        public SubstanceStateCollection Inflow { get; private set; } = SubstanceStateCollection.Empty;

        /// <summary>
        /// Returns the actual thrust produced by the engine at this moment in time.
        /// </summary>
        public float GetThrust( float massFlow )
        {
            return (this.Isp * g) * massFlow * Throttle;
        }

        /*[ControlIn( "set.throttle", "Set Throttle" )]
        public void SetThrottle( float value )
        {
            this.Throttle = value;
        }*/

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.W ) )
            {
                Throttle = Throttle > 0.5f ? 0.0f : 1.0f;
            }
        }

        void FixedUpdate()
        {
            float thrust = GetThrust( Inflow.GetMass() );
            if( this.Throttle > 0.0f )
            {
                Vessel vessel = this.transform.GetVessel();
                if( vessel != null )
                {
                    vessel.PhysicsObject.AddForceAtPosition( this.ThrustTransform.forward * thrust, this.ThrustTransform.position );
                }
            }
            _currentThrust = thrust;
        }

        public void ClampIn( SubstanceStateCollection inflow, float dt )
        {
            FlowUtils.ClampMaxMassFlow( inflow, Inflow.GetMass(), MaxMassFlow * Throttle );
        }

        public FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea )
        {
            return FluidState.Vacuum; // temp, inlet condition (possible backflow, etc).
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "max_thrust", this.MaxThrust },
                { "isp", this.Isp },
                { "throttle", this.Throttle },
                { "thrust_transform", s.WriteObjectReference( this.ThrustTransform ) }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "max_thrust", out var maxThrust ) )
                this.MaxThrust = (float)maxThrust;
            if( data.TryGetValue( "isp", out var isp ) )
                this.Isp = (float)isp;
            if( data.TryGetValue( "throttle", out var throttle ) )
                this.Throttle = (float)throttle;
            if( data.TryGetValue( "thrust_transform", out var thrustTransform ) )
                this.ThrustTransform = (Transform)l.ReadObjectReference( thrustTransform );
        }
    }
}