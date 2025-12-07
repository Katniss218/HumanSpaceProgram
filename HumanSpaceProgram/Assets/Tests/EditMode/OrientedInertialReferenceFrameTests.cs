using HSP.ReferenceFrames;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class OrientedInertialReferenceFrameTests
    {
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 1e-12 );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 1e-12 );

        [Test]
        public void Constructor___SetsCorrectValues()
        {
            var position = new Vector3Dbl( 10.0, 20.0, 30.0 );
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var velocity = new Vector3Dbl( 1.0, 2.0, 3.0 );
            var referenceUT = 1000.0;
            var frame = new OrientedInertialReferenceFrame( referenceUT, position, rotation, velocity );

            Assert.That( frame.ReferenceUT, Is.EqualTo( referenceUT ) );
            Assert.That( frame.Position, Is.EqualTo( position ) );
            Assert.That( frame.Rotation.x, Is.EqualTo( rotation.x ).Within( 1e-10 ) );
            Assert.That( frame.Rotation.y, Is.EqualTo( rotation.y ).Within( 1e-10 ) );
            Assert.That( frame.Rotation.z, Is.EqualTo( rotation.z ).Within( 1e-10 ) );
            Assert.That( frame.Rotation.w, Is.EqualTo( rotation.w ).Within( 1e-10 ) );
            Assert.That( frame.Velocity, Is.EqualTo( velocity ) );
        }

        [Test]
        public void AtUT___CalculatesCorrectPosition()
        {
            var frame = new OrientedInertialReferenceFrame( 0.0, Vector3Dbl.zero, QuaternionDbl.identity, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var futureFrame = (OrientedInertialReferenceFrame)frame.AtUT( 10.0 );

            var expectedPosition = new Vector3Dbl( 10.0, 20.0, 30.0 );
            Assert.That( futureFrame.Position, Is.EqualTo( expectedPosition ) );
        }

        [Test]
        public void TransformVelocity___RotatesAndAddsFrameVelocity()
        {
            var rotation = QuaternionDbl.AngleAxis( 90.0, Vector3Dbl.up );
            var frame = new OrientedInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, new Vector3Dbl( 5.0, 10.0, 15.0 ) );
            var localVel = new Vector3Dbl( 1.0, 0.0, 0.0 );

            var globalVel = frame.TransformVelocity( localVel );
            // Rotated velocity (1,0,0) becomes (0,0,-1), then add frame velocity (5,10,15)
            var expectedVel = new Vector3Dbl( 5.0, 10.0, 14.0 );

            Assert.That( globalVel.x, Is.EqualTo( expectedVel.x ).Within( 1e-10 ) );
            Assert.That( globalVel.y, Is.EqualTo( expectedVel.y ).Within( 1e-10 ) );
            Assert.That( globalVel.z, Is.EqualTo( expectedVel.z ).Within( 1e-10 ) );
        }

        [Test]
        public void AllTransforms___RoundTripAccuracy()
        {
            var rotation = QuaternionDbl.AngleAxis( 30.0, Vector3Dbl.up );
            var frame = new OrientedInertialReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ), rotation, new Vector3Dbl( 0.1, 0.2, 0.3 ) );
            ReferenceFrameTestHelpers.AssertRoundTripAccuracy( frame, "OrientedInertialReferenceFrame" );
        }

        [Test]
        public void TimeEvolution___PreservesProperties()
        {
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var frame = new OrientedInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            ReferenceFrameTestHelpers.AssertTimeEvolution( frame, 100.0, "OrientedInertialReferenceFrame" );
        }

        [Test]
        public void Equals___WorksCorrectly()
        {
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var frame1 = new OrientedInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var frame2 = new OrientedInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var frame3 = new OrientedInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, new Vector3Dbl( 1.0, 2.0, 4.0 ) );

            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame2, true, "OrientedInertialReferenceFrame" );
            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame3, false, "OrientedInertialReferenceFrame" );
        }
    }
}