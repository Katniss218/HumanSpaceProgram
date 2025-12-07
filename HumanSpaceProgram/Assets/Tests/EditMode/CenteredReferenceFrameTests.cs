using HSP.ReferenceFrames;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class CenteredReferenceFrameTests
    {
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 1e-12 );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 1e-12 );

        [Test]
        public void Constructor___SetsCorrectValues()
        {
            var position = new Vector3Dbl( 10.0, 20.0, 30.0 );
            var referenceUT = 1000.0;
            var frame = new CenteredReferenceFrame( referenceUT, position );

            Assert.That( frame.ReferenceUT, Is.EqualTo( referenceUT ) );
            Assert.That( frame.Position, Is.EqualTo( position ) );
        }

        [Test]
        public void AtUT___ReturnsNewFrameWithUpdatedTime()
        {
            var frame = new CenteredReferenceFrame( 0.0, Vector3Dbl.zero );
            var newFrame = (CenteredReferenceFrame)frame.AtUT( 100.0 );

            Assert.That( newFrame.ReferenceUT, Is.EqualTo( 100.0 ) );
            Assert.That( newFrame.Position, Is.EqualTo( Vector3Dbl.zero ) );
        }

        [Test]
        public void TransformPosition___AddsPositionOffset()
        {
            var center = new Vector3Dbl( 5.0, 10.0, 15.0 );
            var frame = new CenteredReferenceFrame( 0.0, center );
            var localPos = new Vector3Dbl( 1.0, 2.0, 3.0 );

            var globalPos = frame.TransformPosition( localPos );
            var expectedPos = new Vector3Dbl( 6.0, 12.0, 18.0 );

            Assert.That( globalPos, Is.EqualTo( expectedPos ) );
        }

        [Test]
        public void InverseTransformPosition___SubtractsPositionOffset()
        {
            var center = new Vector3Dbl( 5.0, 10.0, 15.0 );
            var frame = new CenteredReferenceFrame( 0.0, center );
            var globalPos = new Vector3Dbl( 6.0, 12.0, 18.0 );

            var localPos = frame.InverseTransformPosition( globalPos );
            var expectedPos = new Vector3Dbl( 1.0, 2.0, 3.0 );

            Assert.That( localPos, Is.EqualTo( expectedPos ) );
        }

        [Test]
        public void AllTransforms___RoundTripAccuracy()
        {
            var frame = new CenteredReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            ReferenceFrameTestHelpers.AssertRoundTripAccuracy( frame, "CenteredReferenceFrame" );
        }

        [Test]
        public void TimeEvolution___PreservesProperties()
        {
            var frame = new CenteredReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            ReferenceFrameTestHelpers.AssertTimeEvolution( frame, 100.0, "CenteredReferenceFrame" );
        }

        [Test]
        public void Equals___WorksCorrectly()
        {
            var frame1 = new CenteredReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var frame2 = new CenteredReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var frame3 = new CenteredReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 4.0 ) );

            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame2, true, "CenteredReferenceFrame" );
            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame3, false, "CenteredReferenceFrame" );
        }
    }
}