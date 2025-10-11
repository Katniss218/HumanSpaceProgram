using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP_Tests_PlayMode.NUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace HSP_Tests_PlayMode
{
    public class ReferenceFrametransformTests2
    {
        static IEqualityComparer<Vector3> vector3ApproxComparer = new Vector3ApproximateComparer( 0.0001f );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );
        static IEqualityComparer<Quaternion> quaternionApproxComparer = new QuaternionApproximateComparer( 0.0001f );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0001 );

        /// <summary>
        /// Creates a test scene with TimeManager and ReferenceFrameManager
        /// </summary>
        static (GameObject manager, TimeManager timeManager, GameplaySceneReferenceFrameManager refFrameManager, AssertMonoBehaviour assertMonoBeh) CreateTestScene()
        {
            GameObject manager = new GameObject( "TestManager" );
            TimeManager timeManager = manager.AddComponent<TimeManager>();
            TimeManager.SetUT( 0 );
            GameplaySceneReferenceFrameManager refFrameManager = manager.AddComponent<GameplaySceneReferenceFrameManager>();
            GameplaySceneReferenceFrameManager.Instance = refFrameManager;
            KinematicReferenceFrameTransform.AddPlayerLoopSystem();
            var assertMonoBeh = manager.AddComponent<AssertMonoBehaviour>();

            return (manager, timeManager, refFrameManager, assertMonoBeh);
        }

        static IReferenceFrameTransform CreateObject( Type type, ISceneReferenceFrameProvider provider )
        {
            if( type == null && !typeof( IReferenceFrameTransform ).IsAssignableFrom( type ) )
            {
                throw new ArgumentException( "Type must be non-null and implement IReferenceFrameTransform", nameof( type ) );
            }

            GameObject go = new GameObject();

            IReferenceFrameTransform trans = (IReferenceFrameTransform)go.AddComponent( type );
            trans.SceneReferenceFrameProvider = provider;

            return trans;
        }

        private static void AssertCorrectReferenceFrameTransformValues( IReferenceFrameTransform sut, Vector3Dbl initialPosition, QuaternionDbl initialRotation, Vector3Dbl initialVelocity, Vector3Dbl initialAngularVelocity, double deltaTime )
        {
            Vector3Dbl expectedAbsPos = initialPosition + (initialVelocity * deltaTime);

            double omegaMag = initialAngularVelocity.magnitude;
            Vector3Dbl axis = omegaMag > 0.0 ? initialAngularVelocity.normalized : new Vector3Dbl( 1, 0, 0 );
            double angle = omegaMag * deltaTime;
            QuaternionDbl expectedAbsRot;
            expectedAbsRot = QuaternionDbl.AngleAxis( angle * 57.29577951308232, axis ) * initialRotation;

            Vector3Dbl expectedAbsVel = initialVelocity;
            Vector3Dbl expectedAbsAngVel = initialAngularVelocity;

            Vector3Dbl expectedAbsAcc = Vector3Dbl.zero;
            Vector3Dbl expectedAbsAngAcc = Vector3Dbl.zero;

            IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;

            Vector3 expectedScenePos = (Vector3)sceneRef.InverseTransformPosition( expectedAbsPos );
            Quaternion expectedSceneRot = (Quaternion)sceneRef.InverseTransformRotation( expectedAbsRot );
            Vector3 expectedSceneVel = (Vector3)sceneRef.InverseTransformVelocity( expectedAbsVel );
            Vector3 expectedSceneAngVel = (Vector3)sceneRef.InverseTransformAngularVelocity( expectedAbsAngVel );
            Vector3 expectedSceneAcc = (Vector3)sceneRef.InverseTransformAcceleration( expectedAbsAcc );
            Vector3 expectedSceneAngAcc = (Vector3)sceneRef.InverseTransformAngularAcceleration( expectedAbsAngAcc );

            Assert.That( sut.AbsolutePosition, Is.EqualTo( expectedAbsPos ).Using( vector3DblApproxComparer ) );
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( expectedAbsRot ).Using( quaternionDblApproxComparer ) );
            Assert.That( sut.AbsoluteVelocity, Is.EqualTo( expectedAbsVel ).Using( vector3DblApproxComparer ) );
            Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( expectedAbsAngVel ).Using( vector3DblApproxComparer ) );
            Assert.That( sut.AbsoluteAcceleration, Is.EqualTo( expectedAbsAcc ).Using( vector3DblApproxComparer ) );
            Assert.That( sut.AbsoluteAngularAcceleration, Is.EqualTo( expectedAbsAngAcc ).Using( vector3DblApproxComparer ) );

            Assert.That( sut.Position, Is.EqualTo( expectedScenePos ).Using( vector3ApproxComparer ) );
            Assert.That( sut.Rotation, Is.EqualTo( expectedSceneRot ).Using( quaternionApproxComparer ) );
            Assert.That( sut.Velocity, Is.EqualTo( expectedSceneVel ).Using( vector3ApproxComparer ) );
            Assert.That( sut.AngularVelocity, Is.EqualTo( expectedSceneAngVel ).Using( vector3ApproxComparer ) );
            Assert.That( sut.Acceleration, Is.EqualTo( expectedSceneAcc ).Using( vector3ApproxComparer ) );
            Assert.That( sut.AngularAcceleration, Is.EqualTo( expectedSceneAngAcc ).Using( vector3ApproxComparer ) );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), ExpectedResult = null )]
        [UnityTest]
        public IEnumerator VelocityIntegration( Type transformType )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( 1, 0, 0 );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( 1, 0, 0 );

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteVelocity = initialVelocity;
            sut.AbsoluteAngularVelocity = initialAngularVelocity;

            // we want to create the object, set the initial values, set up the asserts, and then let it run for a while.
            // this case doesn't switch, but another test should switch at various times.

            yield return new WaitForFixedUpdate();

            int fixedUpdateStepsSinceLastUpdate = 0;
            int fixedUpdateStepsTotal = 0;

            double startUT = TimeManager.UT;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, () =>
            {
                fixedUpdateStepsSinceLastUpdate++;
                fixedUpdateStepsTotal++;
                double deltaTime = TimeManager.UT - startUT; // We use TimeManager.UT here, even though TimeManager.OldUT is typical for FixedUpdate
                                                             // because this is a duration, and it matches the value when the startUT was retrieved.

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, () =>
            {
                fixedUpdateStepsSinceLastUpdate = 0;

                double deltaTime = (TimeManager.UT + (TimeManager.UT - TimeManager.OldUT)) - startUT;

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
            } );
            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 1 );

            UnityEngine.Object.DestroyImmediate( manager );
        }
    }
}