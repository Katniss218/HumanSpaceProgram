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
    public class HybridReferenceFrameTransformTests : ReferenceFrameTransformTestBase
    {
        private struct HybridSimState : IWithSimulationTime
        {
            public float GetSimulationTime() => (float)UT;
            public double UT { get; set; }
            public Vector3Dbl AbsolutePos { get; set; }
            public QuaternionDbl AbsoluteRot { get; set; }
            public Vector3Dbl AbsoluteVel { get; set; }
            public Vector3Dbl AbsoluteAngVel { get; set; }

            public Vector3 LocalPos { get; set; }
            public Quaternion LocalRot { get; set; }
            public Vector3 LocalVel { get; set; }
            public Vector3 LocalAngVel { get; set; }

            public bool IsSceneSpace { get; set; }
            public bool IsKinematic { get; set; }

            public IReferenceFrame SceneRefFrame { get; set; }
        }

        private static bool GetIsSceneSpace( HybridReferenceFrameTransform sut )
        {
            var isSceneSpaceField = typeof( HybridReferenceFrameTransform ).GetField( "_isSceneSpace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
            return (bool)isSceneSpaceField.GetValue( sut );
        }

        private static HybridSimState ExtractHybridState( HybridReferenceFrameTransform sut )
        {
            Rigidbody rb = sut.GetComponent<Rigidbody>();
            return new HybridSimState()
            {
                UT = TimeManager.UT,
                AbsolutePos = sut.GetAbsolutePosition(),
                AbsoluteRot = sut.GetAbsoluteRotation(),
                AbsoluteVel = sut.GetAbsoluteVelocity(),
                AbsoluteAngVel = sut.GetAbsoluteAngularVelocity(),

                LocalPos = sut.GetPosition(),
                LocalRot = sut.GetRotation(),
                LocalVel = sut.GetVelocity(),
                LocalAngVel = sut.GetAngularVelocity(),

                IsSceneSpace = GetIsSceneSpace( sut ),
                IsKinematic = rb.isKinematic,

                SceneRefFrame = GameplaySceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT )
            };
        }

        private static void VerifyHybridTestState(
            HybridSimState state,
            Vector3Dbl initialPosition,
            QuaternionDbl initialRotation,
            Vector3Dbl initialVelocity,
            Vector3Dbl initialAngularVelocity,
            double startUT,
            bool expectedSceneSpace )
        {
            double deltaTime = state.UT - startUT;

            Vector3Dbl expectedAbsPos = initialPosition + (initialVelocity * deltaTime);

            double omegaMag = initialAngularVelocity.magnitude;
            Vector3Dbl axis = omegaMag > 0.0 ? initialAngularVelocity.normalized : new Vector3Dbl( 1, 0, 0 );
            double angle = omegaMag * deltaTime;
            QuaternionDbl expectedAbsRot = QuaternionDbl.AngleAxis( angle * 57.29577951308232, axis ) * initialRotation;

            Vector3Dbl expectedAbsVel = initialVelocity;
            Vector3Dbl expectedAbsAngVel = initialAngularVelocity;

            IReferenceFrame sceneRef = state.SceneRefFrame;

            Vector3 expectedScenePos = (Vector3)sceneRef.InverseTransformPosition( expectedAbsPos );
            Quaternion expectedSceneRot = (Quaternion)sceneRef.InverseTransformRotation( expectedAbsRot );
            Vector3 expectedSceneVel = (Vector3)sceneRef.InverseTransformVelocity( expectedAbsVel );
            Vector3 expectedSceneAngVel = (Vector3)sceneRef.InverseTransformAngularVelocity( expectedAbsAngVel );

            Assert.That( state.AbsolutePos, Is.EqualTo( expectedAbsPos ).Using( vector3DblApproxComparer ), "Absolute position should match expected integrated position" );
            Assert.That( state.AbsoluteRot, Is.EqualTo( expectedAbsRot ).Using( quaternionDblApproxComparer ), "Absolute rotation should match expected integrated rotation" );
            Assert.That( state.AbsoluteVel, Is.EqualTo( expectedAbsVel ).Using( vector3DblApproxComparer ), "Absolute velocity should remain constant" );
            Assert.That( state.AbsoluteAngVel, Is.EqualTo( expectedAbsAngVel ).Using( vector3DblApproxComparer ), "Absolute angular velocity should remain constant" );

            Assert.That( state.LocalPos, Is.EqualTo( expectedScenePos ).Using( vector3ApproxComparer ), "Local scene position should match expected transformed position" );
            Assert.That( state.LocalRot, Is.EqualTo( expectedSceneRot ).Using( quaternionApproxComparer ), "Local scene rotation should match expected transformed rotation" );
            Assert.That( state.LocalVel, Is.EqualTo( expectedSceneVel ).Using( vector3ApproxComparer ), "Local scene velocity should match expected transformed velocity" );
            Assert.That( state.LocalAngVel, Is.EqualTo( expectedSceneAngVel ).Using( vector3ApproxComparer ), "Local scene angular velocity should match expected transformed angular velocity" );

            Assert.That( state.IsSceneSpace, Is.EqualTo( expectedSceneSpace ), "Simulation space (isSceneSpace) mismatch" );
            Assert.That( state.IsKinematic, Is.EqualTo( !expectedSceneSpace ), "Rigidbody.isKinematic mismatch" );
        }

        [TestCase( 1, 2, 3, ExpectedResult = null )]
        [TestCase( 10, 20, 30, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator HybridTransform_WithLimitsDefault_SimulatesInSceneSpace( double vx, double vy, double vz )
        {
            IReferenceFrameTransform trans = CreateSut( typeof( HybridReferenceFrameTransform ) );
            HybridReferenceFrameTransform sut = (HybridReferenceFrameTransform)trans;

            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( vx, vy, vz );
            Vector3Dbl initialAngularVelocity = new Vector3Dbl( 0.1, 0.2, 0.3 );

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            using var history = new HistoryRecorder( 10f );
            yield return new WaitForFixedUpdate();
            double startUT = TimeManager.UT;

            history.Record<UnityPlus.PlayerLoop.Phases.FixedUpdate, HybridSimState>( () => ExtractHybridState( sut ) );
            history.Record<UnityPlus.PlayerLoop.Phases.Update, HybridSimState>( () =>
            {
                var s = ExtractHybridState( sut );
                s.UT = TimeManager.UT + (TimeManager.UT - TimeManager.OldUT);
                return s;
            } );

            yield return new WaitForSeconds( 0.5f );

            var timeline = history.AssertTimeline<HybridSimState>().StartingHere();
            for( int i = 0; i < 3; i++ )
            {
                timeline
                    .NextFixedUpdate().Verify( d => VerifyHybridTestState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT, expectedSceneSpace: true ) )
                    .NextUpdate().Verify( d => VerifyHybridTestState( d, initialPosition, initialRotation, initialVelocity, initialAngularVelocity, startUT, expectedSceneSpace: true ) );
            }

            DestroySut( sut );
        }

        [TestCase( 10, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator HybridTransform_ExceedingPositionRange_SwitchesToAbsoluteMode( double startPosVal )
        {
            IReferenceFrameTransform trans = CreateSut( typeof( HybridReferenceFrameTransform ) );
            HybridReferenceFrameTransform sut = (HybridReferenceFrameTransform)trans;

            // Set small position limit so we can easily exceed it
            sut.PositionRange = 5f;

            Vector3Dbl initialPosition = new Vector3Dbl( startPosVal, 0, 0 );
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = Vector3Dbl.zero;
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            yield return new WaitForFixedUpdate();
            double startUT = TimeManager.UT;

            // Check if it is correctly in absolute mode when startPosVal > PositionRange, and scene mode when startPosVal <= PositionRange
            bool expectedInitialSceneSpace = Math.Abs( startPosVal ) <= sut.PositionRange;
            Assert.That( GetIsSceneSpace( sut ), Is.EqualTo( expectedInitialSceneSpace ), "Should initialize in correct space mode according to limits" );
            Assert.That( sut.GetComponent<Rigidbody>().isKinematic, Is.EqualTo( !expectedInitialSceneSpace ), "IsKinematic mismatch" );

            // Dynamically mutate position to exceed PositionRange
            sut.SetAbsolutePosition( new Vector3Dbl( 100, 0, 0 ) );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.False, "Should have transitioned to absolute space after position exceeded range" );
            Assert.That( sut.GetComponent<Rigidbody>().isKinematic, Is.True, "Rigidbody should be kinematic in absolute mode" );

            // Dynamically mutate position back within limits
            sut.SetAbsolutePosition( new Vector3Dbl( 1, 0, 0 ) );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should have transitioned back to scene space after returning within range" );
            Assert.That( sut.GetComponent<Rigidbody>().isKinematic, Is.False, "Rigidbody should not be kinematic in scene mode" );

            DestroySut( sut );
        }

        [TestCase( 5, ExpectedResult = null )]
        [UnityTest]
        public IEnumerator HybridTransform_ExceedingVelocityRange_SwitchesToAbsoluteMode( double startVelVal )
        {
            IReferenceFrameTransform trans = CreateSut( typeof( HybridReferenceFrameTransform ) );
            HybridReferenceFrameTransform sut = (HybridReferenceFrameTransform)trans;

            sut.VelocityRange = 3f;

            Vector3Dbl initialPosition = Vector3Dbl.zero;
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl initialVelocity = new Vector3Dbl( startVelVal, 0, 0 );
            Vector3Dbl initialAngularVelocity = Vector3Dbl.zero;

            sut.SetAbsolutePosition( initialPosition );
            sut.SetAbsoluteRotation( initialRotation );
            sut.SetAbsoluteVelocity( initialVelocity );
            sut.SetAbsoluteAngularVelocity( initialAngularVelocity );

            yield return new WaitForFixedUpdate();

            bool expectedInitialSceneSpace = Math.Abs( startVelVal ) <= sut.VelocityRange;
            Assert.That( GetIsSceneSpace( sut ), Is.EqualTo( expectedInitialSceneSpace ), "Should initialize in correct space mode according to velocity limits" );

            // Dynamically mutate velocity to exceed limit
            sut.SetAbsoluteVelocity( new Vector3Dbl( 20, 0, 0 ) );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.False, "Should transitioned to absolute space due to high velocity" );

            // Return velocity to within limits
            sut.SetAbsoluteVelocity( new Vector3Dbl( 1, 0, 0 ) );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should transition back to scene space when velocity is within limits" );

            DestroySut( sut );
        }

        [UnityTest]
        public IEnumerator HybridTransform_ExceedingTimeScale_SwitchesToAbsoluteMode()
        {
            IReferenceFrameTransform trans = CreateSut( typeof( HybridReferenceFrameTransform ) );
            HybridReferenceFrameTransform sut = (HybridReferenceFrameTransform)trans;

            sut.MaxTimeScale = 2f;

            sut.SetAbsolutePosition( Vector3Dbl.zero );
            sut.SetAbsoluteRotation( QuaternionDbl.identity );
            sut.SetAbsoluteVelocity( Vector3Dbl.zero );
            sut.SetAbsoluteAngularVelocity( Vector3Dbl.zero );

            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should be in scene space mode under normal timescale" );

            // Set time-scale past threshold
            TimeManager.SetTimeScale( 5f );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.False, "Should have transitioned to absolute space after timescale exceeded MaxTimeScale" );

            // Reset timescale
            TimeManager.SetTimeScale( 1f );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should have transitioned back to scene space under normal timescale" );

            DestroySut( sut );
        }

        [UnityTest]
        public IEnumerator HybridTransform_StationaryInSceneSpace_AbsolutePositionDoesNotJumpToZero()
        {
            IReferenceFrameTransform trans = CreateSut( typeof( HybridReferenceFrameTransform ) );
            HybridReferenceFrameTransform sut = (HybridReferenceFrameTransform)trans;

            // Ensure we are in scene space by being close to scene origin (0,0,0)
            Vector3Dbl initialPosition = new Vector3Dbl( 500, 0, 0 );
            sut.SetAbsolutePosition( initialPosition );

            yield return new WaitForFixedUpdate();

            // Should be in scene space
            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should be in scene space" );

            // Wait for a few more frames to let the system run
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Check absolute position. It should still be (500, 0, 0)
            Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( initialPosition ).Using( vector3DblApproxComparer ), "Absolute position jump detected! (Likely stale _requestedState overwrote _state.Position)" );

            DestroySut( sut );
        }

        [UnityTest]
        public IEnumerator HybridTransform_AbsolutePosition_FrameSwitchConsistency()
        {
            IReferenceFrameTransform trans = CreateSut( typeof( HybridReferenceFrameTransform ) );
            HybridReferenceFrameTransform sut = (HybridReferenceFrameTransform)trans;

            Vector3Dbl initialPosition = new Vector3Dbl( 100, 0, 0 );
            sut.SetAbsolutePosition( initialPosition );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should be in scene space initially" );

            // Shift the scene frame
            Vector3Dbl shift = new Vector3Dbl( 50, 0, 0 );
            IReferenceFrame newFrame = new CenteredReferenceFrame( TimeManager.UT, shift );
            refFrameManager.RequestReferenceFrameSwitch( newFrame );

            yield return new WaitForFixedUpdate();

            // After switch, it should still be in scene space (pos is 50 now relative to new frame)
            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should stay in scene space after switch" );

            // Absolute position should STILL be 100.
            Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( initialPosition ).Using( vector3DblApproxComparer ), "Absolute position jumped after frame switch in scene space!" );

            DestroySut( sut );
        }

        [UnityTest]
        public IEnumerator HybridTransform_SceneSpaceMode_MaintainPositionWhenStationary()
        {
            IReferenceFrameTransform trans = CreateSut( typeof( HybridReferenceFrameTransform ) );
            HybridReferenceFrameTransform sut = (HybridReferenceFrameTransform)trans;

            Vector3Dbl initialPosition = new Vector3Dbl( 100, 0, 0 );
            sut.SetAbsolutePosition( initialPosition );
            yield return new WaitForFixedUpdate();

            Assert.That( GetIsSceneSpace( sut ), Is.True, "Should be in scene space" );

            // 1. Move the Rigidbody. This makes the cache value (absolute pos) 110.
            Vector3 movement = new Vector3( 10, 0, 0 );
            sut.GetComponent<Rigidbody>().position += movement;

            // 2. Warm the cache with the NEW position.
            // This sets _state.Position = 110, _lastCachedPosition = 110 (local).
            // But _requestedState.Position is STILL 100!
            Vector3Dbl posAfterMove = sut.GetAbsolutePosition();
            Assert.That( posAfterMove, Is.EqualTo( initialPosition + (Vector3Dbl)movement ).Using( vector3DblApproxComparer ) );

            // 3. Wait for one physics frame where the Rigidbody DOES NOT move further.
            // PhysicsStep will run and do: _state.Position = _requestedState.Position (which is 100).
            yield return new WaitForFixedUpdate();

            // 4. Check absolute position. 
            // IsCacheValid() will see that _rb.position is still what it was in step 2.
            // So it returns true and gives us the (now stale) _state.Position (100).
            Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( initialPosition + (Vector3Dbl)movement ).Using( vector3DblApproxComparer ),
                "Object jumped back to _requestedState even though it was stationary in scene space!" );

            DestroySut( sut );
        }
    }
}
