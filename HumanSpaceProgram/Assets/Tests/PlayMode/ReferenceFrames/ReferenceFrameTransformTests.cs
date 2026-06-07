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
    public class ReferenceFrameTransformTests
    {
        static IEqualityComparer<Vector3> vector3ApproxComparer = new Vector3ApproximateComparer( 0.0005f );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0005 );
        static IEqualityComparer<Quaternion> quaternionApproxComparer = new QuaternionApproximateComparer( 0.0005f );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0005 );

        /// <summary>
        /// Creates a test scene with TimeManager and ReferenceFrameManager
        /// </summary>
        static (GameObject manager, TimeManager timeManager, GameplaySceneReferenceFrameManager refFrameManager) CreateTestScene()
        {
            GameObject manager = new GameObject( "TestManager" );
            TimeManager timeManager = manager.AddComponent<TimeManager>();
            TimeManager.SetUT( 0 );
            GameplaySceneReferenceFrameManager refFrameManager = manager.AddComponent<GameplaySceneReferenceFrameManager>();

            return (manager, timeManager, refFrameManager);
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

        private struct TestTransformState : IWithSimulationTime
        {
            public float GetSimulationTime() => (float)UT;
            public double UT { get; set; }
            public Vector3Dbl Position { get; set; }
            public QuaternionDbl Rotation { get; set; }
            public Vector3Dbl Velocity { get; set; }
            public Vector3Dbl AngularVelocity { get; set; }
            public Vector3Dbl Acceleration { get; set; }
            public Vector3Dbl AngularAcceleration { get; set; }

            public Vector3 LocalPosition { get; set; }
            public Quaternion LocalRotation { get; set; }
            public Vector3 LocalVelocity { get; set; }
            public Vector3 LocalAngularVelocity { get; set; }
            public Vector3 LocalAcceleration { get; set; }
            public Vector3 LocalAngularAcceleration { get; set; }

            public IReferenceFrame SceneRefFrame { get; set; }
        }

        static TestTransformState ExtractState( IReferenceFrameTransform sut ) => new TestTransformState()
        {
            UT = TimeManager.UT,
            Position = sut.GetAbsolutePosition(),
            Rotation = sut.GetAbsoluteRotation(),
            Velocity = sut.GetAbsoluteVelocity(),
            AngularVelocity = sut.GetAbsoluteAngularVelocity(),
            Acceleration = sut.GetAbsoluteAcceleration(),
            AngularAcceleration = sut.GetAbsoluteAngularAcceleration(),

            LocalPosition = sut.GetPosition(),
            LocalRotation = sut.GetRotation(),
            LocalVelocity = sut.GetVelocity(),
            LocalAngularVelocity = sut.GetAngularVelocity(),
            LocalAcceleration = sut.GetAcceleration(),
            LocalAngularAcceleration = sut.GetAngularAcceleration(),

            SceneRefFrame = GameplaySceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT )
        };

        private static void AssertCorrectState( TestTransformState state, Vector3Dbl initialPosition, QuaternionDbl initialRotation, Vector3Dbl initialVelocity, Vector3Dbl initialAngularVelocity, double startUT )
        {
            double deltaTime = state.UT - startUT;

            Vector3Dbl expectedAbsPos = initialPosition + (initialVelocity * deltaTime);

            double omegaMag = initialAngularVelocity.magnitude;
            Vector3Dbl axis = omegaMag > 0.0 ? initialAngularVelocity.normalized : new Vector3Dbl( 1, 0, 0 );
            double angle = omegaMag * deltaTime;
            QuaternionDbl expectedAbsRot = QuaternionDbl.AngleAxis( angle * 57.29577951308232, axis ) * initialRotation;

            Vector3Dbl expectedAbsVel = initialVelocity;
            Vector3Dbl expectedAbsAngVel = initialAngularVelocity;

            Vector3Dbl expectedAbsAcc = Vector3Dbl.zero;
            Vector3Dbl expectedAbsAngAcc = Vector3Dbl.zero;

            IReferenceFrame sceneRef = state.SceneRefFrame;

            Vector3 expectedScenePos = (Vector3)sceneRef.InverseTransformPosition( expectedAbsPos );
            Quaternion expectedSceneRot = (Quaternion)sceneRef.InverseTransformRotation( expectedAbsRot );
            Vector3 expectedSceneVel = (Vector3)sceneRef.InverseTransformVelocity( expectedAbsVel );
            Vector3 expectedSceneAngVel = (Vector3)sceneRef.InverseTransformAngularVelocity( expectedAbsAngVel );
            Vector3 expectedSceneAcc = (Vector3)sceneRef.InverseTransformAcceleration( expectedAbsAcc );
            Vector3 expectedSceneAngAcc = (Vector3)sceneRef.InverseTransformAngularAcceleration( expectedAbsAngAcc );

            Assert.That( state.Position, Is.EqualTo( expectedAbsPos ).Using( vector3DblApproxComparer ), "Absolute Position" );
            Assert.That( state.Rotation, Is.EqualTo( expectedAbsRot ).Using( quaternionDblApproxComparer ), "Absolute Rotation" );
            Assert.That( state.Velocity, Is.EqualTo( expectedAbsVel ).Using( vector3DblApproxComparer ), "Absolute Velocity" );
            Assert.That( state.AngularVelocity, Is.EqualTo( expectedAbsAngVel ).Using( vector3DblApproxComparer ), "Absolute Angular Velocity" );
            Assert.That( state.Acceleration, Is.EqualTo( expectedAbsAcc ).Using( vector3DblApproxComparer ), "Absolute Acceleration" );
            Assert.That( state.AngularAcceleration, Is.EqualTo( expectedAbsAngAcc ).Using( vector3DblApproxComparer ), "Absolute Angular Acceleration" );

            Assert.That( state.LocalPosition, Is.EqualTo( expectedScenePos ).Using( vector3ApproxComparer ), "Scene Position" );
            Assert.That( state.LocalRotation, Is.EqualTo( expectedSceneRot ).Using( quaternionApproxComparer ), "Scene Rotation" );
            Assert.That( state.LocalVelocity, Is.EqualTo( expectedSceneVel ).Using( vector3ApproxComparer ), "Scene Velocity" );
            Assert.That( state.LocalAngularVelocity, Is.EqualTo( expectedSceneAngVel ).Using( vector3ApproxComparer ), "Scene Angular Velocity" );
            Assert.That( state.LocalAcceleration, Is.EqualTo( expectedSceneAcc ).Using( vector3ApproxComparer ), "Scene Acceleration" );
            Assert.That( state.LocalAngularAcceleration, Is.EqualTo( expectedSceneAngAcc ).Using( vector3ApproxComparer ), "Scene Angular Acceleration" );
        }

        private struct PinnedSimState : IWithSimulationTime
        {
            public float GetSimulationTime() => (float)UT;
            public double UT { get; set; }
            public Vector3Dbl AbsoluteRefPos { get; set; }
            public QuaternionDbl AbsoluteRefRot { get; set; }
            public Vector3Dbl AbsoluteRefVel { get; set; }
            public Vector3Dbl AbsoluteRefAngVel { get; set; }

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

            Vector3Dbl expectedPinnedAbsAcc = Vector3Dbl.zero;
            Vector3Dbl expectedPinnedAbsAngAcc = Vector3Dbl.zero;

            IReferenceFrame sceneRef = state.SceneRefFrame;

            Vector3 expectedPinnedScenePos = (Vector3)sceneRef.InverseTransformPosition( expectedPinnedAbsPos );
            Quaternion expectedPinnedSceneRot = (Quaternion)sceneRef.InverseTransformRotation( expectedPinnedAbsRot );
            Vector3 expectedPinnedSceneVel = (Vector3)sceneRef.InverseTransformVelocity( expectedPinnedAbsVel );
            Vector3 expectedPinnedSceneAngVel = (Vector3)sceneRef.InverseTransformAngularVelocity( expectedPinnedAbsAngVel );
            Vector3 expectedPinnedSceneAcc = (Vector3)sceneRef.InverseTransformAcceleration( expectedPinnedAbsAcc );
            Vector3 expectedPinnedSceneAngAcc = (Vector3)sceneRef.InverseTransformAngularAcceleration( expectedPinnedAbsAngAcc );

            Assert.That( state.AbsolutePinnedPos, Is.EqualTo( expectedPinnedAbsPos ).Using( vector3DblApproxComparer ), "Pinned Absolute Position" );
            Assert.That( state.AbsolutePinnedRot, Is.EqualTo( expectedPinnedAbsRot ).Using( quaternionDblApproxComparer ), "Pinned Absolute Rotation" );
            Assert.That( state.AbsolutePinnedVel, Is.EqualTo( expectedPinnedAbsVel ).Using( vector3DblApproxComparer ), "Pinned Absolute Velocity" );
            Assert.That( state.AbsolutePinnedAngVel, Is.EqualTo( expectedPinnedAbsAngVel ).Using( vector3DblApproxComparer ), "Pinned Absolute AngVel" );

            Assert.That( state.LocalPinnedPos, Is.EqualTo( expectedPinnedScenePos ).Using( vector3ApproxComparer ), "Pinned Scene Position" );
            Assert.That( state.LocalPinnedRot, Is.EqualTo( expectedPinnedSceneRot ).Using( quaternionApproxComparer ), "Pinned Scene Rotation" );
            Assert.That( state.LocalPinnedVel, Is.EqualTo( expectedPinnedSceneVel ).Using( vector3ApproxComparer ), "Pinned Scene Velocity" );
            Assert.That( state.LocalPinnedAngVel, Is.EqualTo( expectedPinnedSceneAngVel ).Using( vector3ApproxComparer ), "Pinned Scene AngVel" );
            Assert.That( state.LocalPinnedAcc, Is.EqualTo( expectedPinnedSceneAcc ).Using( vector3ApproxComparer ), "Pinned Scene Acceleration" );
            Assert.That( state.LocalPinnedAngAcc, Is.EqualTo( expectedPinnedSceneAngAcc ).Using( vector3ApproxComparer ), "Pinned Scene AngAcc" );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, -1, 2, 3, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator VelocityIntegration( Type transformType, double vx, double vy, double vz, double ax, double ay, double az )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( ax, ay, az );

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            using var history = new HistoryRecorder( 30f );
            yield return new WaitForFixedUpdate();
            double startUT = TimeManager.UT;

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, TestTransformState>( () => ExtractState( sut ) );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, TestTransformState>( () =>
            {
                var s = ExtractState( sut );
                s.UT = TimeManager.UT + (TimeManager.UT - TimeManager.OldUT);
                return s;
            } );

            yield return new WaitForSeconds( 1.1f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 5; i++ )
            {
                timeline
                    .NextFixedUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) )
                    .NextUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, -1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, -1, 2, 3, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator VelocityIntegration_WithSwitching( Type transformType, double vx, double vy, double vz, double ax, double ay, double az )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( ax, ay, az );

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            var scheduledSwitches = new double[] { 0.1, 0.3, 0.666, 0.9 };
            var scheduledFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition() ),
                () => new OrientedReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteRotation() ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteVelocity() ),
                () => new OrientedInertialReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteRotation(), sut.GetAbsoluteVelocity() ),
            };
            int nextSwitchIndex = 0;

            using var history = new HistoryRecorder( 30f );
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

            yield return new WaitForSeconds( 1.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 5; i++ )
            {
                timeline
                    .NextFixedUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) )
                    .NextUpdate().Verify( d => AssertCorrectState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT ) );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100_000_000, 200_000_000, 300_000_000, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator VelocityIntegration_WithPinned_WithSwitching( Type transformType, double vx, double vy, double vz )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
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

            var scheduledSwitches = new double[] { 0.1, 0.3, 0.666, 0.9 };
            var scheduledFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition() ),
                () => new OrientedReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteRotation() ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteVelocity() ),
                () => new OrientedInertialReferenceFrame( TimeManager.UT, sut.GetAbsolutePosition(), sut.GetAbsoluteRotation(), sut.GetAbsoluteVelocity() ),
            };
            int nextSwitchIndex = 0;

            using var history = new HistoryRecorder( 30f );
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
                    AbsoluteRefPos = sut.GetAbsolutePosition(),
                    AbsoluteRefRot = sut.GetAbsoluteRotation(),
                    AbsoluteRefVel = sut.GetAbsoluteVelocity(),
                    AbsoluteRefAngVel = sut.GetAbsoluteAngularVelocity(),

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
                AbsoluteRefPos = sut.GetAbsolutePosition(),
                AbsoluteRefRot = sut.GetAbsoluteRotation(),
                AbsoluteRefVel = sut.GetAbsoluteVelocity(),
                AbsoluteRefAngVel = sut.GetAbsoluteAngularVelocity(),

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

            yield return new WaitForSeconds( 1.5f );

            var timeline = history.AssertTimeline<PinnedSimState>().StartingHere();
            for( int i = 0; i < 5; i++ )
            {
                timeline
                    .NextFixedUpdate().Verify( d => VerifyPinnedTestState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, startUT ) )
                    .NextUpdate().Verify( d => VerifyPinnedTestState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, pinPosition, pinRotation, startUT ) );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
            UnityEngine.Object.DestroyImmediate( pinned );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator ForceApplication( Type transformType, double vx, double vy, double vz )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
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

            using var history = new HistoryRecorder( 30f );

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

            yield return new WaitForSeconds( 1.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 5; i++ )
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
                    Assert.That( d.Position, Is.EqualTo( valExpectedPosition ).Using( vector3DblApproxComparer ), "Position" );
                    Assert.That( d.Velocity, Is.EqualTo( valExpectedVelocity ).Using( vector3DblApproxComparer ), "Velocity" );
                    Assert.That( d.Acceleration, Is.EqualTo( valExpectedAcceleration ).Using( vector3DblApproxComparer ), "Acceleration" );

                    Assert.That( d.LocalPosition, Is.EqualTo( (Vector3)sceneRef.InverseTransformPosition( valExpectedPosition ) ).Using( vector3ApproxComparer ), "LocalPosition" );
                    Assert.That( d.LocalVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformVelocity( valExpectedVelocity ) ).Using( vector3ApproxComparer ), "LocalVelocity" );
                    Assert.That( d.LocalAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAcceleration( valExpectedAcceleration ) ).Using( vector3ApproxComparer ), "LocalAcceleration" );
                } );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 10000, 20000, 30000, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator TorqueApplication( Type transformType, double vx, double vy, double vz )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
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

            using var history = new HistoryRecorder( 30f );

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

            yield return new WaitForSeconds( 1.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 5; i++ )
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
                    Assert.That( d.Rotation, Is.EqualTo( valExpectedRotation ).Using( quaternionDblApproxComparer ), "Rotation" );
                    Assert.That( d.AngularVelocity, Is.EqualTo( valExpectedAngularVelocity ).Using( vector3DblApproxComparer ), "AngularVelocity" );
                    Assert.That( d.AngularAcceleration, Is.EqualTo( valExpectedAngularAcceleration ).Using( vector3DblApproxComparer ), "AngularAcceleration" );

                    Assert.That( d.LocalRotation, Is.EqualTo( (Quaternion)sceneRef.InverseTransformRotation( valExpectedRotation ) ).Using( quaternionApproxComparer ), "LocalRotation" );
                    Assert.That( d.LocalAngularVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularVelocity( valExpectedAngularVelocity ) ).Using( vector3ApproxComparer ), "LocalAngularVelocity" );
                    Assert.That( d.LocalAngularAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularAcceleration( valExpectedAngularAcceleration ) ).Using( vector3ApproxComparer ), "LocalAngularAcceleration" );
                } );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 1, 2, 3, ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), 100, 200, 300, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator ForceAtPositionApplication( Type transformType, double vx, double vy, double vz )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );
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

            using var history = new HistoryRecorder( 30f );

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

            yield return new WaitForSeconds( 1.5f );

            var timeline = history.AssertTimeline<TestTransformState>().StartingHere();
            for( int i = 0; i < 5; i++ )
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
                    Assert.That( d.Position, Is.EqualTo( valExpectedPosition ).Using( vector3DblApproxComparer ), "Position" );
                    Assert.That( d.Rotation, Is.EqualTo( valExpectedRotation ).Using( quaternionDblApproxComparer ), "Rotation" );
                    Assert.That( d.Velocity, Is.EqualTo( valExpectedVelocity ).Using( vector3DblApproxComparer ), "Velocity" );
                    Assert.That( d.AngularVelocity, Is.EqualTo( valExpectedAngularVelocity ).Using( vector3DblApproxComparer ), "AngularVelocity" );
                    Assert.That( d.Acceleration, Is.EqualTo( valExpectedAcceleration ).Using( vector3DblApproxComparer ), "Acceleration" );
                    Assert.That( d.AngularAcceleration, Is.EqualTo( valExpectedAngularAcceleration ).Using( vector3DblApproxComparer ), "AngularAcceleration" );

                    Assert.That( d.LocalPosition, Is.EqualTo( (Vector3)sceneRef.InverseTransformPosition( valExpectedPosition ) ).Using( vector3ApproxComparer ), "LocalPosition" );
                    Assert.That( d.LocalRotation, Is.EqualTo( (Quaternion)sceneRef.InverseTransformRotation( valExpectedRotation ) ).Using( quaternionApproxComparer ), "LocalRotation" );
                    Assert.That( d.LocalVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformVelocity( valExpectedVelocity ) ).Using( vector3ApproxComparer ), "LocalVelocity" );
                    Assert.That( d.LocalAngularVelocity, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularVelocity( valExpectedAngularVelocity ) ).Using( vector3ApproxComparer ), "LocalAngularVelocity" );
                    Assert.That( d.LocalAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAcceleration( valExpectedAcceleration ) ).Using( vector3ApproxComparer ), "LocalAcceleration" );
                    Assert.That( d.LocalAngularAcceleration, Is.EqualTo( (Vector3)sceneRef.InverseTransformAngularAcceleration( valExpectedAngularAcceleration ) ).Using( vector3ApproxComparer ), "LocalAngularAcceleration" );
                } );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), ExpectedResult = null )]
        [UnityTest]
        public IEnumerator ManualValueSetting( Type transformType )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );

            yield return new WaitForFixedUpdate();

            var testReferenceFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ),
                () => new OrientedReferenceFrame( TimeManager.UT, Vector3Dbl.zero, QuaternionDbl.Euler( 0, 45, 0 ) ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 100, 0, 0 ) ),
                () => new OrientedInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, QuaternionDbl.Euler( 0, 90, 0 ), new Vector3Dbl( 0, 0, 50 ) ),
            };

            foreach( var frameGetter in testReferenceFrames )
            {
                var currentFrame = frameGetter.Invoke();
                refFrameManager.RequestReferenceFrameSwitch( currentFrame );

                yield return new WaitForFixedUpdate();

                // Test absolute value setting in FixedUpdate
                Vector3Dbl testAbsPos = new Vector3Dbl( 10, 20, 30 );
                QuaternionDbl testAbsRot = QuaternionDbl.Euler( 15, 30, 45 );
                Vector3Dbl testAbsVel = new Vector3Dbl( 1, 2, 3 );
                Vector3Dbl testAbsAngVel = new Vector3Dbl( 0.1, 0.2, 0.3 );

                IReferenceFrame sceneRefFrame = GameplaySceneReferenceFrameManager.ReferenceFrame;
                Vector3 testLocalPos = (Vector3)sceneRefFrame.InverseTransformPosition( testAbsPos );
                Quaternion testLocalRot = (Quaternion)sceneRefFrame.InverseTransformRotation( testAbsRot );

                sut.SetAbsolutePosition( testAbsPos );
                sut.SetAbsoluteRotation( testAbsRot );
                sut.SetAbsoluteVelocity( testAbsVel );
                sut.SetAbsoluteAngularVelocity( testAbsAngVel );

                Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( testAbsPos ).Using( vector3DblApproxComparer ), "AbsPos set in Fixed" );
                Assert.That( sut.GetAbsoluteRotation(), Is.EqualTo( testAbsRot ).Using( quaternionDblApproxComparer ), "AbsRot set in Fixed" );
                Assert.That( sut.GetAbsoluteVelocity(), Is.EqualTo( testAbsVel ).Using( vector3DblApproxComparer ), "AbsVel set in Fixed" );
                Assert.That( sut.GetAbsoluteAngularVelocity(), Is.EqualTo( testAbsAngVel ).Using( vector3DblApproxComparer ), "AbsAngVel set in Fixed" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPos ).Using( vector3ApproxComparer ), "LocalPos after AbsPos set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRot ).Using( quaternionApproxComparer ), "LocalRot after AbsRot set" );

                // Test local value setting in FixedUpdate
                Vector3 testLocalPosSet = new Vector3( 5, 10, 15 );
                Quaternion testLocalRotSet = Quaternion.Euler( 10, 20, 30 );
                Vector3 testLocalVelSet = new Vector3( 0.5f, 1.0f, 1.5f );
                Vector3 testLocalAngVelSet = new Vector3( 0.05f, 0.1f, 0.15f );

                sut.SetPosition( testLocalPosSet );
                sut.SetRotation( testLocalRotSet );
                sut.SetVelocity( testLocalVelSet );
                sut.SetAngularVelocity( testLocalAngVelSet );

                Assert.That( sut.GetPosition(), Is.EqualTo( testLocalPosSet ).Using( vector3ApproxComparer ), "LocalPos set in Fixed" );
                Assert.That( sut.GetRotation(), Is.EqualTo( testLocalRotSet ).Using( quaternionApproxComparer ), "LocalRot set in Fixed" );
                Assert.That( sut.GetVelocity(), Is.EqualTo( testLocalVelSet ).Using( vector3ApproxComparer ), "LocalVel set in Fixed" );
                Assert.That( sut.GetAngularVelocity(), Is.EqualTo( testLocalAngVelSet ).Using( vector3ApproxComparer ), "LocalAngVel set in Fixed" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPosSet ).Using( vector3ApproxComparer ), "Local transform Pos after set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRotSet ).Using( quaternionApproxComparer ), "Local transform Rot after set" );

                yield return null; // Wait for Update phase

                // Test absolute value setting in Update
                Vector3Dbl testAbsPosUpdate = new Vector3Dbl( 25, 35, 45 );
                QuaternionDbl testAbsRotUpdate = QuaternionDbl.Euler( 25, 40, 55 );
                Vector3Dbl testAbsVelUpdate = new Vector3Dbl( 2, 3, 4 );
                Vector3Dbl testAbsAngVelUpdate = new Vector3Dbl( 0.2, 0.3, 0.4 );

                IReferenceFrame sceneRefFrameUpdate = GameplaySceneReferenceFrameManager.ReferenceFrame;
                Vector3 testLocalPosUpdate = (Vector3)sceneRefFrameUpdate.InverseTransformPosition( testAbsPosUpdate );
                Quaternion testLocalRotUpdate = (Quaternion)sceneRefFrameUpdate.InverseTransformRotation( testAbsRotUpdate );

                sut.SetAbsolutePosition( testAbsPosUpdate );
                sut.SetAbsoluteRotation( testAbsRotUpdate );
                sut.SetAbsoluteVelocity( testAbsVelUpdate );
                sut.SetAbsoluteAngularVelocity( testAbsAngVelUpdate );

                Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( testAbsPosUpdate ).Using( vector3DblApproxComparer ), "AbsPos set in Update" );
                Assert.That( sut.GetAbsoluteRotation(), Is.EqualTo( testAbsRotUpdate ).Using( quaternionDblApproxComparer ), "AbsRot set in Update" );
                Assert.That( sut.GetAbsoluteVelocity(), Is.EqualTo( testAbsVelUpdate ).Using( vector3DblApproxComparer ), "AbsVel set in Update" );
                Assert.That( sut.GetAbsoluteAngularVelocity(), Is.EqualTo( testAbsAngVelUpdate ).Using( vector3DblApproxComparer ), "AbsAngVel set in Update" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPosUpdate ).Using( vector3ApproxComparer ), "Local transform Pos after AbsPos Update set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRotUpdate ).Using( quaternionApproxComparer ), "Local transform Rot after AbsRot Update set" );

                // Test local value setting in Update
                Vector3 testLocalPosSetUpdate = new Vector3( 8, 12, 16 );
                Quaternion testLocalRotSetUpdate = Quaternion.Euler( 15, 25, 35 );
                Vector3 testLocalVelSetUpdate = new Vector3( 0.8f, 1.2f, 1.6f );
                Vector3 testLocalAngVelSetUpdate = new Vector3( 0.08f, 0.12f, 0.16f );

                sut.SetPosition( testLocalPosSetUpdate );
                sut.SetRotation( testLocalRotSetUpdate );
                sut.SetVelocity( testLocalVelSetUpdate );
                sut.SetAngularVelocity( testLocalAngVelSetUpdate );

                Assert.That( sut.GetPosition(), Is.EqualTo( testLocalPosSetUpdate ).Using( vector3ApproxComparer ), "LocalPos set in Update" );
                Assert.That( sut.GetRotation(), Is.EqualTo( testLocalRotSetUpdate ).Using( quaternionApproxComparer ), "LocalRot set in Update" );
                Assert.That( sut.GetVelocity(), Is.EqualTo( testLocalVelSetUpdate ).Using( vector3ApproxComparer ), "LocalVel set in Update" );
                Assert.That( sut.GetAngularVelocity(), Is.EqualTo( testLocalAngVelSetUpdate ).Using( vector3ApproxComparer ), "LocalAngVel set in Update" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPosSetUpdate ).Using( vector3ApproxComparer ), "Local transform Pos after LocalPos Update set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRotSetUpdate ).Using( quaternionApproxComparer ), "Local transform Rot after LocalRot Update set" );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }

        [TestCase( typeof( FreeReferenceFrameTransform ), ExpectedResult = null )]
        [TestCase( typeof( KinematicReferenceFrameTransform ), ExpectedResult = null )]
        [TestCase( typeof( HybridReferenceFrameTransform ), ExpectedResult = null )]
        [UnityTest]
        public IEnumerator ManualValueSetting_WhenDisabled( Type transformType )
        {
            PlayerLoopManager.Initialize( BucketHandling.IncludeThrow );
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            IReferenceFrameTransform sut = CreateObject( transformType, new GameplaySceneReferenceFrameProvider() );

            yield return new WaitForFixedUpdate();

            var testReferenceFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ),
                () => new OrientedReferenceFrame( TimeManager.UT, Vector3Dbl.zero, QuaternionDbl.Euler( 0, 45, 0 ) ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 100, 0, 0 ) ),
                () => new OrientedInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, QuaternionDbl.Euler( 0, 90, 0 ), new Vector3Dbl( 0, 0, 50 ) ),
            };

            foreach( var frameGetter in testReferenceFrames )
            {
                var currentFrame = frameGetter.Invoke();
                refFrameManager.RequestReferenceFrameSwitch( currentFrame );

                yield return new WaitForFixedUpdate();

                // Test absolute value setting in FixedUpdate when disabled
                Vector3Dbl testAbsPos = new Vector3Dbl( 10, 20, 30 );
                QuaternionDbl testAbsRot = QuaternionDbl.Euler( 15, 30, 45 );
                Vector3Dbl testAbsVel = new Vector3Dbl( 1, 2, 3 );
                Vector3Dbl testAbsAngVel = new Vector3Dbl( 0.1, 0.2, 0.3 );

                IReferenceFrame sceneRefFrame = GameplaySceneReferenceFrameManager.ReferenceFrame;
                Vector3 testLocalPos = (Vector3)sceneRefFrame.InverseTransformPosition( testAbsPos );
                Quaternion testLocalRot = (Quaternion)sceneRefFrame.InverseTransformRotation( testAbsRot );

                sut.SetAbsolutePosition( testAbsPos );
                sut.SetAbsoluteRotation( testAbsRot );
                sut.SetAbsoluteVelocity( testAbsVel );
                sut.SetAbsoluteAngularVelocity( testAbsAngVel );

                sut.gameObject.SetActive( false );

                Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( testAbsPos ).Using( vector3DblApproxComparer ), "Disabled: AbsPos set in Fixed" );
                Assert.That( sut.GetAbsoluteRotation(), Is.EqualTo( testAbsRot ).Using( quaternionDblApproxComparer ), "Disabled: AbsRot set in Fixed" );
                Assert.That( sut.GetAbsoluteVelocity(), Is.EqualTo( testAbsVel ).Using( vector3DblApproxComparer ), "Disabled: AbsVel set in Fixed" );
                Assert.That( sut.GetAbsoluteAngularVelocity(), Is.EqualTo( testAbsAngVel ).Using( vector3DblApproxComparer ), "Disabled: AbsAngVel set in Fixed" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPos ).Using( vector3ApproxComparer ), "Disabled: LocalPos after AbsPos set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRot ).Using( quaternionApproxComparer ), "Disabled: LocalRot after AbsRot set" );

                sut.gameObject.SetActive( true );

                // Test local value setting in FixedUpdate when disabled
                Vector3 testLocalPosSet = new Vector3( 5, 10, 15 );
                Quaternion testLocalRotSet = Quaternion.Euler( 10, 20, 30 );
                Vector3 testLocalVelSet = new Vector3( 0.5f, 1.0f, 1.5f );
                Vector3 testLocalAngVelSet = new Vector3( 0.05f, 0.1f, 0.15f );

                sut.SetPosition( testLocalPosSet );
                sut.SetRotation( testLocalRotSet );
                sut.SetVelocity( testLocalVelSet );
                sut.SetAngularVelocity( testLocalAngVelSet );

                sut.gameObject.SetActive( false );

                Assert.That( sut.GetPosition(), Is.EqualTo( testLocalPosSet ).Using( vector3ApproxComparer ), "Disabled: LocalPos set in Fixed" );
                Assert.That( sut.GetRotation(), Is.EqualTo( testLocalRotSet ).Using( quaternionApproxComparer ), "Disabled: LocalRot set in Fixed" );
                Assert.That( sut.GetVelocity(), Is.EqualTo( testLocalVelSet ).Using( vector3ApproxComparer ), "Disabled: LocalVel set in Fixed" );
                Assert.That( sut.GetAngularVelocity(), Is.EqualTo( testLocalAngVelSet ).Using( vector3ApproxComparer ), "Disabled: LocalAngVel set in Fixed" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPosSet ).Using( vector3ApproxComparer ), "Disabled: Local transform Pos after set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRotSet ).Using( quaternionApproxComparer ), "Disabled: Local transform Rot after set" );

                sut.gameObject.SetActive( true );

                yield return null; // Wait for Update phase

                // Test absolute value setting in Update when disabled
                Vector3Dbl testAbsPosUpdate = new Vector3Dbl( 25, 35, 45 );
                QuaternionDbl testAbsRotUpdate = QuaternionDbl.Euler( 25, 40, 55 );
                Vector3Dbl testAbsVelUpdate = new Vector3Dbl( 2, 3, 4 );
                Vector3Dbl testAbsAngVelUpdate = new Vector3Dbl( 0.2, 0.3, 0.4 );

                IReferenceFrame sceneRefFrameUpdate = GameplaySceneReferenceFrameManager.ReferenceFrame;
                Vector3 testLocalPosUpdate = (Vector3)sceneRefFrameUpdate.InverseTransformPosition( testAbsPosUpdate );
                Quaternion testLocalRotUpdate = (Quaternion)sceneRefFrameUpdate.InverseTransformRotation( testAbsRotUpdate );

                sut.SetAbsolutePosition( testAbsPosUpdate );
                sut.SetAbsoluteRotation( testAbsRotUpdate );
                sut.SetAbsoluteVelocity( testAbsVelUpdate );
                sut.SetAbsoluteAngularVelocity( testAbsAngVelUpdate );

                sut.gameObject.SetActive( false );

                Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( testAbsPosUpdate ).Using( vector3DblApproxComparer ), "Disabled: AbsPos set in Update" );
                Assert.That( sut.GetAbsoluteRotation(), Is.EqualTo( testAbsRotUpdate ).Using( quaternionDblApproxComparer ), "Disabled: AbsRot set in Update" );
                Assert.That( sut.GetAbsoluteVelocity(), Is.EqualTo( testAbsVelUpdate ).Using( vector3DblApproxComparer ), "Disabled: AbsVel set in Update" );
                Assert.That( sut.GetAbsoluteAngularVelocity(), Is.EqualTo( testAbsAngVelUpdate ).Using( vector3DblApproxComparer ), "Disabled: AbsAngVel set in Update" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPosUpdate ).Using( vector3ApproxComparer ), "Disabled: Local transform Pos after AbsPos Update set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRotUpdate ).Using( quaternionApproxComparer ), "Disabled: Local transform Rot after AbsRot Update set" );

                sut.gameObject.SetActive( true );

                // Test local value setting in Update when disabled
                Vector3 testLocalPosSetUpdate = new Vector3( 8, 12, 16 );
                Quaternion testLocalRotSetUpdate = Quaternion.Euler( 15, 25, 35 );
                Vector3 testLocalVelSetUpdate = new Vector3( 0.8f, 1.2f, 1.6f );
                Vector3 testLocalAngVelSetUpdate = new Vector3( 0.08f, 0.12f, 0.16f );

                sut.SetPosition( testLocalPosSetUpdate );
                sut.SetRotation( testLocalRotSetUpdate );
                sut.SetVelocity( testLocalVelSetUpdate );
                sut.SetAngularVelocity( testLocalAngVelSetUpdate );

                sut.gameObject.SetActive( false );

                Assert.That( sut.GetPosition(), Is.EqualTo( testLocalPosSetUpdate ).Using( vector3ApproxComparer ), "Disabled: LocalPos set in Update" );
                Assert.That( sut.GetRotation(), Is.EqualTo( testLocalRotSetUpdate ).Using( quaternionApproxComparer ), "Disabled: LocalRot set in Update" );
                Assert.That( sut.GetVelocity(), Is.EqualTo( testLocalVelSetUpdate ).Using( vector3ApproxComparer ), "Disabled: LocalVel set in Update" );
                Assert.That( sut.GetAngularVelocity(), Is.EqualTo( testLocalAngVelSetUpdate ).Using( vector3ApproxComparer ), "Disabled: LocalAngVel set in Update" );

                Assert.That( sut.transform.position, Is.EqualTo( testLocalPosSetUpdate ).Using( vector3ApproxComparer ), "Disabled: Local transform Pos after LocalPos Update set" );
                Assert.That( sut.transform.rotation, Is.EqualTo( testLocalRotSetUpdate ).Using( quaternionApproxComparer ), "Disabled: Local transform Rot after LocalRot Update set" );

                sut.gameObject.SetActive( true );
            }

            UnityEngine.Object.DestroyImmediate( manager );
            UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }
    }
}
