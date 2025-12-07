using HSP.ResourceFlow;
using UnityEngine;

namespace HSP_Tests
{
    public static class FlowNetworkTestHelper
    {
        public static FlowTank CreateTestTank( double volume, Vector3 acceleration, Vector3 offset )
        {
            var tank = new FlowTank( volume );
            var nodes = new[]
            {
                new Vector3( 0, 1, 0 ) + offset,  // Node 0: Top
                new Vector3( 0, -1, 0 ) + offset, // Node 1: Bottom
                new Vector3( 1, 0, 0 ) + offset,  // Node 2: Right
                new Vector3( -1, 0, 0 ) + offset, // Node 3: Left
                new Vector3( 0, 0, 1 ) + offset,  // Node 4: Forward
                new Vector3( 0, 0, -1 ) + offset  // Node 5: Back
            };

            var inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1, 0 ) + offset ), // Attached to Top
                new ResourceInlet( 1, new Vector3( 0, -1, 0 ) + offset )  // Attached to Bottom
            };
            tank.SetNodes( nodes, inlets );
            tank.FluidAcceleration = acceleration;
            tank.FluidState = new FluidState( pressure: 101325.0, temperature: 293.15, velocity: 0.0 );
            return tank;
        }

        public static FlowTank CreateTestTank( double volume, Vector3 acceleration, Vector3 offset, ISubstance substance, double mass )
        {
            var tank = CreateTestTank(volume, acceleration, offset );

            if( substance != null && mass > 0 )
            {
                tank.Contents.Add( substance, mass );
            }

            return tank;
        }
    }
}