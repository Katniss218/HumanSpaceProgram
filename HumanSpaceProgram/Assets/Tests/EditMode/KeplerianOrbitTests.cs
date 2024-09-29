using HSP.Trajectories;
using HSP.Vanilla.Trajectories;
using HSP_Tests_EditMode.NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class KeplerianOrbitTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        static IEqualityComparer<Vector3Dbl> posApproxComparer = new Vector3DblApproximateComparer( 0.1 );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0001 );

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

        [Test]
        public void CalculateTrueAnomaly___Circular___IsCorrect_AfterStepping_WideStep()
        {
            // Arrange
            const double step = 314.8 / 4; // Take a large (wide) step (keplerian should be deterministic so this should work).

            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;

            // Act
            sut.Step( new[] { parent.GetCurrentState() }, step );
            double trueAnomaly = sut.TrueAnomaly;

            // Assert
            Assert.That( sut.MeanAnomaly, Is.EqualTo( 1.57079632679 ).Within( 0.025 ) ); // 90 deg
            Assert.That( trueAnomaly, Is.EqualTo( 1.57079632679 ).Within( 0.025 ) );
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

            Assert.That( sut.SemiMajorAxis, Is.EqualTo( 1_000_000 ).Within( 0.00001 ) );
            Assert.That( sut.Eccentricity, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.Inclination, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.LongitudeOfAscendingNode, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.ArgumentOfPeriapsis, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.MeanAnomaly, Is.EqualTo( 0 ).Within( 0.00001 ) );
            Assert.That( sut.TrueAnomaly, Is.EqualTo( 0 ).Within( 0.00001 ) );
        }

        [Test]
        public void FromStateVector___Eccentric___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1, 0, 0, 0, 0, 0, 1 );
            sut.ParentBody = parent;

            TrajectoryBodyState state = new TrajectoryBodyState(
                new Vector3Dbl( 500_000, 0, 0 ),
                new Vector3Dbl( 0, 34574.08, 0 ),
                new Vector3Dbl( 398.45571, 0, 0 ),
                1 );

            sut.SetCurrentState( state );

            Assert.That( sut.SemiMajorAxis, Is.EqualTo( 1_000_000 ).Within( 0.00001 ) );
            Assert.That( sut.Eccentricity, Is.EqualTo( 0.5 ).Within( 0.00001 ) );
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

        [Test]
        public void RoundTrip___Complicated___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000, 0.5, 0.523598775598, 0.698131700798, 0.872664625997, 1.0471975512, 1 );
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
        public void RoundTrip___ComplicatedBig___IsCorrect()
        {
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            KeplerianOrbit sut = new KeplerianOrbit( 0, null, 1_000_000_000_000, 0.5, 0.872664625997, 0.698131700798, 0.523598775598, 1.0471975512, 1 );
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

            // Assert SemiMajorAxis of large orbit with lower precision because the orbit itself is so big that we'll start losing precision anyway.
            Assert.That( sut2.SemiMajorAxis, Is.EqualTo( sut.SemiMajorAxis ).Within( 0.00000001 ) );
            Assert.That( sut2.Eccentricity, Is.EqualTo( sut.Eccentricity ).Within( 0.00000001 ) );
            Assert.That( sut2.Inclination, Is.EqualTo( sut.Inclination ).Within( 0.00000001 ) );
            Assert.That( sut2.LongitudeOfAscendingNode, Is.EqualTo( sut.LongitudeOfAscendingNode ).Within( 0.00000001 ) );
            Assert.That( sut2.ArgumentOfPeriapsis, Is.EqualTo( sut.ArgumentOfPeriapsis ).Within( 0.00000001 ) );
            Assert.That( sut2.MeanAnomaly, Is.EqualTo( sut.MeanAnomaly ).Within( 0.00000001 ) );
            Assert.That( sut2.TrueAnomaly, Is.EqualTo( sut.TrueAnomaly ).Within( 0.00000001 ) );
        }
    }
}