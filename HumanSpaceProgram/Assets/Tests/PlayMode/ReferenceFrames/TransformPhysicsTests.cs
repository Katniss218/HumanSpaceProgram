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
    public class TransformPhysicsTests : ReferenceFrameTransformTestBase
    {
        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator ForceApplication( Type transformType, double vx, double vy, double vz )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );
            IPhysicsTransform physicsSut = (IPhysicsTransform)sut;

            Vector3Dbl initialPosition = new Vector3Dbl( 0, 0, 0 );
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            physicsSut.Mass = 1000f;
            physicsSut.MomentsOfInertia = new Vector3( 1000f, 1000f, 1000f );

            Vector3 forceVectorInSceneSpace = new Vector3( 1000f, 0f, 0f );
            Vector3 forceVectorInAbsoluteSpace = new Vector3( 500f, 0f, 0f );

            yield return new WaitForFixedUpdate();

            Vector3Dbl expectedPosition = sut.GetAbsolutePosition();
            Vector3Dbl expectedVelocity = sut.GetAbsoluteVelocity();
            Vector3Dbl expectedAcceleration = sut.GetAbsoluteAcceleration();

            using var history = new HistoryRecorder( 10f );

            bool isFirstCallForForce = true;
            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestTransformState>( () =>
            {
                if( !isFirstCallForForce )
                {
                    physicsSut.AddForce( forceVectorInSceneSpace );
                    physicsSut.AddAbsoluteForce( forceVectorInAbsoluteSpace );
                }
                else
                {
                    isFirstCallForForce = false;
                }
                return ExtractState( sut );
            } );

            yield return new WaitForSeconds( 0.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 3; i++ )
            {
                var prevExpectedPosition = expectedPosition;
                var prevExpectedVelocity = expectedVelocity;

                Vector3Dbl absoluteFromScene = forceVectorInSceneSpace;
                Vector3Dbl absoluteFromAbsolute = forceVectorInAbsoluteSpace;
                var force = absoluteFromScene + absoluteFromAbsolute;

                var nextAcceleration = force / physicsSut.Mass;
                var nextVelocity = prevExpectedVelocity + nextAcceleration * TimeManager.FixedDeltaTime;
                var nextPosition = prevExpectedPosition + nextVelocity * TimeManager.FixedDeltaTime;

                expectedPosition = nextPosition;
                expectedVelocity = nextVelocity;
                expectedAcceleration = nextAcceleration;

                IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;
                var valExpectedPosition = prevExpectedPosition;
                var valExpectedVelocity = prevExpectedVelocity;
                var valExpectedAcceleration = nextAcceleration;

                timeline.NextFixedUpdate().Verify( ( d ) =>
                {
                    Assert.That( d.Position, Is.EqualTo( valExpectedPosition ).Using( vector3DblApproxComparer ), "Absolute position should match expected position after force application" );
                    Assert.That( d.Velocity, Is.EqualTo( valExpectedVelocity ).Using( vector3DblApproxComparer ), "Absolute velocity should match expected velocity after force application" );
                    Assert.That( d.Acceleration, Is.EqualTo( valExpectedAcceleration ).Using( vector3DblApproxComparer ), "Absolute acceleration should match expected acceleration from forces" );

                    Assert.That( d.LocalPosition, Is.EqualTo( (Vector3)sceneRef.InverseTransformPosition( valExpectedPosition ) ).Using( vector3ApproxComparer ), "Local scene position should match expected transformed position" );
                    Assert.That( d.LocalVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformVelocity( valExpectedVelocity ) ).Using( vector3ApproxComparer ), "Local scene velocity should match expected transformed velocity" );
                    Assert.That( d.LocalAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAcceleration( valExpectedAcceleration ) ).Using( vector3ApproxComparer ), "Local scene acceleration should match expected transformed acceleration" );
                } );
            }

            DestroySut( sut );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator TorqueApplication( Type transformType, double vx, double vy, double vz )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );
            IPhysicsTransform physicsSut = (IPhysicsTransform)sut;

            Vector3Dbl initialPosition = new Vector3Dbl( 0, 0, 0 );
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            physicsSut.Mass = 1000f;
            physicsSut.MomentsOfInertia = new Vector3( 1000f, 500f, 200f );

            Vector3 torqueInSceneSpace = new Vector3( 0f, 1000f, 0f );
            Vector3 torqueInAbsoluteSpace = new Vector3( 0f, 500f, 0f );

            yield return new WaitForFixedUpdate();

            QuaternionDbl expectedRotation = sut.GetAbsoluteRotation();
            Vector3Dbl expectedAngularVelocity = sut.GetAbsoluteAngularVelocity();
            Vector3Dbl expectedAngularAcceleration = sut.GetAbsoluteAngularAcceleration();

            using var history = new HistoryRecorder( 10f );

            bool isFirstCallForTorque = true;
            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestTransformState>( () =>
            {
                if( !isFirstCallForTorque )
                {
                    physicsSut.AddTorque( torqueInSceneSpace );
                    physicsSut.AddAbsoluteTorque( torqueInAbsoluteSpace );
                }
                else
                {
                    isFirstCallForTorque = false;
                }
                return ExtractState( sut );
            } );

            yield return new WaitForSeconds( 0.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 3; i++ )
            {
                var prevExpectedRotation = expectedRotation;
                var prevExpectedAngularVelocity = expectedAngularVelocity;

                Vector3Dbl absoluteFromSceneTorque = torqueInSceneSpace;
                Vector3Dbl absoluteFromAbsoluteTorque = torqueInAbsoluteSpace;
                var netTorque = absoluteFromSceneTorque + absoluteFromAbsoluteTorque;

                var I = physicsSut.MomentsOfInertia;
                Vector3Dbl nextAngularAcceleration = new Vector3Dbl(
                    netTorque.x / I.x,
                    netTorque.y / I.y,
                    netTorque.z / I.z
                );

                var nextAngularVelocity = prevExpectedAngularVelocity + nextAngularAcceleration * TimeManager.FixedDeltaTime;
                QuaternionDbl deltaRotation = QuaternionDbl.AngleAxis( nextAngularVelocity.magnitude * TimeManager.FixedDeltaTime * 57.29577951308232, nextAngularVelocity );
                var nextRotation = deltaRotation * prevExpectedRotation;

                expectedRotation = nextRotation;
                expectedAngularVelocity = nextAngularVelocity;
                expectedAngularAcceleration = nextAngularAcceleration;

                IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;
                var valExpectedRotation = prevExpectedRotation;
                var valExpectedAngularVelocity = prevExpectedAngularVelocity;
                var valExpectedAngularAcceleration = nextAngularAcceleration;

                timeline.NextFixedUpdate().Verify( ( d ) =>
                {
                    Assert.That( d.Rotation, Is.EqualTo( valExpectedRotation ).Using( quaternionDblApproxComparer ), "Absolute rotation should match expected rotation after torque application" );
                    Assert.That( d.AngularVelocity, Is.EqualTo( valExpectedAngularVelocity ).Using( vector3DblApproxComparer ), "Absolute angular velocity should match expected angular velocity after torque application" );
                    Assert.That( d.AngularAcceleration, Is.EqualTo( valExpectedAngularAcceleration ).Using( vector3DblApproxComparer ), "Absolute angular acceleration should match expected angular acceleration from torques" );

                    Assert.That( d.LocalRotation, Is.EqualTo( (Quaternion)sceneRef.InverseTransformRotation( valExpectedRotation ) ).Using( quaternionApproxComparer ), "Local scene rotation should match expected transformed rotation" );
                    Assert.That( d.LocalAngularVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularVelocity( valExpectedAngularVelocity ) ).Using( vector3ApproxComparer ), "Local scene angular velocity should match expected transformed angular velocity" );
                    Assert.That( d.LocalAngularAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularAcceleration( valExpectedAngularAcceleration ) ).Using( vector3ApproxComparer ), "Local scene angular acceleration should match expected transformed angular acceleration" );
                } );
            }

            DestroySut( sut );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator ForceAtPositionApplication( Type transformType, double vx, double vy, double vz )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );
            IPhysicsTransform physicsSut = (IPhysicsTransform)sut;

            Vector3Dbl initialPosition = new Vector3Dbl( 0, 0, 0 );
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            physicsSut.Mass = 1000f;
            physicsSut.MomentsOfInertia = new Vector3( 1000f, 1000f, 1000f );

            Vector3 forceInSceneSpace = new Vector3( 1000f, 0f, 0f );
            Vector3 pointInSceneSpace = new Vector3( 0.0f, 1.0f, 0.0f );
            Vector3 forceInAbsoluteSpace = new Vector3( 500f, 0f, 0f );
            Vector3 pointInAbsoluteSpace = new Vector3( 0.0f, -2.0f, 0.0f );

            yield return new WaitForFixedUpdate();

            Vector3Dbl expectedPosition = sut.GetAbsolutePosition();
            QuaternionDbl expectedRotation = sut.GetAbsoluteRotation();
            Vector3Dbl expectedVelocity = sut.GetAbsoluteVelocity();
            Vector3Dbl expectedAcceleration = sut.GetAbsoluteAcceleration();
            Vector3Dbl expectedAngularVelocity = sut.GetAbsoluteAngularVelocity();
            Vector3Dbl expectedAngularAcceleration = sut.GetAbsoluteAngularAcceleration();

            using var history = new HistoryRecorder( 10f );

            bool isFirstCallForForceAtPos = true;
            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestTransformState>( () =>
            {
                if( !isFirstCallForForceAtPos )
                {
                    physicsSut.AddForceAtPosition( forceInSceneSpace, pointInSceneSpace );
                    physicsSut.AddAbsoluteForceAtPosition( forceInAbsoluteSpace, pointInAbsoluteSpace );
                }
                else
                {
                    isFirstCallForForceAtPos = false;
                }
                return ExtractState( sut );
            } );

            yield return new WaitForSeconds( 0.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 3; i++ )
            {
                var prevExpectedPosition = expectedPosition;
                var prevExpectedRotation = expectedRotation;
                var prevExpectedVelocity = expectedVelocity;
                var prevExpectedAngularVelocity = expectedAngularVelocity;

                Vector3Dbl absForceFromScene = forceInSceneSpace;
                Vector3Dbl absPointFromScene = pointInSceneSpace;
                Vector3Dbl absForceFromAbsolute = forceInAbsoluteSpace;
                Vector3Dbl absPointFromAbsolute = pointInAbsoluteSpace;

                Vector3Dbl torqueFromScene = Vector3Dbl.Cross( absPointFromScene - prevExpectedPosition, absForceFromScene );
                Vector3Dbl torqueFromAbsolute = Vector3Dbl.Cross( absPointFromAbsolute - prevExpectedPosition, absForceFromAbsolute );
                Vector3Dbl netTorque = torqueFromScene + torqueFromAbsolute;

                var I = physicsSut.MomentsOfInertia;
                Vector3Dbl nextAngularAcceleration = new Vector3Dbl(
                    netTorque.x / I.x,
                    netTorque.y / I.y,
                    netTorque.z / I.z
                );
                var force = absForceFromScene + absForceFromAbsolute;

                var nextAcceleration = force / physicsSut.Mass;
                var nextVelocity = prevExpectedVelocity + nextAcceleration * TimeManager.FixedDeltaTime;
                var nextPosition = prevExpectedPosition + nextVelocity * TimeManager.FixedDeltaTime;
                var nextAngularVelocity = prevExpectedAngularVelocity + nextAngularAcceleration * TimeManager.FixedDeltaTime;
                QuaternionDbl deltaRotation = QuaternionDbl.AngleAxis( nextAngularVelocity.magnitude * TimeManager.FixedDeltaTime * 57.29577951308232, nextAngularVelocity );
                var nextRotation = deltaRotation * prevExpectedRotation;

                expectedPosition = nextPosition;
                expectedRotation = nextRotation;
                expectedVelocity = nextVelocity;
                expectedAngularVelocity = nextAngularVelocity;
                expectedAcceleration = nextAcceleration;
                expectedAngularAcceleration = nextAngularAcceleration;

                IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;
                var valExpectedPosition = prevExpectedPosition;
                var valExpectedRotation = prevExpectedRotation;
                var valExpectedVelocity = prevExpectedVelocity;
                var valExpectedAngularVelocity = prevExpectedAngularVelocity;
                var valExpectedAcceleration = nextAcceleration;
                var valExpectedAngularAcceleration = nextAngularAcceleration;

                timeline.NextFixedUpdate().Verify( ( d ) =>
                {
                    Assert.That( d.Position, Is.EqualTo( valExpectedPosition ).Using( vector3DblApproxComparer ), "Absolute position should match expected position after ForceAtPosition application" );
                    Assert.That( d.Rotation, Is.EqualTo( valExpectedRotation ).Using( quaternionDblApproxComparer ), "Absolute rotation should match expected rotation after ForceAtPosition application" );
                    Assert.That( d.Velocity, Is.EqualTo( valExpectedVelocity ).Using( vector3DblApproxComparer ), "Absolute velocity should match expected velocity after ForceAtPosition application" );
                    Assert.That( d.AngularVelocity, Is.EqualTo( valExpectedAngularVelocity ).Using( vector3DblApproxComparer ), "Absolute angular velocity should match expected angular velocity after ForceAtPosition application" );
                    Assert.That( d.Acceleration, Is.EqualTo( valExpectedAcceleration ).Using( vector3DblApproxComparer ), "Absolute acceleration should match expected acceleration from forces" );
                    Assert.That( d.AngularAcceleration, Is.EqualTo( valExpectedAngularAcceleration ).Using( vector3DblApproxComparer ), "Absolute angular acceleration should match expected angular acceleration from torques" );

                    Assert.That( d.LocalPosition, Is.EqualTo( (Vector3)sceneRef.InverseTransformPosition( valExpectedPosition ) ).Using( vector3ApproxComparer ), "Local scene position should match expected transformed position" );
                    Assert.That( d.LocalRotation, Is.EqualTo( (Quaternion)sceneRef.InverseTransformRotation( valExpectedRotation ) ).Using( quaternionApproxComparer ), "Local scene rotation should match expected transformed rotation" );
                    Assert.That( d.LocalVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformVelocity( valExpectedVelocity ) ).Using( vector3ApproxComparer ), "Local scene velocity should match expected transformed velocity" );
                    Assert.That( d.LocalAngularVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularVelocity( valExpectedAngularVelocity ) ).Using( vector3ApproxComparer ), "Local scene angular velocity should match expected transformed angular velocity" );
                    Assert.That( d.LocalAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAcceleration( valExpectedAcceleration ) ).Using( vector3ApproxComparer ), "Local scene acceleration should match expected transformed acceleration" );
                    Assert.That( d.LocalAngularAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularAcceleration( valExpectedAngularAcceleration ) ).Using( vector3ApproxComparer ), "Local scene angular acceleration should match expected transformed angular acceleration" );
                } );
            }

            DestroySut( sut );
        }
    }
}
