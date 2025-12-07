using HSP.ReferenceFrames;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class OrientedReferenceFrameTests
    {
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 1e-12 );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 1e-12 );

        [Test]
        public void Constructor___SetsCorrectValues()
        {
            var position = new Vector3Dbl( 10.0, 20.0, 30.0 );
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var referenceUT = 1000.0;
            var frame = new OrientedReferenceFrame( referenceUT, position, rotation );

            Assert.That( frame.ReferenceUT, Is.EqualTo( referenceUT ) );
            Assert.That( frame.Position, Is.EqualTo( position ) );
            Assert.That( frame.Rotation, Is.EqualTo( rotation ) );
        }

        [Test]
        public void AtUT___ReturnsNewFrameWithUpdatedTime()
        {
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var frame = new OrientedReferenceFrame( 0.0, Vector3Dbl.zero, rotation );
            var newFrame = (OrientedReferenceFrame)frame.AtUT( 100.0 );

            Assert.That( newFrame.ReferenceUT, Is.EqualTo( 100.0 ) );
            Assert.That( newFrame.Position, Is.EqualTo( Vector3Dbl.zero ) );
            Assert.That( newFrame.Rotation, Is.EqualTo( rotation ) );
        }

        [Test]
        public void TransformPosition___RotatesAndTranslates()
        {
            var position = new Vector3Dbl( 1.0, 0.0, 0.0 );
            var rotation = QuaternionDbl.AngleAxis( 90.0, Vector3Dbl.up );
            var frame = new OrientedReferenceFrame( 0.0, Vector3Dbl.zero, rotation );
            var localPos = new Vector3Dbl( 1.0, 0.0, 0.0 );

            var globalPos = frame.TransformPosition( localPos );
            // Rotation of (1,0,0) by 90 degrees around Y should give (0,0,-1)
            var expectedPos = new Vector3Dbl( 0.0, 0.0, -1.0 );

            Assert.That( globalPos.x, Is.EqualTo( expectedPos.x ).Within( 1e-10 ) );
            Assert.That( globalPos.y, Is.EqualTo( expectedPos.y ).Within( 1e-10 ) );
            Assert.That( globalPos.z, Is.EqualTo( expectedPos.z ).Within( 1e-10 ) );
        }

        [Test]
        public void TransformDirection___RotatesDirection()
        {
            var rotation = QuaternionDbl.AngleAxis( 90.0, Vector3Dbl.up );
            var frame = new OrientedReferenceFrame( 0.0, Vector3Dbl.zero, rotation );
            var localDir = new Vector3( 1.0f, 0.0f, 0.0f );

            var globalDir = frame.TransformDirection( localDir );
            // Rotation of (1,0,0) by 90 degrees around Y should give (0,0,-1)
            var expectedDir = new Vector3( 0.0f, 0.0f, -1.0f );

            Assert.That( globalDir.x, Is.EqualTo( expectedDir.x ).Within( 1e-6 ) );
            Assert.That( globalDir.y, Is.EqualTo( expectedDir.y ).Within( 1e-6 ) );
            Assert.That( globalDir.z, Is.EqualTo( expectedDir.z ).Within( 1e-6 ) );
        }

        [Test]
        public void AllTransforms___RoundTripAccuracy()
        {
            var rotation = QuaternionDbl.AngleAxis( 30.0, Vector3Dbl.up );
            var frame = new OrientedReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ), rotation );
            ReferenceFrameTestHelpers.AssertRoundTripAccuracy( frame, "OrientedReferenceFrame" );
        }

        [Test]
        public void TimeEvolution___PreservesProperties()
        {
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var frame = new OrientedReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ), rotation );
            ReferenceFrameTestHelpers.AssertTimeEvolution( frame, 100.0, "OrientedReferenceFrame" );
        }

        [Test]
        public void Equals___WorksCorrectly()
        {
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var frame1 = new OrientedReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ), rotation );
            var frame2 = new OrientedReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ), rotation );
            var frame3 = new OrientedReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 4.0 ), rotation );

            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame2, true, "OrientedReferenceFrame" );
            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame3, false, "OrientedReferenceFrame" );
        }
    }
}