using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.ResourceFlow;
using HSP.Time;
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
        event Action OnAfterThrustChanged;
    }

    [Serializable]
    public class FRocketEngine : MonoBehaviour, IPropulsion, IBuildsFlowNetwork
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

        /// <summary>
        /// The propellant mixture ratio required by the engine. The mass values represent the ratio.
        /// </summary>
        public IReadonlySubstanceStateCollection PropellantMixture { get; set; } = new SubstanceStateCollection();

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
        public ISubstanceStateCollection Inflow { get; private set; } = new SubstanceStateCollection();

        public ResourceInlet[] Inlets { get; set; }

        public event Action OnAfterIgnite;
        public event Action OnAfterShutdown;
        public event Action OnAfterThrottleChanged;
        public event Action OnAfterThrustChanged;

        private float GetThrust( float massFlow )
        {
            return (this.Isp * g) * massFlow;
        }

        void Awake()
        {
            SetThrottle ??= new ControlleeInput<float>( SetThrottleListener );
        }

        void FixedUpdate()
        {
            float massFlowRate = 0f;
            if( Inflow != null && TimeManager.FixedDeltaTime > 0 )
            {
                massFlowRate = (float)(Inflow.GetMass() / TimeManager.FixedDeltaTime);
            }

            // Throttle is already accounted for in the demand, which limits the inflow.
            this.Thrust = GetThrust( massFlowRate );

            // Safety check: if for any reason there's flow while throttled down, produce no thrust.
            if( this.Throttle <= 0.0f )
            {
                this.Thrust = 0f;
            }
            Debug.Log( Inflow.Count );
            if( this.Thrust > 0.0f )
            {
                Vessel vessel = this.transform.GetVessel();
                if( vessel != null )
                {
                    vessel.PhysicsTransform.AddForceAtPosition( this.ThrustTransform.forward * this.Thrust, this.ThrustTransform.position );
                }
            }
        }

        GenericConsumer _cachedConsumer;

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( Inlets == null || Inlets.Length == 0 )
            {
                return BuildFlowResult.Finished;
            }

            Vessel vessel = this.transform.GetVessel();
            if( vessel == null || vessel.RootPart == null )
            {
                return BuildFlowResult.Retry;
            }
            Transform reference = vessel.ReferenceTransform;

            _cachedConsumer ??= new GenericConsumer();
            c.TryAddFlowObj( this, _cachedConsumer );

            foreach( var inlet in Inlets )
            {
                Vector3 inletPosInReferenceSpace = reference.InverseTransformPoint( this.transform.TransformPoint( inlet.LocalPosition ) );
                FlowPipe.Port flowInlet = new FlowPipe.Port( _cachedConsumer, inletPosInReferenceSpace, inlet.NominalArea );
                c.TryAddFlowObj( inlet, flowInlet );
            }

            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
            // The engine is always considered valid once built.
            return true;
        }

        public void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            if( _cachedConsumer != null )
            {
                double massFlowDemand = Throttle * MaxMassFlow;
                if( massFlowDemand > 0 && PropellantMixture != null && !PropellantMixture.IsEmpty() )
                {
                    // Propellant density is approximated at STP for demand calculation.
                    double averageDensity = PropellantMixture.GetAverageDensity( 293.15, 101325 );
                    if( averageDensity > 0 )
                    {
                        double volumeFlowDemand = massFlowDemand / averageDensity;
                        _cachedConsumer.Demand = volumeFlowDemand;
                    }
                    else
                    {
                        _cachedConsumer.Demand = 0;
                    }
                }
                else
                {
                    _cachedConsumer.Demand = 0;
                }
            }
        }

        public void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // After the simulation step, pull the actual resource flow from the consumer proxy.
            if( _cachedConsumer != null && _cachedConsumer.Inflow != null )
            {
                this.Inflow = _cachedConsumer.Inflow.Clone();
            }
            else
            {
                this.Inflow?.Clear();
            }
        }


        [MapsInheritingFrom( typeof( FRocketEngine ) )]
        public static SerializationMapping FRocketEngineMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngine>()
                .WithMember( "max_thrust", o => o.MaxThrust )
                .WithMember( "set_throttle", o => o.SetThrottle )
                .WithMember( "isp", o => o.Isp )
                .WithMember( "throttle", o => o.Throttle )
                .WithMember( "thrust_transform", ObjectContext.Ref, o => o.ThrustTransform )
                .WithMember( "inlets", o => o.Inlets )
                .WithMember( "propellant_mixture", o => o.PropellantMixture );
        }
    }
}