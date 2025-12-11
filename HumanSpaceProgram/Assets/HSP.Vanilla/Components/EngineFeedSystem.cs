using HSP.ResourceFlow;
using UnityEngine;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// A massless, flow-through component that represents an engine's inlet manifold.
    /// It implements IResourceConsumer and tells the flow network how strongly it is "pulling" by creating a negative potential
    /// based on a 'TargetPressure' value set by its parent FRocketEngine.
    /// </summary>
    public sealed class EngineFeedSystem : IResourceConsumer
    {
        // IResourceConsumer properties
        public Vector3 FluidAcceleration { get; set; }
        public Vector3 FluidAngularVelocity { get; set; }
        public ISubstanceStateCollection Inflow { get; set; } = new SubstanceStateCollection();

        // New model properties
        public double TargetPressure { get; set; }
        public double ExpectedDensity { get; set; } = 800.0;
        public double ActualMassFlow_LastStep { get; private set; }

        public EngineFeedSystem()
        {
        }

        /// <summary>
        /// Resets the internal state of the feed system.
        /// </summary>
        public void Reset()
        {
            TargetPressure = 0;
            ActualMassFlow_LastStep = 0;
            Inflow.Clear();
        }

        public double GetAvailableInflowVolume( double dt )
        {
            // A pump-like consumer has effectively infinite demand capacity.
            // The flow will be limited by the supply from producers and pipe conductance.
            return double.PositiveInfinity;
        }

        public void ApplyFlows( double deltaTime )
        {
            if( deltaTime <= 1e-9 )
            {
                ActualMassFlow_LastStep = 0;
                // Inflow is not cleared here. The FRocketEngine is responsible for "consuming" it
                // after it has had a chance to read the composition for performance calculations.
                return;
            }

            // Calculate the actual mass flow rate from the inflow provided by the solver.
            ActualMassFlow_LastStep = Inflow.GetMass() / deltaTime;
        }

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            // The "pull" of the engine is represented by its turbopump demanding a certain inlet pressure.
            // We convert this pressure into a potential using the standard P/rho formula.
            // Since this is a consumer, we make it a negative potential to signal suction to the solver.
            if( ExpectedDensity <= 1e-9 )
            {
                return new FluidState( 0, 0, 0 ) { FluidSurfacePotential = 0 };
            }

            double potentialFromPressure = TargetPressure / ExpectedDensity;
            return new FluidState( 0, 0, 0 ) { FluidSurfacePotential = -potentialFromPressure };
        }
    }
}
