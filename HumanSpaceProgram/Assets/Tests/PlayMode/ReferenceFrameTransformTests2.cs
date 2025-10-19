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

namespace HSP_Tests_PlayMode
{
    public class ReferenceFrametransformTests2
    {
        static IEqualityComparer<Vector3> vector3ApproxComparer = new Vector3ApproximateComparer( 0.0005f );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0005 );
        static IEqualityComparer<Quaternion> quaternionApproxComparer = new QuaternionApproximateComparer( 0.0005f );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0005 );

        /// <summary>
        /// Creates a test scene with TimeManager and ReferenceFrameManager
        /// </summary>
        static (GameObject manager, TimeManager timeManager, GameplaySceneReferenceFrameManager refFrameManager, AssertMonoBehaviour assertMonoBeh) CreateTestScene()
        {
            GameObject manager = new GameObject( "TestManager" );
            TimeManager timeManager = manager.AddComponent<TimeManager>();
            TimeManager.SetUT( 0 );
            GameplaySceneReferenceFrameManager refFrameManager = manager.AddComponent<GameplaySceneReferenceFrameManager>();
            var assertMonoBeh = manager.AddComponent<AssertMonoBehaviour>();

            return (manager, timeManager, refFrameManager, assertMonoBeh);
        }

        static IReferenceFrameTransform CreateObject( Type type, ISceneReferenceFrameProvider provider )
        {
            if( type == null || !typeof( IReferenceFrameTransform ).IsAssignableFrom( type ) )
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

        private static void AssertCorrectPinnedReferenceFrameTransformValues( IReferenceFrameTransform sut, IReferenceFrameTransform pinnedSut, Vector3Dbl initialPosition, QuaternionDbl initialRotation, Vector3Dbl initialVelocity, Vector3Dbl initialAngularVelocity, Vector3Dbl pinnedPos, QuaternionDbl pinnedRot, double deltaTime )
        {
            // Calculate expected absolute position and rotation of the reference transform
            Vector3Dbl expectedRefAbsPos = initialPosition + (initialVelocity * deltaTime);

            double omegaMag = initialAngularVelocity.magnitude;
            Vector3Dbl axis = omegaMag > 0.0 ? initialAngularVelocity.normalized : new Vector3Dbl( 1, 0, 0 );
            double angle = omegaMag * deltaTime;
            QuaternionDbl expectedRefAbsRot;
            expectedRefAbsRot = QuaternionDbl.AngleAxis( angle * 57.29577951308232, axis ) * initialRotation;

            // Calculate expected absolute position and rotation of the pinned transform
            // The pinned transform should be at the pinned position relative to the reference transform
            Vector3Dbl expectedPinnedAbsPos = expectedRefAbsPos + (expectedRefAbsRot * pinnedPos);
            QuaternionDbl expectedPinnedAbsRot = expectedRefAbsRot * pinnedRot;

            // Pinned transforms inherit velocity and angular velocity from their reference transform
            // but linear velocity also includes the tangential term omega x r_rel (r_rel in absolute coords).
            Vector3Dbl tangential = Vector3Dbl.Cross( initialAngularVelocity, expectedRefAbsRot * pinnedPos );
            Vector3Dbl expectedPinnedAbsVel = initialVelocity + tangential;
            Vector3Dbl expectedPinnedAbsAngVel = initialAngularVelocity;

            // Pinned transforms have zero acceleration (they're kinematic)
            Vector3Dbl expectedPinnedAbsAcc = Vector3Dbl.zero;
            Vector3Dbl expectedPinnedAbsAngAcc = Vector3Dbl.zero;

            IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;

            // Convert to scene space
            Vector3 expectedPinnedScenePos = (Vector3)sceneRef.InverseTransformPosition( expectedPinnedAbsPos );
            Quaternion expectedPinnedSceneRot = (Quaternion)sceneRef.InverseTransformRotation( expectedPinnedAbsRot );
            Vector3 expectedPinnedSceneVel = (Vector3)sceneRef.InverseTransformVelocity( expectedPinnedAbsVel );
            Vector3 expectedPinnedSceneAngVel = (Vector3)sceneRef.InverseTransformAngularVelocity( expectedPinnedAbsAngVel );
            Vector3 expectedPinnedSceneAcc = (Vector3)sceneRef.InverseTransformAcceleration( expectedPinnedAbsAcc );
            Vector3 expectedPinnedSceneAngAcc = (Vector3)sceneRef.InverseTransformAngularAcceleration( expectedPinnedAbsAngAcc );

            // Assert pinned transform values
            Assert.That( pinnedSut.AbsolutePosition, Is.EqualTo( expectedPinnedAbsPos ).Using( vector3DblApproxComparer ) );
            Assert.That( pinnedSut.AbsoluteRotation, Is.EqualTo( expectedPinnedAbsRot ).Using( quaternionDblApproxComparer ) );
            Assert.That( pinnedSut.AbsoluteVelocity, Is.EqualTo( expectedPinnedAbsVel ).Using( vector3DblApproxComparer ) );
            Assert.That( pinnedSut.AbsoluteAngularVelocity, Is.EqualTo( expectedPinnedAbsAngVel ).Using( vector3DblApproxComparer ) );
            Assert.That( pinnedSut.AbsoluteAcceleration, Is.EqualTo( expectedPinnedAbsAcc ).Using( vector3DblApproxComparer ) );
            Assert.That( pinnedSut.AbsoluteAngularAcceleration, Is.EqualTo( expectedPinnedAbsAngAcc ).Using( vector3DblApproxComparer ) );

            Assert.That( pinnedSut.Position, Is.EqualTo( expectedPinnedScenePos ).Using( vector3ApproxComparer ) );
            Assert.That( pinnedSut.Rotation, Is.EqualTo( expectedPinnedSceneRot ).Using( quaternionApproxComparer ) );
            Assert.That( pinnedSut.Velocity, Is.EqualTo( expectedPinnedSceneVel ).Using( vector3ApproxComparer ) );
            Assert.That( pinnedSut.AngularVelocity, Is.EqualTo( expectedPinnedSceneAngVel ).Using( vector3ApproxComparer ) );
            Assert.That( pinnedSut.Acceleration, Is.EqualTo( expectedPinnedSceneAcc ).Using( vector3ApproxComparer ) );
            Assert.That( pinnedSut.AngularAcceleration, Is.EqualTo( expectedPinnedSceneAngAcc ).Using( vector3ApproxComparer ) );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )] // tests scene sim and switch
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, -1, 2, 3, ExpectedResult = null )] // tests absolute sim and switch
        [UnityTest]
        public IEnumerator VelocityIntegration( Type transformType, double vx, double vy, double vz, double ax, double ay, double az )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( ax, ay, az );

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteVelocity = initialVelocity;
            sut.AbsoluteAngularVelocity = initialAngularVelocity;

            yield return new WaitForFixedUpdate();

            double startUT = TimeManager.UT;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                double deltaTime = TimeManager.UT - startUT; // We use TimeManager.UT here, even though TimeManager.OldUT is typical for FixedUpdate
                                                             // because this is a duration, and it matches the value when the startUT was retrieved.

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                double deltaTime = (TimeManager.UT + (TimeManager.UT - TimeManager.OldUT)) - startUT;

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
            } );
            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 1 );

            UnityEngine.Object.DestroyImmediate( manager );
        }


        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )] // tests scene sim and switch
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, -1, 2, 3, ExpectedResult = null )] // tests absolute sim and switch
        [UnityTest]
        public IEnumerator VelocityIntegration_WithSwitching( Type transformType, double vx, double vy, double vz, double ax, double ay, double az )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( ax, ay, az );

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteVelocity = initialVelocity;
            sut.AbsoluteAngularVelocity = initialAngularVelocity;

            var scheduledSwitches = new double[]
            {
                0.1, 0.3, 0.666, 0.9
            };
            var scheduledFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, sut.AbsolutePosition ),
                () => new OrientedReferenceFrame( TimeManager.UT, sut.AbsolutePosition, sut.AbsoluteRotation ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, sut.AbsolutePosition, sut.AbsoluteVelocity ),
                () => new OrientedInertialReferenceFrame( TimeManager.UT, sut.AbsolutePosition, sut.AbsoluteRotation, sut.AbsoluteVelocity ),
            };
            int nextSwitchIndex = 0;

            yield return new WaitForFixedUpdate();

            double startUT = TimeManager.UT;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                double deltaTime = TimeManager.UT - startUT;
                // Perform any scheduled switches whose time has arrived (only one per FixedUpdate to preserve ordering)
                if( nextSwitchIndex < scheduledSwitches.Length )
                {
                    double switchTime = scheduledSwitches[nextSwitchIndex];
                    if( deltaTime >= switchTime )
                    {
                        refFrameManager.RequestReferenceFrameSwitch( scheduledFrames[nextSwitchIndex].Invoke() );
                        nextSwitchIndex++;
                    }
                }
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                double deltaTime = TimeManager.UT - startUT; // We use TimeManager.UT here, even though TimeManager.OldUT is typical for FixedUpdate
                                                             // because this is a duration, and it matches the value when the startUT was retrieved.

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                double deltaTime = (TimeManager.UT + (TimeManager.UT - TimeManager.OldUT)) - startUT;

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
            } );
            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 1.5f );

            UnityEngine.Object.DestroyImmediate( manager );
        }


        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )] // tests scene sim and switch
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )] // tests absolute sim and switch
        [UnityTest]
        public IEnumerator VelocityIntegration_WithPinned_WithSwitching( Type transformType, double vx, double vy, double vz )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            GameObject pinned = new GameObject( "Pinned" );
            PinnedReferenceFrameTransform pinnedSut = pinned.AddComponent<PinnedReferenceFrameTransform>();
            pinnedSut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            Vector3Dbl pinPosition = new Vector3Dbl( 5, 10, 0 );
            QuaternionDbl pinRotation = Quaternion.Euler( 45, 90, 135 );
            pinnedSut.SetReference( sut, pinPosition, pinRotation );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( 1, 2, 3 );

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteVelocity = initialVelocity;
            sut.AbsoluteAngularVelocity = initialAngularVelocity;

            var scheduledSwitches = new double[]
            {
                0.1, 0.3, 0.666, 0.9
            };
            var scheduledFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, sut.AbsolutePosition ),
                () => new OrientedReferenceFrame( TimeManager.UT, sut.AbsolutePosition, sut.AbsoluteRotation ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, sut.AbsolutePosition, sut.AbsoluteVelocity ),
                () => new OrientedInertialReferenceFrame( TimeManager.UT, sut.AbsolutePosition, sut.AbsoluteRotation, sut.AbsoluteVelocity ),
            };
            int nextSwitchIndex = 0;

            yield return new WaitForFixedUpdate();

            double startUT = TimeManager.UT;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                double deltaTime = TimeManager.UT - startUT;
                // Perform any scheduled switches whose time has arrived (only one per FixedUpdate to preserve ordering)
                if( nextSwitchIndex < scheduledSwitches.Length )
                {
                    double switchTime = scheduledSwitches[nextSwitchIndex];
                    if( deltaTime >= switchTime )
                    {
                        refFrameManager.RequestReferenceFrameSwitch( scheduledFrames[nextSwitchIndex].Invoke() );
                        nextSwitchIndex++;
                    }
                }
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                double deltaTime = TimeManager.UT - startUT; // We use TimeManager.UT here, even though TimeManager.OldUT is typical for FixedUpdate
                                                             // because this is a duration, and it matches the value when the startUT was retrieved.

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectPinnedReferenceFrameTransformValues( sut, pinnedSut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, deltaTime );
                AssertCorrectPinnedReferenceFrameTransformValues( sut, pinnedSut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, deltaTime );
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                double deltaTime = (TimeManager.UT + (TimeManager.UT - TimeManager.OldUT)) - startUT;

                // Assert twice to ensure that the getters are idempotent.
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectReferenceFrameTransformValues( sut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, deltaTime );
                AssertCorrectPinnedReferenceFrameTransformValues( sut, pinnedSut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, deltaTime );
                AssertCorrectPinnedReferenceFrameTransformValues( sut, pinnedSut, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, deltaTime );
            } );
            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 1.5f );

            UnityEngine.Object.DestroyImmediate( manager );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )] // tests scene sim and switch
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )] // tests absolute sim and switch
        [UnityTest]
        public IEnumerator ForceApplication( Type transformType, double vx, double vy, double vz )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            IPhysicsTransform physicsSut = (IPhysicsTransform)sut;

            // --- Initial state (absolute and scene aligned for test simplicity) ---
            Vector3Dbl initialPosition = new Vector3Dbl( 0, 0, 0 );
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteVelocity = initialVelocity;
            sut.AbsoluteAngularVelocity = initialAngularVelocity;

            // Set mass and inertia for predictable physics
            physicsSut.Mass = 1000f; // [kg]
            physicsSut.MomentsOfInertia = new Vector3( 1000f, 1000f, 1000f );

            // Forces to apply each fixed update:
            Vector3 forceVectorInSceneSpace = new Vector3( 1000f, 0f, 0f );
            Vector3 forceVectorInAbsoluteSpace = new Vector3( 500f, 0f, 0f );

            Vector3Dbl expectedPosition = initialPosition;
            Vector3Dbl expectedVelocity = initialVelocity;
            Vector3Dbl expectedAcceleration = Vector3Dbl.zero;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                var prevExpectedPosition = expectedPosition;
                var prevExpectedVelocity = expectedVelocity;
                var prevExpectedAcceleration = expectedAcceleration;

                Vector3Dbl absoluteFromScene = forceVectorInSceneSpace;       // scene->absolute (identity in test)
                Vector3Dbl absoluteFromAbsolute = forceVectorInAbsoluteSpace; // already absolute

                var force = absoluteFromScene + absoluteFromAbsolute;

                // integrate velocity and position (semi-implicit Euler)
                var nextAcceleration = force / physicsSut.Mass;
                var nextVelocity = prevExpectedVelocity + nextAcceleration * TimeManager.FixedDeltaTime;
                var nextPosition = prevExpectedPosition + nextVelocity * TimeManager.FixedDeltaTime;

                physicsSut.AddForce( forceVectorInSceneSpace );
                physicsSut.AddAbsoluteForce( forceVectorInAbsoluteSpace );

                // Update the expected state to the "post-step" values so Update() assertions can compare against them.
                expectedPosition = nextPosition;
                expectedVelocity = nextVelocity;
                expectedAcceleration = nextAcceleration;

                IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;
                Assert.That( sut.Position, Is.EqualTo( (Vector3)sceneRef.InverseTransformPosition( prevExpectedPosition ) ).Using( vector3ApproxComparer ) );
                Assert.That( sut.Velocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformVelocity( prevExpectedVelocity ) ).Using( vector3ApproxComparer ) );

                Assert.That( sut.Acceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAcceleration( expectedAcceleration ) ).Using( vector3ApproxComparer ) );

                Assert.That( sut.AbsolutePosition, Is.EqualTo( prevExpectedPosition ).Using( vector3DblApproxComparer ) );
                Assert.That( sut.AbsoluteVelocity, Is.EqualTo( prevExpectedVelocity ).Using( vector3DblApproxComparer ) );

                Assert.That( sut.AbsoluteAcceleration, Is.EqualTo( expectedAcceleration ).Using( vector3DblApproxComparer ) );
            } );

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                //Assert.That( sut.AbsolutePosition, Is.EqualTo( expectedPosition ).Using( vector3DblApproxComparer ) );
                //Assert.That( sut.AbsoluteVelocity, Is.EqualTo( expectedVelocity ).Using( vector3DblApproxComparer ) );
                //Assert.That( sut.AbsoluteAcceleration, Is.EqualTo( expectedAcceleration ).Using( vector3DblApproxComparer ) );
            } );

            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 2f );

            UnityEngine.Object.DestroyImmediate( manager );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )] // tests scene sim and switch
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )] // tests absolute sim and switch
        [UnityTest]
        public IEnumerator TorqueApplication( Type transformType, double vx, double vy, double vz )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            IPhysicsTransform physicsSut = (IPhysicsTransform)sut;

            // --- Initial state (absolute and scene aligned for test simplicity) ---
            Vector3Dbl initialPosition = new Vector3Dbl( 0, 0, 0 );
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteVelocity = initialVelocity;
            sut.AbsoluteAngularVelocity = initialAngularVelocity;

            physicsSut.Mass = 1000f;
            physicsSut.MomentsOfInertia = new Vector3( 1000f, 500f, 200f );

            Vector3 torqueInSceneSpace = new Vector3( 0f, 1000f, 0f );    // scene-space torque
            Vector3 torqueInAbsoluteSpace = new Vector3( 0f, 500f, 0f );  // absolute torque

            QuaternionDbl expectedRotation = initialRotation;
            Vector3Dbl expectedAngularVelocity = initialAngularVelocity;
            Vector3Dbl expectedAngularAcceleration = Vector3Dbl.zero;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                var prevExpectedRotation = expectedRotation;
                var prevExpectedAngularVelocity = expectedAngularVelocity;
                var prevExpectedAngularAcceleration = expectedAngularAcceleration;

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

                physicsSut.AddTorque( torqueInSceneSpace );
                physicsSut.AddAbsoluteTorque( torqueInAbsoluteSpace );

                expectedRotation = nextRotation;
                expectedAngularVelocity = nextAngularVelocity;
                expectedAngularAcceleration = nextAngularAcceleration;

                IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;
                Assert.That( sut.Rotation, Is.EqualTo( (Quaternion)sceneRef.InverseTransformRotation( prevExpectedRotation ) ).Using( quaternionApproxComparer ) );
                Assert.That( sut.AngularVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularVelocity( prevExpectedAngularVelocity ) ).Using( vector3ApproxComparer ) );

                Assert.That( sut.AngularAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularAcceleration( expectedAngularAcceleration ) ).Using( vector3ApproxComparer ) );

                Assert.That( sut.AbsoluteRotation, Is.EqualTo( prevExpectedRotation ).Using( quaternionDblApproxComparer ) );
                Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( prevExpectedAngularVelocity ).Using( vector3DblApproxComparer ) );

                Assert.That( sut.AbsoluteAngularAcceleration, Is.EqualTo( expectedAngularAcceleration ).Using( vector3DblApproxComparer ) );
            } );

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                //Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( expectedAngularVelocity ).Using( vector3DblApproxComparer ) );
                //Assert.That( sut.AbsoluteAngularAcceleration, Is.EqualTo( expectedAngularAcceleration ).Using( vector3DblApproxComparer ) );
            } );

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) => { } );

            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 2f );

            UnityEngine.Object.DestroyImmediate( manager );
        }

        // ----- Force-at-position application tests -----
        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )] // tests scene sim and switch
        [TestCase( typeof( HybridReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )] // tests absolute sim and switch
        [UnityTest]
        public IEnumerator ForceAtPositionApplication( Type transformType, double vx, double vy, double vz )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            IPhysicsTransform physicsSut = (IPhysicsTransform)sut;

            // --- Initial state (absolute and scene aligned for test simplicity) ---
            Vector3Dbl initialPosition = new Vector3Dbl( 0, 0, 0 );
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteVelocity = initialVelocity;
            sut.AbsoluteAngularVelocity = initialAngularVelocity;

            // Set mass and inertia for predictable physics
            physicsSut.Mass = 1000f; // [kg]
            physicsSut.MomentsOfInertia = new Vector3( 1000f, 1000f, 1000f );

            // Forces + application points (scene-space and absolute-space)
            Vector3 forceInSceneSpace = new Vector3( 1000f, 0f, 0f );
            Vector3 pointInSceneSpace = new Vector3( 0.0f, 1.0f, 0.0f );

            Vector3 forceInAbsoluteSpace = new Vector3( 500f, 0f, 0f );
            Vector3 pointInAbsoluteSpace = new Vector3( 0.0f, -2.0f, 0.0f );

            Vector3Dbl expectedPosition = initialPosition;
            QuaternionDbl expectedRotation = initialRotation;
            Vector3Dbl expectedVelocity = initialVelocity;
            Vector3Dbl expectedAcceleration = Vector3Dbl.zero;
            Vector3Dbl expectedAngularVelocity = initialAngularVelocity;
            Vector3Dbl expectedAngularAcceleration = Vector3Dbl.zero;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                var prevExpectedPosition = expectedPosition;
                var prevExpectedRotation = expectedRotation;
                var prevExpectedVelocity = expectedVelocity;
                var prevExpectedAcceleration = expectedAcceleration;
                var prevExpectedAngularVelocity = expectedAngularVelocity;
                var prevExpectedAngularAcceleration = expectedAngularAcceleration;

                // Convert scene-space force/point to absolute (identity in our test)
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

                // integrate velocity and position (semi-implicit Euler)
                var nextAcceleration = force / physicsSut.Mass;
                var nextVelocity = prevExpectedVelocity + nextAcceleration * TimeManager.FixedDeltaTime;
                var nextPosition = prevExpectedPosition + nextVelocity * TimeManager.FixedDeltaTime;
                var nextAngularVelocity = prevExpectedAngularVelocity + nextAngularAcceleration * TimeManager.FixedDeltaTime;
                QuaternionDbl deltaRotation = QuaternionDbl.AngleAxis( nextAngularVelocity.magnitude * TimeManager.FixedDeltaTime * 57.29577951308232, nextAngularVelocity );
                var nextRotation = deltaRotation * prevExpectedRotation;

                physicsSut.AddForceAtPosition( forceInSceneSpace, pointInSceneSpace );
                physicsSut.AddAbsoluteForceAtPosition( forceInAbsoluteSpace, pointInAbsoluteSpace );

                // Update the expected state to the "post-step" values so Update() assertions can compare against them.
                expectedPosition = nextPosition;
                expectedRotation = nextRotation;
                expectedVelocity = nextVelocity;
                expectedAngularVelocity = nextAngularVelocity; 
                expectedAcceleration = nextAcceleration;
                expectedAngularAcceleration = nextAngularAcceleration;

                IReferenceFrame sceneRef = GameplaySceneReferenceFrameManager.ReferenceFrame;
                Assert.That( sut.Position, Is.EqualTo( (Vector3)sceneRef.InverseTransformPosition( prevExpectedPosition ) ).Using( vector3ApproxComparer ) );
                Assert.That( sut.Rotation, Is.EqualTo( (Quaternion)sceneRef.InverseTransformRotation( prevExpectedRotation ) ).Using( quaternionApproxComparer ) );
                Assert.That( sut.Velocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformVelocity( prevExpectedVelocity ) ).Using( vector3ApproxComparer ) );
                Assert.That( sut.AngularVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularVelocity( prevExpectedAngularVelocity ) ).Using( vector3ApproxComparer ) );

                Assert.That( sut.Acceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAcceleration( expectedAcceleration ) ).Using( vector3ApproxComparer ) );
                Assert.That( sut.AngularAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularAcceleration( expectedAngularAcceleration ) ).Using( vector3ApproxComparer ) );

                Assert.That( sut.AbsolutePosition, Is.EqualTo( prevExpectedPosition ).Using( vector3DblApproxComparer ) );
                Assert.That( sut.AbsoluteRotation, Is.EqualTo( prevExpectedRotation ).Using( quaternionDblApproxComparer ) );
                Assert.That( sut.AbsoluteVelocity, Is.EqualTo( prevExpectedVelocity ).Using( vector3DblApproxComparer ) );
                Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( prevExpectedAngularVelocity ).Using( vector3DblApproxComparer ) );

                Assert.That( sut.AbsoluteAcceleration, Is.EqualTo( expectedAcceleration ).Using( vector3DblApproxComparer ) );
                Assert.That( sut.AbsoluteAngularAcceleration, Is.EqualTo( expectedAngularAcceleration ).Using( vector3DblApproxComparer ) );
            } );

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) => 
            { 

            } );

            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 2f );

            UnityEngine.Object.DestroyImmediate( manager );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), ExpectedResult = null )]
        [UnityTest]
        public IEnumerator ManualValueSetting( Type transformType )
        {
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();

            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );

            yield return new WaitForFixedUpdate();

            // Test setting values in different reference frames
            var testReferenceFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ),
                () => new OrientedReferenceFrame( TimeManager.UT, Vector3Dbl.zero, QuaternionDbl.Euler( 0, 45, 0 ) ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 100, 0, 0 ) ),
                () => new OrientedInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, QuaternionDbl.Euler( 0, 90, 0 ), new Vector3Dbl( 0, 0, 50 ) ),
            };
            IReferenceFrame currentFrame = null;
            bool updateHappened = true;
            bool fixedUpdateHappened = true;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {
                // Test setting values in FixedUpdate for the current reference frame
                if( currentFrame != null && !fixedUpdateHappened )
                {
                    fixedUpdateHappened = true;

                    // Test absolute value setting
                    Vector3Dbl testAbsPos = new Vector3Dbl( 10, 20, 30 );
                    QuaternionDbl testAbsRot = QuaternionDbl.Euler( 15, 30, 45 );
                    Vector3Dbl testAbsVel = new Vector3Dbl( 1, 2, 3 );
                    Vector3Dbl testAbsAngVel = new Vector3Dbl( 0.1, 0.2, 0.3 );

                    sut.AbsolutePosition = testAbsPos;
                    sut.AbsoluteRotation = testAbsRot;
                    sut.AbsoluteVelocity = testAbsVel;
                    sut.AbsoluteAngularVelocity = testAbsAngVel;

                    // Assert immediately after setting absolute values
                    Assert.That( sut.AbsolutePosition, Is.EqualTo( testAbsPos ).Using( vector3DblApproxComparer ),
                        $"Absolute position not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AbsoluteRotation, Is.EqualTo( testAbsRot ).Using( quaternionDblApproxComparer ),
                        $"Absolute rotation not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AbsoluteVelocity, Is.EqualTo( testAbsVel ).Using( vector3DblApproxComparer ),
                        $"Absolute velocity not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( testAbsAngVel ).Using( vector3DblApproxComparer ),
                        $"Absolute angular velocity not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );

                    // Test local value setting
                    Vector3 testLocalPos = new Vector3( 5, 10, 15 );
                    Quaternion testLocalRot = Quaternion.Euler( 10, 20, 30 );
                    Vector3 testLocalVel = new Vector3( 0.5f, 1.0f, 1.5f );
                    Vector3 testLocalAngVel = new Vector3( 0.05f, 0.1f, 0.15f );

                    sut.Position = testLocalPos;
                    sut.Rotation = testLocalRot;
                    sut.Velocity = testLocalVel;
                    sut.AngularVelocity = testLocalAngVel;

                    // Assert immediately after setting local values
                    Assert.That( sut.Position, Is.EqualTo( testLocalPos ).Using( vector3ApproxComparer ),
                        $"Local position not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.Rotation, Is.EqualTo( testLocalRot ).Using( quaternionApproxComparer ),
                        $"Local rotation not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.Velocity, Is.EqualTo( testLocalVel ).Using( vector3ApproxComparer ),
                        $"Local velocity not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AngularVelocity, Is.EqualTo( testLocalAngVel ).Using( vector3ApproxComparer ),
                        $"Local angular velocity not set correctly in FixedUpdate for frame {currentFrame.GetType().Name}" );
                }

                // Switch to next reference frame if both tests are complete
                if( fixedUpdateHappened && updateHappened )
                {
                    int currentIndex = Array.IndexOf( testReferenceFrames, currentFrame != null ?
                        Array.Find( testReferenceFrames, f => f.Invoke().GetType() == currentFrame.GetType() ) : null );

                    if( currentIndex >= 0 && currentIndex < testReferenceFrames.Length - 1 )
                    {
                        // Switch to next reference frame
                        refFrameManager.RequestReferenceFrameSwitch( testReferenceFrames[currentIndex + 1].Invoke() );
                        currentFrame = testReferenceFrames[currentIndex + 1].Invoke();
                        fixedUpdateHappened = false;
                        updateHappened = false;
                    }
                    else if( currentIndex == -1 )
                    {
                        // Start with first reference frame
                        refFrameManager.RequestReferenceFrameSwitch( testReferenceFrames[0].Invoke() );
                        currentFrame = testReferenceFrames[0].Invoke();
                        fixedUpdateHappened = false;
                        updateHappened = false;
                    }
                }
            } );

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                // Test setting values in Update for the current reference frame
                if( currentFrame != null && !updateHappened )
                {
                    updateHappened = true;

                    // Test absolute value setting
                    Vector3Dbl testAbsPos = new Vector3Dbl( 25, 35, 45 );
                    QuaternionDbl testAbsRot = QuaternionDbl.Euler( 25, 40, 55 );
                    Vector3Dbl testAbsVel = new Vector3Dbl( 2, 3, 4 );
                    Vector3Dbl testAbsAngVel = new Vector3Dbl( 0.2, 0.3, 0.4 );

                    sut.AbsolutePosition = testAbsPos;
                    sut.AbsoluteRotation = testAbsRot;
                    sut.AbsoluteVelocity = testAbsVel;
                    sut.AbsoluteAngularVelocity = testAbsAngVel;

                    // Assert immediately after setting absolute values
                    Assert.That( sut.AbsolutePosition, Is.EqualTo( testAbsPos ).Using( vector3DblApproxComparer ),
                        $"Absolute position not set correctly in Update for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AbsoluteRotation, Is.EqualTo( testAbsRot ).Using( quaternionDblApproxComparer ),
                        $"Absolute rotation not set correctly in Update for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AbsoluteVelocity, Is.EqualTo( testAbsVel ).Using( vector3DblApproxComparer ),
                        $"Absolute velocity not set correctly in Update for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( testAbsAngVel ).Using( vector3DblApproxComparer ),
                        $"Absolute angular velocity not set correctly in Update for frame {currentFrame.GetType().Name}" );

                    // Test local value setting
                    Vector3 testLocalPos = new Vector3( 8, 12, 16 );
                    Quaternion testLocalRot = Quaternion.Euler( 15, 25, 35 );
                    Vector3 testLocalVel = new Vector3( 0.8f, 1.2f, 1.6f );
                    Vector3 testLocalAngVel = new Vector3( 0.08f, 0.12f, 0.16f );

                    sut.Position = testLocalPos;
                    sut.Rotation = testLocalRot;
                    sut.Velocity = testLocalVel;
                    sut.AngularVelocity = testLocalAngVel;

                    // Assert immediately after setting local values
                    Assert.That( sut.Position, Is.EqualTo( testLocalPos ).Using( vector3ApproxComparer ),
                        $"Local position not set correctly in Update for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.Rotation, Is.EqualTo( testLocalRot ).Using( quaternionApproxComparer ),
                        $"Local rotation not set correctly in Update for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.Velocity, Is.EqualTo( testLocalVel ).Using( vector3ApproxComparer ),
                        $"Local velocity not set correctly in Update for frame {currentFrame.GetType().Name}" );
                    Assert.That( sut.AngularVelocity, Is.EqualTo( testLocalAngVel ).Using( vector3ApproxComparer ),
                        $"Local angular velocity not set correctly in Update for frame {currentFrame.GetType().Name}" );
                }
            } );

            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 1f );

            UnityEngine.Object.DestroyImmediate( manager );
        }

        // test in non-inertial reference scene frames.
    }
}