using HSP.Trajectories;
using HSP.Vanilla.Trajectories;
using HSP_Tests_EditMode.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class KeplerianOrbitTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        static IEqualityComparer<Vector3Dbl> posApproxComparer = new Vector3DblApproximateComparer( 0.1 );
        static IEqualityComparer<Vector3Dbl> posLowResApproxComparer = new Vector3DblApproximateComparer( 5 );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );

        [Test]
        public void CalculateTrueAnomaly___Circular___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;

            // Act
            double trueAnomaly = sut.TrueAnomaly;

            // Assert
            Assert.That( sut.MeanAnomaly, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( trueAnomaly, Is.EqualTo( 0 ).Within( 0.00001 ) );
        }

        [TestCase( 1 )]
        [TestCase( 100 )]
        [TestCase( 100000 )]
        [Test]
        public void CalculateTrueAnomaly___Circular___IsCorrect_AfterStepping( int stepCount )
        {
            // Arrange
            const double time = 31_556_926 / 4;
            double step = time / stepCount;

            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 1.989e30 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 149_600_000_000, 0, 0, 0, 0, 0, 5.97e24 );
            sut.ParentBody = parent;

            // Act
            for( int i = 0; i < stepCount; i++ )
            {
                sut.Step( new[] { parent.GetCurrentState() }, step );
            }
            double trueAnomaly = sut.TrueAnomaly;

            // Assert
            Assert.That( sut.MeanAnomaly, Is.EqualTo( 1.57079632679 ).Within( 0.0025 ) ); // 90 deg
            Assert.That( trueAnomaly, Is.EqualTo( 1.57079632679 ).Within( 0.0025 ) );
        }

        [Test]
        public void GetStateVector___Circular___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 1_000_000, 0, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( 0, 19961.355414901063, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( 398.45571, 0, 0 ) ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void GetStateVector___Inclined___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 1.57079632679, 0, 0, 0, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 1_000_000, 0, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( 0, 0, 19961.36 ) ).Using( posApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( 398.45571, 0, 0 ) ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void GetStateVector___Eccentric___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0.5, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 500_000, 0, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( 0, 34574.08, 0 ) ).Using( posApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( 1593.82, 0, 0 ) ).Using( posApproxComparer ) );
        }

        [Test]
        public void GetStateVector___90Degrees___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 0, 0, 0, 1.57079632679, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 0, 1_000_000, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( -19961.355414901063, 0, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( 0, 398.45571, 0 ) ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void GetStateVector___90Degrees_Inclined___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 1.57079632679, 0, 0, 1.57079632679, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 0, 0, 1_000_000 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( -19961.355414901063, 0, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( 0, 0, 398.45571 ) ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void GetStateVector___70Degrees___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 0, 0, 0, 1.2217304764, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 342020.15, 939692.63, 0 ) ).Using( posApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( -18757.54, 6827.19, 0 ) ).Using( posApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( 136.28, 374.43, 0 ) ).Using( posApproxComparer ) );
        }

        [Test]
        public void GetStateVector___70Degrees_Inclined___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 1.57079632679, 0, 0, 1.2217304764, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 342020.15, 0, 939692.63 ) ).Using( posApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( -18757.54, 0, 6827.19 ) ).Using( posApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( 136.28, 0.00, 374.43 ) ).Using( posApproxComparer ) );
        }

        [Test]
        public void GetStateVector___Complicated___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0.5, 0.523598775598, 0.698131700798, 0.872664625997, 1.0471975512, 1 );
            sut.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( -849274.84, -495934.29, 95837.62 ) ).Using( posApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( -1724.35, -18632.69, -7600.85 ) ).Using( new Vector3DblApproximateComparer( 3 ) ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( -350.74, -204.81, 39.58 ) ).Using( posApproxComparer ) );
        }

        [Test]
        public void FromStateVector___Circular___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;

            TrajectoryBodyState state = new TrajectoryBodyState(
                new Vector3Dbl( 1_000_000, 0, 0 ),
                new Vector3Dbl( 0, 19961.355414901063, 0 ),
                new Vector3Dbl( 398.45571, 0, 0 ),
                1 );

            sut.SetCurrentState( state );
            var x = sut.GetCurrentState();

            Assert.That( sut.SemiMajorAxis, Is.EqualTo( 1_000_000 ).Within( 0.00001 ) );
            Assert.That( sut.Eccentricity, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.Inclination, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.LongitudeOfAscendingNode, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.ArgumentOfPeriapsis, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.MeanAnomaly, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.TrueAnomaly, Is.EqualTo( 0 ).Within( 0.00001 ) );
        }

        [Test]
        public void RoundTrip___Circular___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;
            KeplerianOrbit sut2 = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 1 );
            sut2.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();
            sut2.SetCurrentState( state );

            Assert.That( sut2.SemiMajorAxis, Is.EqualTo( sut.SemiMajorAxis ).Within( 0.00000001 ) );
            Assert.That( sut2.Eccentricity, Is.EqualTo( sut.Eccentricity ).Within( 0.00000001 ) );
            Assert.That( sut2.Inclination, Is.EqualTo( sut.Inclination ).Within( 0.00000001 ) );
            Assert.That( sut2.LongitudeOfAscendingNode, Is.EqualTo( sut.LongitudeOfAscendingNode ).Within( 0.00000001 ) );
            Assert.That( sut2.ArgumentOfPeriapsis, Is.EqualTo( sut.ArgumentOfPeriapsis ).Within( 0.00000001 ) );
            Assert.That( sut2.MeanAnomaly, Is.EqualTo( sut.MeanAnomaly ).Within( 0.00000001 ) );
            Assert.That( sut2.TrueAnomaly, Is.EqualTo( sut.TrueAnomaly ).Within( 0.00000001 ) );
        }

        [TestCase( 1_500_000, 0, -0.152359877559, 0.198131700798, 1.872664625997, -2.0471975512 )]
        [TestCase( 1_500_000, 0.1, -0.152359877559, 0.198131700798, 1.872664625997, -2.0471975512 )]
        [TestCase( 1_000_000, 0.5, 0.523598775598, 0.698131700798, 0.872664625997, 1.0471975512 )]
        [TestCase( 1_500_000, 0.8, 1.152359877559, 0.198131700798, -1.872664625997, 0.0471975512 )]
        [TestCase( 1_500_000, 0.95, 1.152359877559, 0.198131700798, -1.872664625997, -0.0471975512 )]
        [Test]
        public void RoundTrip___Complicated___IsCorrect( double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double initialMeanAnomaly )
        {
#warning todo - eccentricity being 0 is fucked up, rtest seems to work, at least for a time
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, initialMeanAnomaly, 1 );
            sut.ParentBody = parent;
            KeplerianOrbit sut2 = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 1 );
            sut2.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();
            sut2.SetCurrentState( state );

            Assert.That( sut2.SemiMajorAxis, Is.EqualTo( sut.SemiMajorAxis ).Within( 0.0000001 ) );
            Assert.That( sut2.Eccentricity, Is.EqualTo( sut.Eccentricity ).Within( 0.0000001 ) );
            Assert.That( sut2.Inclination, Is.EqualTo( sut.Inclination ).Within( 0.0000001 ) );
            Assert.That( sut2.LongitudeOfAscendingNode, Is.EqualTo( sut.LongitudeOfAscendingNode ).Within( 0.0000001 ) );
            Assert.That( sut2.TrueAnomaly, Is.EqualTo( sut.TrueAnomaly ).Within( 0.0000001 ) );
            Assert.That( sut2.MeanAnomaly, Is.EqualTo( sut.MeanAnomaly ).Within( 0.0000001 ) );
            Assert.That( sut2.ArgumentOfPeriapsis, Is.EqualTo( sut.ArgumentOfPeriapsis ).Within( 0.0000001 ) );
        }

        [TestCase( 1_000_000, 0.9999999, Math.PI, Math.PI, Math.PI, Math.PI )] // large eccentricity
        [TestCase( 1_000_000_000_000, 0.5, 0.872664625997, 0.698131700798, 0.523598775598, 1.0471975512 )]
        [TestCase( 150_000_000_000, 0.0, 1.152359877559, 0.198131700798, -1.872664625997, 0.00000001 )]
        [TestCase( 150_000_000_000, 0.0001, 1.152359877559, 0.198131700798, -1.872664625997, 0.00000001 )]
        [Test]
        public void RoundTrip___ComplicatedBig___IsCorrect( double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double initialMeanAnomaly )
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e30 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000_000_000, 0.5, 0.872664625997, 0.698131700798, 0.523598775598, 1.0471975512, 1e24 );
            sut.ParentBody = parent;
            KeplerianOrbit sut2 = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 1 );
            sut2.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();
            sut2.SetCurrentState( state );

            // Assert SemiMajorAxis of large orbit with lower precision because the orbit itself is so big that we'll start losing precision anyway.
            Assert.That( sut2.SemiMajorAxis, Is.EqualTo( sut.SemiMajorAxis ).Within( 0.001 ) );
            Assert.That( sut2.Eccentricity, Is.EqualTo( sut.Eccentricity ).Within( 0.00000001 ) );
            Assert.That( sut2.Inclination, Is.EqualTo( sut.Inclination ).Within( 0.00000001 ) );
            Assert.That( sut2.LongitudeOfAscendingNode, Is.EqualTo( sut.LongitudeOfAscendingNode ).Within( 0.00000001 ) );
            Assert.That( sut2.ArgumentOfPeriapsis, Is.EqualTo( sut.ArgumentOfPeriapsis ).Within( 0.00000001 ) );
            Assert.That( sut2.MeanAnomaly, Is.EqualTo( sut.MeanAnomaly ).Within( 0.00000001 ) );
            Assert.That( sut2.TrueAnomaly, Is.EqualTo( sut.TrueAnomaly ).Within( 0.00000001 ) );
        }

