using HSP.ResourceFlow;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowTankTests2
    {
        private FlowTank _tank;

        [SetUp]
        public void SetUp()
        {
            // Standard tank volume 1.0
            _tank = new FlowTank( 1.0 );
        }

        // --- HELPER: Create Geometry ---
        // Creates a 1x1x1 cube centered at (0,0,0)
        // This ensures we have perpendicular edges at Y=-0.5 and Y=0.5
        private void SetupUnitCubeTank()
        {
            var nodes = new List<Vector3>
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                Vector3.zero // Center node to ensure clean tetrahedralization
            };

            var inlets = new[]
            {
                new ResourceInlet(0, new Vector3(0, 0.5f, 0)),
                new ResourceInlet(0, new Vector3(0, -0.5f, 0))
            };

            _tank.SetNodes( nodes.ToArray(), inlets );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 ); // Standard Gravity
        }

        // --- SECTION 1: Volume & Edge Fix Tests ---

        [Test]
        public void CalculatedVolume_MatchesInputVolume()
        {
            SetupUnitCubeTank();
            // The tank volume is defined in constructor as 1.0.
            // The geometry is a 1x1x1 cube.
            // The system should normalize the geometry volume to match the logical volume.

            // Allow small float error
            Assert.AreEqual( 1.0, _tank.CalculatedVolume, 1e-4 );
        }

        [Test]
        public void CenterOfMass_HalfFull_Vertical_IsCorrect()
        {
            // This implicitly tests the Perpendicular Edge Fix.
            // If the horizontal edges at the bottom (y=-0.5) were distributed incorrectly,
            // the CoM would shift downwards or upwards artificially.

            SetupUnitCubeTank();
            _tank.Contents.Add( TestSubstances.Water, 500 ); // 0.5 volume (Half full)
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();

            // Geometric center of the bottom half of a 1x1x1 cube (from -0.5 to 0.0)
            // X and Z should be 0.
            // Y should be -0.25.

            Assert.AreEqual( 0.0f, com.x, 1e-3, "CoM X deviation" );
            Assert.AreEqual( 0.0f, com.z, 1e-3, "CoM Z deviation" );
            Assert.AreEqual( -0.25f, com.y, 0.05f, "CoM Y deviation - If this fails significantly, volume distribution is skewed" );
        }

        [Test]
        public void CenterOfMass_RotatedGravity_ShiftsCorrectly()
        {
            SetupUnitCubeTank();
            _tank.Contents.Add( TestSubstances.Water, 500 ); // Half full

            // Gravity pointing Right (-X is Down potential)
            _tank.FluidAcceleration = new Vector3( 10, 0, 0 );
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();

            // Fluid should pool at X = -0.5.
            // Center of that mass is X = -0.25.
            Assert.AreEqual( -0.25f, com.x, 0.05f );
            Assert.AreEqual( 0.0f, com.y, 1e-3 );
        }

        // --- SECTION 2: Stratification Tests ---

        [Test]
        public void Stratification_HeavyFluidSinks()
        {
            SetupUnitCubeTank();
            _tank.Contents.Add( TestSubstances.Oil, 250 );   // Light
            _tank.Contents.Add( TestSubstances.Water, 250 ); // Heavy
            _tank.ForceRecalculateCache();

            // Sample bottom (Heavy should be here)
            var bottomSample = _tank.SampleSubstances( new Vector3( 0, -0.4f, 0 ), 1, 1 );
            Assert.IsTrue( bottomSample.Contains( TestSubstances.Water ), "Bottom should contain water" );
            Assert.IsFalse( bottomSample.Contains( TestSubstances.Oil ), "Bottom should not contain oil" );

            // Sample top (Light should be here)
            var topSample = _tank.SampleSubstances( new Vector3( 0, -0.1f, 0 ), 1, 1 );
            Assert.IsTrue( topSample.Contains( TestSubstances.Oil ), "Top should contain oil" );
            Assert.IsFalse( topSample.Contains( TestSubstances.Water ), "Top should not contain water" );
        }

        [Test]
        public void Stratification_ThreeLayers()
        {
            SetupUnitCubeTank();
            double vol = 333; // 1/3rd roughly
            _tank.Contents.Add( TestSubstances.Water, vol * TestSubstances.Water.GetDensityAtSTP() );
            _tank.Contents.Add( TestSubstances.Mercury, vol * TestSubstances.Mercury.GetDensityAtSTP() );
            _tank.Contents.Add( TestSubstances.Oil, vol * TestSubstances.Oil.GetDensityAtSTP() );
            _tank.ForceRecalculateCache();

            // Mercury (13500) -> Water (1000) -> Oil (800)

            var s1 = _tank.SampleSubstances( new Vector3( 0, -0.45f, 0 ), 1, 1 );
            Assert.IsTrue( s1.Contains( TestSubstances.Mercury ) );

            var s2 = _tank.SampleSubstances( new Vector3( 0, 0f, 0 ), 1, 1 );
            Assert.IsTrue( s2.Contains( TestSubstances.Water ) );

            var s3 = _tank.SampleSubstances( new Vector3( 0, 0.45f, 0 ), 1, 1 );
            Assert.IsTrue( s3.Contains( TestSubstances.Oil ) );
        }

        // --- SECTION 3: Pressure & Potentials ---

        [Test]
        public void HydrostaticPressure_CalculatesCorrectly()
        {
            SetupUnitCubeTank(); // 1m high (-0.5 to 0.5)
            _tank.Contents.Add( TestSubstances.Water, 1000 ); // Full tank
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 ); // g = 10
            _tank.ForceRecalculateCache();

            // Pressure at top (0.5) should be 0 (gauge pressure)
            var topState = _tank.Sample( new Vector3( 0, 0.5f, 0 ), 0.1 );
            Assert.AreEqual( 0.0, topState.Pressure, 1.0 );

            // Pressure at bottom (-0.5) 
            // h = 1m, rho = 1000, g = 10. P = 10,000.
            var botState = _tank.Sample( new Vector3( 0, -0.5f, 0 ), 0.1 );
            Assert.AreEqual( 10000.0, botState.Pressure, 100.0 ); // Allow loose tolerance for discretized slicing
        }

        [Test]
        public void RotationalPotential_CentrifugeEffect()
        {
            SetupUnitCubeTank();
            _tank.Contents.Add( TestSubstances.Water, 200 ); // Small amount

            // No Gravity, Only Spinning around Y
            _tank.FluidAcceleration = Vector3.zero;
            _tank.FluidAngularVelocity = new Vector3( 0, 10, 0 ); // 10 rad/s
            _tank.ForceRecalculateCache();

            // Fluid should be pushed to the furthest X/Z points (corners).
            // Center (0,0,0) should be empty.
            var centerSample = _tank.SampleSubstances( Vector3.zero, 1, 1 );
            Assert.AreEqual( 0, centerSample.Count, "Center should be empty in centrifuge" );

            // Corner (0.5, -0.5, 0.5) should have fluid
            var cornerSample = _tank.SampleSubstances( new Vector3( 0.5f, -0.5f, 0.5f ), 1, 1 );
            Assert.IsTrue( cornerSample.Contains( TestSubstances.Water ), "Outer corner should have fluid" );
        }

        // --- SECTION 4: Edge Cases ---

        [Test]
        public void Overfill_DoesNotCrash_AndClampsToVolume()
        {
            SetupUnitCubeTank();
            // Try to add 2.0m^3 to a 1.0m^3 tank
            _tank.Contents.Add( TestSubstances.Water, 2000 );
            _tank.ForceRecalculateCache();

            // Logic dictates it compresses or clamps. 
            // In your implementation: "scale = totalTankVolume / totalFluidVolume"
            // So internal volumes are scaled down.

            // Check that Sample returns valid pressure (high) but not infinite
            var state = _tank.Sample( new Vector3( 0, -0.5f, 0 ), 0.1 );
            Assert.Greater( state.Pressure, 0 );

            // Check CoM is simply the center of the tank (0,0,0) because it's uniformly full
            Vector3 com = _tank.GetCenterOfMass();
            Assert.AreEqual( 0.0f, com.y, 1e-3 );
        }

        [Test]
        public void EmptyTank_ReturnsInletPotential()
        {
            SetupUnitCubeTank();
            _tank.Contents.Clear();
            _tank.ForceRecalculateCache();

            // Sample arbitrary point
            var pt = new Vector3( 0, 0, 0 );
            var state = _tank.Sample( pt, 0.1 );

            // Pressure should be 0
            Assert.AreEqual( 0.0, state.Pressure );

            // Potential should match point potential (sink behavior)
            double pot = _tank.GetPotentialAt( pt );
            Assert.AreEqual( pot, state.FluidSurfacePotential, 1e-5 );
        }
    }


    [TestFixture]
    public class FlowTankTests
    {
        private FlowTank _tank;

        [SetUp]
        public void SetUp()
        {
            _tank = new FlowTank( 1.0 ); // 1 m^3
            var nodes = new[]
            {
                new Vector3( 0, 1, 0 ),  // Node 0: Top
                new Vector3( 0, -1, 0 ), // Node 1: Bottom
                new Vector3( 1, 0, 0 ),  // Node 2: Right
                new Vector3( -1, 0, 0 ), // Node 3: Left
                new Vector3( 0, 0, 1 ),  // Node 4: Forward
                new Vector3( 0, 0, -1 )  // Node 5: Back
            };

            // Create inlets at the Top (Node 0) and Bottom (Node 1).
            // NOTE: You may need to adjust the arguments inside 'new ResourceInlet(...)'
            // based on your specific class definition (e.g., node index, bore size, etc.).
            var inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1, 0 ) ), // Attached to Top
                new ResourceInlet( 1, new Vector3( 0, -1, 0 ) )  // Attached to Bottom
            };
            _tank.SetNodes( nodes, inlets );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 ); // Gravity
        }

        [Test]
        public void Sample_FluidSurfacePotential_EmptyTank_IsPortPotential()
        {
            // Arrange
            _tank.Contents.Clear();
            _tank.ForceRecalculateCache();
            var portPosition = new Vector3( 0, 0.5f, 0 );
            double expectedPotential = _tank.GetPotentialAt( portPosition );

            // Act
            FluidState state = _tank.Sample( portPosition, 0.1 );

            // Assert
            Assert.AreEqual( expectedPotential, state.FluidSurfacePotential, 1e-9 );
        }

        [Test]
        public void Sample_FluidSurfacePotential_FullTank_IsFluidSurface()
        {
            // Arrange
            // Fill tank with 0.5 m^3 of water. Since total vol is 1, it should fill halfway up.
            // Potential at y=-1 is 10. Potential at y=1 is -10. Total span 20.
            // A half-full tank should have its surface at y=0, which corresponds to potential 0.
            _tank.Contents.Add( TestSubstances.Water, 500 ); // 0.5 m^3
            _tank.ForceRecalculateCache();
            var portPosition = new Vector3( 0, 0, 0 );

            // Act
            FluidState state = _tank.Sample( portPosition, 0.1 );

            // Assert
            Assert.AreEqual( 0.0, state.FluidSurfacePotential, 1e-3 );
        }

        [Test]
        public void Sample_FluidSurfacePotential_StratifiedTank_IsTopFluidSurface()
        {
            // Arrange
            _tank.Contents.Add( TestSubstances.Water, 250 ); // 0.25 m^3
            _tank.Contents.Add( TestSubstances.Oil, 200 ); // 0.25 m^3
            _tank.ForceRecalculateCache();
            // Total volume is 0.5m^3, so surface is at y=0, potential=0. Oil is on top.
            var portPosition = new Vector3( 0, -0.8f, 0 );

            // Act
            FluidState state = _tank.Sample( portPosition, 0.1 );

            // Assert
            Assert.AreEqual( 0.0, state.FluidSurfacePotential, 1e-9 );
        }

        [Test]
        public void SampleSubstances_SamplesCorrectLayer_FromBottom()
        {
            // Arrange
            _tank.Contents.Add( TestSubstances.Water, 250 ); // 0.25 m^3, bottom layer
            _tank.Contents.Add( TestSubstances.Oil, 200 );   // 0.25 m^3, top layer
            _tank.ForceRecalculateCache();

            // Port at the bottom of the tank (y=-1)
            var portPosition = new Vector3( 0, -1, 0 );

            // Act
            var sampled = _tank.SampleSubstances( portPosition, 0.1, 1.0 );

            // Assert
            Assert.AreEqual( 1, sampled.Count );
            Assert.IsTrue( sampled.Contains( TestSubstances.Water ) );
            Assert.IsFalse( sampled.Contains( TestSubstances.Oil ) );
        }

        [Test]
        public void SampleSubstances_SamplesCorrectLayer_FromMiddle()
        {
            // Arrange
            _tank.Contents.Add( TestSubstances.Water, 250 ); // 0.25 m^3, bottom layer (-1 to -0.5)
            _tank.Contents.Add( TestSubstances.Oil, 200 );   // 0.25 m^3, top layer (-0.5 to 0)
            _tank.ForceRecalculateCache();

            // Port in the middle of the top (oil) layer
            var portPosition = new Vector3( 0, -0.25f, 0 );

            // Act
            var sampled = _tank.SampleSubstances( portPosition, 0.1, 1.0 );

            // Assert
            Assert.AreEqual( 1, sampled.Count );
            Assert.IsFalse( sampled.Contains( TestSubstances.Water ) );
            Assert.IsTrue( sampled.Contains( TestSubstances.Oil ) );
        }
    }
}
