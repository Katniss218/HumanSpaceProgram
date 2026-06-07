using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityPlus.PlayerLoop;

namespace HSP_Tests_PlayMode.ReferenceFrames
{
    public class SceneReferenceFrameManagerTests
    {
        private static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );

        private struct TestFrameState
        {
            public IReferenceFrame ReferenceFrame;
            public bool IsSwitchRequested;
            public double UT;
            public double OldUT;
        }

        [UnityTest]
        public IEnumerator MovingReferenceFrame_PropagatesCorrectly()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            using var history = new HistoryRecorder( 5f );

            yield return new WaitForFixedUpdate();

            double startUT = TimeManager.UT;

            const double velocity = 10;
            IReferenceFrame cif = new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( velocity, 0, 0 ) );
            sman.RequestReferenceFrameSwitch( cif );

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestFrameState>( () => new TestFrameState()
            {
                ReferenceFrame = sman.referenceFrame,
                UT = TimeManager.UT,
                OldUT = TimeManager.OldUT
            } );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestFrameState>( () => new TestFrameState()
            {
                ReferenceFrame = sman.referenceFrame,
                UT = TimeManager.UT,
                OldUT = TimeManager.OldUT
            } );
            history.Record<UnityPlus.PlayerLoop.Phases.LateUpdate, TestFrameState>( () => new TestFrameState()
            {
                ReferenceFrame = sman.referenceFrame,
                UT = TimeManager.UT,
                OldUT = TimeManager.OldUT
            } );

            yield return new WaitForSeconds( 1 );

            var track = history.GetHistory<TestFrameState>();
            Assert.That( track, Is.Not.Empty );

            foreach( var fu in track.InPhase<TestFrameState, UnityPlus.PlayerLoop.Phases.FixedUpdate>() )
            {
                Vector3Dbl expectedPos = new Vector3Dbl( velocity * (fu.Data.OldUT - startUT), 0, 0 );
                fu.AssertState(
                    d => d.ReferenceFrame.TransformPosition( Vector3Dbl.zero ),
                    Is.EqualTo( expectedPos ).Using( vector3DblApproxComparer ),
                    "ReferenceFrame.TransformPosition(Vector3Dbl.zero)"
                );
            }

            foreach( var u in track.InPhase<TestFrameState, UnityPlus.PlayerLoop.Phases.Update>() )
            {
                Vector3Dbl expectedPos = new Vector3Dbl( velocity * (u.Data.UT - startUT), 0, 0 );
                u.AssertState(
                    d => d.ReferenceFrame.TransformPosition( Vector3Dbl.zero ),
                    Is.EqualTo( expectedPos ).Using( vector3DblApproxComparer ),
                    "ReferenceFrame.TransformPosition(Vector3Dbl.zero)"
                );
            }

            foreach( var lu in track.InPhase<TestFrameState, UnityPlus.PlayerLoop.Phases.LateUpdate>() )
            {
                Vector3Dbl expectedPos = new Vector3Dbl( velocity * (lu.Data.UT - startUT), 0, 0 );
                lu.AssertState(
                    d => d.ReferenceFrame.TransformPosition( Vector3Dbl.zero ),
                    Is.EqualTo( expectedPos ).Using( vector3DblApproxComparer ),
                    "ReferenceFrame.TransformPosition(Vector3Dbl.zero)"
                );
            }

            UnityEngine.Object.DestroyImmediate( go );
        }

        [UnityTest]
        public IEnumerator RequestReferenceFrameSwitch_ReferenceFrameUpdatesAfterPhysicsProcessing()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            using var history = new HistoryRecorder( 5f );

            yield return new WaitForFixedUpdate();

            IReferenceFrame initialFrame = sman.referenceFrame;

            IReferenceFrame newFrame = new CenteredReferenceFrame( TimeManager.UT, new Vector3Dbl( 100, 0, 0 ) );
            sman.RequestReferenceFrameSwitch( newFrame );

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestFrameState>( () => new TestFrameState()
            {
                ReferenceFrame = sman.referenceFrame
            } );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestFrameState>( () => new TestFrameState()
            {
                ReferenceFrame = sman.referenceFrame
            } );

            yield return new WaitForSeconds( 1.1f );

            history.AssertTimeline<TestFrameState>()
                .StartingHere( d => d.ReferenceFrame, Is.EqualTo( initialFrame ), "ReferenceFrame" )
                .NextFixedUpdate().Verify( d => d.ReferenceFrame, Is.EqualTo( initialFrame ), "ReferenceFrame" )
                .NextUpdate().Verify( d => d.ReferenceFrame, Is.EqualTo( newFrame ), "ReferenceFrame" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [UnityTest]
        public IEnumerator TargetObject_Moving_RequestsSwitchWhenExceedingBounds()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            using var history = new HistoryRecorder( 5f );

            yield return new WaitForFixedUpdate();

            sman.MaxRelativePosition = 100f;
            sman.MaxRelativeVelocity = 1000000f;

            var mockTarget = new MockReferenceFrameTransform();
            mockTarget.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            Vector3Dbl expectedAbsolutePosition = new Vector3Dbl( 150, 0, 0 );
            Vector3Dbl expectedAbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
            mockTarget.SetAbsolutePosition( new Vector3Dbl( 50, 0, 0 ) );
            mockTarget.SetAbsoluteVelocity( expectedAbsoluteVelocity );

            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False );

            yield return new WaitForFixedUpdate();

            Assert.That( sman.IsSwitchRequested, Is.False );

            mockTarget.SetAbsolutePosition( expectedAbsolutePosition );

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestFrameState>( () => new TestFrameState()
            {
                IsSwitchRequested = sman.IsSwitchRequested,
                ReferenceFrame = sman.referenceFrame
            } );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestFrameState>( () => new TestFrameState()
            {
                IsSwitchRequested = sman.IsSwitchRequested,
                ReferenceFrame = sman.referenceFrame
            } );

            yield return new WaitForSeconds( 1.1f );

            history.AssertTimeline<TestFrameState>()
                .StartingHere()
                .NextFixedUpdate().Verify( d => d.IsSwitchRequested, Is.False, "IsSwitchRequested" )
                .NextFixedUpdate().Verify( d => d.IsSwitchRequested, Is.False, "IsSwitchRequested" )
                .Verify( d => d.ReferenceFrame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ), "ReferenceFrame.TransformPosition(Vector3Dbl.zero)" )
                .Verify( d => d.ReferenceFrame.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( expectedAbsoluteVelocity ).Using( vector3DblApproxComparer ), "ReferenceFrame.TransformVelocity(Vector3Dbl.zero)" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [Test]
        public void TargetObject_SetWithinPositionBounds_DoesNotRequestSwitch()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            sman.MaxRelativePosition = 100f;
            sman.MaxRelativeVelocity = 1000000f;

            var mockTarget = new MockReferenceFrameTransform();
            mockTarget.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            mockTarget.SetAbsolutePosition( new Vector3Dbl( 50, 0, 0 ) );
            mockTarget.SetAbsoluteVelocity( new Vector3Dbl( 0, 0, 0 ) );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when position is within bounds" );

            mockTarget.SetAbsolutePosition( new Vector3Dbl( 100, 0, 0 ) );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when position is exactly at bounds" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [UnityTest]
        public IEnumerator TargetObject_SetExceedingPositionBounds_RequestsSwitch()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            using var history = new HistoryRecorder( 5f );

            yield return new WaitForFixedUpdate();

            sman.MaxRelativePosition = 100f;
            sman.MaxRelativeVelocity = 1000000f;

            var mockTarget = new MockReferenceFrameTransform();
            mockTarget.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            Vector3Dbl expectedAbsolutePosition = new Vector3Dbl( 1000, 2000, 3000 );
            Vector3Dbl expectedAbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
            mockTarget.SetAbsolutePosition( expectedAbsolutePosition );
            mockTarget.SetAbsoluteVelocity( expectedAbsoluteVelocity );

            sman.targetObject = mockTarget;

            Assert.That( sman.IsSwitchRequested, Is.True );

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestFrameState>( () => new TestFrameState()
            {
                IsSwitchRequested = sman.IsSwitchRequested,
                ReferenceFrame = sman.referenceFrame
            } );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestFrameState>( () => new TestFrameState()
            {
                IsSwitchRequested = sman.IsSwitchRequested,
                ReferenceFrame = sman.referenceFrame
            } );

            yield return new WaitForSeconds( 1.1f );

            history.AssertTimeline<TestFrameState>()
                .StartingHere()
                .NextFixedUpdate().Verify( d => d.IsSwitchRequested, Is.True, "IsSwitchRequested" )
                .NextFixedUpdate().Verify( d => d.IsSwitchRequested, Is.False, "IsSwitchRequested" )
                .Verify( d => d.ReferenceFrame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ), "ReferenceFrame.TransformPosition(Vector3Dbl.zero)" )
                .Verify( d => d.ReferenceFrame.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( expectedAbsoluteVelocity ).Using( vector3DblApproxComparer ), "ReferenceFrame.TransformVelocity(Vector3Dbl.zero)" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [Test]
        public void TargetObject_SetWithinVelocityBounds_DoesNotRequestSwitch()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            sman.MaxRelativePosition = 1000000f;
            sman.MaxRelativeVelocity = 50f;

            var mockTarget = new MockReferenceFrameTransform();
            mockTarget.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            mockTarget.SetAbsolutePosition( new Vector3Dbl( 0, 0, 0 ) );
            mockTarget.SetAbsoluteVelocity( new Vector3Dbl( 0, 0, 25 ) );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when velocity is within bounds" );

            mockTarget.SetAbsoluteVelocity( new Vector3Dbl( 0, 0, 50 ) );
            sman.targetObject = mockTarget;
            Assert.That( sman.IsSwitchRequested, Is.False, "No switch should be requested when velocity is exactly at bounds" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [UnityTest]
        public IEnumerator TargetObject_SetExceedingVelocityBounds_RequestsSwitch()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            using var history = new HistoryRecorder( 5f );

            yield return new WaitForFixedUpdate();

            sman.MaxRelativePosition = 1000000f;
            sman.MaxRelativeVelocity = 50f;

            var mockTarget = new MockReferenceFrameTransform();
            mockTarget.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            Vector3Dbl expectedAbsolutePosition = new Vector3Dbl( 500, 1000, 1500 );
            Vector3Dbl expectedAbsoluteVelocity = new Vector3Dbl( 0, 0, 75 );
            mockTarget.SetAbsolutePosition( expectedAbsolutePosition );
            mockTarget.SetAbsoluteVelocity( expectedAbsoluteVelocity );

            sman.targetObject = mockTarget;

            Assert.That( sman.IsSwitchRequested, Is.True );

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestFrameState>( () => new TestFrameState()
            {
                IsSwitchRequested = sman.IsSwitchRequested,
                ReferenceFrame = sman.referenceFrame
            } );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestFrameState>( () => new TestFrameState()
            {
                IsSwitchRequested = sman.IsSwitchRequested,
                ReferenceFrame = sman.referenceFrame
            } );

            yield return new WaitForSeconds( 1.1f );

            history.AssertTimeline<TestFrameState>()
                .StartingHere()
                .NextFixedUpdate().Verify( d => d.IsSwitchRequested, Is.True, "IsSwitchRequested" )
                .NextFixedUpdate().Verify( d => d.IsSwitchRequested, Is.False, "IsSwitchRequested" )
                .Verify( d => d.ReferenceFrame.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ), "ReferenceFrame.TransformPosition(Vector3Dbl.zero)" )
                .Verify( d => d.ReferenceFrame.TransformVelocity( Vector3Dbl.zero ), Is.EqualTo( expectedAbsoluteVelocity ).Using( vector3DblApproxComparer ), "ReferenceFrame.TransformVelocity(Vector3Dbl.zero)" );

            UnityEngine.Object.DestroyImmediate( go );
        }

        [Test]
        public void RequestReferenceFrameSwitch_MismatchingUT_ThrowsArgumentException()
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
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
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
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
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );

            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager sman = go.AddComponent<GameplaySceneReferenceFrameManager>();
            TimeManager.SetUT( 0 );

            Assert.That( sman.IsSwitchRequested, Is.False, "IsSwitchRequested should be false when no switch is queued" );

            IReferenceFrame newFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );
            sman.RequestReferenceFrameSwitch( newFrame );

            Assert.That( sman.IsSwitchRequested, Is.True, "IsSwitchRequested should be true when a switch is queued" );

            UnityEngine.Object.DestroyImmediate( go );
        }
    }
}