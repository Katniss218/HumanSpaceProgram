using System;
using System.Collections;
using System.Collections.Generic;
using HSP;
using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using HSP_Tests_PlayMode.NUnit;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
{
    public class ReferenceFrameTransformTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        static IEqualityComparer<Vector3> vector3ApproxComparer = new Vector3ApproximateComparer( 0.0001f );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );

        [UnityTest]
        public IEnumerator PositionChange()
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            //HybridReferenceFrameTransform.AddPlayerLoopSystem();
            //KinematicReferenceFrameTransform.AddPlayerLoopSystem();

            yield return new WaitForFixedUpdate(); // resumes right before Update

            GameObject reference = new GameObject();
            FreeReferenceFrameTransform sut = reference.AddComponent<FreeReferenceFrameTransform>();

            sut.Position = Vector3.zero;


            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, new Vector3Dbl( 3, 4, 5 ) ) );

            yield return new WaitForFixedUpdate();

            Assert.That( sut.Position, Is.EqualTo( new Vector3( -3, -4, -5 ) ) );
            Assert.That( sut.AbsolutePosition, Is.EqualTo( Vector3Dbl.zero ) );

            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, new Vector3Dbl( 1, 1, 1 ) ) );

            yield return new WaitForFixedUpdate();

            Assert.That( sut.Position, Is.EqualTo( new Vector3( -1, -1, -1 ) ) );
            Assert.That( sut.AbsolutePosition, Is.EqualTo( Vector3Dbl.zero ) );
        }

        // Fixed doesn't move, so in rested frame it should be at (0, 0, 0) - needs a separate test method.
        /*[UnityTest]
        public IEnumerator Velocity_In_RestedFrame_Fixed()
        {
            yield return Velocity_In_RestedFrame_T<FixedReferenceFrameTransform>();
        }*/


        [UnityTest]
        public IEnumerator Velocity_In_RestedFrame_Free()
        {
            yield return Velocity_In_RestedFrame_T<FreeReferenceFrameTransform>();
        }

        [UnityTest]
        public IEnumerator Velocity_In_RestedFrame_Kinematic()
        {
            yield return Velocity_In_RestedFrame_T<KinematicReferenceFrameTransform>();
        }

        public IEnumerator Velocity_In_RestedFrame_T<T>() where T : Component, IReferenceFrameTransform
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            AssertMonoBehaviour assertHandler = manager.AddComponent<AssertMonoBehaviour>();

            //HybridReferenceFrameTransform.AddPlayerLoopSystem();
            //KinematicReferenceFrameTransform.AddPlayerLoopSystem();

            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 5 );

            double ut = TimeManager.UT;
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, Vector3Dbl.zero ) );
            GameObject reference = new GameObject();
            assertHandler.sut = reference.AddComponent<T>();

            assertHandler.sut.AbsolutePosition = Vector3.zero;
            assertHandler.sut.AbsoluteVelocity = new Vector3Dbl( 10, 0, 0 );

            for( int i = 0; i < 10; i++ )
            {
                yield return new WaitForFixedUpdate();

                var pos = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( Vector3Dbl.zero );
                SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, pos, Vector3Dbl.zero ) );
            }

            for( int i = 0; i < 1000; i++ )
            {
                if( Math.Abs( TimeManager.UT - (ut + 1.0) ) < 0.000001 )
                {
                    // Already after physicsprocessing.

                    Debug.Log( "E" + TimeManager.UT );
                    Debug.Log( "E" + assertHandler.sut.Position.magnitude );
                    Debug.Log( "E" + assertHandler.sut.AbsolutePosition.magnitude );
                    Assert.That( assertHandler.sut.Position, Is.EqualTo( new Vector3( 10, 0, 0 ) ).Using( vector3ApproxComparer ) );
                    Assert.That( assertHandler.sut.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 10, 0, 0 ) ).Using( vector3DblApproxComparer ) );
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            Debug.LogWarning( "Test failed within the specified number of frames." );
            Assert.IsTrue( false );
        }

        [UnityTest]
        public IEnumerator Velocity_In_MovingFrame_Free()
        {
            yield return Velocity_In_MovingFrame_T<FreeReferenceFrameTransform>();
        }

        [UnityTest]
        public IEnumerator Velocity_In_MovingFrame_Kinematic()
        {
            yield return Velocity_In_MovingFrame_T<KinematicReferenceFrameTransform>();
        }

        public IEnumerator Velocity_In_MovingFrame_T<T>() where T : Component, IReferenceFrameTransform
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            AssertMonoBehaviour assertHandler = manager.AddComponent<AssertMonoBehaviour>();

            //HybridReferenceFrameTransform.AddPlayerLoopSystem();
            //KinematicReferenceFrameTransform.AddPlayerLoopSystem();

            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 5 );

            double ut = TimeManager.UT;
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 0, 10, 0 ) ) );
            GameObject reference = new GameObject();
            assertHandler.sut = reference.AddComponent<T>();

            assertHandler.sut.AbsolutePosition = Vector3.zero;
            assertHandler.sut.AbsoluteVelocity = new Vector3Dbl( 10, 0, 0 );

            for( int i = 0; i < 10; i++ )
            {
                yield return new WaitForFixedUpdate();

                var pos = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( Vector3Dbl.zero );
                Debug.Log( "POS: " + pos );
                SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, pos, new Vector3Dbl( 0, 10, 0 ) ) );
            }

            for( int i = 0; i < 1000; i++ )
            {
                if( Math.Abs( TimeManager.UT - (ut + 1.0) ) < 0.000001 )
                {
                    // Already after physicsprocessing.

                    Debug.Log( "E" + TimeManager.UT );
                    Debug.Log( "E" + assertHandler.sut.Position.magnitude );
                    Debug.Log( "E" + assertHandler.sut.AbsolutePosition.magnitude );
                    Assert.That( assertHandler.sut.Position, Is.EqualTo( new Vector3( 10, -10, 0 ) ).Using( vector3ApproxComparer ) );
                    Assert.That( assertHandler.sut.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 10, 0, 0 ) ).Using( vector3DblApproxComparer ) );
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            Debug.LogWarning( "Test failed within the specified number of frames." );
            Assert.IsTrue( false );
        }


        [UnityTest]
        public IEnumerator PositionChangeTimewarped2_Fixed()
        {
            yield return PositionChangeTimewarped2_T<FixedReferenceFrameTransform>();
        }


        [UnityTest]
        public IEnumerator PositionChangeTimewarped2_Free()
        {
            yield return PositionChangeTimewarped2_T<FreeReferenceFrameTransform>();
        }

        [UnityTest]
        public IEnumerator PositionChangeTimewarped2_Kinematic()
        {
            yield return PositionChangeTimewarped2_T<KinematicReferenceFrameTransform>();
        }

        public IEnumerator PositionChangeTimewarped2_T<T>() where T : Component, IReferenceFrameTransform
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            AssertMonoBehaviour assertHandler = manager.AddComponent<AssertMonoBehaviour>();

            //HybridReferenceFrameTransform.AddPlayerLoopSystem();
            //KinematicReferenceFrameTransform.AddPlayerLoopSystem();

            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 5 );

            double ut = TimeManager.UT;
            GameObject reference = new GameObject();
            assertHandler.sut = reference.AddComponent<T>();

            assertHandler.sut.AbsolutePosition = Vector3.zero;
            assertHandler.sut.AbsoluteVelocity = Vector3.zero;
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 10, 0, 0 ) ) );

            for( int i = 0; i < 10; i++ )
            {
                yield return new WaitForFixedUpdate();

                var pos = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( Vector3Dbl.zero );
                SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, pos, new Vector3Dbl( 10, 0, 0 ) ) );
            }

            for( int i = 0; i < 1000; i++ )
            {
                if( Math.Abs( TimeManager.UT - (ut + 1.0) ) < 0.000001 )
                {
                    // Already after physicsprocessing.

                    Debug.Log( "E" + TimeManager.UT );
                    Debug.Log( "E" + assertHandler.sut.Position.magnitude );
                    Debug.Log( "E" + assertHandler.sut.AbsolutePosition.magnitude );
                    Assert.That( assertHandler.sut.Position, Is.EqualTo( new Vector3( -10, 0, 0 ) ).Using( vector3ApproxComparer ) );
                    Assert.That( assertHandler.sut.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 0, 0, 0 ) ).Using( vector3DblApproxComparer ) );
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            Debug.LogWarning( "Test failed within the specified number of frames." );
            Assert.IsTrue( false );
        }
    }
}