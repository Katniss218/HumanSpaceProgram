using HSP.Trajectories;
using HSP.Vanilla.Trajectories;
using HSP_Tests_EditMode.NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class NewtonianOrbitTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0001 );

        [Test]
        public void GetStateVector___IsCorrect()
        {
            // Arrange
            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            NewtonianOrbit sut = new NewtonianOrbit( 0, new Vector3Dbl( 6_371_000.0 + 1000, 0, 0 ), Vector3Dbl.zero, Vector3Dbl.zero, 1 );

            // Act
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 6_371_000.0 + 1000, 0, 0 ) ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void GetStateVector___IsCorrect_AfterStepping()
        {
            IEqualityComparer<Vector3Dbl> maxAccumulatedErrorComparer = new Vector3DblApproximateComparer( 0.01 );

            // Arrange
            const double seconds = 10;
            const int stepCount = 100000;
            const double step = seconds / stepCount;

            FixedOrbit parent = new FixedOrbit( 0, Vector3Dbl.zero, QuaternionDbl.identity, 5.97e24 );
            NewtonianOrbit sut = new NewtonianOrbit( 0, new Vector3Dbl( 6_371_000.0 + 1000, 0, 0 ), Vector3Dbl.zero, Vector3Dbl.zero, 1 );

            double time = 0;
            // Act
            for( int i = 0; i < stepCount; i++ )
            {
                sut.Step( new[] { parent.GetCurrentState() }, step );
                time += step;
            }
            TrajectoryBodyState state = sut.GetCurrentState();

            // Assert
            Assert.That( time, Is.EqualTo( seconds ).Within( 0.0000001 ) );
            Assert.That( state.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 6371509.30717223, 0, 0 ) ).Using( maxAccumulatedErrorComparer ) );
            Assert.That( state.AbsoluteVelocity, Is.EqualTo( new Vector3Dbl( -98.1410750335993, 0, 0 ) ).Using( maxAccumulatedErrorComparer ) );
            Assert.That( state.AbsoluteAcceleration, Is.EqualTo( new Vector3Dbl( -9.8151154221740831690796219438688485, 0, 0 ) ).Using( vector3DblApproxComparer ) );
        }
    }
}