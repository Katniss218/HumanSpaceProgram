using HSP;
using HSP.Trajectories;
using HSP.Trajectories.TrajectoryIntegrators;
using NUnit.Framework;
using System;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    class CentralAttractorProvider : ITrajectoryStepProvider
    {
        readonly int _parentBodyIndex;
        public CentralAttractorProvider( int parentBodyIndex ) => _parentBodyIndex = parentBodyIndex;

        public ITrajectoryStepProvider Clone( ITrajectoryTransform self, IReadonlyTrajectorySimulator simulator )
        {
            throw new NotImplementedException(); // not needed
        }

        public Vector3Dbl GetAcceleration( in TrajectorySimulationContext context )
        {
            if( _parentBodyIndex == -1 )
                return Vector3Dbl.zero; // no parent body, no acceleration.

            Vector3Dbl selfPos = context.Self.AbsolutePosition;
            var parent = context.GetAttractor( _parentBodyIndex );

            Vector3Dbl toParent = parent.AbsolutePosition - selfPos;
            double accelerationMagnitude = PhysicalConstants.G * (parent.Mass / toParent.sqrMagnitude);
            return toParent.normalized * accelerationMagnitude;
        }

        public double? GetMass( in TrajectorySimulationContext context ) => null;
    }

    public class TrajectoryIntegratorTests
    {
        TrajectoryStateVector CreateCircularOrbitState( double radius, double primaryMass )
        {
            double velocity = Math.Sqrt( PhysicalConstants.G * primaryMass / radius );
            return new TrajectoryStateVector(
                new Vector3Dbl( radius, 0.0, 0.0 ),
                new Vector3Dbl( 0.0, velocity, 0.0 ),
                new Vector3Dbl( 0.0, 0.0, 0.0 ),
                1 );
        }

        TrajectoryStateVector RunIntegratorSteps( TrajectoryStateVector centralState, TrajectoryStateVector initialState, ITrajectoryIntegrator integrator, ITrajectoryStepProvider[] providers, double duration, double step, double maxStep = -1, bool adapt = false )
        {
            var currentStates = new TrajectoryStateVector[2] { centralState, initialState };
            var nextStates = new TrajectoryStateVector[2] { centralState, initialState };
            double ut = 0;
            while( duration - ut > 1e-4 )
            {
                if( maxStep != -1 && step > maxStep )
                {
                    step = maxStep;
                }

                if( step > (duration - ut) )
                {
                    step = duration - ut; // don't overshoot the end time.
                }

                double newstep = integrator.Step( new TrajectorySimulationContext( ut, step, currentStates[1], -1, currentStates ), providers, out nextStates[1] );
                if( adapt )
                    step = newstep;

                var temp = currentStates;
                currentStates = nextStates;
                nextStates = temp;
                ut += step;
            }

            return currentStates[1];
        }

        TrajectoryStateVector[] RunIntegratorSteps( TrajectoryStateVector centralState, TrajectoryStateVector initialState, ITrajectoryIntegrator integrator, ITrajectoryStepProvider[] providers, double duration, double step,
            double[] sampleTimes, Ephemeris2 ephemeris )
        {
            var currentStates = new TrajectoryStateVector[2] { centralState, initialState };
            var nextStates = new TrajectoryStateVector[2] { centralState, initialState };
            double ut = 0;

            var samplePoints = new TrajectoryStateVector[sampleTimes.Length];
            int sampleIndex = 0; // ordered samples

            ephemeris.InsertAdaptive( ut, currentStates[1] );
            while( duration - ut > 1e-4 )
            {
                if( step > (duration - ut) )
                {
                    Debug.Log( "end" );
                    step = duration - ut; // don't overshoot the end time.
                }

                integrator.Step( new TrajectorySimulationContext( ut, step, currentStates[1], -1, currentStates ), providers, out nextStates[1] );
                ut += step;

                const double tolerance = 1e-10;
                while( sampleIndex < sampleTimes.Length && ut >= sampleTimes[sampleIndex] - tolerance )
                {
                    samplePoints[sampleIndex] = VectorInterpolationUtils.CubicHermite(
                        new Ephemeris2.Sample( ut - step, currentStates[1], false, Ephemeris2.SampleType.Continuous ),
                        new Ephemeris2.Sample( ut, nextStates[1], false, Ephemeris2.SampleType.Continuous ),
                        sampleTimes[sampleIndex] );
                    sampleIndex++;
                }
                ephemeris.InsertAdaptive( ut, nextStates[1] );

                var temp = currentStates;
                currentStates = nextStates;
                nextStates = temp;
            }

            return samplePoints;
        }

        double TotalEnergy( in TrajectoryStateVector sat, in TrajectoryStateVector central )
        {
            double m = sat.Mass;
            var rVec = sat.AbsolutePosition - central.AbsolutePosition;
            double r = Math.Sqrt( rVec.sqrMagnitude );
            double ke = 0.5 * m * (sat.AbsoluteVelocity.sqrMagnitude);
            double pe = -PhysicalConstants.G * (m * central.Mass) / r;
            return ke + pe;
        }

        [Test]
        [TestCase( 1.989e30, 150_000_000_000.0, 1000000 )] // sun
        [TestCase( 5.97e24, 6_371_000.0 + 300_000, 1000000 )] // earth
        public void Euler_Circular( double centralMass, double radius, double stepsPerOrbit )
        {
            // Arrange
            ITrajectoryIntegrator integrator = new EulerIntegrator();

            TrajectoryStateVector initialState = CreateCircularOrbitState( radius, centralMass );

            TrajectoryStateVector centralBody = new TrajectoryStateVector( Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, centralMass );

            double period = 2.0 * Math.PI * Math.Sqrt( radius * radius * radius / (PhysicalConstants.G * centralMass) );
            double dt = period / stepsPerOrbit;

            var providers = new ITrajectoryStepProvider[] { new CentralAttractorProvider( 0 ) };

            // Act
            TrajectoryStateVector finalState = RunIntegratorSteps( centralBody, initialState, integrator, providers, period, dt );

            double radiusFinal = finalState.AbsolutePosition.magnitude;
            double error = Math.Abs( radiusFinal - radius );

            double e0 = TotalEnergy( initialState, centralBody );
            double e1 = TotalEnergy( finalState, centralBody );
            double absEnergyDrift = Math.Abs( e1 - e0 );
            double relEnergyDrift = Math.Abs( (e1 - e0) / e0 );

            Debug.Log( integrator.GetType().Name + " error [m]: " + error );
            Debug.Log( integrator.GetType().Name + " error [N]: " + absEnergyDrift );
            // Assert
            Assert.That( error / radius, Is.LessThan( 1e-12 ) );
            Assert.That( relEnergyDrift, Is.LessThan( 1e-12 ) );
        }

        [Test]
        [TestCase( 1.989e30, 150_000_000_000.0, 2000 )] // sun
        [TestCase( 5.97e24, 6_371_000.0 + 300_000, 2000 )] // earth
        public void RK4_Circular( double centralMass, double radius, double stepsPerOrbit )
        {
            // Arrange
            ITrajectoryIntegrator integrator = new RK4Integrator();

            TrajectoryStateVector initialState = CreateCircularOrbitState( radius, centralMass );

            TrajectoryStateVector centralBody = new TrajectoryStateVector( Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, centralMass );

            double period = 2.0 * Math.PI * Math.Sqrt( radius * radius * radius / (PhysicalConstants.G * centralMass) );
            double dt = period / stepsPerOrbit;

            var providers = new ITrajectoryStepProvider[] { new CentralAttractorProvider( 0 ) };

            // Act
            TrajectoryStateVector finalState = RunIntegratorSteps( centralBody, initialState, integrator, providers, period, dt );

            double radiusFinal = finalState.AbsolutePosition.magnitude;
            double error = Math.Abs( radiusFinal - radius );

            double e0 = TotalEnergy( initialState, centralBody );
            double e1 = TotalEnergy( finalState, centralBody );
            double absEnergyDrift = Math.Abs( e1 - e0 );
            double relEnergyDrift = Math.Abs( (e1 - e0) / e0 );

            Debug.Log( integrator.GetType().Name + " error [m]: " + error );
            Debug.Log( integrator.GetType().Name + " error [N]: " + absEnergyDrift );
            // Assert
            Assert.That( error / radius, Is.LessThan( 1e-12 ) );
            Assert.That( relEnergyDrift, Is.LessThan( 1e-12 ) );
        }

        [Test]
        [TestCase( 1.989e30, 150_000_000_000.0, 1000 )] // sun
        [TestCase( 5.97e24, 6_371_000.0 + 300_000, 1000 )] // earth
        public void Verlet_Circular( double centralMass, double radius, double stepsPerOrbit )
        {
            // Arrange
            ITrajectoryIntegrator integrator = new VerletIntegrator();

            TrajectoryStateVector initialState = CreateCircularOrbitState( radius, centralMass );

            TrajectoryStateVector centralBody = new TrajectoryStateVector( Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, centralMass );

            double period = 2.0 * Math.PI * Math.Sqrt( radius * radius * radius / (PhysicalConstants.G * centralMass) );
            double dt = period / stepsPerOrbit;

            var providers = new ITrajectoryStepProvider[] { new CentralAttractorProvider( 0 ) };

            // Act
            TrajectoryStateVector finalState = RunIntegratorSteps( centralBody, initialState, integrator, providers, period, dt );

            double radiusFinal = finalState.AbsolutePosition.magnitude;
            double error = Math.Abs( radiusFinal - radius );

            double e0 = TotalEnergy( initialState, centralBody );
            double e1 = TotalEnergy( finalState, centralBody );
            double absEnergyDrift = Math.Abs( e1 - e0 );
            double relEnergyDrift = Math.Abs( (e1 - e0) / e0 );

            Debug.Log( integrator.GetType().Name + " error [m]: " + error );
            Debug.Log( integrator.GetType().Name + " error [N]: " + absEnergyDrift );
            // Assert
            Assert.That( error / radius, Is.LessThan( 1e-12 ) );
            Assert.That( relEnergyDrift, Is.LessThan( 1e-12 ) );
        }

        [Test]
        [TestCase( 1.989e30, 150_000_000_000.0, 500 )] // sun
        [TestCase( 5.97e24, 6_371_000.0 + 300_000, 500 )] // earth
        public void Yoshida4_Circular( double centralMass, double radius, double stepsPerOrbit )
        {
            // Arrange
            ITrajectoryIntegrator integrator = new Yoshida4Integrator();

            TrajectoryStateVector initialState = CreateCircularOrbitState( radius, centralMass );

            TrajectoryStateVector centralBody = new TrajectoryStateVector( Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, centralMass );

            double period = 2.0 * Math.PI * Math.Sqrt( radius * radius * radius / (PhysicalConstants.G * centralMass) );
            double dt = period / stepsPerOrbit;

            var providers = new ITrajectoryStepProvider[] { new CentralAttractorProvider( 0 ) };

            // Act
            TrajectoryStateVector finalState = RunIntegratorSteps( centralBody, initialState, integrator, providers, period, dt );

            double radiusFinal = finalState.AbsolutePosition.magnitude;
            double error = Math.Abs( radiusFinal - radius );

            double e0 = TotalEnergy( initialState, centralBody );
            double e1 = TotalEnergy( finalState, centralBody );
            double absEnergyDrift = Math.Abs( e1 - e0 );
            double relEnergyDrift = Math.Abs( (e1 - e0) / e0 );

            Debug.Log( integrator.GetType().Name + " error [m]: " + error );
            Debug.Log( integrator.GetType().Name + " error [N]: " + absEnergyDrift );
            // Assert
            Assert.That( error / radius, Is.LessThan( 1e-12 ) );
            Assert.That( relEnergyDrift, Is.LessThan( 1e-12 ) );
        }

        [Test]
        [TestCase( 1.989e30, 150_000_000_000.0, 511 )] // sun
        [TestCase( 1.989e30, 150_000_000_000_000.0, 511 )] // bigsun
        [TestCase( 5.97e24, 6_371_000.0 + 300_000, 555 )] // earth
        public void Yoshida4_CompareToEphemeris( double centralMass, double radius, double stepsPerOrbit )
        {
            // Arrange
            ITrajectoryIntegrator integrator = new Yoshida4Integrator();

            TrajectoryStateVector initialState = CreateCircularOrbitState( radius, centralMass );

            TrajectoryStateVector centralBody = new TrajectoryStateVector( Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, centralMass );

            double period = 2.0 * Math.PI * Math.Sqrt( radius * radius * radius / (PhysicalConstants.G * centralMass) );
            double dt = period / stepsPerOrbit;
            
            double[] sampleTimes = new[]
            {
                // boundaries
                0.0, 1.0,
                double.Epsilon, 1.0 - double.Epsilon,

                // small values
                1e-12, 1e-9, 1e-6, 1e-3, 0.0001, 0.00123,

                // simple fractions
                0.25, 0.5, 0.75,
                1.0/3.0, 2.0/3.0,
                1.0/7.0, 2.0/7.0, 3.0/7.0, 6.0/7.0,

                // round decimals
                0.1, 0.2, 0.3, 0.4, 0.6, 0.7, 0.8, 0.9,
                0.01, 0.02, 0.05, 0.95, 0.99,

                // weird precise decimals
                0.12341, 0.123456, 0.123456789,
                0.34523, 0.87654321, 0.987654321,
                0.2718281828, // e - 2
                0.314159265,  // pi / 10
                0.707106781,  // sqrt(2)/2
                0.5772156649, // Euler-Mascheroni
                0.6180339887, // golden ratio φ - 1
                0.6931471806, // ln(2)
                0.3010299957, // log10(2)

                // near-one values
                0.999, 0.9999, 0.99999, 0.999999,
                0.9999999, 0.99999999, 0.999999999
            };
            double[] sampleTimes2 = new double[sampleTimes.Length];
            Array.Sort( sampleTimes );
            Array.Copy( sampleTimes, sampleTimes2, sampleTimes.Length );
            for( int j = 0; j < sampleTimes.Length; j++ )
                sampleTimes[j] *= period;

            Ephemeris2 ephemeris = new Ephemeris2( 1000, 0.01 );

            var providers = new ITrajectoryStepProvider[] { new CentralAttractorProvider( 0 ) };

            // Act
            TrajectoryStateVector[] samplePoints = RunIntegratorSteps( centralBody, initialState, integrator, providers, period, dt, sampleTimes, ephemeris );

            int i = 0;
            foreach( var finalState in samplePoints )
            {
                var eval = ephemeris.Evaluate( sampleTimes[i] );
                double error = Math.Abs(( eval.AbsolutePosition - finalState.AbsolutePosition).magnitude);

                double e0 = TotalEnergy( initialState, centralBody );
                double e1 = TotalEnergy( finalState, centralBody );
                double absEnergyDrift = Math.Abs( e1 - e0 );
                double relEnergyDrift = Math.Abs( (e1 - e0) / e0 );

                Debug.Log( sampleTimes[i] + " : " + integrator.GetType().Name + " error [m]: " + error );
                Debug.Log( sampleTimes[i] + " : " + integrator.GetType().Name + " error [N]: " + absEnergyDrift );
                // Assert
                Assert.That( error / radius, Is.LessThan( 1e-3 ) );
                Assert.That( relEnergyDrift, Is.LessThan( 1e-3 ) );
                i++;
            }
        }
    }
}