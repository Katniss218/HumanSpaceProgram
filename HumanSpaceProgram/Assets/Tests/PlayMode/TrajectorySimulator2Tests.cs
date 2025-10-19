using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Trajectories;
using HSP.Vanilla.Scenes.GameplayScene;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

namespace HSP_Tests_PlayMode
{
    /*internal class DummyTrajectoryTransform : ITrajectoryTransform
    {

    }*/

    public class TrajectorySimulator2Tests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        public IEnumerator Velocity_In_RestedFrame_T<T>() where T : Component, IReferenceFrameTransform
        {
            // Arrange
            GameObject manager = new GameObject();
            manager.AddComponent<TimeManager>();
            manager.AddComponent<GameplaySceneReferenceFrameManager>();
            AssertMonoBehaviour assertHandler = manager.AddComponent<AssertMonoBehaviour>();

            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );


            //HybridReferenceFrameTransform.AddPlayerLoopSystem();
            //KinematicReferenceFrameTransform.AddPlayerLoopSystem();

            //sut.TryAddBody

            yield return new WaitForFixedUpdate();

            TimeManager.SetTimeScale( 5 );

            Debug.LogWarning( "Test failed within the specified number of frames." );
            Assert.IsTrue( false );
        }
    }
}