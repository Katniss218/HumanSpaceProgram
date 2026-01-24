using HSP.ReferenceFrames;
using HSP.Time;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
{
    public class SceneReferenceFrameManagerTests
    {
        private static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );

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
            public Vector3 Acceleration => Vector3.zero;
            public Vector3Dbl AbsoluteAcceleration => Vector3Dbl.zero;
            public Vector3 AngularAcceleration => Vector3.zero;
            public Vector3Dbl AbsoluteAngularAcceleration => Vector3Dbl.zero;

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

        [UnityTest]
        public IEnumerator MovingReferenceFrame_PropagatesCorrectly()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            var assertMonoBeh = go.AddComponent<AssertMonoBehaviour>();

            yield return new WaitForFixedUpdate();

            double startUT = TimeManager.UT;

            const double velocity = 10;
            IReferenceFrame cif = new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( velocity, 0, 0 ) );
            sman.RequestReferenceFrameSwitch( cif );

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                // In fixed update, the frame is before it updates (updates during physics step), so we use oldUT here.
                IReferenceFrame frame = sman.referenceFrame;
                Vector3Dbl expectedPos = new Vector3Dbl( velocity * (TimeManager.OldUT - startUT), 0, 0 );

                Assert.That( frame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedPos ).Using( vector3DblApproxComparer ) );
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                // In update, the frame is after it updates (updates during physics step), so we use UT here.
                IReferenceFrame frame = sman.referenceFrame;
                Vector3Dbl expectedPos = new Vector3Dbl( velocity * (TimeManager.UT - startUT), 0, 0 );

                Assert.That( frame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedPos ).Using( vector3DblApproxComparer ) );
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.LateUpdate, ( frameInfo ) =>
            {
                // In late update, the frame is after it updates (updates during physics step), so we use UT here.
                IReferenceFrame frame = sman.referenceFrame;
                Vector3Dbl expectedPos = new Vector3Dbl( velocity * (TimeManager.UT - startUT), 0, 0 );

                Assert.That( frame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedPos ).Using( vector3DblApproxComparer ) );
            } );
            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 1 );

            UnityEngine.Object.DestroyImmediate( go );
        }


        [UnityTest]
        public IEnumerator RequestReferenceFrameSwitch_ReferenceFrameUpdatesAfterPhysicsProcessing()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            var assertMonoBeh = go.AddComponent<AssertMonoBehaviour>();

            yield return new WaitForFixedUpdate();

            IReferenceFrame initialFrame = sman.referenceFrame;

            IReferenceFrame newFrame = new CenteredReferenceFrame( TimeManager.UT, new Vector3Dbl( 100, 0, 0 ) );
            sman.RequestReferenceFrameSwitch( newFrame );

            // Verify that the reference frame hasn't changed yet in FixedUpdate.
            bool fixedUpdateRan = false;
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, () => true, isOneShot: true, ( frameInfo ) =>
            {
                fixedUpdateRan = true;
                Assert.That( sman.referenceFrame, Is.EqualTo( initialFrame ) );
            } );

            // Verify that the reference frame has changed in Update (after physics step).
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, () => fixedUpdateRan, isOneShot: true, ( frameInfo ) =>
            {
                Assert.That( sman.referenceFrame, Is.EqualTo( newFrame ) );
            } );

            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 0.1f );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [UnityTest]
        public IEnumerator TargetObject_Moving_RequestsSwitchWhenExceedingBounds()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            var assertMonoBeh = go.AddComponent<AssertMonoBehaviour>();

            yield return new WaitForFixedUpdate();

            sman.MaxRelativePosition = 100f;
            sman.MaxRelativeVelocity = 1000000f;

            var mockTarget = new MockReferenceFrameTransform();
            Vector3Dbl expectedAbsolutePosition = new Vector3Dbl( 1000, 2000, 3000 );
            Vector3Dbl expectedAbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
            mockTarget.AbsolutePosition = expectedAbsolutePosition;
            mockTarget.AbsoluteVelocity = expectedAbsoluteVelocity;

            // Set initial position within bounds
            mockTarget.Position = new Vector3( 50, 0, 0 );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False );

            yield return new WaitForFixedUpdate();

            Assert.That( sman.IsSwitchRequested, Is.False );

            mockTarget.Position = new Vector3( 150, 0, 0 ); // Simulate movement (mock transform) by updating position manually.

            bool fixedUpdateRan = false;
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, () => true, isOneShot: true, ( frameInfo ) =>
            {
                // switch not requested yet, but will be automatically if the position exceeds the bounds.
                Assert.That( sman.IsSwitchRequested, Is.False );
                fixedUpdateRan = true;
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, () => fixedUpdateRan, isOneShot: true, ( frameInfo ) =>
            {
                Assert.That( sman.IsSwitchRequested, Is.False );

                IReferenceFrame frame = sman.referenceFrame;
                Assert.That( frame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ) );
                Assert.That( frame.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( expectedAbsoluteVelocity ).Using( vector3DblApproxComparer ) );
            } );
            assertMonoBeh.Enable();

            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds( 0.1f );

            UnityEngine.Object.DestroyImmediate( go );
        }
        
        [Test]
        public void TargetObject_SetWithinPositionBounds_DoesNotRequestSwitch()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            sman.MaxRelativePosition = 100f;
            sman.MaxRelativeVelocity = 1000000f;

            var mockTarget = new MockReferenceFrameTransform();
            mockTarget.AbsolutePosition = new Vector3Dbl( 0, 0, 0 );
            mockTarget.AbsoluteVelocity = new Vector3Dbl( 0, 0, 0 );

            // Test position within bounds.
            mockTarget.Position = new Vector3( 50, 0, 0 );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when position is within bounds" );

            // Test position exactly at bounds.
            mockTarget.Position = new Vector3( 100, 0, 0 );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when position is exactly at bounds" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [UnityTest]
        public IEnumerator TargetObject_SetExceedingPositionBounds_RequestsSwitch()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            var assertMonoBeh = go.AddComponent<AssertMonoBehaviour>();

            yield return new WaitForFixedUpdate();

            sman.MaxRelativePosition = 100f;
            sman.MaxRelativeVelocity = 1000000f;

            var mockTarget = new MockReferenceFrameTransform();
            Vector3Dbl expectedAbsolutePosition = new Vector3Dbl( 1000, 2000, 3000 );
            Vector3Dbl expectedAbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
            mockTarget.AbsolutePosition = expectedAbsolutePosition;
            mockTarget.AbsoluteVelocity = expectedAbsoluteVelocity;

            mockTarget.Position = new Vector3( 150, 0, 0 );
            sman.targetObject = mockTarget;

            // Switch requested immediately, but actually switched after the next physics step.
            Assert.That( sman.IsSwitchRequested, Is.True );

            bool fixedUpdateRan = false;
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, () => true, isOneShot: true, ( frameInfo ) =>
            {
                Assert.That( sman.IsSwitchRequested, Is.True );
                fixedUpdateRan = true;
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, () => fixedUpdateRan, isOneShot: true, ( frameInfo ) =>
            {
                Assert.That( sman.IsSwitchRequested, Is.False );

                IReferenceFrame frame = sman.referenceFrame;
                Assert.That( frame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ) );
                Assert.That( frame.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( expectedAbsoluteVelocity ).Using( vector3DblApproxComparer ) );
            } );

            assertMonoBeh.Enable();

            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds( 0.1f );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [Test]
        public void TargetObject_SetWithinVelocityBounds_DoesNotRequestSwitch()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            sman.MaxRelativePosition = 1000000f;
            sman.MaxRelativeVelocity = 50f;

            var mockTarget = new MockReferenceFrameTransform();
            mockTarget.AbsolutePosition = new Vector3Dbl( 0, 0, 0 );
            mockTarget.AbsoluteVelocity = new Vector3Dbl( 0, 0, 0 );

            // Test velocity within bounds.
            mockTarget.Velocity = new Vector3( 0, 0, 25 );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when velocity is within bounds" );

            // Test velocity exactly at bounds.
            mockTarget.Velocity = new Vector3( 0, 0, 50 );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when velocity is exactly at bounds" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [UnityTest]
        public IEnumerator TargetObject_SetExceedingVelocityBounds_RequestsSwitch()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            var assertMonoBeh = go.AddComponent<AssertMonoBehaviour>();

            yield return new WaitForFixedUpdate();

            sman.MaxRelativePosition = 1000000f;
            sman.MaxRelativeVelocity = 50f;

            var mockTarget = new MockReferenceFrameTransform();
            Vector3Dbl expectedAbsolutePosition = new Vector3Dbl( 500, 1000, 1500 );
            Vector3Dbl expectedAbsoluteVelocity = new Vector3Dbl( 5, 10, 15 );
            mockTarget.AbsolutePosition = expectedAbsolutePosition;
            mockTarget.AbsoluteVelocity = expectedAbsoluteVelocity;

            // Test velocity exceeding bounds.
            mockTarget.Velocity = new Vector3( 0, 0, 75 );
            sman.targetObject = mockTarget;

            Assert.That( sman.IsSwitchRequested, Is.True );

            bool fixedUpdateRan = false;
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, () => true, isOneShot: true, ( frameInfo ) =>
            {
                Assert.That( sman.IsSwitchRequested, Is.True );
                fixedUpdateRan = true;
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, () => fixedUpdateRan, isOneShot: true, ( frameInfo ) =>
            {
                Assert.That( sman.IsSwitchRequested, Is.False );

                IReferenceFrame frame = sman.referenceFrame;
                Assert.That( frame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ) );
                Assert.That( frame.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( expectedAbsoluteVelocity ).Using( vector3DblApproxComparer ) );
            } );

            assertMonoBeh.Enable();

            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds( 0.1f );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [Test]
        public void RequestReferenceFrameSwitch_MismatchingUT_ThrowsArgumentException()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 100.0 );

            IReferenceFrame mismatchedFrame = new CenteredReferenceFrame( 200.0, Vector3Dbl.zero );

            Assert.Throws<ArgumentException>( () =>
            {
                sman.RequestReferenceFrameSwitch( mismatchedFrame );
            } );

            mismatchedFrame = new CenteredReferenceFrame( 0.0, Vector3Dbl.zero );

            Assert.Throws<ArgumentException>( () =>
            {
                sman.RequestReferenceFrameSwitch( mismatchedFrame );
            } );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [Test]
        public void RequestReferenceFrameSwitch_MatchingUT_DoesntThrow()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 100.0 );

            IReferenceFrame validFrame = new CenteredReferenceFrame( 100.0, Vector3Dbl.zero );

            Assert.DoesNotThrow( () =>
            {
                sman.RequestReferenceFrameSwitch( validFrame );
            } );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [Test]
        public void IsSwitchRequested_NoSwitchQueued_ReturnsCorrectValue()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            SceneReferenceFrameManager sman = go.AddComponent<SceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            Assert.That( sman.IsSwitchRequested, Is.False, "IsSwitchRequested should be false when no switch is queued" );

            IReferenceFrame newFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );
            sman.RequestReferenceFrameSwitch( newFrame );

            Assert.That( sman.IsSwitchRequested, Is.True, "IsSwitchRequested should be true when a switch is queued" );

            UnityEngine.Object.DestroyImmediate( go );
        }
    }
}