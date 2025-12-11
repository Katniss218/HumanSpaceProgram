using HSP.ResourceFlow;
using NUnit.Framework;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class StiffnessProviderTests
    {
        [Test]
        public void FlowTank_GetPotentialDerivativeWrtVolume_GasOnly_IsCorrect()
        {
            // Arrange
            var tank = new FlowTank( 1.0 ); // 1 m^3
            tank.Contents.Add( TestSubstances.Air, 1.2 ); // approx 1 atm at 300K
            tank.FluidState = new FluidState( 0, 300, 0 );
            tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( tank.Contents, tank.FluidState, tank.Volume ), 300, 0 );

            // Act
            double dPdM = (tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            // Assert
            // dP/dM for ideal gas is R_specific * T / V
            double expected_dPdM = TestSubstances.Air.SpecificGasConstant * 300 / 1.0;
            Assert.That( dPdM, Is.EqualTo( expected_dPdM ).Within( 1e-6 ) );
        }

        [Test]
        public void FlowTank_GetPotentialDerivativeWrtVolume_OverfilledLiquid_IsCorrect()
        {
            // Arrange
            var tank = new FlowTank( 1.0 ); // 1 m^3
            double density = TestSubstances.Water.GetDensity( 300, 101325 );
            tank.Contents.Add( TestSubstances.Water, density * 1.1 ); // 10% overfill
            tank.FluidState = new FluidState( 0, 300, 0 );
            tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( tank.Contents, tank.FluidState, tank.Volume ), 300, 0 );

            // Act
            double dPdM = (tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            // Assert
            // dP/dM for liquid is K / (rho_0 * V)
            double expected_dPdM = TestSubstances.Water.BulkModulus / (TestSubstances.Water.ReferenceDensity * 1.0);
            Assert.That( dPdM, Is.EqualTo( expected_dPdM ).Within( 1e-6 ) );
        }

        [Test]
        public void FlowTank_GetPotentialDerivativeWrtVolume_Empty_IsZero()
        {
            // Arrange
            var tank = new FlowTank( 1.0 );
            tank.FluidState = new FluidState( 0, 300, 0 );

            // Act
            double dPdM = (tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            // Assert
            Assert.That( dPdM, Is.EqualTo( 0.0 ) );
        }

        [Test]
        public void FlowTank_GetPotentialDerivativeWrtVolume_MixedPhase_IsLow()
        {
            // Arrange
            var tank = new FlowTank( 1.0 ); // 1 m^3
            // Half full of liquid, half ullage for gas.
            tank.Contents.Add( TestSubstances.Water, 500 );
            tank.Contents.Add( TestSubstances.Air, 0.6 );
            tank.FluidState = new FluidState( 0, 300, 0 );
            tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( tank.Contents, tank.FluidState, tank.Volume ), 300, 0 );

            // Act
            double dPdM_mixed = (tank as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            // Assert
            // The derivative should be dominated by the compressible gas.
            // dP/dM_gas = ( (w_gas/MM_gas) * R * T * V_tank ) / ( V_ullage^2 )
            double totalMass = 500.6;
            double w_gas = 0.6 / totalMass;
            double invMM_gas = 1.0 / TestSubstances.Air.MolarMass;
            double B = w_gas * invMM_gas;
            double R = 8.31446;
            double T = 300;
            double V_tank = 1.0;
            double V_ullage = 0.5;
            double expected_dPdM = (B * R * T * V_tank) / (V_ullage * V_ullage);

            Assert.That( dPdM_mixed, Is.EqualTo( expected_dPdM ).Within( 1.0 ).Percent );
            Assert.That( dPdM_mixed, Is.LessThan( 1e6 ), "Stiffness should be low due to gas ullage." );
        }

        [Test]
        public void FlowTank_GetPotentialDerivativeWrtVolume_StiffnessIncreasesNearFull()
        {
            // Arrange
            var tank_half = new FlowTank( 1.0 );
            tank_half.Contents.Add( TestSubstances.Water, 500 ); // 50% full
            tank_half.FluidState = new FluidState( 101325, 300, 0 );

            var tank_nearFull = new FlowTank( 1.0 );
            tank_nearFull.Contents.Add( TestSubstances.Water, 999 ); // 99.9% full
            tank_nearFull.FluidState = new FluidState( 101325, 300, 0 );

            // Act
            double dPdM_half = (tank_half as IStiffnessProvider).GetPotentialDerivativeWrtVolume();
            double dPdM_nearFull = (tank_nearFull as IStiffnessProvider).GetPotentialDerivativeWrtVolume();

            // Assert
            // A half-full tank with no gas is treated as having vacuum ullage, so pressure is low.
            // A nearly-full tank is nearly incompressible.
            Assert.That( dPdM_half, Is.Positive );
            Assert.That( dPdM_nearFull, Is.Positive );
            Assert.That( dPdM_nearFull, Is.GreaterThan( dPdM_half * 1000 ), "Stiffness should increase exponentially as the tank fills with liquid." );
        }
    }
}