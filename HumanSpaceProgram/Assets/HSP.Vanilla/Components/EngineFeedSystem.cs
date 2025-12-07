using HSP.ResourceFlow;
using System;
using UnityEngine;
using static HSP.ResourceFlow.VaporLiquidEquilibrium;

namespace HSP.Vanilla.Components
{
    public sealed class EngineFeedSystem : IResourceConsumer, IStiffnessProvider
    {
        public Vector3 FluidAcceleration { get; set; }
        public Vector3 FluidAngularVelocity { get; set; }
        public ISubstanceStateCollection Inflow { get; set; } = new SubstanceStateCollection();
        public IReadonlySubstanceStateCollection Contents => _contents;
        ISubstanceStateCollection _contents = new SubstanceStateCollection();

        public double Demand { get; set; }

        public bool IsOutflowEnabled { get; set; } = false;

        /// <summary>
        /// Pressure added by the pump, controlled by throttle, in [Pa].
        /// </summary>
        public double PumpPressureRise { get; set; }

        /// <summary>
        /// The pressure existing in the combustion chamber, which the manifold must overcome. [Pa]
        /// </summary>
        public double ChamberPressure { get; set; }

        /// <summary>
        /// How easily fluid flows through the injector. [kg/s / sqrt(Pa)]
        /// </summary>
        public double InjectorConductance { get; set; } = 1.0;

        public double MassConsumedLastStep { get; private set; }

        public double ManifoldVolume => _manifoldVolume;

        public double ManifoldPressure => _manifoldPressure;

        private readonly double _manifoldVolume;
        private double _manifoldPressure;
        private double _cachedPressureDerivative;

        /// <param name="manifoldVolume">The internal volume of the manifold, used for pressure calculation, in [m^3].</param>
        public EngineFeedSystem( double manifoldVolume )
        {
            // Ensure volume isn't microscopic to prevent singularity.
            _manifoldVolume = Math.Max( manifoldVolume, 0.001 ); // 1 Liter minimum
            _manifoldPressure = 0;
        }

        public double GetAvailableInflowVolume( double dt )
        {
            // For a rate-limited consumer like an engine, the "capacity" for a timestep is its demand rate * time.
            return Demand * dt;
        }

