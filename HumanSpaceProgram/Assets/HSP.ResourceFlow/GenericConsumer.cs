using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class GenericConsumer : IResourceConsumer
    {
        private const double DEMAND_TO_POTENTIAL_SCALAR = 100000.0;

        public Vector3 FluidAcceleration { get; set; }
        public Vector3 FluidAngularVelocity { get; set; }
        public ISubstanceStateCollection Inflow { get; set; } = new SubstanceStateCollection();

        public double Demand { get; set; } = 0;

        public double? ForcedSuctionPotential { get; set; } = null;

        public bool IsEnabled { get; set; } = false;

        public void PreSolveUpdate( double deltaTime ) { }

        public double GetAvailableInflowVolume( double dt )
        {
            return Demand * dt;
        }

        public void ApplySolveResults( double deltaTime )
        {
            // This is a proxy object. The owning component is responsible for handling Inflow.
        }

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            if( !IsEnabled )
            {
                return new FluidState( 0, 0, 0 ) { FluidSurfacePotential = double.MaxValue }; // Closed valve
            }

            // If a forced suction potential is active (e.g., during engine spinup), use it directly.
            if( ForcedSuctionPotential.HasValue )
            {
                return new FluidState( 0, 0, 0 ) { FluidSurfacePotential = ForcedSuctionPotential.Value };
            }

            // In normal operation, potential is a combination of passive drain and active demand.
            double potential = -1.0; // Small passive potential allows for gravity-fed drain when enabled but demand is zero.
            if( Demand > 0 )
            {
                potential -= Demand * DEMAND_TO_POTENTIAL_SCALAR; // Strong, demand-driven potential for active pumping.
            }

            return new FluidState( 0, 0, 0 ) { FluidSurfacePotential = potential };
        }
    }
}