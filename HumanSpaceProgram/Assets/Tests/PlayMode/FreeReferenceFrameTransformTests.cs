using System;
using System.Collections;
using System.Collections.Generic;
using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla;
using HSP_Tests_PlayMode.NUnit;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
{
    public class FreeReferenceFrameTransformTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        [UnityTest]
        public IEnumerator PositionChange()
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ) );

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForFixedUpdate();

            GameObject reference = new GameObject();
            FreeReferenceFrameTransform sut = reference.AddComponent<FreeReferenceFrameTransform>();

            sut.Position = Vector3.zero;


            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, new Vector3Dbl( 3, 4, 5 ) ) );

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForFixedUpdate();

            Assert.That( sut.Position, Is.EqualTo( new Vector3( -3, -4, -5 ) ) );
            Assert.That( sut.AbsolutePosition, Is.EqualTo( Vector3Dbl.zero ) );

            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredReferenceFrame( TimeManager.UT, new Vector3Dbl( 1, 1, 1 ) ) );

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForFixedUpdate();

            Assert.That( sut.Position, Is.EqualTo( new Vector3( -1, -1, -1 ) ) );
            Assert.That( sut.AbsolutePosition, Is.EqualTo( Vector3Dbl.zero ) );
        }

        [UnityTest]
        public IEnumerator PositionChangeTimewarped2()
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            AssertMonoBehaviour assertHandler = manager.AddComponent<AssertMonoBehaviour>();

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 5 );

            double ut = TimeManager.UT;
            GameObject reference = new GameObject();
            assertHandler.sut = reference.AddComponent<FreeReferenceFrameTransform>();

            assertHandler.sut.AbsolutePosition = Vector3.zero;
            assertHandler.sut.AbsoluteVelocity = Vector3.zero;
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 10, 0, 0 ) ) );

            for( int i = 0; i < 10; i++ )
            {
#warning TODO - something is wrong with switching indeed. During a switch it looks as if the position change from that frame isn't applied (if the frame is moving).
                // regardless of timewarp actually.
                // regardless of if the switch happens at the beginning or in physicsprocessing.
                // if I switch it after physicsprocessing, it goes in the wrong direction actually.

                yield return new WaitForFixedUpdate();

                SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 10, 0, 0 ) ) );
            }

            for( int i = 0; i < 1000; i++ )
            {
                if( Math.Abs( TimeManager.UT - (ut + 1.0) ) < 0.000001 )
                {
                    Debug.Log( "E" + TimeManager.UT );
                    Debug.Log( "E" + assertHandler.sut.Position.magnitude );
                    Debug.Log( "E" + assertHandler.sut.AbsolutePosition.magnitude );
                    Assert.That( assertHandler.sut.Position, Is.EqualTo( new Vector3( -10, 0, 0 ) ).Using( vector3ApproxComparer ) );
                    Assert.That( assertHandler.sut.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 0, 0, 0 ) ).Using( vector3DblApproxComparer ) );
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTest]
        public IEnumerator PositionChangeTimewarped2_Kinematic()
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            AssertMonoBehaviour assertHandler = manager.AddComponent<AssertMonoBehaviour>();

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 5 );

            double ut = TimeManager.UT;
            GameObject reference = new GameObject();
            assertHandler.sut = reference.AddComponent<KinematicReferenceFrameTransform>();

            assertHandler.sut.AbsolutePosition = Vector3.zero;
            assertHandler.sut.AbsoluteVelocity = Vector3.zero;
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 10, 0, 0 ) ) );

            for( int i = 0; i < 10; i++ )
            {
#warning TODO - something is wrong with switching indeed. During a switch it looks as if the position change from that frame isn't applied (if the frame is moving).
                // regardless of timewarp actually.
                // regardless of if the switch happens at the beginning or in physicsprocessing.
                // if I switch it after physicsprocessing, it goes in the wrong direction actually.

                yield return new WaitForFixedUpdate();

                SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 10, 0, 0 ) ) );
            }

            for( int i = 0; i < 1000; i++ )
            {
                if( Math.Abs( TimeManager.UT - (ut + 1.0) ) < 0.000001 )
                {
                    Debug.Log( "E" + TimeManager.UT );
                    Debug.Log( "E" + assertHandler.sut.Position.magnitude );
                    Debug.Log( "E" + assertHandler.sut.AbsolutePosition.magnitude );
                    Assert.That( assertHandler.sut.Position, Is.EqualTo( new Vector3( -10, 0, 0 ) ).Using( vector3ApproxComparer ) );
                    Assert.That( assertHandler.sut.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 0, 0, 0 ) ).Using( vector3DblApproxComparer ) );
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        static IEqualityComparer<Vector3> vector3ApproxComparer = new Vector3ApproximateComparer( 0.0001f );
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 0.0001 );

        [UnityTest]
        public IEnumerator PositionChangeTimewarped3()
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<SceneReferenceFrameManager>();
            AssertMonoBehaviour assertHandler = manager.AddComponent<AssertMonoBehaviour>();

            Debug.Log( "A" + TimeManager.UT );
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 5 );

            Debug.Log( "B" + TimeManager.UT );
            double ut = TimeManager.UT;
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, Vector3Dbl.zero ) );
            GameObject reference = new GameObject();
            assertHandler.sut = reference.AddComponent<FreeReferenceFrameTransform>();

            assertHandler.sut.AbsolutePosition = Vector3.zero;
            assertHandler.sut.AbsoluteVelocity = new Vector3Dbl( 10, 0, 0 );

            yield return new WaitForFixedUpdate();

            Debug.Log( "C" + TimeManager.UT );
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, Vector3Dbl.zero ) );

            yield return new WaitForFixedUpdate();

            Debug.Log( "D" + TimeManager.UT );
            SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, Vector3Dbl.zero ) );

            for( int i = 0; i < 1000; i++ )
            {
                if( Math.Abs( TimeManager.UT - (ut + 1.0) ) < 0.000001 )
                {
                    Debug.Log( "E" + TimeManager.UT );
                    Debug.Log( "E" + assertHandler.sut.Position.magnitude );
                    Debug.Log( "E" + assertHandler.sut.AbsolutePosition.magnitude );
                    Assert.That( assertHandler.sut.Position, Is.EqualTo( new Vector3( 10, 0, 0 ) ).Using( vector3ApproxComparer ) );
                    Assert.That( assertHandler.sut.AbsolutePosition, Is.EqualTo( new Vector3Dbl( 10, 0, 0 ) ).Using( vector3DblApproxComparer ) );
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }
        }
    }
}