#warning TODO - hyperbolic (escaping) orbits.
        [Test]
        public void RoundTrip___Hyperbolic___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, -1_000_000, 1.5, 0.872664625997, 0.698131700798, 0.523598775598, 1.0471975512, 1 );
            sut.ParentBody = parent;
            KeplerianOrbit sut2 = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 1 );
            sut2.ParentBody = parent;

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();
            sut2.SetCurrentState( state );

            Assert.That( sut2.SemiMajorAxis, Is.EqualTo( sut.SemiMajorAxis ).Within( 0.00000001 ) );
            Assert.That( sut2.Eccentricity, Is.EqualTo( sut.Eccentricity ).Within( 0.00000001 ) );
            Assert.That( sut2.Inclination, Is.EqualTo( sut.Inclination ).Within( 0.00000001 ) );
            Assert.That( sut2.LongitudeOfAscendingNode, Is.EqualTo( sut.LongitudeOfAscendingNode ).Within( 0.00000001 ) );
            Assert.That( sut2.ArgumentOfPeriapsis, Is.EqualTo( sut.ArgumentOfPeriapsis ).Within( 0.00000001 ) );
            Assert.That( sut2.MeanAnomaly, Is.EqualTo( sut.MeanAnomaly ).Within( 0.00000001 ) );
            Assert.That( sut2.TrueAnomaly, Is.EqualTo( sut.TrueAnomaly ).Within( 0.00000001 ) );
        }

        [Test]
        public void RoundTrip___RadialVelOnly___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;
            TrajectoryBodyState origState = new TrajectoryBodyState( new Vector3Dbl( -1000000, 0, 0 ), new Vector3Dbl( 100, 0, 0 ), Vector3Dbl.zero, 1 );
            sut.SetCurrentState( origState );

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            Assert.That( state.AbsolutePosition, Is.EqualTo( origState.AbsolutePosition ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( origState.AbsoluteVelocity ).Using( vector3DblApproxComparer ) );
            Assert.That( state.Mass, Is.EqualTo( origState.Mass ) );
        }

        [Test]
        public void RoundTrip___0Vel___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;
            TrajectoryBodyState origState = new TrajectoryBodyState( new Vector3Dbl( -1000000, 0, 0 ), Vector3Dbl.zero, Vector3Dbl.zero, 1 );
            sut.SetCurrentState( origState );

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            Assert.That( state.AbsolutePosition, Is.EqualTo( origState.AbsolutePosition ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( origState.AbsoluteVelocity ).Using( vector3DblApproxComparer ) );
            Assert.That( state.Mass, Is.EqualTo( origState.Mass ) );
        }

        [TestCase( 150_000_000_000, 0, 0, 0, 29750, 0 )]
        [TestCase( 150_000_000_000, 1_000_000, 0, 50, 29750, 0 )]
        [TestCase( 150_000_000_000, 1_000_000, 0, -50, 29750, 0 )]
        [TestCase( 150_000_000_000, -1_000_000, 0, 50, 29750, 0 )]
        [TestCase( 150_000_000_000, -1_000_000, 0, -50, 29750, 0 )]
        [TestCase( 150_000_000_000, 0, 0, 0, -29750, 0 )]
        [TestCase( 150_000_000_000, 1_000_000, 0, 50, -29750, 0 )]
        [TestCase( 150_000_000_000, 1_000_000, 0, -50, -29750, 0 )]
        [TestCase( 150_000_000_000, -1_000_000, 0, 50, -29750, 0 )]
        [TestCase( 150_000_000_000, -1_000_000, 0, -50, -29750, 0 )]
        [TestCase( 150_000_000_000, 0, 0, 50, -29750, 0 )]
        [TestCase( 150_000_000_000, 0, 0, -50, -29750, 0 )]
        [Test]
        public void RoundTrip___EarthWeirdCase___IsCorrect( double rx, double ry, double rz, double vx, double vy, double vz )
        {
            // This test is here because I identified that these parameters failed in the past (for some reason).
            // We'll keep it here just in case.

            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 1.989e30 );
            KeplerianOrbit target = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 5.97e24 );
            target.ParentBody = parent;
            TrajectoryBodyState source = new TrajectoryBodyState( new Vector3Dbl( rx, ry, rz ), new Vector3Dbl( vx, vy, vz ), Vector3Dbl.zero, 5.97e24 );
            target.SetCurrentState( source );

            // Act
            TrajectoryBodyState targetState = target.GetCurrentState();

            Assert.That( targetState.AbsoluteVelocity, Is.EqualTo( source.AbsoluteVelocity ).Using( vector3DblApproxComparer ) );
            Assert.That( targetState.AbsolutePosition, Is.EqualTo( source.AbsolutePosition ).Using( posLowResApproxComparer ) );
            Assert.That( targetState.Mass, Is.EqualTo( source.Mass ) );
        }

        [Test]
        public void RoundTrip___EarthWeirdCase_RoundTrip___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 1.989e30 );
            KeplerianOrbit source = new KeplerianOrbit( 0, null, 150000000000, 1.40217974709181E-17, 0, 0, 1.5707963267949, 4.71238898831779, 5.97e24 );
            source.ParentBody = parent;
            KeplerianOrbit target = new KeplerianOrbit( 0, null, 0, 0, 0, 0, 0, 0, 1 ); // dummy constructor
            target.ParentBody = parent;

            // Act
            TrajectoryBodyState state = source.GetCurrentState();
            target.SetCurrentState( state );

            // Assert SemiMajorAxis of large orbit with lower precision because the orbit itself is so big that we'll start losing precision anyway.
            Assert.That( target.SemiMajorAxis, Is.EqualTo( source.SemiMajorAxis ).Within( 0.001 ) );
            Assert.That( target.Eccentricity, Is.EqualTo( source.Eccentricity ).Within( 0.00000001 ) );
            Assert.That( target.Inclination, Is.EqualTo( source.Inclination ).Within( 0.00000001 ) );
            Assert.That( target.LongitudeOfAscendingNode, Is.EqualTo( source.LongitudeOfAscendingNode ).Within( 0.00000001 ) );
            Assert.That( target.ArgumentOfPeriapsis, Is.EqualTo( source.ArgumentOfPeriapsis ).Within( 0.00000001 ) );
            Assert.That( target.MeanAnomaly, Is.EqualTo( source.MeanAnomaly ).Within( 0.00000001 ) );
            Assert.That( target.TrueAnomaly, Is.EqualTo( source.TrueAnomaly ).Within( 0.00000001 ) );
        }

        [TestCase( 150_000_000_000, 0, 0, 0, 29749.1543788567, 0 )]
        [TestCase( 150_000_000_000, 0, 0, 0, -29749.1543788567, 0 )]
        [TestCase( 150_000_000_000, -500, 0, 20000, 10000, 50 )]
        [TestCase( 150_000_000_000, 0, 500, -5000, -20000, -500 )]
        [Test]
        public void KeplerianFollowsNewtonian( double rx, double ry, double rz, double vx, double vy, double vz )
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 1.989e30 );
            TrajectoryBodyState state = new TrajectoryBodyState( new Vector3Dbl( rx, ry, rz ), new Vector3Dbl( vx, vy, vz ), Vector3Dbl.zero, 5.97e24 );
            KeplerianOrbit kepler = new KeplerianOrbit( 0, null, 1, 0, 0, 0, 0, 0, 1 );
            kepler.ParentBody = parent;
            NewtonianOrbit newton = new NewtonianOrbit( 0, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, 1 );

            kepler.SetCurrentState( state );
            newton.SetCurrentState( state );

            TrajectoryBodyState[] immovingSun = new TrajectoryBodyState[] { parent.GetCurrentState() };

            for( int i = 0; i < 1000; i++ )
            {
                const double step = 0.02;

                kepler.Step( immovingSun, step );
                newton.Step( immovingSun, step );

                var testStateKepler = kepler.GetCurrentState();
                var testStateNewton = newton.GetCurrentState();

                kepler.SetCurrentState( testStateKepler );
                newton.SetCurrentState( testStateNewton );

                Debug.Log( i + " : " + testStateKepler.AbsolutePosition + " : " + testStateNewton.AbsolutePosition );

                Assert.That( testStateKepler.AbsolutePosition, Is.EqualTo( testStateNewton.AbsolutePosition ).Using( posApproxComparer ) );
                Assert.That( testStateKepler.AbsoluteVelocity, Is.EqualTo( testStateNewton.AbsoluteVelocity ).Using( posApproxComparer ) );
                Assert.That( testStateKepler.AbsoluteAcceleration, Is.EqualTo( testStateNewton.AbsoluteAcceleration ).Using( posApproxComparer ) );
                Assert.That( testStateKepler.Mass, Is.EqualTo( testStateNewton.Mass ) );
            }
        }

        [TestCase( 150_000_000_000, 0, 0, 0, 0, 0 )]
        [TestCase( 150_000_000_000, 0, Math.PI, 0, 0, 0 )]
        [TestCase( 150_000_000_000, 0.1, 0, 0, 0, 0 )]
        [TestCase( 150_000_000_000, 0.01, Math.PI, 0, 0, 0 )]
        [Test]
        public void KeplerianStepWithBackfeeding( double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double initialMeanAnomaly )
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 1.989e30 );
            KeplerianOrbit kepler = new KeplerianOrbit( 0, null, semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, initialMeanAnomaly, 5.97e24 );
            kepler.ParentBody = parent;

            TrajectoryBodyState[] immovingSun = new TrajectoryBodyState[] { parent.GetCurrentState() };

            const double stepSize = 0.02 * 2500;

            for( int i = 0; i < 10000; i++ )
            {
                kepler.Step( immovingSun, stepSize );

                var testStateKepler = kepler.GetCurrentState();

                kepler.SetCurrentState( testStateKepler );

                Assert.That( kepler.SemiMajorAxis, Is.EqualTo( semiMajorAxis ).Within( 0.01 ) );
                Assert.That( kepler.Eccentricity, Is.EqualTo( eccentricity ).Within( 0.000001 ) );
                Assert.That( kepler.Inclination, Is.EqualTo( inclination ).Within( 0.000001 ) );
                Assert.That( kepler.LongitudeOfAscendingNode, Is.EqualTo( longitudeOfAscendingNode ).Within( 0.000001 ) );
                Assert.That( kepler.ArgumentOfPeriapsis, Is.EqualTo( argumentOfPeriapsis ).Within( 0.000001 ) );
            }
        }

        // we should convert to keplerian, then step feed back, and see if the replicated sequence is the same

        // before pi
        // 149999999991.503 : 1596934.59912459 : 0 :::::::: -0.31671613056214 : 29749.1548970355 : 0
        // output is pi
        // 149999999991.496 : 1597529.58220924 : 0 :::::::: -0.323486335355483 : 29748.1714346503 : 0

        // before pi
        // 149999999990.706 : 1670117.51857185 : 0 :::::::: -0.331230546942488 : 29749.1548971115 : 0
        // now two pi
        // 149999999990.699 : 1670712.50165649 : 0 :::::::: -0.334167487937975 : 29768.1656557121 : 0               -- velocity here is weird

        // two pi
        // 149999999930.454 : 4444201.32299392 : 0 :::::::: -0.954028346081486 : 29750.0851034101 : 0
        // 4.4
        // 149999999930.435 : 4444796.32468269 : 0 :::::::: -0.953674337722533 : 29749.135936172 : 0

        // E0.0190578779189829 : 1.89207801459168 : 0 : 1.89206609604709 : 0 : 149151449344.682
        //      149999999989.376 : 1787781.68553105 : 0 :::::::: 539.218019470941 : 29659.508245391 : 0
        // E0.584587717658743 : 2.20304172563509 : 3.14159265358979 : 2.20305364813426 : 0 : 149151449344.681
        //      150000000000.161 : 1788374.87568269 : 0 :::::::: 17341.1957428645 : -24067.8232399869 : 0
        // E0.597345441783282 : 2.21860001343521 : 0 : 2.21858809414511 : 0 : 149151449344.682
        //      150000000346.985 : 1787893.51922865 : 0 :::::::: 17719.0991978275 : 23790.9794844313 : -2.91355468741892E-12
        // E0.60950756252826 : 2.23361462862339 : 3.14159265358979 : 2.23362655108559 : 0 : 149151449344.682
        //      150000000701.367 : 1788369.33880771 : -5.82710924459179E-14 :::::::: 18080.4671177412 : -23517.5229527587 : 0


        [Test]
        public void KeplerTest()
        {
            // both should be the same orbit
            // 149999999990.673 , 1673092.43402889 , 0 ,,,,,,,, -0.33182054440848 , 29749.1548977251 , 0
            // 149999999990.666 , 1673687.41711355 , 0 ,,,,,,,, -0.332641608997619 , 29712.5494536955 , 0

            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 1.989e30 );
            KeplerianOrbit kepler1 = new KeplerianOrbit( 0, null, 1, 0, 0, 0, 0, 0, 1 );
            kepler1.ParentBody = parent;
            KeplerianOrbit kepler2 = new KeplerianOrbit( 0, null, 1, 0, 0, 0, 0, 0, 1 );
            kepler2.ParentBody = parent;

            TrajectoryBodyState state1 = new TrajectoryBodyState( new Vector3Dbl( 149999999990.673, 1673092.43402889, 0 ), new Vector3Dbl( -0.33182054440848, 29749.1548977251, 0 ), Vector3Dbl.zero, 5.97e24 );
            TrajectoryBodyState state2 = new TrajectoryBodyState( new Vector3Dbl( 149999999990.666, 1673687.41711355, 0 ), new Vector3Dbl( -0.332641608997619, 29712.5494536955, 0 ), Vector3Dbl.zero, 5.97e24 );
            kepler1.SetCurrentState( state1 );
            kepler2.SetCurrentState( state2 );

            var state1back = kepler1.GetCurrentState();

            kepler1.Step( new TrajectoryBodyState[] { parent.GetCurrentState() }, 0.02 );

            var state2alt = kepler1.GetCurrentState();

            Assert.That( kepler1.Eccentricity, Is.EqualTo( kepler2.Eccentricity ).Within( 0.000001 ) );
            Assert.That( kepler1.SemiMajorAxis, Is.EqualTo( kepler2.SemiMajorAxis ).Within( 0.01 ) );
            Assert.That( kepler1.Inclination, Is.EqualTo( kepler2.Inclination ).Within( 0.000001 ) );
            Assert.That( kepler1.LongitudeOfAscendingNode, Is.EqualTo( kepler2.LongitudeOfAscendingNode ).Within( 0.000001 ) );
            Assert.That( kepler1.ArgumentOfPeriapsis, Is.EqualTo( kepler2.ArgumentOfPeriapsis ).Within( 0.000001 ) );
        }
    }
}