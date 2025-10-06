using System;
using System.Collections;
using System.Collections.Generic;
using HSP;
using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP_Tests_PlayMode.NUnit;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
{
    /// <summary>
    /// Comprehensive test suite for PinnedReferenceFrameTransform that verifies:
    /// - Correct frame timing behavior (values before/after physics update)
    /// - Integration with target reference frame transforms
    /// - Proper following behavior when pinned to moving targets
    /// - Reference frame switching behavior
    /// - Event firing and value consistency
    /// </summary>
    public class PinnedReferenceFrameTransformTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        static IEqualityComparer<Vector3> vector3ApproxComparer = new Vector3ApproximateComparer( 0.0001f );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );
        static IEqualityComparer<Quaternion> quaternionApproxComparer = new QuaternionApproximateComparer( 0.0001f );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 0.0001 );

        #region Test Data and Setup

        /// <summary>
        /// Test configuration for different target reference frame transform types
        /// </summary>
        public class TargetConfig
        {
            public Type TargetTransformType { get; set; }
            public string TestName { get; set; }
            public bool SupportsVelocity { get; set; }
            public bool SupportsAcceleration { get; set; }
            public bool IsFixed { get; set; }
        }

        /// <summary>
        /// All target reference frame transform types to test with pinned transforms
        /// </summary>
        static readonly TargetConfig[] TargetConfigs = new TargetConfig[]
        {
            new TargetConfig
            {
                TargetTransformType = typeof(FreeReferenceFrameTransform),
                TestName = "Free",
                SupportsVelocity = true,
                SupportsAcceleration = true,
                IsFixed = false
            },
            new TargetConfig
            {
                TargetTransformType = typeof(FixedReferenceFrameTransform),
                TestName = "Fixed",
                SupportsVelocity = false,
                SupportsAcceleration = false,
                IsFixed = true
            },
            new TargetConfig
            {
                TargetTransformType = typeof(KinematicReferenceFrameTransform),
                TestName = "Kinematic",
                SupportsVelocity = true,
                SupportsAcceleration = true,
                IsFixed = false
            },
            new TargetConfig
            {
                TargetTransformType = typeof(DummyReferenceFrameTransform),
                TestName = "Dummy",
                SupportsVelocity = false,
                SupportsAcceleration = false,
                IsFixed = false
            }
        };

        /// <summary>
        /// Creates a test scene with TimeManager and ReferenceFrameManager
        /// </summary>
        static (GameObject manager, TimeManager timeManager, GameplaySceneReferenceFrameManager refFrameManager) CreateTestScene()
        {
            GameObject manager = new GameObject( "TestManager" );
            TimeManager timeManager = manager.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager refFrameManager = manager.AddComponent<GameplaySceneReferenceFrameManager>();
            GameplaySceneReferenceFrameManager.Instance = refFrameManager;
            KinematicReferenceFrameTransform.AddPlayerLoopSystem();

            return (manager, timeManager, refFrameManager);
        }

        /// <summary>
        /// Creates a reference frame transform of the specified type
        /// </summary>
        static IReferenceFrameTransform CreateReferenceFrameTransform( Type transformType, GameObject parent = null )
        {
            GameObject go = parent ?? new GameObject( $"Test{transformType.Name}" );

            if( transformType == typeof( FreeReferenceFrameTransform ) )
            {
                go.AddComponent<Rigidbody>();
                return go.AddComponent<FreeReferenceFrameTransform>();
            }
            else if( transformType == typeof( FixedReferenceFrameTransform ) )
            {
                go.AddComponent<Rigidbody>();
                return go.AddComponent<FixedReferenceFrameTransform>();
            }
            else if( transformType == typeof( KinematicReferenceFrameTransform ) )
            {
                go.AddComponent<Rigidbody>();
                return go.AddComponent<KinematicReferenceFrameTransform>();
            }
            else if( transformType == typeof( DummyReferenceFrameTransform ) )
            {
                return go.AddComponent<DummyReferenceFrameTransform>();
            }

            throw new ArgumentException( $"Unsupported transform type: {transformType}" );
        }

        /// <summary>
        /// Creates a pinned reference frame transform
        /// </summary>
        static PinnedReferenceFrameTransform CreatePinnedReferenceFrameTransform( GameObject parent = null )
        {
            GameObject go = parent ?? new GameObject( "TestPinned" );
            go.AddComponent<Rigidbody>();
            return go.AddComponent<PinnedReferenceFrameTransform>();
        }

        #endregion

        #region Basic Pinning Tests

        [UnityTest]
        public IEnumerator BasicPinning_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return BasicPinning_T( config );
            }
        }

        public IEnumerator BasicPinning_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            targetTransform.AbsoluteRotation = QuaternionDbl.Euler( 45, 90, 135 );

            // Create pinned transform
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Set up pinning
            Vector3Dbl referencePosition = new Vector3Dbl( 10, 20, 30 );
            QuaternionDbl referenceRotation = QuaternionDbl.Euler( 15, 30, 45 );
            pinnedTransform.SetReference( targetTransform, referencePosition, referenceRotation );

            // Verify pinned transform follows target
            Vector3Dbl expectedAbsolutePosition = targetTransform.AbsolutePosition + (targetTransform.AbsoluteRotation * referencePosition);
            QuaternionDbl expectedAbsoluteRotation = targetTransform.AbsoluteRotation * referenceRotation;

            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Pinned transform should follow target position" );
            Assert.That( pinnedTransform.AbsoluteRotation, Is.EqualTo( expectedAbsoluteRotation ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: Pinned transform should follow target rotation" );

            yield return new WaitForFixedUpdate();

            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Pinned transform should follow target position" );
            Assert.That( pinnedTransform.AbsoluteRotation, Is.EqualTo( expectedAbsoluteRotation ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: Pinned transform should follow target rotation" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator PinningWithoutTarget_DefaultBehavior()
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create pinned transform without target
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Set reference position and rotation
            Vector3Dbl referencePosition = new Vector3Dbl( 50, 100, 150 );
            QuaternionDbl referenceRotation = QuaternionDbl.Euler( 30, 60, 90 );
            pinnedTransform.ReferencePosition = referencePosition;
            pinnedTransform.ReferenceRotation = referenceRotation;

            // Verify pinned transform uses default behavior (centered at origin)
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( referencePosition ).Using( vector3DblApproxComparer ),
                "Pinned transform without target should use reference position as absolute position" );
            Assert.That( pinnedTransform.AbsoluteRotation, Is.EqualTo( referenceRotation ).Using( quaternionDblApproxComparer ),
                "Pinned transform without target should use reference rotation as absolute rotation" );

            yield return new WaitForFixedUpdate();

            // Verify pinned transform uses default behavior (centered at origin)
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( referencePosition ).Using( vector3DblApproxComparer ),
                "Pinned transform without target should use reference position as absolute position" );
            Assert.That( pinnedTransform.AbsoluteRotation, Is.EqualTo( referenceRotation ).Using( quaternionDblApproxComparer ),
                "Pinned transform without target should use reference rotation as absolute rotation" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Frame Timing Tests

        [UnityTest]
        public IEnumerator FrameTiming_ValueConsistency_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return FrameTiming_ValueConsistency_T( config );
            }
        }

        public IEnumerator FrameTiming_ValueConsistency_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );

            // Create pinned transform
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.SetReference( targetTransform, new Vector3Dbl( 10, 20, 30 ), QuaternionDbl.identity );

            // Store values before physics update
            Vector3Dbl positionBeforePhysics = pinnedTransform.AbsolutePosition;
            QuaternionDbl rotationBeforePhysics = pinnedTransform.AbsoluteRotation;

            // Wait for physics update
            yield return new WaitForFixedUpdate();

            // Store values after physics update
            Vector3Dbl positionAfterPhysics = pinnedTransform.AbsolutePosition;
            QuaternionDbl rotationAfterPhysics = pinnedTransform.AbsoluteRotation;

            // Values should be consistent within the same frame
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( positionAfterPhysics ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Position should be consistent within the same frame" );
            Assert.That( pinnedTransform.AbsoluteRotation, Is.EqualTo( rotationAfterPhysics ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: Rotation should be consistent within the same frame" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator FrameTiming_VelocityAndAcceleration_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return FrameTiming_VelocityAndAcceleration_T( config );
            }
        }

        public IEnumerator FrameTiming_VelocityAndAcceleration_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );

            if( config.SupportsVelocity )
            {
                targetTransform.AbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
            }

            // Create pinned transform
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.SetReference( targetTransform, new Vector3Dbl( 5, 10, 15 ), QuaternionDbl.identity );

            yield return new WaitForFixedUpdate();

            // Verify velocity and acceleration are calculated correctly
            // For pinned transforms, velocity should match target velocity (since it's pinned to the target)
