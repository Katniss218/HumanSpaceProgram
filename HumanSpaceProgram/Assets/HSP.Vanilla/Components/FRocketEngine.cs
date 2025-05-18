using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.ResourceFlow;
using HSP.Vessels;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public interface IPropulsion
    {
        public Transform ThrustTransform { get; }
        float Thrust { get; }
        float MaxThrust { get; }

        event Action OnAfterIgnite;
        event Action OnAfterShutdown;
    }

    [Serializable]
    public class FRocketEngine : MonoBehaviour, IPropulsion, IResourceConsumer
    {
        const float g = 9.80665f;

        float _thrust;
        /// <summary>
        /// The thrust that the engine is currently producing, in [N].
        /// </summary>
        public float Thrust
        {

            get => _thrust;
            set
            {
                float oldThrust = _thrust;

                _thrust = value;

                if( oldThrust != value )
                    OnAfterThrustChanged?.Invoke();
            }
        }

        /// <summary>
        /// The maximum thrust that the engine can produce, in [N].
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

        [SerializeField]
        float _throttle;
        /// <summary>
        /// The current throttle level, in [0..1].
        /// </summary>
        public float Throttle
        {
            get => _throttle;
            set
            {
                float oldThrottle = _throttle;

                _throttle = value;

                if( oldThrottle == 0 && value > 0 )
                    OnAfterIgnite?.Invoke();
                else if( oldThrottle > 0 && value == 0 )
                    OnAfterShutdown?.Invoke();

                if( oldThrottle != value )
                    OnAfterThrottleChanged?.Invoke();
            }
        }

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

        public event Action OnAfterIgnite;
        public event Action OnAfterShutdown;
        public event Action OnAfterThrottleChanged;
        public event Action OnAfterThrustChanged;

        private float GetThrust( float massFlow )
        {
            return (this.Isp * g) * massFlow * Throttle;
        }

        void Awake()
        {
            SetThrottle ??= new ControlleeInput<float>( SetThrottleListener );
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
                .WithMember( "max_thrust", o => o.MaxThrust )
                .WithMember( "set_throttle", o => o.SetThrottle )
                .WithMember( "isp", o => o.Isp )
                .WithMember( "throttle", o => o.Throttle )
                .WithMember( "thrust_transform", ObjectContext.Ref, o => o.ThrustTransform );
        }
    }
}