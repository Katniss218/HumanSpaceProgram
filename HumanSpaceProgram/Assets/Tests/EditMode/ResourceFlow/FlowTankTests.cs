using HSP.ResourceFlow;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using HSP_Tests;
using HSP_Tests.NUnit;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowTankAndCacheTests
    {
        private FlowTank _tank;
        private const float FLOAT_TOLERANCE = 1e-3f;
        private const double DOUBLE_TOLERANCE = 1e-6;

        [SetUp]
        public void SetUp()
        {
            // Standard tank volume 1.0 m^3
            _tank = new FlowTank( 1.0 );
        }

        // Helper to create a 1x1x1 cube geometry centered at the origin.
        private void SetupUnitCubeTank( FlowTank tank )
        {
            var nodes = new List<Vector3>
            {
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f), new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f), new Vector3( 0.5f,  0.5f,  0.5f),
                Vector3.zero // Center node to ensure clean tetrahedralization
            };

            var inlets = new[]
            {
                new ResourceInlet(0.1f, new Vector3(0, 0.5f, 0)),
                new ResourceInlet(0.1f, new Vector3(0, -0.5f, 0))
            };

            tank.SetNodes( nodes.ToArray(), inlets );
        }

        #region Potential Tests
        [Test, Description( "Tests GetPotentialAt() with only linear acceleration (gravity)." )]
        public void GetPotentialAt_LinearAcceleration_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -9.81f, 0 );
            _tank.FluidAngularVelocity = Vector3.zero;
            _tank.ForceRecalculateCache();

            double potentialAtTop = _tank.GetPotentialAt( new Vector3( 0, 1, 0 ) );
            double potentialAtBottom = _tank.GetPotentialAt( new Vector3( 0, -1, 0 ) );
            double potentialAtOrigin = _tank.GetPotentialAt( Vector3.zero );

            // Potential = -g . r = -(0, -9.81, 0) . (x, y, z) = 9.81y
            Assert.That( potentialAtTop, Is.EqualTo( 9.81 ).Within( DOUBLE_TOLERANCE ) );
            Assert.That( potentialAtBottom, Is.EqualTo( -9.81 ).Within( DOUBLE_TOLERANCE ) );
            Assert.That( potentialAtOrigin, Is.EqualTo( 0.0 ).Within( DOUBLE_TOLERANCE ) );
        }

        [Test, Description( "Tests GetPotentialAt() with only angular velocity (centrifugal force)." )]
        public void GetPotentialAt_AngularVelocity_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = Vector3.zero;
            _tank.FluidAngularVelocity = new Vector3( 0, 10, 0 ); // Spinning around Y
            _tank.ForceRecalculateCache();

            double potentialAtCenter = _tank.GetPotentialAt( Vector3.zero );
            double potentialAtEdge = _tank.GetPotentialAt( new Vector3( 1, 0, 0 ) );

            // Potential = -0.5 * |omega x r|^2
            // At center, r=0, potential = 0
            // At edge, r=(1,0,0), omega=(0,10,0). omega x r = (0,0,-10). |omega x r|^2 = 100. Potential = -50.
            Assert.That( potentialAtCenter, Is.EqualTo( 0.0 ).Within( DOUBLE_TOLERANCE ) );
            Assert.That( potentialAtEdge, Is.EqualTo( -50.0 ).Within( DOUBLE_TOLERANCE ) );
        }

        [Test, Description( "Tests GetPotentialAt() with combined linear and angular acceleration." )]
        public void GetPotentialAt_Combined_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.FluidAngularVelocity = new Vector3( 0, 10, 0 );
            _tank.ForceRecalculateCache();

            var testPoint = new Vector3( 1, 1, 0 );

            // Linear: -(-10 * 1) = 10
            // Rotational: omega x r = (0,10,0)x(1,1,0) = (0,0,-10). |omega x r|^2 = 100. Pot = -50
            // Total = 10 - 50 = -40
            double potential = _tank.GetPotentialAt( testPoint );
            Assert.That( potential, Is.EqualTo( -40.0 ).Within( DOUBLE_TOLERANCE ) );
        }
        #endregion

        #region Geometry & Volume Tests

        [Test, Description( "Tests that after baking potential slices, the total calculated geometric volume matches the tank's configured volume." )]
        public void BakePotentialSlices_TotalVolume_MatchesTankVolume()
        {
            SetupUnitCubeTank( _tank );
            _tank.ForceRecalculateCache();
            Assert.That( _tank.CalculatedVolume, Is.EqualTo( _tank.Volume ).Within( FLOAT_TOLERANCE ) );
        }

        #endregion

        #region Stratification Tests

        [Test, Description( "Tests that two immiscible liquids stratify correctly under gravity, with the denser liquid at the bottom." )]
        public void Stratification_HeavyFluidSinks_UnderGravity()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            double vol = 0.5;
            _tank.Contents.Add( TestSubstances.Oil, vol * TestSubstances.Oil.GetDensityAtSTP() );   // Light, density ~800
            _tank.Contents.Add( TestSubstances.Water, vol * TestSubstances.Water.GetDensityAtSTP() ); // Heavy, density 1000
            _tank.ForceRecalculateCache();

            using var bottomSample = _tank.SampleSubstances( new Vector3( 0, -0.4f, 0 ), 1.0 );
            Assert.That( bottomSample.IsPure( out var subBottom ), Is.True, "Bottom layer should be one substance." );
            Assert.That( subBottom == TestSubstances.Water, Is.True, "Bottom layer should be water." );

            using var topSample = _tank.SampleSubstances( new Vector3( 0, 0.1f, 0 ), 1.0 );
            Assert.That( topSample.IsPure( out var subTop ), Is.True, "Top layer should be one substance." );
            Assert.That( subTop == TestSubstances.Oil, Is.True, "Top layer should be oil." );
        }

        [Test, Description( "Tests stratification with three fluids of different densities." )]
        public void Stratification_ThreeLayers_AreCorrectlyOrdered()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            double vol = 0.333;
            _tank.Contents.Add( TestSubstances.Water, vol * TestSubstances.Water.GetDensityAtSTP() );     // mid
            _tank.Contents.Add( TestSubstances.Mercury, vol * TestSubstances.Mercury.GetDensityAtSTP() ); // heavy
            _tank.Contents.Add( TestSubstances.Oil, vol * TestSubstances.Oil.GetDensityAtSTP() );       // light
            _tank.ForceRecalculateCache();

            // Order: Mercury (13500) -> Water (1000) -> Oil (800)

            using var s1 = _tank.SampleSubstances( new Vector3( 0, -0.45f, 0 ), 1.0 );
            Assert.That( s1.IsPure( out var sub1 ), Is.True, "Bottom layer should be one substance." );
            Assert.That( sub1 == TestSubstances.Mercury, Is.True, "Bottom layer should be Mercury." );

            using var s2 = _tank.SampleSubstances( new Vector3( 0, 0f, 0 ), 1.0 );
            Assert.That( s2.IsPure( out var sub2 ), Is.True, "Middle layer should be one substance." );
            Assert.That( sub2 == TestSubstances.Water, Is.True, "Middle layer should be Water." );

            using var s3 = _tank.SampleSubstances( new Vector3( 0, 0.45f, 0 ), 1.0 );
            Assert.That( s3.IsPure( out var sub3 ), Is.True, "Top layer should be one substance." );
            Assert.That( sub3 == TestSubstances.Oil, Is.True, "Top layer should be Oil." );
        }

        [Test, Description( "Tests that in a spinning tank with no gravity, denser fluids move to the outer radius." )]
        public void Stratification_WithSpin_DenserFluidMovesOutward()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = Vector3.zero;
            _tank.FluidAngularVelocity = new Vector3( 0, 10, 0 ); // Spin around Y
            double vol = 0.5;
            _tank.Contents.Add( TestSubstances.Oil, vol * TestSubstances.Oil.GetDensityAtSTP() );   // Light, density ~800
            _tank.Contents.Add( TestSubstances.Water, vol * TestSubstances.Water.GetDensityAtSTP() ); // Heavy, density 1000
            _tank.ForceRecalculateCache();

            using var centerSample = _tank.SampleSubstances( new Vector3( 0.1f, 0, 0.1f ), 1.0 );
            Assert.That( centerSample.IsPure( out var subCenter ), Is.True, "Center should be one substance." );
            Assert.That( subCenter == TestSubstances.Oil, Is.True, "Center should contain the lighter fluid (Oil)." );

            using var cornerSample = _tank.SampleSubstances( new Vector3( 0.45f, 0, 0.45f ), 1.0 );
            Assert.That( cornerSample.IsPure( out var subCorner ), Is.True, "Outer edge should be one substance." );
            Assert.That( subCorner == TestSubstances.Water, Is.True, "Outer edge should contain the denser fluid (Water)." );
        }
        #endregion

        #region Center of Mass Tests

        [Test]
        public void GetCenterOfMass_EmptyTank_IsAtOrigin()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Clear();
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();
            Assert.That( com.x, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.y, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.z, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
        }

        [Test]
        public void GetCenterOfMass_FullHomogeneousTank_IsAtOrigin()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Add( TestSubstances.Water, 1000 );
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();
            Assert.That( com.x, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.y, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.z, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
        }

        [Test]
        public void GetCenterOfMass___OverfilledHomogeneousTank___IsAtOrigin()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Add( TestSubstances.Water, 2000 ); // 2.0m^3 in a 1.0m^3 tank
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();
            Assert.That( com.x, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.y, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.z, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
        }

        [Test]
        public void GetCenterOfMass_HalfFull_Vertical_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Add( TestSubstances.Water, 500 ); // 0.5 volume (Half full)
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();

            Assert.That( com.x, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.y, Is.EqualTo( -0.25f ).Within( 5e-2 ) );
            Assert.That( com.z, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
        }

        [Test]
        public void GetCenterOfMass_HalfFull_RotatedX_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( -10, 0, 0 );
            _tank.Contents.Add( TestSubstances.Water, 500 ); // Half full
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();

            Assert.That( com.x, Is.EqualTo( -0.25f ).Within( 5e-2 ) );
            Assert.That( com.y, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.z, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
        }

        [Test]
        public void GetCenterOfMass_HalfFull_RotatedZ_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, 0, -10 );
            _tank.Contents.Add( TestSubstances.Water, 500 ); // Half full
            _tank.ForceRecalculateCache();

            Vector3 com = _tank.GetCenterOfMass();

            Assert.That( com.x, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.y, Is.EqualTo( 0.0f ).Within( 5e-2 ) );
            Assert.That( com.z, Is.EqualTo( -0.25f ).Within( 5e-2 ) );
        }

        [Test]
        public void GetCenterOfMass_Stratified_IsLowerThanHomogeneous()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Add( TestSubstances.Mercury, 500 );
            _tank.ForceRecalculateCache();
            Vector3 comDenser = _tank.GetCenterOfMass();

            _tank.Contents.Clear();

            _tank.Contents.Add( TestSubstances.Water, 500 );
            _tank.ForceRecalculateCache();
            Vector3 comLighter = _tank.GetCenterOfMass();

            Assert.That( comDenser.y, Is.LessThan( comLighter.y ), "CoM of denser fluid should be lower for the same mass." );
        }

        #endregion

        #region IStiffnessProvider Tests

        [Test, Description( "Verifies that dP/dM for a gas-only tank matches the ideal gas law derivation." )]
        public void GetPotentialDerivativeWrtVolume_GasOnly_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Add( TestSubstances.Air, 1.2 ); // approx 1 atm in 1m^3 at 300K
            _tank.FluidState = new FluidState( 0, 300, 0 );
            _tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( _tank.Contents, _tank.FluidState, _tank.Volume ), 300, 0 );

            double dPdM = (_tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            // dP/dM for ideal gas is R_specific * T / V
            double expected_dPdM = TestSubstances.Air.SpecificGasConstant * 300 / 1.0;
            Assert.That( dPdM, Is.EqualTo( expected_dPdM ).Within( 1e-6 ) );
        }

        [Test, Description( "Verifies that dP/dM for an overfilled liquid tank is high and matches the bulk modulus derivation." )]
        public void GetPotentialDerivativeWrtVolume_OverfilledLiquid_IsCorrect()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            double density = TestSubstances.Water.GetDensity( 300, 101325 );
            _tank.Contents.Add( TestSubstances.Water, density * 1.1 ); // 10% overfill
            _tank.FluidState = new FluidState( 0, 300, 0 );
            _tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( _tank.Contents, _tank.FluidState, _tank.Volume ), 300, 0 );

            double dPdM = (_tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            // dP/dM for liquid is K / (rho_0 * V)
            double expected_dPdM = TestSubstances.Water.GetBulkModulus( 300, 101325 ) / (TestSubstances.Water.GetDensity( 300, 101325 ) * 1.0);
            Assert.That( dPdM, Is.EqualTo( expected_dPdM ).Within( 1.0 ).Percent );
        }

        [Test, Description( "Verifies that dP/dM for an empty tank is zero." )]
        public void GetPotentialDerivativeWrtVolume_Empty_IsZero()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.FluidState = new FluidState( 0, 300, 0 );
            double dPdM = (_tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();
            Assert.That( dPdM, Is.EqualTo( 0.0 ) );
        }

        [Test, Description( "Verifies that a mixed-phase tank has low stiffness, dominated by its gas content." )]
        public void GetPotentialDerivativeWrtVolume_MixedPhase_IsLow()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Add( TestSubstances.Water, 500 ); // 50% liquid
            _tank.Contents.Add( TestSubstances.Air, 0.6 );   // 50% gas ullage
            _tank.FluidState = new FluidState( 0, 300, 0 );
            _tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( _tank.Contents, _tank.FluidState, _tank.Volume ), 300, 0 );

            double dPdM_mixed = (_tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            double totalMass = 500.6;
            double w_gas = 0.6 / totalMass;
            double B = w_gas / TestSubstances.Air.MolarMass;
            const double R = 8.31446;
            double T = 300;
            double V_tank = 1.0;
            double V_ullage = 0.5;
            double expected_dPdM = (B * R * T * V_tank) / (V_ullage * V_ullage);

            Assert.That( dPdM_mixed, Is.EqualTo( expected_dPdM ).Within( 1.0 ).Percent );
            Assert.That( dPdM_mixed, Is.LessThan( 1e6 ), "Stiffness should be low due to gas ullage." );
        }

        [Test, Description( "Verifies that stiffness increases dramatically as a liquid-only tank approaches full capacity." )]
        public void GetPotentialDerivativeWrtVolume_StiffnessIncreasesNearFull()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            var tank_half = new FlowTank( 1.0 );
            tank_half.Contents.Add( TestSubstances.Water, 500 ); // 50% full
            tank_half.FluidState = new FluidState( 101325, 300, 0 );

            var tank_nearFull = new FlowTank( 1.0 );
            tank_nearFull.Contents.Add( TestSubstances.Water, 999 ); // 99.9% full
            tank_nearFull.FluidState = new FluidState( 101325, 300, 0 );

            double dPdM_half = (tank_half as IStiffnessProvider).GetPotentialDerivativeWrtVolume();
            double dPdM_nearFull = (tank_nearFull as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            Assert.That( dPdM_half, Is.Positive );
            Assert.That( dPdM_nearFull, Is.Positive );
            Assert.That( dPdM_nearFull, Is.GreaterThan( dPdM_half * 1000 ), "Stiffness should increase exponentially as the tank fills with liquid." );
        }

        #endregion

        #region Edge Case Tests

        [Test, Description( "Verifies that overfilling a tank results in very high pressure but does not crash the simulation." )]
        public void Overfill_DoesNotCrash_AndClampsToVolume()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Add( TestSubstances.Water, 2000 ); // 2.0m^3 in a 1.0m^3 tank
            _tank.ForceRecalculateCache();

            var state = _tank.Sample( new Vector3( 0, -0.5f, 0 ), 0.1 );
            Assert.That( state.Pressure, Is.GreaterThan( 1e8 ), "Overfilled tank should have very high pressure." );
        }

        [Test, Description( "Verifies that sampling an empty tank returns a state with zero pressure and a potential equal to the geometric potential at that point." )]
        public void Sample_OnEmptyTank_ReturnsGeometricPotential()
        {
            SetupUnitCubeTank( _tank );
            _tank.FluidAcceleration = new Vector3( 0, -10, 0 );
            _tank.Contents.Clear();
            _tank.ForceRecalculateCache();

            var pt = new Vector3( 0, 0.25f, 0 );
            var state = _tank.Sample( pt, 0.1 );

            Assert.That( state.Pressure, Is.EqualTo( 0.0 ).Within( 1e-6 ) );
            double expectedPotential = _tank.GetPotentialAt( pt );
            Assert.That( state.FluidSurfacePotential, Is.EqualTo( expectedPotential ).Within( 1e-5 ) );
        }

        #endregion
    }
}
