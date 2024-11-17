using HSP.Trajectories;
using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.ResourceFlow;
using HSP.Vessels;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    [Serializable]
    public class FRocketEngine : MonoBehaviour, IResourceConsumer
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

        [NamedControl( "Throttle", "Connect this to the controller's throttle output." )]
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
                vessel.PhysicsTransform.AddForceAtPosition( this.ThrustTransform.forward * this.Thrust, this.ThrustTransform.position );
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


        [MapsInheritingFrom( typeof( FRocketEngine ) )]
        public static SerializationMapping FRocketEngineMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngine>()
            {
                ("max_thrust", new Member<FRocketEngine, float>( o => o.MaxThrust )),
                ("set_throttle", new Member<FRocketEngine, ControlleeInput<float>>( o => o.SetThrottle )),
                ("isp", new Member<FRocketEngine, float>( o => o.Isp )),
                ("throttle", new Member<FRocketEngine, float>( o => o.Throttle )),
                ("thrust_transform", new Member<FRocketEngine, Transform>( ObjectContext.Ref, o => o.ThrustTransform ))
            };
        }
    }
}