        public void ApplyFlows( double deltaTime )
        {
            if( deltaTime <= 0 )
                return;

            // 1) Add inflow mass to internal contents (same as your current code).
            _contents.Add( Inflow );

            // Shortcut: if no outflow allowed, compute pressure directly from mass and return.
            if( !IsOutflowEnabled )
            {
                // Compute pressure that corresponds to current contents mass
                // (ComputePressureOnly should be able to compute pressure from the current _contents)
                var (p, dPdM) = VaporLiquidEquilibrium.ComputePressureAndDerivativeWrtMass( _contents, new FluidState( _manifoldPressure, FluidState.STP.Temperature, FluidState.STP.Velocity ), _manifoldVolume );
                _manifoldPressure = p;
                _cachedPressureDerivative = dPdM;
                MassConsumedLastStep = 0.0;
                return;
            }

            double M0 = _contents.GetMass(); // mass after inflow
            if( M0 <= 1e-9 )
            {
                // empty manifold -> pressure from zero mass (vacuum / baseline)
                var (p, dPdM) = VaporLiquidEquilibrium.ComputePressureAndDerivativeWrtMass( _contents, new FluidState( _manifoldPressure, FluidState.STP.Temperature, FluidState.STP.Velocity ), _manifoldVolume );
                _manifoldPressure = p;
                _cachedPressureDerivative = dPdM;
                MassConsumedLastStep = 0.0;
                return;
            }

            // Pre-compute the aggregate mixture properties once, before the solver loop.
            var mixtureProps = new MixtureProperties( _contents, new FluidState( _manifoldPressure, FluidState.STP.Temperature, FluidState.STP.Velocity ), _manifoldVolume );
            _cachedPressureDerivative = mixtureProps.GetStateAtMass( M0 ).dPdM;

            double Pc = ChamberPressure;
            double C = InjectorConductance;
            double dt = deltaTime;

            // --- Newton-Raphson Solver ---
            const int MAX_ITER = 8;
            const double ABS_TOLERANCE = 1e-3; // Absolute tolerance in Pascals for stability near zero.
            const double REL_TOLERANCE = 5e-4; // The primary relative tolerance for convergence.
            double P_candidate = Math.Max( _manifoldPressure, Pc + 1.0 ); // Initial guess from previous frame's pressure.
            int i = 0;
            bool converged = false;
            for( i = 0; i < MAX_ITER; i++ )
            {
                double dP_injector = P_candidate - Pc;

                // 1. Calculate mass removed and final mass for this candidate pressure
                double mRemoved = 0.0;
                if( dP_injector > 0.0 )
                {
                    mRemoved = C * Math.Sqrt( dP_injector ) * dt;
                    if( mRemoved > M0 ) mRemoved = M0;
                }

                double M_final = Math.Max( 0.0, M0 - mRemoved );

                // 2. Get pressure and derivative from VLE for this final mass
                // OPTIMIZED: Use the pre-computed mixture properties for an O(1) calculation.
                var (P_from_mass, dPdM) = mixtureProps.GetStateAtMass( M_final );

                // 3. Calculate residual and its derivative
                double residual = P_from_mass - P_candidate;

                double dM_final_dP = 0.0;
                if( dP_injector > 1e-9 ) // Avoid division by zero for the derivative
                {
                    // dM_final/dP = -d(mRemoved)/dP = - (C * dt) / (2 * sqrt(P - Pc))
                    dM_final_dP = -(C * dt) / (2.0 * Math.Sqrt( dP_injector ));
                }

                double residual_derivative = (dPdM * dM_final_dP) - 1.0;

                // 4. Newton-Raphson Step
                if( Math.Abs( residual_derivative ) < 1e-9 )
                {
                    // Derivative is zero, solver cannot proceed. Exit loop.
                    break;
                }

                double P_next = P_candidate - residual / residual_derivative;

                // Check for convergence using a mixed relative and absolute tolerance.
                // This is robust: it relies on relative tolerance for typical pressures,
                // but falls back to an absolute tolerance when pressure is near zero.
                if( Math.Abs( P_next - P_candidate ) <= ABS_TOLERANCE + REL_TOLERANCE * Math.Abs( P_next ) )
                {
                    converged = true;
                    P_candidate = P_next;
                    break;
                }

                // Update for next iteration, with some damping to prevent overshooting
                P_candidate = P_candidate * 0.5 + P_next * 0.5;

                // Ensure candidate stays physically plausible
                if( P_candidate < 0 ) P_candidate = 0;
            }
            if( converged )
                Debug.Log( $"[EngineFeedSystem] Bisection converged in {i} iters to P={P_candidate} Pa" );
            else
                Debug.Log( $"[EngineFeedSystem] Bisection did not converge" );

            double P_solution = Math.Max( 0.0, P_candidate );

            // compute final removed mass and update contents and manifold pressure
            double dP_sol = Math.Max( 0.0, P_solution - Pc );
            double mRemovedSol = (dP_sol > 0.0) ? C * Math.Sqrt( dP_sol ) * dt : 0.0;
            mRemovedSol = Math.Min( mRemovedSol, M0 );

            double M_finalSolution = Math.Max( 0.0, M0 - mRemovedSol );

            // scale contents down to M_finalSolution
            double massBefore = _contents.GetMass();
            if( massBefore > 1e-12 )
                _contents.Scale( M_finalSolution / massBefore );

            _manifoldPressure = P_solution;
            MassConsumedLastStep = mRemovedSol;
        }


        /// <summary>
        /// Provides the potential at the inlet, which the solver uses to calculate flow for the next frame.
        /// </summary>
        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            // Even if outflow is disabled, we still want to sample correctly 
            // so the manifold can fill up before ignition.

            double inletPressure = _manifoldPressure - PumpPressureRise;

            // Prevent singularity when empty
            double density = Contents.GetAverageDensity( FluidState.STP.Temperature, inletPressure );
            if( density <= 1e-9 )
                density = 800.0;

            return new FluidState( inletPressure, 293, 0 )
            {
                FluidSurfacePotential = inletPressure / density
            };
        }

        public double GetPotentialDerivativeWrtVolume()
        {
            // Per the roadmap and interface remarks, this is approximately dP/dM.
            // This value is cached during ApplyFlows for efficiency.
            return _cachedPressureDerivative;
        }
    }
}