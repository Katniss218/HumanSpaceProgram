using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.ResourceFlow;
using HSP.Time;
using HSP.Vessels;
using System;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityPlus.Serialization;
using static HSP.Vanilla.Components.IPropulsion;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Defines the complete propellant, Isp, and ignition requirements for a rocket engine cycle.
    /// </summary>
    public class EnginePropellant
    {
        public ISubstanceStateCollection PropellantMixture { get; set; }

        /// <summary>
        /// The optional requirements for an ignitor fluid.
        /// </summary>
        public ISubstanceStateCollection Ignitor { get; set; }

        /// <summary>
        /// The nominal specific impulse (Isp) of the engine in seconds, at ideal mixture ratio.
        /// </summary>
        public float NominalIsp { get; set; }

        [MapsInheritingFrom( typeof( EnginePropellant ) )]
        public static SerializationMapping EnginePropellantMapping()
        {
            return new MemberwiseSerializationMapping<EnginePropellant>()
                .WithMember( "requirements", o => o.PropellantMixture )
                .WithMember( "ignitor", o => o.Ignitor )
                .WithMember( "nominal_isp", o => o.NominalIsp );
        }
    }

    public interface IPropulsion
    {
        public enum EngineState
        {
            Off,
            Igniting,
            Running,
            ShuttingDown
        }

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
        /// The maximum thrust that the engine can produce, in [N], under ideal conditions.
        /// </summary>
        [field: SerializeField]
        public float MaxThrust { get; set; } = 10000f;

        /// <summary>
        /// The data-driven definition of this engine's propellant requirements, Isp, and ignition behavior.
        /// </summary>
        [field: SerializeField]
        public EnginePropellant Propellant { get; set; }

        /// <summary>
        /// The maximum mass flow (when thrust = max thrust), in [kg/s].
        /// </summary>
        public float MaxMassFlow => MaxThrust / (Propellant.NominalIsp * g);

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
                _throttle = Mathf.Clamp01( value );

                // Manually request shutdown if throttle is cut while running.
                if( oldThrottle > 0 && _throttle == 0 && _currentState == EngineState.Running )
                    ShutdownListener();

                // Auto-ignite on throttle-up from zero if the engine is off (KSP-like behavior).
                if( oldThrottle == 0 && _throttle > 0 && _currentState == EngineState.Off )
                    IgniteListener();

                if( oldThrottle != _throttle )
                    OnAfterThrottleChanged?.Invoke();
            }
        }

        /// <summary>
        /// The remaining number of ignitions for this engine. A value less than 0 means infinite ignitions.
        /// </summary>
        [field: SerializeField]
        public int Ignitions { get; set; } = -1;

        /// <summary>
        /// The grace period in seconds after an ignition command during which the engine will wait for propellant flow before flaming out.
        /// This is also the duration of the "spin-up" phase where the engine applies forced suction.
        /// </summary>
        [field: SerializeField]
        public double IgnitionGracePeriod { get; set; } = 0.5;

        /// <summary>
        /// The forced potential applied by the engine's turbopumps during the spin-up phase, in [J/kg].
        /// A large negative value creates strong suction to prime the engine.
        /// </summary>
        [field: SerializeField]
        public double SpinupSuctionPotential { get; set; } = -10.0;

        /// <summary>
        /// The time in seconds that the engine spends in the 'ShuttingDown' state before turning off completely.
        /// Allows for spool-down effects.
        /// </summary>
        [field: SerializeField]
        public double ShutdownDuration { get; set; } = 0.2;

        private IPropulsion.EngineState _currentState = IPropulsion.EngineState.Off;
        private double _ignitionAttemptUT;
        private double _shutdownRequestUT;

        [NamedControl( "Throttle", "Connect this to the controller's throttle output." )]
        public ControlleeInput<float> SetThrottle;
        private void SetThrottleListener( float value )
        {
            this.Throttle = value;
        }

        [NamedControl( "Ignite", "Manually ignite the engine if it has available ignitions." )]
        public ControlleeInput Ignite;
        private void IgniteListener()
        {
            if( _currentState != IPropulsion.EngineState.Off )
                return;

            if( Ignitions == 0 ) // No more ignitions
                return;

            if( Ignitions > 0 )
            {
                Ignitions--;
            }

            _currentState = IPropulsion.EngineState.Igniting;
            _ignitionAttemptUT = TimeManager.UT;
            OnAfterIgnite?.Invoke();
        }

        [NamedControl( "Shutdown", "Manually shut down the engine." )]
        public ControlleeInput Shutdown;
        private void ShutdownListener()
        {
            if( _currentState == IPropulsion.EngineState.Off || _currentState == IPropulsion.EngineState.ShuttingDown )
                return;

            _currentState = IPropulsion.EngineState.ShuttingDown;
            _shutdownRequestUT = TimeManager.UT;
            OnAfterShutdown?.Invoke();
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

        void Awake()
        {
            SetThrottle ??= new ControlleeInput<float>( SetThrottleListener );
            Ignite ??= new ControlleeInput( IgniteListener );
            Shutdown ??= new ControlleeInput( ShutdownListener );
        }

        void FixedUpdate()
        {
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
            _cachedConsumer.IsEnabled = (_currentState == EngineState.Igniting || _currentState == EngineState.Running);

            switch( _currentState )
            {
                case IPropulsion.EngineState.Igniting:
                    // During spin-up, apply a strong, constant suction regardless of throttle.
                    _cachedConsumer.ForcedSuctionPotential = this.SpinupSuctionPotential;
                    _cachedConsumer.Demand = MaxMassFlow;
                    break;

                case IPropulsion.EngineState.Running:
                    _cachedConsumer.ForcedSuctionPotential = null;
                    // Demand is only present if throttled up.
                    if( Throttle > 0 && Propellant != null && !Propellant.PropellantMixture.IsEmpty() )
                    {
                        double massFlowDemand = Throttle * MaxMassFlow;
                        double idealAverageDensity = Propellant.PropellantMixture.GetAverageDensity( 293.15, 101325 );

                        if( idealAverageDensity > 0 )
                        {
                            _cachedConsumer.Demand = massFlowDemand / idealAverageDensity;
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
                    break;

                case IPropulsion.EngineState.Off:
                case IPropulsion.EngineState.ShuttingDown:
                default:
                    // No suction or demand when off or shutting down.
                    _cachedConsumer.ForcedSuctionPotential = null;
                    _cachedConsumer.Demand = 0;
                    break;
            }
            Debug.Log( Inflow.GetMass() + " : " + _cachedConsumer.ForcedSuctionPotential + " : " + _currentState );
        }

        public void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            if( _cachedConsumer != null && _cachedConsumer.Inflow != null )
            {
                this.Inflow = _cachedConsumer.Inflow.Clone();
            }
            else
            {
                this.Inflow?.Clear();
            }

            bool hasFlow = this.Inflow != null && !this.Inflow.IsEmpty();

            switch( _currentState )
            {
                case EngineState.Off:
                    this.Thrust = 0f;
                    break;

                case EngineState.Igniting:
                    this.Thrust = 0f; // No thrust during ignition sequence.

                    // Check if we have received a valid propellant mixture to transition to running.
                    if( hasFlow && CalculatePerformanceScalar( this.Inflow ) > 0.0f && Throttle > 0 )
                    {
                        _currentState = EngineState.Running;
                    }
                    // If not, check if our grace period for ignition has expired.
                    else if( TimeManager.UT > _ignitionAttemptUT + IgnitionGracePeriod )
                    {
                        ShutdownListener(); // Failed start.
                    }
                    break;

                case EngineState.Running:
                    float performanceScalar = hasFlow ? CalculatePerformanceScalar( this.Inflow ) : 0f;
                    if( performanceScalar <= 0.0f )
                    {
                        ShutdownListener(); // Flameout due to starvation or bad mixture.
                        this.Thrust = 0f;
                        break;
                    }

                    // We have flow and a good mixture. Calculate final thrust.
                    float actualMassFlow = (float)(this.Inflow.GetMass() / TimeManager.FixedDeltaTime);
                    float effectiveIsp = Propellant.NominalIsp * performanceScalar;
                    this.Thrust = effectiveIsp * g * actualMassFlow;
                    break;

                case EngineState.ShuttingDown:
                    this.Thrust = 0f;
                    if( TimeManager.UT > _shutdownRequestUT + ShutdownDuration )
                    {
                        _currentState = EngineState.Off;
                    }
                    break;
            }
        }

        private float CalculatePerformanceScalar( IReadonlySubstanceStateCollection inflow )
        {
            if( Propellant == null || Propellant.PropellantMixture.IsEmpty() )
            {
                return 0.0f; // No propellant definition means no thrust.
            }

            // Monopropellant Case
            if( Propellant.PropellantMixture.Count == 1 )
            {
                (ISubstance s, double mass) = Propellant.PropellantMixture[0];
                if( s == null )
                    return 0.0f;

                if( inflow.TryGet( s, out double mass2 ) && mass2 > 0 )
                {
                    return 1.0f; // As long as some of the required monoprop is flowing, performance is 100%.
                }
                return 0.0f; // Required monopropellant not found.
            }

            // Bipropellant (or more) Case
            var primaryReq = Propellant.PropellantMixture[0];
            if( primaryReq.s == null )
                return 0.0f;

            if( !inflow.TryGet( primaryReq.s, out double primaryMass ) || primaryMass <= 1e-9 )
            {
                return 0.0f; // Primary propellant is missing.
            }

            float worstEfficiency = 1.0f;

            // Check every other propellant against the primary.
            for( int i = 1; i < Propellant.PropellantMixture.Count; i++ )
            {
                var (s, mass) = Propellant.PropellantMixture[i];
                if( s == null )
                    return 0.0f;

                if( !inflow.TryGet( s, out double secondaryMass ) || secondaryMass <= 1e-9 )
                {
                    return 0.0f; // A required propellant is missing.
                }

                double idealRatio = mass / primaryReq.mass;
                double actualRatio = secondaryMass / primaryMass;

                // Simple linear falloff.
                float efficiency = (actualRatio < idealRatio)
                    ? (float)(actualRatio / idealRatio)
                    : (float)(idealRatio / actualRatio);

                if( efficiency < worstEfficiency )
                {
                    worstEfficiency = efficiency;
                }
            }

            return worstEfficiency;
        }


        [MapsInheritingFrom( typeof( FRocketEngine ) )]
        public static SerializationMapping FRocketEngineMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngine>()
                .WithMember( "max_thrust", o => o.MaxThrust )
                .WithMember( "propellant", o => o.Propellant )
                .WithMember( "throttle", o => o.Throttle )
                .WithMember( "ignitions", o => o.Ignitions )
                .WithMember( "ignition_grace_period", o => o.IgnitionGracePeriod )
                .WithMember( "spinup_suction_potential", o => o.SpinupSuctionPotential )
                .WithMember( "shutdown_duration", o => o.ShutdownDuration )
                .WithMember( "current_state", o => o._currentState )
                .WithMember( "set_throttle", o => o.SetThrottle )
                .WithMember( "ignite", o => o.Ignite )
                .WithMember( "shutdown", o => o.Shutdown )
                .WithMember( "thrust_transform", ObjectContext.Ref, o => o.ThrustTransform )
                .WithMember( "inlets", o => o.Inlets );
        }
    }
}