using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.ReferenceFrames;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode.ReferenceFrames
{
    public class TransformVelocityIntegrationTests : ReferenceFrameTransformTestBase
    {
        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, -1, 2, 3, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator VelocityIntegration( Type transformType, double vx, double vy, double vz, double ax, double ay, double az )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( ax, ay, az );

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            using var history = new HistoryRecorder( 10f );
            yield return new WaitForFixedUpdate();
            double startUT = TimeManager.UT;

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestTransformState>( () => ExtractState( sut ) );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestTransformState>( () =>
            {
                var s = ExtractState( sut );
                s.UT = TimeManager.UT + (TimeManager.UT - TimeManager.OldUT);
                return s;
            } );

            yield return new WaitForSeconds( 0.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 3; i++ )
            {
                timeline
                    .NextFixedUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) )
                    .NextUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) );
            }

            DestroySut( sut );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, -1, 2, 3, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator VelocityIntegration_WithSwitching( Type transformType, double vx, double vy, double vz, double ax, double ay, double az )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( ax, ay, az );

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            var scheduledSwitches = new double[] { 0.05, 0.15, 0.25 };
            var scheduledFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition() ),
                () => new OrientedReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteRotation() ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteVelocity() ),
            };
            int nextSwitchIndex = 0;

            using var history = new HistoryRecorder( 10f );
            yield return new WaitForFixedUpdate();
            double startUT = TimeManager.UT;

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestTransformState>( () =>
            {
                double deltaTime = TimeManager.UT - startUT;
                if( nextSwitchIndex < scheduledSwitches.Length && deltaTime >= scheduledSwitches[nextSwitchIndex] )
                {
                    refFrameManager.RequestReferenceFrameSwitch( scheduledFrames[nextSwitchIndex].Invoke() );
                    nextSwitchIndex++;
                }
                return ExtractState( sut );
            } );

            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestTransformState>( () =>
            {
                var s = ExtractState( sut );
                s.UT = TimeManager.UT + (TimeManager.UT - TimeManager.OldUT);
                return s;
            } );

            yield return new WaitForSeconds( 0.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 3; i++ )
            {
                timeline
                    .NextFixedUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) )
                    .NextUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) );
            }

            DestroySut( sut );
        }
    }
}
