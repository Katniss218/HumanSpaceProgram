using System.Collections;
using System.Collections.Generic;
using HSP.ReferenceFrames;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_EditMode
{
    public class CenteredInertialReferenceFrameTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        [Test]
        public void AtUt___WorksCorrectly()
        {
            CenteredInertialReferenceFrame sut = new CenteredInertialReferenceFrame( 0, Vector3Dbl.zero, Vector3Dbl.one );

            var sut2 = sut.AtUT( 1 );

            Assert.That( sut2.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( Vector3Dbl.one ) );
        }

        [Test]
        public void AtUtBackwards___WorksCorrectly()
        {
            CenteredInertialReferenceFrame sut = new CenteredInertialReferenceFrame( 0, Vector3Dbl.zero, Vector3Dbl.one );

            var sut2 = sut.AtUT( -1 );

            Assert.That( sut2.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( -Vector3Dbl.one ) );
        }
    }
}