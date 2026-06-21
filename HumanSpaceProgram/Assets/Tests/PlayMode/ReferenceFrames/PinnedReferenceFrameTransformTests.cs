using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode.ReferenceFrames
{
    public class PinnedReferenceFrameTransformTests : ReferenceFrameTransformTestBase
    {
        private struct PinnedSimState : IWithSimulationTime
        {
            public float GetSimulationTime() => (float)UT;
            public double UT { get; set; }
            public Vector3Dbl AbsolutePinnedPos { get; set; }
            public QuaternionDbl AbsolutePinnedRot { get; set; }
            public Vector3Dbl AbsolutePinnedVel { get; set; }
            public Vector3Dbl AbsolutePinnedAngVel { get; set; }

            public Vector3 LocalPinnedPos { get; set; }
            public Quaternion LocalPinnedRot { get; set; }
            public Vector3 LocalPinnedVel { get; set; }
            public Vector3 LocalPinnedAngVel { get; set; }
            public Vector3 LocalPinnedAcc { get; set; }
            public Vector3 LocalPinnedAngAcc { get; set; }

            public IReferenceFrame SceneRefFrame { get; set; }
        }

        private static void VerifyPinnedTestState( PinnedSimState state, Vector3Dbl initialPosition, QuaternionDbl initialRotation, Vector3Dbl initialVelocity, Vector3Dbl initialAngularVelocity, Vector3Dbl pinnedPos, QuaternionDbl pinnedRot, double startUT )
        {
            double deltaTime = state.UT - startUT;

            Vector3Dbl expectedRefAbsPos = initialPosition + (initialVelocity * deltaTime);

            double omegaMag = initialAngularVelocity.magnitude;
            Vector3Dbl axis = omegaMag > 0.0 ? initialAngularVelocity.normalized : new Vector3Dbl( 1, 0, 0 );
            double angle = omegaMag * deltaTime;
            QuaternionDbl expectedRefAbsRot = QuaternionDbl.AngleAxis( angle * 57.29577951308232, axis ) * initialRotation;

            Vector3Dbl expectedPinnedAbsPos = expectedRefAbsPos + (expectedRefAbsRot * pinnedPos);
            QuaternionDbl expectedPinnedAbsRot = expectedRefAbsRot * pinnedRot;

            Vector3Dbl tangential = Vector3Dbl.Cross( initialAngularVelocity, expectedRefAbsRot * pinnedPos );
            Vector3Dbl expectedPinnedAbsVel = initialVelocity + tangential;
            Vector3Dbl expectedPinnedAbsAngVel = initialAngularVelocity;

            IReferenceFrame sceneRef = state.SceneRefFrame;

            Vector3 expectedPinnedScenePos = (Vector3)sceneRef.InverseTransformPosition( expectedPinnedAbsPos );
            Quaternion expectedPinnedSceneRot = (Quaternion)sceneRef.InverseTransformRotation( expectedPinnedAbsRot );
            Vector3 expectedPinnedSceneVel = (Vector3)sceneRef.InverseTransformVelocity( expectedPinnedAbsVel );
            Vector3 expectedPinnedSceneAngVel = (Vector3)sceneRef.InverseTransformAngularVelocity( expectedPinnedAbsAngVel );
            Vector3 expectedPinnedSceneAcc = (Vector3)sceneRef.InverseTransformAcceleration( Vector3Dbl.zero );
            Vector3 expectedPinnedSceneAngAcc = (Vector3)sceneRef.InverseTransformAngularAcceleration( Vector3Dbl.zero );

            Assert.That( state.AbsolutePinnedPos, Is.EqualTo( expectedPinnedAbsPos ).Using( vector3DblApproxComparer ), "Pinned absolute position should match expected relative position to moving reference" );
            Assert.That( state.AbsolutePinnedRot, Is.EqualTo( expectedPinnedAbsRot ).Using( quaternionDblApproxComparer ), "Pinned absolute rotation should match expected relative rotation to moving reference" );
            Assert.That( state.AbsolutePinnedVel, Is.EqualTo( expectedPinnedAbsVel ).Using( vector3DblApproxComparer ), "Pinned absolute velocity should match expected tangential velocity from rotating reference" );
            Assert.That( state.AbsolutePinnedAngVel, Is.EqualTo( expectedPinnedAbsAngVel ).Using( vector3DblApproxComparer ), "Pinned absolute angular velocity should match reference angular velocity" );

            Assert.That( state.LocalPinnedPos, Is.EqualTo( expectedPinnedScenePos ).Using( vector3ApproxComparer ), "Pinned local scene position should match expected transformed position" );
            Assert.That( state.LocalPinnedRot, Is.EqualTo( expectedPinnedSceneRot ).Using( quaternionApproxComparer ), "Pinned local scene rotation should match expected transformed rotation" );
            Assert.That( state.LocalPinnedVel, Is.EqualTo( expectedPinnedSceneVel ).Using( vector3ApproxComparer ), "Pinned local scene velocity should match expected transformed velocity" );
            Assert.That( state.LocalPinnedAngVel, Is.EqualTo( expectedPinnedSceneAngVel ).Using( vector3ApproxComparer ), "Pinned local scene angular velocity should match expected transformed angular velocity" );
            Assert.That( state.LocalPinnedAcc, Is.EqualTo( expectedPinnedSceneAcc ).Using( vector3ApproxComparer ), "Pinned local scene acceleration should match expected transformed acceleration" );
            Assert.That( state.LocalPinnedAngAcc, Is.EqualTo( expectedPinnedSceneAngAcc ).Using( vector3ApproxComparer ), "Pinned local scene angular acceleration should match expected transformed angular acceleration" );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator VelocityIntegration_WithPinned_WithSwitching( Type transformType, double vx, double vy, double vz )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );
            GameObject pinned = new GameObject( "Pinned" );
            PinnedReferenceFrameTransform pinnedSutConcrete = pinned.AddComponent<PinnedReferenceFrameTransform>();
            IReferenceFrameTransform pinnedSut = pinnedSutConcrete;
            pinnedSutConcrete.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            Vector3Dbl pinPosition = new Vector3Dbl( 5, 10, 0 );
            QuaternionDbl pinRotation = Quaternion.Euler( 45, 90, 135 );
            pinnedSutConcrete.SetReference( sut, pinPosition, pinRotation );

            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( 1, 2, 3 );

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

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, PinnedSimState>( () =>
            {
                double deltaTime = TimeManager.UT - startUT;
                if( nextSwitchIndex < scheduledSwitches.Length && deltaTime >= scheduledSwitches[nextSwitchIndex] )
                {
                    refFrameManager.RequestReferenceFrameSwitch( scheduledFrames[nextSwitchIndex].Invoke() );
                    nextSwitchIndex++;
                }

                return new PinnedSimState()
                {
                    UT = TimeManager.UT,
                    AbsolutePinnedPos = pinnedSut.GetAbsolutePosition(),
                    AbsolutePinnedRot = pinnedSut.GetAbsoluteRotation(),
                    AbsolutePinnedVel = pinnedSut.GetAbsoluteVelocity(),
                    AbsolutePinnedAngVel = pinnedSut.GetAbsoluteAngularVelocity(),

                    LocalPinnedPos = pinnedSut.GetPosition(),
                    LocalPinnedRot = pinnedSut.GetRotation(),
                    LocalPinnedVel = pinnedSut.GetVelocity(),
                    LocalPinnedAngVel = pinnedSut.GetAngularVelocity(),
                    LocalPinnedAcc = pinnedSut.GetAcceleration(),
                    LocalPinnedAngAcc = pinnedSut.GetAngularAcceleration(),

                    SceneRefFrame = GameplaySceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT )
                };
            } );

            history.Record<UnityPlus.PlayerLoop.Phases.Update, PinnedSimState>( () => new PinnedSimState()
            {
                UT = TimeManager.UT + (TimeManager.UT - TimeManager.OldUT),
                AbsolutePinnedPos = pinnedSut.GetAbsolutePosition(),
                AbsolutePinnedRot = pinnedSut.GetAbsoluteRotation(),
                AbsolutePinnedVel = pinnedSut.GetAbsoluteVelocity(),
                AbsolutePinnedAngVel = pinnedSut.GetAbsoluteAngularVelocity(),

                LocalPinnedPos = pinnedSut.GetPosition(),
                LocalPinnedRot = pinnedSut.GetRotation(),
                LocalPinnedVel = pinnedSut.GetVelocity(),
                LocalPinnedAngVel = pinnedSut.GetAngularVelocity(),
                LocalPinnedAcc = pinnedSut.GetAcceleration(),
                LocalPinnedAngAcc = pinnedSut.GetAngularAcceleration(),

                SceneRefFrame = GameplaySceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT )
            } );

            yield return new WaitForSeconds( 0.5f );

            var timeline = history.AssertTimeline<PinnedSimState>().StartingHere();
            for( int i = 0; i < 3; i++ )
            {
                timeline
                    .NextFixedUpdate().Verify( d => VerifyPinnedTestState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, startUT ) )
                    .NextUpdate().Verify( d => VerifyPinnedTestState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, startUT ) );
            }

            DestroySut( sut );
            UnityEngine.Object.DestroyImmediate( pinned );
        }
    }
}
