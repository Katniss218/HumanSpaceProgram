

using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.ResourceFlow;
using HSP.Time;
using HSP.Vessels;
using System;
using System.Linq;
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

        // Heuristic constants for back-calculating physics properties from gameplay stats.
        private const double NominalFullThrottleManifoldPressure = 60e5; // 60 bar
        private const double NominalFullThrottleChamberPressure = 50e5;  // 50 bar
        private const double NominalPressureDelta = NominalFullThrottleManifoldPressure - NominalFullThrottleChamberPressure; // 10 bar = 1e6 Pa

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
        /// The maximum chamber pressure the engine can withstand before being destroyed, in [Pa].
        /// </summary>
        [field: SerializeField]
        public float MaxChamberPressure { get; set; } = 75e5f; // 75 bar

        /// <summary>
        /// The data-driven definition of this engine's propellant requirements, Isp, and ignition behavior.
        /// </summary>
        [field: SerializeField]
        public EnginePropellant Propellant { get; set; }

        /// <summary>
        /// The maximum mass flow (when thrust = max thrust), in [kg/s].
        /// </summary>
        public float MaxMassFlow => (Propellant?.NominalIsp > 0) ? MaxThrust / (Propellant.NominalIsp * g) : 0f;

        /// <summary>
        /// The ratio of chamber pressure to mass flow rate under nominal conditions. [Pa / (kg/s)]
        /// Used to calculate emergent chamber pressure from actual mass flow.
        /// </summary>
        public float ChamberPressureToMassFlowRatio => (MaxMassFlow > 0) ? (float)(NominalFullThrottleChamberPressure / MaxMassFlow) : 0f;

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
        /// The time in seconds that the engine spends in the 'ShuttingDown' state before turning off completely.
        /// Allows for spool-down effects.
        /// </summary>
        [field: SerializeField]
        public double ShutdownDuration { get; set; } = 0.2;

        private IPropulsion.EngineState _currentState = IPropulsion.EngineState.Off;
        private double _ignitionAttemptUT;
        private double _shutdownRequestUT;
        private bool _isPrimed = false;

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
            _isPrimed = false;
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

        EngineFeedSystem _feedSystem;

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

            _feedSystem ??= new EngineFeedSystem( 0.01 ); // 10 liter manifold volume.
            c.TryAddFlowObj( this, _feedSystem );

            foreach( var inlet in Inlets )
            {
                Vector3 inletPosInReferenceSpace = reference.InverseTransformPoint( this.transform.TransformPoint( inlet.LocalPosition ) );
                FlowPipe.Port flowInlet = new FlowPipe.Port( _feedSystem, inletPosInReferenceSpace, inlet.NominalArea );
                c.TryAddFlowObj( inlet, flowInlet );
            }

            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
            return true;
        }

        public void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            if( _feedSystem == null )
                return;

            _feedSystem.IsOutflowEnabled = (_currentState == EngineState.Igniting || _currentState == EngineState.Running);

            // Set injector conductance.
            if( Propellant != null && Propellant.NominalIsp > 0 )
            {
                // Back-calculate the required injector conductance to achieve MaxThrust at a nominal pressure delta.
                // This allows designers to use Thrust/Isp as primary stats, which are more intuitive than conductance.
                // Formula from orifice flow: MassFlow = C * sqrt(deltaP) => C = MassFlow / sqrt(deltaP)
                _feedSystem.InjectorConductance = MaxMassFlow / Math.Sqrt( NominalPressureDelta );
            }
            else
            {
                _feedSystem.InjectorConductance = 0;
            }

            double massFlowDemand = 0;
            switch( _currentState )
            {
                case EngineState.Igniting:
                    _feedSystem.PumpPressureRise = 1e5; // 1 bar of suction.
                    _feedSystem.ChamberPressure = 0.5e5; // 0.5 bar, low pressure target to ensure manifold fills.
                    massFlowDemand = MaxMassFlow * 1.2; // High fixed demand to prime manifold
                    break;

                case EngineState.Running:
                    _feedSystem.PumpPressureRise = Throttle * 100e5; // 10 bar pump at full throttle.

#warning TODO - chamber pressure raises by an order of magnitude when transitioning to Running.
                    // Chamber pressure is now an emergent property based on the PREVIOUS frame's mass flow.
                    // This provides a stable feedback loop for the solver.
                    double lastMassFlow = _feedSystem.MassConsumedLastStep / TimeManager.FixedDeltaTime;
                    _feedSystem.ChamberPressure = lastMassFlow * ChamberPressureToMassFlowRatio;

                    // Self-regulating demand based on current manifold pressure
                    double pressureDelta = _feedSystem.ManifoldPressure - _feedSystem.ChamberPressure;
                    if( pressureDelta > 0 )
                    {
                        massFlowDemand = _feedSystem.InjectorConductance * Math.Sqrt( pressureDelta );
                    }
                    else
                    {
                        massFlowDemand = 0;
                    }
                    break;

                default:
                    _feedSystem.PumpPressureRise = 0;
                    _feedSystem.ChamberPressure = 0;
                    massFlowDemand = 0;
                    break;
            }

            // CFL Condition: The engine's volumetric demand is a stability constraint for the solver.
            // By telling the solver how much fluid we expect to consume (Demand), we prevent it
            // from over-filling the manifold in a single time-step, which would cause oscillations.
            if( massFlowDemand > 0 && Propellant != null && !Propellant.PropellantMixture.IsEmpty() )
            {
                double avgDensity = Propellant.PropellantMixture.GetAverageDensity( 293, 101325 ); // STP approx
                _feedSystem.Demand = (avgDensity > 0) ? (massFlowDemand / avgDensity) : 0.0;
            }
            else
            {
                _feedSystem.Demand = 0;
            }
        }

        public void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            if( _feedSystem == null )
            {
                this.Inflow?.Clear();
                return;
            }

            this.Inflow.Set( _feedSystem.Inflow );

            bool hasFlow = this.Inflow.GetMass() > 1e-6;

            switch( _currentState )
            {
                case EngineState.Off:
                    this.Thrust = 0f;
                    break;

                case EngineState.Igniting:
                    this.Thrust = 0f;
                    Debug.LogWarning( TimeManager.UT + "IGNITING " + this.Inflow.Count + " : " + hasFlow + " : " + _feedSystem.ManifoldPressure + " : " + _feedSystem.ChamberPressure ); // hasFlow is false right after ignition (first iteration here).

                    if( hasFlow && CalculatePerformanceScalar( this.Inflow ) > 0.1f )
                    {
                        _isPrimed = true;
                    }

                    bool isPressurized = _feedSystem.ManifoldPressure > (_feedSystem.ChamberPressure * 0.9);

                    if( _isPrimed && isPressurized && Throttle > 0f )
                    {
                        _currentState = EngineState.Running;
                        _isPrimed = false;
                    }
                    else if( TimeManager.UT > _ignitionAttemptUT + IgnitionGracePeriod )
                    {
                        Debug.LogWarning( $"[{gameObject.name}] Engine Ignition Failed! No propellant flow detected." );
                        _isPrimed = false;
                        ShutdownListener(); // Failed start.
                    }
                    break;

                case EngineState.Running:
                    float performanceScalar = hasFlow ? CalculatePerformanceScalar( this.Inflow ) : 0f;
                    float actualMassFlow = (float)(_feedSystem.MassConsumedLastStep / TimeManager.FixedDeltaTime);
#warning TODO - _feedSystem.MassConsumedLastStep is nonsero, but this.Inflow is zero on the 2nd frame of 'running'.

                    Debug.LogWarning( TimeManager.UT + "RUNNING " + performanceScalar + " : " + actualMassFlow + " : " + hasFlow );
                    if( actualMassFlow <= 1e-6 )
                    {
                        Debug.LogWarning( $"[{gameObject.name}] Engine Flameout! No propellant flow detected." );
                        ShutdownListener(); // Flameout due to starvation.
                        this.Thrust = 0f;
                        break;
                    }
                    if( performanceScalar <= 0.1f )
                    {
                        Debug.LogWarning( $"[{gameObject.name}] Engine Flameout! Bad propellant mixture." );
                        ShutdownListener(); // Flameout due to bad mixture.
                        this.Thrust = 0f;
                        break;
                    }

                    // Calculate the emergent chamber pressure for THIS frame based on actual flow.
                    float currentChamberPressure = actualMassFlow * ChamberPressureToMassFlowRatio;

                    // Check for overpressure failure.
                    if( currentChamberPressure > MaxChamberPressure )
                    {
                        Debug.LogError( $"[{gameObject.name}] Engine Overpressure! Chamber pressure reached {currentChamberPressure / 1e5f:F1} bar, exceeding limit of {MaxChamberPressure / 1e5f:F1} bar. Engine destroyed." );
                        ShutdownListener();
                        this.Thrust = 0f;
                        break;
                    }

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
                ISubstance s = Propellant.PropellantMixture[0].s;
                if( s == null )
                    return 0.0f;

                if( inflow.TryGet( s, out double mass2 ) && mass2 > 0 )
                {
                    return 1.0f;
                }
                return 0.0f;
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

            for( int i = 1; i < Propellant.PropellantMixture.Count; i++ )
            {
                var (s, mass) = Propellant.PropellantMixture[i];
                if( s == null )
                    return 0.0f;

                if( !inflow.TryGet( s, out double secondaryMass ) || secondaryMass <= 1e-9 )
                {
                    return 0.0f;
                }

                double idealRatio = mass / primaryReq.mass;
                double actualRatio = secondaryMass / primaryMass;

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
                .WithMember( "max_chamber_pressure", o => o.MaxChamberPressure )
                .WithMember( "propellant", o => o.Propellant )
                .WithMember( "throttle", o => o.Throttle )
                .WithMember( "ignitions", o => o.Ignitions )
                .WithMember( "ignition_grace_period", o => o.IgnitionGracePeriod )
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