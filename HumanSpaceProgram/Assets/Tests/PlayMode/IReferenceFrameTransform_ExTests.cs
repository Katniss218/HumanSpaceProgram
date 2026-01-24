using HSP.ReferenceFrames;
using HSP.Time;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
{
    public class IReferenceFrameTransform_ExTests
    {
        private static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );
        private static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0001 );

        private class MockReferenceFrameTransform : IReferenceFrameTransform
        {
            public ISceneReferenceFrameProvider SceneReferenceFrameProvider { get; set; }
            public Vector3 Position { get; set; }
            public Vector3Dbl AbsolutePosition { get; set; }
            public Quaternion Rotation { get; set; }
            public QuaternionDbl AbsoluteRotation { get; set; }
            public Vector3 Velocity { get; set; }
            public Vector3Dbl AbsoluteVelocity { get; set; }
            public Vector3 AngularVelocity { get; set; }
            public Vector3Dbl AbsoluteAngularVelocity { get; set; }
            public Vector3 Acceleration { get; set; }
            public Vector3Dbl AbsoluteAcceleration { get; set; }
            public Vector3 AngularAcceleration { get; set; }
            public Vector3Dbl AbsoluteAngularAcceleration { get; set; }

            public Transform transform => throw new NotImplementedException();

            public GameObject gameObject => throw new NotImplementedException();

            public event Action OnAbsolutePositionChanged;
            public event Action OnAbsoluteRotationChanged;
            public event Action OnAbsoluteVelocityChanged;
            public event Action OnAbsoluteAngularVelocityChanged;
            public event Action OnAnyValueChanged;

            public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
            {
                // Mock implementation - do nothing
            }
        }

        private class MockSceneReferenceFrameProvider : ISceneReferenceFrameProvider
        {
            public IReferenceFrame SceneReferenceFrame { get; set; }

            public IReferenceFrame GetSceneReferenceFrame()
            {
                return SceneReferenceFrame;
            }

            public void SubscribeIfNotSubscribed( IReferenceFrameSwitchResponder responder )
            {
                // Mock implementation - do nothing
            }

            public void UnsubscribeIfSubscribed( IReferenceFrameSwitchResponder responder )
            {
                // Mock implementation - do nothing
            }
        }

        [SetUp]
        public void Setup()
        {
            TimeManager.SetUT( 100.0 );
        }

        [Test]
        public void CenteredReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );

            // Act
            IReferenceFrame result = mockTransform.CenteredReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<CenteredReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsolutePosition ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void CenteredInertialReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            mockTransform.AbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );

            // Act
            IReferenceFrame result = mockTransform.CenteredInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<CenteredInertialReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsolutePosition ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsoluteVelocity ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void OrientedReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            mockTransform.AbsoluteRotation = QuaternionDbl.Euler( 45, 90, 135 );

            // Act
            IReferenceFrame result = mockTransform.OrientedReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsolutePosition ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.AbsoluteRotation ).Using( quaternionDblApproxComparer ) );
        }

        [Test]
        public void OrientedInertialReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            mockTransform.AbsoluteRotation = QuaternionDbl.Euler( 45, 90, 135 );
            mockTransform.AbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );

            // Act
            IReferenceFrame result = mockTransform.OrientedInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedInertialReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsolutePosition ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.AbsoluteRotation ).Using( quaternionDblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsoluteVelocity ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void NonInertialReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            mockTransform.AbsoluteRotation = QuaternionDbl.Euler( 45, 90, 135 );
            mockTransform.AbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
            mockTransform.AbsoluteAngularVelocity = new Vector3Dbl( 1, 2, 3 );
            mockTransform.AbsoluteAcceleration = new Vector3Dbl( 5, 10, 15 );
            mockTransform.AbsoluteAngularAcceleration = new Vector3Dbl( 0.5, 1.0, 1.5 );

            // Act
            INonInertialReferenceFrame result = mockTransform.NonInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedNonInertialReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsolutePosition ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.AbsoluteRotation ).Using( quaternionDblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsoluteVelocity ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAngularVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsoluteAngularVelocity ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAcceleration( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsoluteAcceleration ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAngularAcceleration( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsoluteAngularAcceleration ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void CenteredReferenceFrame_WithDifferentUT_CreatesFrameWithCorrectUT()
        {
            // Arrange
            double customUT = 200.0;
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( customUT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );

            // Act
            IReferenceFrame result = mockTransform.CenteredReferenceFrame();

            // Assert
            Assert.That( result.ReferenceUT, Is.EqualTo( customUT ) );
        }

        [Test]
        public void CenteredInertialReferenceFrame_WithZeroVelocity_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            mockTransform.AbsoluteVelocity = Vector3Dbl.zero;

            // Act
            IReferenceFrame result = mockTransform.CenteredInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<CenteredInertialReferenceFrame>() );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void OrientedReferenceFrame_WithIdentityRotation_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            mockTransform.AbsoluteRotation = QuaternionDbl.identity;

            // Act
            IReferenceFrame result = mockTransform.OrientedReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedReferenceFrame>() );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( QuaternionDbl.identity ).Using( quaternionDblApproxComparer ) );
        }

        [Test]
        public void OrientedInertialReferenceFrame_WithComplexValues_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( -100, 0, 500 );
            mockTransform.AbsoluteRotation = QuaternionDbl.Euler( 180, 270, 45 );
            mockTransform.AbsoluteVelocity = new Vector3Dbl( -5, 10, -15 );

            // Act
            IReferenceFrame result = mockTransform.OrientedInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedInertialReferenceFrame>() );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsolutePosition ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.AbsoluteRotation ).Using( quaternionDblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.AbsoluteVelocity ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void NonInertialReferenceFrame_WithZeroAccelerations_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            mockTransform.AbsoluteRotation = QuaternionDbl.Euler( 45, 90, 135 );
            mockTransform.AbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
            mockTransform.AbsoluteAngularVelocity = new Vector3Dbl( 1, 2, 3 );
            mockTransform.AbsoluteAcceleration = Vector3Dbl.zero;
            mockTransform.AbsoluteAngularAcceleration = Vector3Dbl.zero;

            // Act
            INonInertialReferenceFrame result = mockTransform.NonInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedNonInertialReferenceFrame>() );
            Assert.That( result.TransformAcceleration( Vector3Dbl.zero ), Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAngularAcceleration( Vector3Dbl.zero ), Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ) );
        }
    }
}
