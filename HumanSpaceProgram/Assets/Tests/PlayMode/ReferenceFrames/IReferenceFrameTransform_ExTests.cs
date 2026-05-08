using HSP.ReferenceFrames;
using HSP.Time;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_PlayMode.ReferenceFrames
{
    public class MockReferenceFrameTransform : IReferenceFrameTransform
    {
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider { get; set; }

        private KinematicState _state = KinematicState.GetIdentity();

        public KinematicState GetState( IReferenceFrame requestedFrame )
        {
            return _state.InFrame( requestedFrame );
        }

        public ref readonly KinematicState GetStateRef( out IReferenceFrame referenceFrame )
        {
            referenceFrame = null;
            return ref _state;
        }

        public void SetState( in KinematicState state )
        {
            _state = state.InFrame( null );
            OnStateChanged?.Invoke();
        }

        public void ModifyState( IReferenceFrame requestedFrame, KinematicStateMutator mutator )
        {
            if( requestedFrame == null )
            {
                mutator( ref _state );
            }
            else
            {
                var localState = _state.InFrame( requestedFrame );
                mutator( ref localState );
                _state = localState.InFrame( null );
            }
            OnStateChanged?.Invoke();
        }

        public Transform transform => null;
        public GameObject gameObject => null;

        public event Action OnStateChanged;

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
        }
    }

    public class MockSceneReferenceFrameProvider : ISceneReferenceFrameProvider
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

    public class IReferenceFrameTransform_ExTests
    {
        private static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );
        private static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0001 );

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
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );

            // Act
            IReferenceFrame result = mockTransform.CenteredReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<CenteredReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsolutePosition() ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void CenteredInertialReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );
            mockTransform.SetAbsoluteVelocity( new Vector3Dbl( 10, 20, 30 ) );

            // Act
            IReferenceFrame result = mockTransform.CenteredInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<CenteredInertialReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsolutePosition() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsoluteVelocity() ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void OrientedReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );
            mockTransform.SetAbsoluteRotation( QuaternionDbl.Euler( 45, 90, 135 ) );

            // Act
            IReferenceFrame result = mockTransform.OrientedReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsolutePosition() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.GetAbsoluteRotation() ).Using( quaternionDblApproxComparer ) );
        }

        [Test]
        public void OrientedInertialReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );
            mockTransform.SetAbsoluteRotation( QuaternionDbl.Euler( 45, 90, 135 ) );
            mockTransform.SetAbsoluteVelocity( new Vector3Dbl( 10, 20, 30 ) );

            // Act
            IReferenceFrame result = mockTransform.OrientedInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedInertialReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsolutePosition() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.GetAbsoluteRotation() ).Using( quaternionDblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsoluteVelocity() ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void NonInertialReferenceFrame_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );
            mockTransform.SetAbsoluteRotation( QuaternionDbl.Euler( 45, 90, 135 ) );
            mockTransform.SetAbsoluteVelocity( new Vector3Dbl( 10, 20, 30 ) );
            mockTransform.SetAbsoluteAngularVelocity( new Vector3Dbl( 1, 2, 3 ) );
            mockTransform.SetAbsoluteAcceleration( new Vector3Dbl( 5, 10, 15 ) );
            mockTransform.SetAbsoluteAngularAcceleration( new Vector3Dbl( 0.5, 1.0, 1.5 ) );

            // Act
            INonInertialReferenceFrame result = mockTransform.NonInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedNonInertialReferenceFrame>() );
            Assert.That( result.ReferenceUT, Is.EqualTo( TimeManager.UT ) );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsolutePosition() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.GetAbsoluteRotation() ).Using( quaternionDblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsoluteVelocity() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAngularVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsoluteAngularVelocity() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAcceleration( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsoluteAcceleration() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAngularAcceleration( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsoluteAngularAcceleration() ).Using( vector3DblApproxComparer ) );
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
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );

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
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );
            mockTransform.SetAbsoluteVelocity( Vector3Dbl.zero );

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
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );
            mockTransform.SetAbsoluteRotation( QuaternionDbl.identity );

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
            mockTransform.SetAbsolutePosition( new Vector3Dbl( -100, 0, 500 ) );
            mockTransform.SetAbsoluteRotation( QuaternionDbl.Euler( 180, 270, 45 ) );
            mockTransform.SetAbsoluteVelocity( new Vector3Dbl( -5, 10, -15 ) );

            // Act
            IReferenceFrame result = mockTransform.OrientedInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedInertialReferenceFrame>() );
            Assert.That( result.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsolutePosition() ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformRotation( QuaternionDbl.identity ), Is.EqualTo( mockTransform.GetAbsoluteRotation() ).Using( quaternionDblApproxComparer ) );
            Assert.That( result.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( mockTransform.GetAbsoluteVelocity() ).Using( vector3DblApproxComparer ) );
        }

        [Test]
        public void NonInertialReferenceFrame_WithZeroAccelerations_CreatesCorrectFrame()
        {
            // Arrange
            var mockProvider = new MockSceneReferenceFrameProvider();
            mockProvider.SceneReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            var mockTransform = new MockReferenceFrameTransform();
            mockTransform.SceneReferenceFrameProvider = mockProvider;
            mockTransform.SetAbsolutePosition( new Vector3Dbl( 100, 200, 300 ) );
            mockTransform.SetAbsoluteRotation( QuaternionDbl.Euler( 45, 90, 135 ) );
            mockTransform.SetAbsoluteVelocity( new Vector3Dbl( 10, 20, 30 ) );
            mockTransform.SetAbsoluteAngularVelocity( new Vector3Dbl( 1, 2, 3 ) );
            mockTransform.SetAbsoluteAcceleration( Vector3Dbl.zero );
            mockTransform.SetAbsoluteAngularAcceleration( Vector3Dbl.zero );

            // Act
            INonInertialReferenceFrame result = mockTransform.NonInertialReferenceFrame();

            // Assert
            Assert.That( result, Is.InstanceOf<OrientedNonInertialReferenceFrame>() );
            Assert.That( result.TransformAcceleration( Vector3Dbl.zero ), Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ) );
            Assert.That( result.TransformAngularAcceleration( Vector3Dbl.zero ), Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ) );
        }
    }
}