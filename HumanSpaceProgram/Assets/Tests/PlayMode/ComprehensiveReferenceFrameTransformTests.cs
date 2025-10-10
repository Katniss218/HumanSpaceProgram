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
    /// Comprehensive test suite for reference frame transforms that can test any implementation
    /// of IReferenceFrameTransform with various reference frame types and scenarios.
    /// </summary>
    public class ComprehensiveReferenceFrameTransformTests
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
        /// Test configuration for different reference frame transform types
        /// </summary>
        public class TestConfig
        {
            public Type TransformType { get; set; }
            public string TestName { get; set; }
            public bool RequiresRigidbody { get; set; }
            public bool SupportsVelocity { get; set; }
            public bool SupportsAcceleration { get; set; }
            public bool IsFixed { get; set; }
        }

        /// <summary>
        /// All reference frame transform types to test
        /// </summary>
        static readonly TestConfig[] TestConfigs = new TestConfig[]
        {
            new TestConfig
            {
                TransformType = typeof(FreeReferenceFrameTransform),
                TestName = "Free",
                RequiresRigidbody = true,
                SupportsVelocity = true,
                SupportsAcceleration = true,
                IsFixed = false
            },
            new TestConfig
            {
                TransformType = typeof(FixedReferenceFrameTransform),
                TestName = "Fixed",
                RequiresRigidbody = true,
                SupportsVelocity = false,
                SupportsAcceleration = false,
                IsFixed = true
            },
            new TestConfig
            {
                TransformType = typeof(KinematicReferenceFrameTransform),
                TestName = "Kinematic",
                RequiresRigidbody = true,
                SupportsVelocity = true,
                SupportsAcceleration = true,
                IsFixed = false
            },
            new TestConfig
            {
                TransformType = typeof(DummyReferenceFrameTransform),
                TestName = "Dummy",
                RequiresRigidbody = false,
                SupportsVelocity = false,
                SupportsAcceleration = false,
                IsFixed = false
            }
        };

        /// <summary>
        /// Creates a test scene with TimeManager and ReferenceFrameManager
        /// </summary>
        static (GameObject manager, TimeManager timeManager, GameplaySceneReferenceFrameManager refFrameManager, AssertMonoBehaviour assertMonoBeh) CreateTestScene()
        {
            GameObject manager = new GameObject( "TestManager" );
            TimeManager timeManager = manager.AddComponent<TimeManager>();
            GameplaySceneReferenceFrameManager refFrameManager = manager.AddComponent<GameplaySceneReferenceFrameManager>();
            GameplaySceneReferenceFrameManager.Instance = refFrameManager;
            KinematicReferenceFrameTransform.AddPlayerLoopSystem();
            var assertMonoBeh = manager.AddComponent<AssertMonoBehaviour>();

            return (manager, timeManager, refFrameManager, assertMonoBeh);
        }

        /// <summary>
        /// Creates a reference frame transform of the specified type
        /// </summary>
        static IReferenceFrameTransform CreateReferenceFrameTransform( Type transformType, GameObject parent = null )
        {
            GameObject go = parent ?? new GameObject( $"Test{transformType.Name}" );

            if( transformType == typeof( FreeReferenceFrameTransform ) )
            {
                var rb = go.AddComponent<Rigidbody>();
                return go.AddComponent<FreeReferenceFrameTransform>();
            }
            else if( transformType == typeof( FixedReferenceFrameTransform ) )
            {
                var rb = go.AddComponent<Rigidbody>();
                return go.AddComponent<FixedReferenceFrameTransform>();
            }
            else if( transformType == typeof( KinematicReferenceFrameTransform ) )
            {
                var rb = go.AddComponent<Rigidbody>();
                return go.AddComponent<KinematicReferenceFrameTransform>();
            }
            else if( transformType == typeof( DummyReferenceFrameTransform ) )
            {
                return go.AddComponent<DummyReferenceFrameTransform>();
            }

            throw new ArgumentException( $"Unsupported transform type: {transformType}" );
        }

        #endregion

        #region Manual Value Setting Tests

        [UnityTest]
        public IEnumerator ManualPositionSetting_AllTypes()
        {
            foreach( var config in TestConfigs )
            {
                yield return ManualPositionSetting_T( config );
            }
        }

        public IEnumerator ManualPositionSetting_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Test setting position to zero
            sut.Position = Vector3.zero;
            Assert.That( sut.Position, Is.EqualTo( Vector3.zero ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Position should be zero after setting to zero" );
            Assert.That( sut.AbsolutePosition, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsolutePosition should be zero after setting Position to zero" );

            // Test setting position to a specific value
            Vector3 testPosition = new Vector3( 10, 20, 30 );
            sut.Position = testPosition;
            Assert.That( sut.Position, Is.EqualTo( testPosition ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Position should match set value" );
            Assert.That( sut.AbsolutePosition, Is.EqualTo( (Vector3Dbl)testPosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsolutePosition should match set value" );

            // Test setting absolute position directly
            Vector3Dbl testAbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            sut.AbsolutePosition = testAbsolutePosition;
            Assert.That( sut.AbsolutePosition, Is.EqualTo( testAbsolutePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsolutePosition should match set value" );
            Assert.That( sut.Position, Is.EqualTo( (Vector3)testAbsolutePosition ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Position should match absolute value" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator ManualRotationSetting_AllTypes()
        {
            foreach( var config in TestConfigs )
            {
                yield return ManualRotationSetting_T( config );
            }
        }

        public IEnumerator ManualRotationSetting_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Test setting rotation to identity
            sut.Rotation = Quaternion.identity;
            Assert.That( sut.Rotation, Is.EqualTo( Quaternion.identity ).Using( quaternionApproxComparer ),
                $"{config.TestName}: Rotation should be identity after setting to identity" );
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( QuaternionDbl.identity ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: AbsoluteRotation should be identity after setting Rotation to identity" );

            // Test setting rotation to a specific value
            Quaternion testRotation = Quaternion.Euler( 45, 90, 135 );
            sut.Rotation = testRotation;
            Assert.That( sut.Rotation, Is.EqualTo( testRotation ).Using( quaternionApproxComparer ),
                $"{config.TestName}: Rotation should match set value" );
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( (QuaternionDbl)testRotation ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: AbsoluteRotation should match set value" );

            // Test setting absolute rotation directly
            QuaternionDbl testAbsoluteRotation = QuaternionDbl.Euler( 60, 120, 180 );
            sut.AbsoluteRotation = testAbsoluteRotation;
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( testAbsoluteRotation ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: AbsoluteRotation should match set value" );
            Assert.That( sut.Rotation, Is.EqualTo( (Quaternion)testAbsoluteRotation ).Using( quaternionApproxComparer ),
                $"{config.TestName}: Rotation should match absolute value" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator ManualVelocitySetting_SupportedTypes()
        {
            foreach( var config in TestConfigs )
            {
                if( config.SupportsVelocity )
                {
                    yield return ManualVelocitySetting_T( config );
                }
            }
        }

        public IEnumerator ManualVelocitySetting_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Test setting velocity to zero
            sut.Velocity = Vector3.zero;
            Assert.That( sut.Velocity, Is.EqualTo( Vector3.zero ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Velocity should be zero after setting to zero" );
            Assert.That( sut.AbsoluteVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsoluteVelocity should be zero after setting Velocity to zero" );

            // Test setting velocity to a specific value
            Vector3 testVelocity = new Vector3( 5, 10, 15 );
            sut.Velocity = testVelocity;
            Assert.That( sut.Velocity, Is.EqualTo( testVelocity ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Velocity should match set value" );
            Assert.That( sut.AbsoluteVelocity, Is.EqualTo( (Vector3Dbl)testVelocity ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsoluteVelocity should match set value" );

            // Test setting absolute velocity directly
            Vector3Dbl testAbsoluteVelocity = new Vector3Dbl( 25, 50, 75 );
            sut.AbsoluteVelocity = testAbsoluteVelocity;
            Assert.That( sut.AbsoluteVelocity, Is.EqualTo( testAbsoluteVelocity ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsoluteVelocity should match set value" );
            Assert.That( sut.Velocity, Is.EqualTo( (Vector3)testAbsoluteVelocity ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Velocity should match absolute value" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator ManualAngularVelocitySetting_SupportedTypes()
        {
            foreach( var config in TestConfigs )
            {
                if( config.SupportsVelocity )
                {
                    yield return ManualAngularVelocitySetting_T( config );
                }
            }
        }

        public IEnumerator ManualAngularVelocitySetting_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Test setting angular velocity to zero
            sut.AngularVelocity = Vector3.zero;
            Assert.That( sut.AngularVelocity, Is.EqualTo( Vector3.zero ).Using( vector3ApproxComparer ),
                $"{config.TestName}: AngularVelocity should be zero after setting to zero" );
            Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsoluteAngularVelocity should be zero after setting AngularVelocity to zero" );

            // Test setting angular velocity to a specific value
            Vector3 testAngularVelocity = new Vector3( 1, 2, 3 );
            sut.AngularVelocity = testAngularVelocity;
            Assert.That( sut.AngularVelocity, Is.EqualTo( testAngularVelocity ).Using( vector3ApproxComparer ),
                $"{config.TestName}: AngularVelocity should match set value" );
            Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( (Vector3Dbl)testAngularVelocity ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsoluteAngularVelocity should match set value" );

            // Test setting absolute angular velocity directly
            Vector3Dbl testAbsoluteAngularVelocity = new Vector3Dbl( 4, 5, 6 );
            sut.AbsoluteAngularVelocity = testAbsoluteAngularVelocity;
            Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( testAbsoluteAngularVelocity ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsoluteAngularVelocity should match set value" );
            Assert.That( sut.AngularVelocity, Is.EqualTo( (Vector3)testAbsoluteAngularVelocity ).Using( vector3ApproxComparer ),
                $"{config.TestName}: AngularVelocity should match absolute value" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Reference Frame Switching Tests

        [UnityTest]
        public IEnumerator ReferenceFrameSwitching_PositionPreservation_AllTypes()
        {
            foreach( var config in TestConfigs )
            {
                yield return ReferenceFrameSwitching_PositionPreservation_T( config );
            }
        }

        public IEnumerator ReferenceFrameSwitching_PositionPreservation_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Set initial position
            Vector3Dbl initialAbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            sut.AbsolutePosition = initialAbsolutePosition;

            // Verify initial state
            Assert.That( sut.AbsolutePosition, Is.EqualTo( initialAbsolutePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Initial absolute position should be preserved" );

            // Switch reference frame
            Vector3Dbl newFrameCenter = new Vector3Dbl( 50, 100, 150 );
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, newFrameCenter ) );

            yield return new WaitForFixedUpdate();

            // Verify absolute position is preserved
            Assert.That( sut.AbsolutePosition, Is.EqualTo( initialAbsolutePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Absolute position should be preserved after reference frame switch" );

            // Verify scene position is adjusted correctly
            Vector3 expectedScenePosition = (Vector3)(initialAbsolutePosition - newFrameCenter);
            Assert.That( sut.Position, Is.EqualTo( expectedScenePosition ).Using( vector3ApproxComparer ),
                $"{config.TestName}: Scene position should be adjusted for new reference frame" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator ReferenceFrameSwitching_RotationPreservation_AllTypes()
        {
            foreach( var config in TestConfigs )
            {
                yield return ReferenceFrameSwitching_RotationPreservation_T( config );
            }
        }

        public IEnumerator ReferenceFrameSwitching_RotationPreservation_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Set initial rotation
            QuaternionDbl initialAbsoluteRotation = QuaternionDbl.Euler( 30, 60, 90 );
            sut.AbsoluteRotation = initialAbsoluteRotation;

            // Verify initial state
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( initialAbsoluteRotation ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: Initial absolute rotation should be preserved" );

            // Switch reference frame (rotation should be preserved for CenteredReferenceFrame)
            Vector3Dbl newFrameCenter = new Vector3Dbl( 50, 100, 150 );
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, newFrameCenter ) );

            yield return new WaitForFixedUpdate();

            // Verify absolute rotation is preserved
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( initialAbsoluteRotation ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: Absolute rotation should be preserved after reference frame switch" );

            // Verify scene rotation is preserved (for CenteredReferenceFrame)
            Assert.That( sut.Rotation, Is.EqualTo( (Quaternion)initialAbsoluteRotation ).Using( quaternionApproxComparer ),
                $"{config.TestName}: Scene rotation should be preserved for CenteredReferenceFrame" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Time Simulation Tests

        [UnityTest]
        public IEnumerator TimeSimulation_VelocityIntegration_SupportedTypes()
        {
            foreach( var config in TestConfigs )
            {
                if( config.SupportsVelocity && !config.IsFixed )
                {
                    yield return TimeSimulation_VelocityIntegration_T( config );
                }
            }
        }
#warning TODO - needs reference frame switching while integrating too. test that
#warning tests for the object reference frame getters too.
#warning  TODO - assert test every frame instead of only at the end.
        // maybe using an assert monobehaviour and an assert func?
        // we need to validate the values inside each update callback function.

        public IEnumerator TimeSimulation_VelocityIntegration_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate(); // resumes at the end of fixedupdate, after physicsupdate, right before 'update'

            TimeManager.SetTimeScale( 10 ); // Speed up time for testing

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Set initial position and velocity
            Vector3Dbl initialPosition = Vector3Dbl.zero;
            Vector3Dbl testVelocity = new Vector3Dbl( 10, 0, 0 );

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteVelocity = testVelocity;

            double startTime = TimeManager.UT;
            double testDuration = 1.0; // 1 second

            assertMonoBeh.TimeProvider = () => TimeManager.OldUT;
            assertMonoBeh.OnFixedUpdate( ( ut ) =>
            {
                double deltaTime = TimeManager.OldUT - startTime;
                Vector3Dbl expectedPosition = initialPosition + testVelocity * deltaTime;
                Assert.That( sut.AbsolutePosition, Is.EqualTo( expectedPosition ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: OnFixedUpdate - Position should integrate velocity over time" );
            } );
            assertMonoBeh.OnUpdate( ( ut ) =>
            {
                double deltaTime = TimeManager.UT - startTime;
                Vector3Dbl expectedPosition = initialPosition + testVelocity * deltaTime;
                Assert.That( sut.AbsolutePosition, Is.EqualTo( expectedPosition ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: OnUpdate - Position should integrate velocity over time" );
            } );
            assertMonoBeh.Enable();

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

            Debug.Log( actualDuration );
            // Verify position integration
            Vector3Dbl expectedPosition = initialPosition + testVelocity * actualDuration;
            Assert.That( sut.AbsolutePosition, Is.EqualTo( expectedPosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Finish - Position should integrate velocity over time" );
            assertMonoBeh.Disable();

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator TimeSimulation_AngularVelocityIntegration_SupportedTypes()
        {
            foreach( var config in TestConfigs )
            {
                if( config.SupportsVelocity && !config.IsFixed )
                {
                    yield return TimeSimulation_AngularVelocityIntegration_T( config );
                }
            }
        }

        public IEnumerator TimeSimulation_AngularVelocityIntegration_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 10 ); // Speed up time for testing

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Set initial rotation and angular velocity
            QuaternionDbl initialRotation = QuaternionDbl.identity;
            Vector3Dbl testAngularVelocity = new Vector3Dbl( 0, 0, 90 * 0.0174532925 ); // 90 deg/s around Z axis

            sut.AbsoluteRotation = initialRotation;
            sut.AbsoluteAngularVelocity = testAngularVelocity;

            double startTime = TimeManager.UT;
            double testDuration = 1;

            // Simulate time passage
            for( int i = 0; i < 100; i++ )
            {
                yield return new WaitForFixedUpdate();
                double endTime2 = TimeManager.UT;
                double actualDuration2 = endTime2 - startTime;
                // Verify rotation integration (should be 90 degrees around Z axis)
                QuaternionDbl expectedRotation2 = QuaternionDbl.Euler( testAngularVelocity * (actualDuration2 * 57.29577951308232) );
                Debug.Log( actualDuration2 + " : " + sut.AbsoluteRotation + " : "  + expectedRotation2 );

                if( TimeManager.UT - startTime >= testDuration )
                {
                    break;
                }
            }
            double endTime = TimeManager.UT;
            double actualDuration = endTime - startTime;
            Debug.Log( actualDuration );
            // Verify rotation integration (should be 90 degrees around Z axis)
            QuaternionDbl expectedRotation = QuaternionDbl.Euler( testAngularVelocity * (actualDuration * 57.29577951308232) );
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( expectedRotation ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: Rotation should integrate angular velocity over time" );

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Frame Timing Tests

        [UnityTest]
        public IEnumerator FrameTiming_ValueConsistency_SupportedTypes()
        {
            foreach( var config in TestConfigs )
            {
                if( config.SupportsVelocity )
                {
                    yield return FrameTiming_ValueConsistency_T( config );
                }
            }
        }

        public IEnumerator FrameTiming_ValueConsistency_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Set initial values
            Vector3Dbl initialPosition = new Vector3Dbl( 100, 200, 300 );
            Vector3Dbl initialVelocity = new Vector3Dbl( 10, 20, 30 );

            sut.AbsolutePosition = initialPosition;
            sut.AbsoluteVelocity = initialVelocity;

            // Store values before physics update
            Vector3Dbl positionBeforePhysics = sut.AbsolutePosition;
            Vector3Dbl velocityBeforePhysics = sut.AbsoluteVelocity;

            // Wait for physics update
            yield return new WaitForFixedUpdate();

            // Store values after physics update
            Vector3Dbl positionAfterPhysics = sut.AbsolutePosition;
            Vector3Dbl velocityAfterPhysics = sut.AbsoluteVelocity;

            // For non-fixed transforms, values should be consistent within the same frame
            // but may change between frames due to physics integration
            if( !config.IsFixed )
            {
                // Values should be consistent within the same frame
                Assert.That( sut.AbsolutePosition, Is.EqualTo( positionAfterPhysics ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Position should be consistent within the same frame" );
                Assert.That( sut.AbsoluteVelocity, Is.EqualTo( velocityAfterPhysics ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Velocity should be consistent within the same frame" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Event Tests

        [UnityTest]
        public IEnumerator EventFiring_ValueChanges_AllTypes()
        {
            foreach( var config in TestConfigs )
            {
                yield return EventFiring_ValueChanges_T( config );
            }
        }

        public IEnumerator EventFiring_ValueChanges_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Track event firing
            bool positionChangedFired = false;
            bool rotationChangedFired = false;
            bool velocityChangedFired = false;
            bool angularVelocityChangedFired = false;
            bool anyValueChangedFired = false;

            sut.OnAbsolutePositionChanged += () => positionChangedFired = true;
            sut.OnAbsoluteRotationChanged += () => rotationChangedFired = true;
            sut.OnAbsoluteVelocityChanged += () => velocityChangedFired = true;
            sut.OnAbsoluteAngularVelocityChanged += () => angularVelocityChangedFired = true;
            sut.OnAnyValueChanged += () => anyValueChangedFired = true;

            // Test position change event
            sut.AbsolutePosition = new Vector3Dbl( 100, 200, 300 );
            Assert.IsTrue( positionChangedFired, $"{config.TestName}: OnAbsolutePositionChanged should fire when position is set" );
            Assert.IsTrue( anyValueChangedFired, $"{config.TestName}: OnAnyValueChanged should fire when position is set" );

            // Reset flags
            positionChangedFired = false;
            anyValueChangedFired = false;

            // Test rotation change event
            sut.AbsoluteRotation = QuaternionDbl.Euler( 45, 90, 135 );
            Assert.IsTrue( rotationChangedFired, $"{config.TestName}: OnAbsoluteRotationChanged should fire when rotation is set" );
            Assert.IsTrue( anyValueChangedFired, $"{config.TestName}: OnAnyValueChanged should fire when rotation is set" );

            // Reset flags
            rotationChangedFired = false;
            anyValueChangedFired = false;

            // Test velocity change event (if supported)
            if( config.SupportsVelocity )
            {
                sut.AbsoluteVelocity = new Vector3Dbl( 10, 20, 30 );
                Assert.IsTrue( velocityChangedFired, $"{config.TestName}: OnAbsoluteVelocityChanged should fire when velocity is set" );
                Assert.IsTrue( anyValueChangedFired, $"{config.TestName}: OnAnyValueChanged should fire when velocity is set" );

                // Reset flags
                velocityChangedFired = false;
                anyValueChangedFired = false;

                // Test angular velocity change event
                sut.AbsoluteAngularVelocity = new Vector3Dbl( 1, 2, 3 );
                Assert.IsTrue( angularVelocityChangedFired, $"{config.TestName}: OnAbsoluteAngularVelocityChanged should fire when angular velocity is set" );
                Assert.IsTrue( anyValueChangedFired, $"{config.TestName}: OnAnyValueChanged should fire when angular velocity is set" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion

        #region Edge Case Tests

        [UnityTest]
        public IEnumerator EdgeCases_ZeroValues_AllTypes()
        {
            foreach( var config in TestConfigs )
            {
                yield return EdgeCases_ZeroValues_T( config );
            }
        }

        public IEnumerator EdgeCases_ZeroValues_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Test setting all values to zero
            sut.AbsolutePosition = Vector3Dbl.zero;
            sut.AbsoluteRotation = QuaternionDbl.identity;

            if( config.SupportsVelocity )
            {
                sut.AbsoluteVelocity = Vector3Dbl.zero;
                sut.AbsoluteAngularVelocity = Vector3Dbl.zero;
            }

            // Verify all values are zero/identity
            Assert.That( sut.AbsolutePosition, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: AbsolutePosition should be zero" );
            Assert.That( sut.AbsoluteRotation, Is.EqualTo( QuaternionDbl.identity ).Using( quaternionDblApproxComparer ),
                $"{config.TestName}: AbsoluteRotation should be identity" );

            if( config.SupportsVelocity )
            {
                Assert.That( sut.AbsoluteVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: AbsoluteVelocity should be zero" );
                Assert.That( sut.AbsoluteAngularVelocity, Is.EqualTo( Vector3Dbl.zero ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: AbsoluteAngularVelocity should be zero" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        [UnityTest]
        public IEnumerator EdgeCases_LargeValues_AllTypes()
        {
            foreach( var config in TestConfigs )
            {
                yield return EdgeCases_LargeValues_T( config );
            }
        }

        public IEnumerator EdgeCases_LargeValues_T( TestConfig config )
        {
            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            GameplaySceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            IReferenceFrameTransform sut = CreateReferenceFrameTransform( config.TransformType );
            sut.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();

            // Test with large values (but within reasonable bounds for testing)
            Vector3Dbl largePosition = new Vector3Dbl( 1e6, 2e6, 3e6 );
            Vector3Dbl largeVelocity = new Vector3Dbl( 1e3, 2e3, 3e3 );

            sut.AbsolutePosition = largePosition;

            if( config.SupportsVelocity )
            {
                sut.AbsoluteVelocity = largeVelocity;
            }

            // Verify large values are handled correctly
            Assert.That( sut.AbsolutePosition, Is.EqualTo( largePosition ).Using( vector3DblApproxComparer ),
                $"{config.TestName}: Should handle large position values" );

            if( config.SupportsVelocity )
            {
                Assert.That( sut.AbsoluteVelocity, Is.EqualTo( largeVelocity ).Using( vector3DblApproxComparer ),
                    $"{config.TestName}: Should handle large velocity values" );
            }

            // Cleanup
            UnityEngine.Object.DestroyImmediate( manager );
        }

        #endregion
    }
}