#warning TODO - should take non-inertial effects into account (tangential velocity).
            if( config.SupportsVelocity )
            {
                Assert.That( pinnedTransform.AbsoluteVelocity, Is.EqualTo( targetTransform.AbsoluteVelocity ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform velocity should match target velocity" );
            }
            else
            {
                Assert.That( pinnedTransform.AbsoluteVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform velocity should be zero when target has no velocity" );
            }

            // Acceleration should be calculated based on target's acceleration
            if( config.SupportsAcceleration )
            {
                // For non-fixed targets, acceleration should match target acceleration
                Assert.That( pinnedTransform.AbsoluteAcceleration, Is.EqualTo( targetTransform.AbsoluteAcceleration ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform acceleration should match target acceleration" );
            }
            else
            {
                // For fixed targets, acceleration should be zero
                Assert.That( pinnedTransform.AbsoluteAcceleration, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform acceleration should be zero for fixed targets" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Integration with Moving Targets

        [UnityTest]
        public IEnumerator IntegrationWithMovingTarget_AllMovingTypes()
        {
            foreach( var config in TargetConfigs )
            {
                if( !config.IsFixed )
                {
                    yield return IntegrationWithMovingTarget_T( config );
                }
            }
        }

        public IEnumerator IntegrationWithMovingTarget_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 10 ); // Speed up time for testing

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = Vector3Dbl.zero;
            Vector3Dbl testVelocity = new Vector3Dbl( 10, 0, 0 );

            if( config.SupportsVelocity )
            {
                targetTransform.AbsoluteVelocity = testVelocity; // Move at 10 m/s in X direction
            }

            // Create pinned transform
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.SetReference( targetTransform, new Vector3Dbl( 0, 5, 0 ), QuaternionDbl.identity ); // 5m offset in Y direction

            double startTime = TimeManager.UT;
            double testDuration = 1.0; // 1 second

            // Simulate time passage
            for( int i = 0; i < 100; i++ )
            {
                yield return new WaitForFixedUpdate();

                if( TimeManager.UT - startTime >= testDuration )
                {
                    break;
                }
            }
            double endTime = TimeManager.UT;
            double actualDuration = endTime - startTime;

            // Verify pinned transform follows target movement
            if( config.SupportsVelocity )
            {
                Vector3Dbl expectedTargetPosition = testVelocity * actualDuration;
                Vector3Dbl expectedPinnedPosition = expectedTargetPosition + new Vector3Dbl( 0, 5, 0 );

                Assert.That( targetTransform.AbsolutePosition, Is.EqualTo( expectedTargetPosition ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Target should move correctly" );
                Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( expectedPinnedPosition ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform should follow target movement" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator IntegrationWithRotatingTarget_AllMovingTypes()
        {
            foreach( var config in TargetConfigs )
            {
                if( !config.IsFixed )
                {
                    yield return IntegrationWithRotatingTarget_T( config );
                }
            }
        }

        public IEnumerator IntegrationWithRotatingTarget_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 10 ); // Speed up time for testing

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = Vector3Dbl.zero;
            targetTransform.AbsoluteRotation = QuaternionDbl.identity;
            Vector3Dbl testAngularVelocity = new Vector3Dbl( 0, 0, 90 * 0.0174532925 ); // 90 deg/s around Z axis

            if( config.SupportsVelocity )
            {
                targetTransform.AbsoluteAngularVelocity = testAngularVelocity;
            }

            // Create pinned transform
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            var initialPos = new Vector3Dbl( 5, 0, 0 );
            pinnedTransform.SetReference( targetTransform, initialPos, QuaternionDbl.identity ); // 5m offset in X direction

            double startTime = TimeManager.UT;
            double testDuration = 1;

            // Simulate time passage
            for( int i = 0; i < 100; i++ )
            {
                yield return new WaitForFixedUpdate();

                if( TimeManager.UT - startTime >= testDuration )
                {
                    break;
                }
            }
            double endTime = TimeManager.UT;
            double actualDuration = endTime - startTime;

            // Verify pinned transform follows target rotation
            if( config.SupportsVelocity )
            {
                // Target should have rotated 90 degrees around Z axis
                QuaternionDbl expectedTargetRotation = QuaternionDbl.Euler( testAngularVelocity * actualDuration * 57.29577951308232 );
                Assert.That( targetTransform.AbsoluteRotation, Is.EqualTo( expectedTargetRotation ).Using( quaternionDblApproxComparer ),
                    $"{config.TestName}: Target should rotate correctly" );

                // Pinned transform should be at the rotated position (5m in Y direction now)
                Vector3Dbl expectedPinnedPosition = expectedTargetRotation * initialPos;
                Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( expectedPinnedPosition ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform should follow target rotation" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Reference Frame Switching Tests

        [UnityTest]
        public IEnumerator ReferenceFrameSwitching_PositionPreservation_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return ReferenceFrameSwitching_PositionPreservation_T( config );
            }
        }

        public IEnumerator ReferenceFrameSwitching_PositionPreservation_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );

            // Create pinned transform
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.SetReference( targetTransform, new Vector3Dbl( 10, 20, 30 ), QuaternionDbl.identity );

            // Calculate expected absolute position
            Vector3Dbl expectedAbsolutePosition = targetTransform.AbsolutePosition + new Vector3Dbl( 10, 20, 30 );

            // Verify initial state
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Initial absolute position should be correct" );

            // Switch reference frame
            Vector3Dbl newFrameCenter = new Vector3Dbl( 50, 100, 150 );
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, newFrameCenter ) );

            yield return new WaitForFixedUpdate();

            // Verify absolute position is preserved
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( expectedAbsolutePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Absolute position should be preserved after reference frame switch" );

            // Verify scene position is adjusted correctly
            Vector3 expectedScenePosition = (Vector3)(expectedAbsolutePosition - newFrameCenter);
            Assert.That( pinnedTransform.Position, Is.EqualTo( expectedScenePosition ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Scene position should be adjusted for new reference frame" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Event Tests

        [UnityTest]
        public IEnumerator EventFiring_ValueChanges_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return EventFiring_ValueChanges_T( config );
            }
        }

        public IEnumerator EventFiring_ValueChanges_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Create pinned transform
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Track event firing
            bool positionChangedFired = false;
            bool rotationChangedFired = false;
            bool anyValueChangedFired = false;

            pinnedTransform.OnAbsolutePositionChanged += () => positionChangedFired = true;
            pinnedTransform.OnAbsoluteRotationChanged += () => rotationChangedFired = true;
            pinnedTransform.OnAnyValueChanged += () => anyValueChangedFired = true;

            // Test setting reference (should fire events)
            pinnedTransform.SetReference( targetTransform, new Vector3Dbl( 10, 20, 30 ), QuaternionDbl.Euler( 45, 90, 135 ) );

            Assert.IsTrue( positionChangedFired, $"{config.TestName}: OnAbsolutePositionChanged should fire when reference is set" );
            Assert.IsTrue( rotationChangedFired, $"{config.TestName}: OnAbsoluteRotationChanged should fire when reference is set" );
            Assert.IsTrue( anyValueChangedFired, $"{config.TestName}: OnAnyValueChanged should fire when reference is set" );

            // Reset flags
            positionChangedFired = false;
            rotationChangedFired = false;
            anyValueChangedFired = false;

            // Test changing reference position
            pinnedTransform.ReferencePosition = new Vector3Dbl( 15, 25, 35 );

            Assert.IsTrue( positionChangedFired, $"{config.TestName}: OnAbsolutePositionChanged should fire when reference position is changed" );
            Assert.IsTrue( anyValueChangedFired, $"{config.TestName}: OnAnyValueChanged should fire when reference position is changed" );

            // Reset flags
            positionChangedFired = false;
            anyValueChangedFired = false;

            // Test changing reference rotation
            pinnedTransform.ReferenceRotation = QuaternionDbl.Euler( 60, 120, 180 );

            Assert.IsTrue( rotationChangedFired, $"{config.TestName}: OnAbsoluteRotationChanged should fire when reference rotation is changed" );
            Assert.IsTrue( anyValueChangedFired, $"{config.TestName}: OnAnyValueChanged should fire when reference rotation is changed" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Edge Case Tests

        [UnityTest]
        public IEnumerator EdgeCases_ZeroValues_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return EdgeCases_ZeroValues_T( config );
            }
        }

        public IEnumerator EdgeCases_ZeroValues_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = Vector3Dbl.zero;
            targetTransform.AbsoluteRotation = QuaternionDbl.identity;

            // Create pinned transform with zero reference values
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.SetReference( targetTransform, Vector3Dbl.zero, QuaternionDbl.identity );

            yield return new WaitForFixedUpdate();

            // Verify zero values are handled correctly
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Should handle zero reference position" );
            Assert.That( pinnedTransform.AbsoluteRotation, Is.EqualTo( QuaternionDbl.identity ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: Should handle zero reference rotation" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator EdgeCases_LargeValues_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return EdgeCases_LargeValues_T( config );
            }
        }

        public IEnumerator EdgeCases_LargeValues_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform with large values
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = new Vector3Dbl( 1e6, 2e6, 3e6 );

            // Create pinned transform with large reference values
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.SetReference( targetTransform, new Vector3Dbl( 1e5, 2e5, 3e5 ), QuaternionDbl.identity );

            yield return new WaitForFixedUpdate();

            // Verify large values are handled correctly
            Vector3Dbl expectedPosition = new Vector3Dbl( 1.1e6, 2.2e6, 3.3e6 );
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( expectedPosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Should handle large position values" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator EdgeCases_NullTarget_DefaultBehavior()
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create pinned transform with null target
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.ReferenceTransform = null; // Explicitly set to null
            pinnedTransform.ReferencePosition = new Vector3Dbl( 100, 200, 300 );
            pinnedTransform.ReferenceRotation = QuaternionDbl.Euler( 45, 90, 135 );

            yield return new WaitForFixedUpdate();

            // Verify default behavior (should use reference values as absolute values)
            Assert.That( pinnedTransform.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 100, 200, 300 ) ).Using( vector3DblApproxComparer ),
                "Pinned transform with null target should use reference position as absolute position" );
            Assert.That( pinnedTransform.AbsoluteRotation, Is.EqualTo( QuaternionDbl.Euler( 45, 90, 135 ) ).Using( quaternionDblApproxComparer ),
                "Pinned transform with null target should use reference rotation as absolute rotation" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Velocity and Acceleration Tests

        [UnityTest]
        public IEnumerator VelocityAndAcceleration_RotatingTangent_AllTargetTypes()
        {
            foreach( var config in TargetConfigs )
            {
                yield return VelocityAndAcceleration_RotatingTangent_T( config );
            }
        }

        public IEnumerator VelocityAndAcceleration_RotatingTangent_T( TargetConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            // Create target transform
            IReferenceFrameTransform targetTransform = CreateReferenceFrameTransform( config.TargetTransformType );
            targetTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            targetTransform.AbsolutePosition = Vector3Dbl.zero;
            targetTransform.AbsoluteRotation = QuaternionDbl.identity;

            if( config.SupportsVelocity )
            {
                targetTransform.AbsoluteVelocity = new Vector3Dbl( 10, 0, 0 );
                targetTransform.AbsoluteAngularVelocity = new Vector3Dbl( 0, 0, 1 ); // 1 rad/s around Z axis
            }

            // Create pinned transform with offset
            PinnedReferenceFrameTransform pinnedTransform = CreatePinnedReferenceFrameTransform();
            pinnedTransform.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            pinnedTransform.SetReference( targetTransform, new Vector3Dbl( 5, 0, 0 ), QuaternionDbl.identity ); // 5m offset in X direction

            yield return new WaitForFixedUpdate();

            // Verify velocity calculation
            if( config.SupportsVelocity )
            {
                // Pinned transform should have the same linear velocity as target
                Assert.That( pinnedTransform.AbsoluteVelocity, Is.EqualTo( targetTransform.AbsoluteVelocity ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform should have same linear velocity as target" );

                // Angular velocity should also match
                Assert.That( pinnedTransform.AbsoluteAngularVelocity, Is.EqualTo( targetTransform.AbsoluteAngularVelocity ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform should have same angular velocity as target" );
            }
            else
            {
                // For fixed targets, velocity should be zero
                Assert.That( pinnedTransform.AbsoluteVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform velocity should be zero for fixed targets" );
                Assert.That( pinnedTransform.AbsoluteAngularVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Pinned transform angular velocity should be zero for fixed targets" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion
    }
}
