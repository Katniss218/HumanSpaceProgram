using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_PlayMode.ReferenceFrames
{
    public abstract class ReferenceFrameTransformTestBase
    {
        protected static IEqualityComparer<Vector3> vector3ApproxComparer = new HSP_Tests.NUnit.Vector3ApproximateComparer( 0.0005f );
        protected static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new HSP_Tests.NUnit.Vector3DblApproximateComparer( 0.0005 );
        protected static IEqualityComparer<Quaternion> quaternionApproxComparer = new HSP_Tests.NUnit.QuaternionApproximateComparer( 0.0005f );
        protected static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new HSP_Tests.NUnit.QuaternionDblApproximateComparer( 0.0005 );

        protected GameObject manager;
        protected TimeManager timeManager;
        protected GameplaySceneReferenceFrameManager refFrameManager;

        [SetUp]
        public void SetUp()
        {
            UnityPlus.PlayerLoop.PlayerLoopManager.Initialize( UnityPlus.PlayerLoop.BucketHandling.IncludeThrow );
            manager = new GameObject( "TestManager" );
            timeManager = manager.AddComponent<TimeManager>();
            TimeManager.SetUT( 0 );
            refFrameManager = manager.AddComponent<GameplaySceneReferenceFrameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if( manager != null )
                UnityEngine.Object.DestroyImmediate( manager );
        }

        protected IReferenceFrameTransform CreateSut( Type type )
        {
            GameObject go = new GameObject( type.Name );
            IReferenceFrameTransform trans = (IReferenceFrameTransform)go.AddComponent( type );
            trans.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            return trans;
        }

        protected void DestroySut( IReferenceFrameTransform sut )
        {
            if( sut != null && sut.gameObject != null )
                UnityEngine.Object.DestroyImmediate( sut.gameObject );
        }

        protected struct TestTransformState : IWithSimulationTime
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

        protected static TestTransformState ExtractState( IReferenceFrameTransform sut ) => new TestTransformState()
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

        protected static void AssertCorrectState( TestTransformState state, Vector3Dbl initialPosition, QuaternionDbl initialRotation, Vector3Dbl initialVelocity, Vector3Dbl initialAngularVelocity, double startUT )
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

            Assert.That( state.Position, Is.EqualTo( expectedAbsPos ).Using( vector3DblApproxComparer ), "Absolute position should match expected integrated position" );
            Assert.That( state.Rotation, Is.EqualTo( expectedAbsRot ).Using( quaternionDblApproxComparer ), "Absolute rotation should match expected integrated rotation" );
            Assert.That( state.Velocity, Is.EqualTo( expectedAbsVel ).Using( vector3DblApproxComparer ), "Absolute velocity should remain constant or match expected" );
            Assert.That( state.AngularVelocity, Is.EqualTo( expectedAbsAngVel ).Using( vector3DblApproxComparer ), "Absolute angular velocity should remain constant or match expected" );
            Assert.That( state.Acceleration, Is.EqualTo( expectedAbsAcc ).Using( vector3DblApproxComparer ), "Absolute acceleration should be zero in this integration test" );
            Assert.That( state.AngularAcceleration, Is.EqualTo( expectedAbsAngAcc ).Using( vector3DblApproxComparer ), "Absolute angular acceleration should be zero in this integration test" );

            Assert.That( state.LocalPosition, Is.EqualTo( expectedScenePos ).Using( vector3ApproxComparer ), "Local scene position should match expected transformed position" );
            Assert.That( state.LocalRotation, Is.EqualTo( expectedSceneRot ).Using( quaternionApproxComparer ), "Local scene rotation should match expected transformed rotation" );
            Assert.That( state.LocalVelocity, Is.EqualTo( expectedSceneVel ).Using( vector3ApproxComparer ), "Local scene velocity should match expected transformed velocity" );
            Assert.That( state.LocalAngularVelocity, Is.EqualTo( expectedSceneAngVel ).Using( vector3ApproxComparer ), "Local scene angular velocity should match expected transformed angular velocity" );
            Assert.That( state.LocalAcceleration, Is.EqualTo( expectedSceneAcc ).Using( vector3ApproxComparer ), "Local scene acceleration should match expected transformed acceleration" );
            Assert.That( state.LocalAngularAcceleration, Is.EqualTo( expectedSceneAngAcc ).Using( vector3ApproxComparer ), "Local scene angular acceleration should match expected transformed angular acceleration" );
        }
    }
}
