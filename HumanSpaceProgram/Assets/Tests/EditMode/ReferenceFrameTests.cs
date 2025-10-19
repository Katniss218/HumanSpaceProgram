using HSP.ReferenceFrames;
using NUnit.Framework;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    /// <summary>
    /// Helper methods for comprehensive reference frame testing
    /// </summary>
    public static class ReferenceFrameTestHelpers
    {
        private const double PRECISION_TOLERANCE = 1e-12;
        private const double PRECISION_TOLERANCE_DIR = 1e-6;

        public static void AssertRoundTripAccuracy( IReferenceFrame frame, string frameType )
        {
            var testPosition = new Vector3Dbl( 1.0, 2.0, 3.0 );
            var testDirection = new Vector3( 0.6f, 0.8f, 0.0f );
            var testRotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up ).normalized;
            var testVelocity = new Vector3Dbl( 10.0, 20.0, 30.0 );
            var testAngularVelocity = new Vector3Dbl( 1.0, 2.0, 3.0 );
            var testAcceleration = new Vector3Dbl( 5.0, 10.0, 15.0 );
            var testAngularAcceleration = new Vector3Dbl( 0.5, 1.0, 1.5 );

            // Position round-trip
            var transformedPos = frame.TransformPosition( testPosition );
            var inverseTransformedPos = frame.InverseTransformPosition( transformedPos );
            Assert.That( inverseTransformedPos.x, Is.EqualTo( testPosition.x ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Position round-trip failed for X component" );
            Assert.That( inverseTransformedPos.y, Is.EqualTo( testPosition.y ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Position round-trip failed for Y component" );
            Assert.That( inverseTransformedPos.z, Is.EqualTo( testPosition.z ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Position round-trip failed for Z component" );

            // Direction round-trip
            var transformedDir = frame.TransformDirection( testDirection );
            var inverseTransformedDir = frame.InverseTransformDirection( transformedDir );
            Assert.That( inverseTransformedDir.x, Is.EqualTo( testDirection.x ).Within( PRECISION_TOLERANCE_DIR ),
                $"{frameType}: Direction round-trip failed for X component" );
            Assert.That( inverseTransformedDir.y, Is.EqualTo( testDirection.y ).Within( PRECISION_TOLERANCE_DIR ),
                $"{frameType}: Direction round-trip failed for Y component" );
            Assert.That( inverseTransformedDir.z, Is.EqualTo( testDirection.z ).Within( PRECISION_TOLERANCE_DIR ),
                $"{frameType}: Direction round-trip failed for Z component" );

            // Rotation round-trip
            var transformedRot = frame.TransformRotation( testRotation );
            var inverseTransformedRot = frame.InverseTransformRotation( transformedRot );
            Assert.That( inverseTransformedRot.x, Is.EqualTo( testRotation.x ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Rotation round-trip failed for X component" );
            Assert.That( inverseTransformedRot.y, Is.EqualTo( testRotation.y ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Rotation round-trip failed for Y component" );
            Assert.That( inverseTransformedRot.z, Is.EqualTo( testRotation.z ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Rotation round-trip failed for Z component" );
            Assert.That( inverseTransformedRot.w, Is.EqualTo( testRotation.w ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Rotation round-trip failed for W component" );

            // Velocity round-trip
            var transformedVel = frame.TransformVelocity( testVelocity );
            var inverseTransformedVel = frame.InverseTransformVelocity( transformedVel );
            Assert.That( inverseTransformedVel.x, Is.EqualTo( testVelocity.x ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Velocity round-trip failed for X component" );
            Assert.That( inverseTransformedVel.y, Is.EqualTo( testVelocity.y ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Velocity round-trip failed for Y component" );
            Assert.That( inverseTransformedVel.z, Is.EqualTo( testVelocity.z ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Velocity round-trip failed for Z component" );

            // Angular velocity round-trip
            var transformedAngVel = frame.TransformAngularVelocity( testAngularVelocity );
            var inverseTransformedAngVel = frame.InverseTransformAngularVelocity( transformedAngVel );
            Assert.That( inverseTransformedAngVel.x, Is.EqualTo( testAngularVelocity.x ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Angular velocity round-trip failed for X component" );
            Assert.That( inverseTransformedAngVel.y, Is.EqualTo( testAngularVelocity.y ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Angular velocity round-trip failed for Y component" );
            Assert.That( inverseTransformedAngVel.z, Is.EqualTo( testAngularVelocity.z ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Angular velocity round-trip failed for Z component" );

            // Acceleration round-trip
            var transformedAcc = frame.TransformAcceleration( testAcceleration );
            var inverseTransformedAcc = frame.InverseTransformAcceleration( transformedAcc );
            Assert.That( inverseTransformedAcc.x, Is.EqualTo( testAcceleration.x ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Acceleration round-trip failed for X component" );
            Assert.That( inverseTransformedAcc.y, Is.EqualTo( testAcceleration.y ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Acceleration round-trip failed for Y component" );
            Assert.That( inverseTransformedAcc.z, Is.EqualTo( testAcceleration.z ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Acceleration round-trip failed for Z component" );

            // Angular acceleration round-trip
            var transformedAngAcc = frame.TransformAngularAcceleration( testAngularAcceleration );
            var inverseTransformedAngAcc = frame.InverseTransformAngularAcceleration( transformedAngAcc );
            Assert.That( inverseTransformedAngAcc.x, Is.EqualTo( testAngularAcceleration.x ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Angular acceleration round-trip failed for X component" );
            Assert.That( inverseTransformedAngAcc.y, Is.EqualTo( testAngularAcceleration.y ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Angular acceleration round-trip failed for Y component" );
            Assert.That( inverseTransformedAngAcc.z, Is.EqualTo( testAngularAcceleration.z ).Within( PRECISION_TOLERANCE ),
                $"{frameType}: Angular acceleration round-trip failed for Z component" );
        }

        public static void AssertTimeEvolution( IReferenceFrame frame, double deltaTime, string frameType )
        {
            var futureFrame = frame.AtUT( frame.ReferenceUT + deltaTime );
            var pastFrame = frame.AtUT( frame.ReferenceUT - deltaTime );

            // Test that time evolution preserves frame properties
            Assert.That( futureFrame.ReferenceUT, Is.EqualTo( frame.ReferenceUT + deltaTime ), $"{frameType}: Future frame has incorrect reference time" );
            Assert.That( pastFrame.ReferenceUT, Is.EqualTo( frame.ReferenceUT - deltaTime ), $"{frameType}: Past frame has incorrect reference time" );

            // Test that zero delta time returns same frame
            var sameFrame = frame.AtUT( frame.ReferenceUT );
            Assert.That( sameFrame.Equals( frame ), Is.True, $"{frameType}: Zero delta time should return same frame" );
        }

        public static void AssertEqualityMethods( IReferenceFrame frame1, IReferenceFrame frame2, bool shouldBeEqual, string frameType )
        {
            Assert.That( frame1.Equals( frame2 ), Is.EqualTo( shouldBeEqual ),
                $"{frameType}: Equals method failed" );
            Assert.That( frame1.EqualsIgnoreUT( frame2 ), Is.EqualTo( shouldBeEqual ),
                $"{frameType}: EqualsIgnoreUT method failed" );
        }
    }
}