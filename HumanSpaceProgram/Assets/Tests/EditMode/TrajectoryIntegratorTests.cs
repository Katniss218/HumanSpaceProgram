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
        [TestCase( 1.989e30, 150_000_000_000.0, 100000 )] // sun
        [TestCase( 5.97e24, 6_371_000.0 + 300_000, 100000 )] // earth
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

            double radiusFinal = Math.Sqrt( finalState.AbsolutePosition.sqrMagnitude );
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

            double radiusFinal = Math.Sqrt( finalState.AbsolutePosition.sqrMagnitude );
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

            double radiusFinal = Math.Sqrt( finalState.AbsolutePosition.sqrMagnitude );
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
    }
}