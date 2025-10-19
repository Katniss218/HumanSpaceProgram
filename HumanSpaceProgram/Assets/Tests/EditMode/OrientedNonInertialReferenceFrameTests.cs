﻿using HSP.ReferenceFrames;
using HSP_Tests_EditMode.NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class OrientedNonInertialReferenceFrameTests
    {
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 1e-12 );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 1e-12 );

        [Test]
        public void Constructor___SetsCorrectValues()
        {
            var position = new Vector3Dbl( 10.0, 20.0, 30.0 );
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var velocity = new Vector3Dbl( 1.0, 2.0, 3.0 );
            var angularVelocity = new Vector3Dbl( 0.1, 0.2, 0.3 );
            var acceleration = new Vector3Dbl( 0.5, 1.0, 1.5 );
            var angularAcceleration = new Vector3Dbl( 0.01, 0.02, 0.03 );
            var referenceUT = 1000.0;

            var frame = new OrientedNonInertialReferenceFrame( referenceUT, position, rotation, velocity, angularVelocity, acceleration, angularAcceleration );

            Assert.That( frame.ReferenceUT, Is.EqualTo( referenceUT ) );
            Assert.That( frame.Position, Is.EqualTo( position ) );
            Assert.That( frame.Rotation.x, Is.EqualTo( rotation.x ).Within( 1e-10 ) );
            Assert.That( frame.Rotation.y, Is.EqualTo( rotation.y ).Within( 1e-10 ) );
            Assert.That( frame.Rotation.z, Is.EqualTo( rotation.z ).Within( 1e-10 ) );
            Assert.That( frame.Rotation.w, Is.EqualTo( rotation.w ).Within( 1e-10 ) );
            Assert.That( frame.Velocity, Is.EqualTo( velocity ) );
            Assert.That( frame.AngularVelocity, Is.EqualTo( angularVelocity ) );
            Assert.That( frame.Acceleration, Is.EqualTo( acceleration ) );
            Assert.That( frame.AngularAcceleration, Is.EqualTo( angularAcceleration ) );
        }

        [Test]
        public void AtUT___CalculatesCorrectPositionAndRotation()
        {
            var rotation = QuaternionDbl.identity;
            var frame = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, Vector3Dbl.zero, new Vector3Dbl( 1.0, 0.0, 0.0 ), Vector3Dbl.zero );
            var futureFrame = (OrientedNonInertialReferenceFrame)frame.AtUT( 2.0 );

            // Position should be 0.5 * acceleration * t^2 = 0.5 * 1.0 * 4.0 = 2.0
            var expectedPosition = new Vector3Dbl( 2.0, 0.0, 0.0 );
            Assert.That( futureFrame.Position.x, Is.EqualTo( expectedPosition.x ).Within( 1e-10 ) );
            Assert.That( futureFrame.Position.y, Is.EqualTo( expectedPosition.y ).Within( 1e-10 ) );
            Assert.That( futureFrame.Position.z, Is.EqualTo( expectedPosition.z ).Within( 1e-10 ) );
        }

        [Test]
        public void TransformAngularVelocity___RotatesAndAddsFrameAngularVelocity()
        {
            var rotation = QuaternionDbl.AngleAxis( 90.0, Vector3Dbl.up );
            var frame = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, new Vector3Dbl( 1.0, 2.0, 3.0 ), Vector3Dbl.zero, Vector3Dbl.zero );
            var localAngVel = new Vector3Dbl( 5.0, 0.0, 0.0 );

            var globalAngVel = frame.TransformAngularVelocity( localAngVel );
            // Rotated angular velocity (5,0,0) becomes (0,0,-5), then add frame angular velocity (1,2,3)
            var expectedAngVel = new Vector3Dbl( 1.0, 2.0, -2.0 );

            Assert.That( globalAngVel.x, Is.EqualTo( expectedAngVel.x ).Within( 1e-10 ) );
            Assert.That( globalAngVel.y, Is.EqualTo( expectedAngVel.y ).Within( 1e-10 ) );
            Assert.That( globalAngVel.z, Is.EqualTo( expectedAngVel.z ).Within( 1e-10 ) );
        }

        [Test]
        public void TransformAcceleration___RotatesAndAddsFrameAcceleration()
        {
            var rotation = QuaternionDbl.AngleAxis( 90.0, Vector3Dbl.up );
            var frame = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, Vector3Dbl.zero, new Vector3Dbl( 5.0, 10.0, 15.0 ), Vector3Dbl.zero );
            var localAcc = new Vector3Dbl( 1.0, 0.0, 0.0 );

            var globalAcc = frame.TransformAcceleration( localAcc );
            // Rotated acceleration (1,0,0) becomes (0,0,-1), then add frame acceleration (5,10,15)
            var expectedAcc = new Vector3Dbl( 5.0, 10.0, 14.0 );

            Assert.That( globalAcc.x, Is.EqualTo( expectedAcc.x ).Within( 1e-10 ) );
            Assert.That( globalAcc.y, Is.EqualTo( expectedAcc.y ).Within( 1e-10 ) );
            Assert.That( globalAcc.z, Is.EqualTo( expectedAcc.z ).Within( 1e-10 ) );
        }

        [Test]
        public void GetTangentialVelocity___CalculatesCorrectly()
        {
            var rotation = QuaternionDbl.identity;
            var frame = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, new Vector3Dbl( 0.0, 0.0, 1.0 ), Vector3Dbl.zero, Vector3Dbl.zero );
            var localPos = new Vector3Dbl( 1.0, 0.0, 0.0 );

            var tangentialVel = frame.GetTangentialVelocity( localPos );
            // First rotate position: (1,0,0) stays (1,0,0) with identity rotation
            // Then cross product: (0,0,1) × (1,0,0) = (0,1,0)
            var expectedVel = new Vector3Dbl( 0.0, 1.0, 0.0 );

            Assert.That( tangentialVel.x, Is.EqualTo( expectedVel.x ).Within( 1e-10 ) );
            Assert.That( tangentialVel.y, Is.EqualTo( expectedVel.y ).Within( 1e-10 ) );
            Assert.That( tangentialVel.z, Is.EqualTo( expectedVel.z ).Within( 1e-10 ) );
        }

        [Test]
        public void GetFicticiousAcceleration___CalculatesCorrectly()
        {
            var rotation = QuaternionDbl.identity;
            var frame = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, new Vector3Dbl( 0.0, 0.0, 1.0 ), Vector3Dbl.zero, Vector3Dbl.zero );
            var localPos = new Vector3Dbl( 1.0, 0.0, 0.0 );
            var localVel = new Vector3Dbl( 0.0, 1.0, 0.0 );

            var ficticiousAcc = frame.GetFicticiousAcceleration( localPos, localVel );

            var omega = new Vector3Dbl( 0, 0, 1 );
            var centrifugal = Vector3Dbl.Cross( -omega, Vector3Dbl.Cross( omega, localPos ) );
            var coriolis = -2 * Vector3Dbl.Cross( omega, localVel );
            var expectedAcc = centrifugal + coriolis; // + Euler + linear (both zero in test)

            Assert.That( ficticiousAcc.x, Is.EqualTo( expectedAcc.x ).Within( 1e-10 ) );
            Assert.That( ficticiousAcc.y, Is.EqualTo( expectedAcc.y ).Within( 1e-10 ) );
            Assert.That( ficticiousAcc.z, Is.EqualTo( expectedAcc.z ).Within( 1e-10 ) );
        }

        [Test]
        public void AllTransforms___RoundTripAccuracy()
        {
            var rotation = QuaternionDbl.AngleAxis( 30.0, Vector3Dbl.up );
            var frame = new OrientedNonInertialReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ), rotation, new Vector3Dbl( 0.1, 0.2, 0.3 ), new Vector3Dbl( 0.01, 0.02, 0.03 ), new Vector3Dbl( 0.001, 0.002, 0.003 ), new Vector3Dbl( 0.0001, 0.0002, 0.0003 ) );
            ReferenceFrameTestHelpers.AssertRoundTripAccuracy( frame, "OrientedNonInertialReferenceFrame" );
        }

        [Test]
        public void TimeEvolution___PreservesProperties()
        {
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var frame = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            ReferenceFrameTestHelpers.AssertTimeEvolution( frame, 100.0, "OrientedNonInertialReferenceFrame" );
        }

        [Test]
        public void Equals___WorksCorrectly()
        {
            var rotation = QuaternionDbl.AngleAxis( 45.0, Vector3Dbl.up );
            var frame1 = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            var frame2 = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            var frame3 = new OrientedNonInertialReferenceFrame( 0.0, Vector3Dbl.zero, rotation, Vector3Dbl.zero, new Vector3Dbl( 0.1, 0.0, 0.0 ), Vector3Dbl.zero, Vector3Dbl.zero );

            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame2, true, "OrientedNonInertialReferenceFrame" );
            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame3, false, "OrientedNonInertialReferenceFrame" );
        }
    }
}