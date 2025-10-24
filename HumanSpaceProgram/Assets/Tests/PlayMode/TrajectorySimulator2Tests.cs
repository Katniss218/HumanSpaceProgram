using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Trajectories;
using HSP.Trajectories.AccelerationProviders;
using HSP.Trajectories.TrajectoryIntegrators;
using HSP.Vanilla;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Trajectories;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
{
    public class DummyTrajectoryTransform : MonoBehaviour, ITrajectoryTransform
    {
        public IReferenceFrameTransform ReferenceFrameTransform { get; set; }

        public IPhysicsTransform PhysicsTransform { get; set; }

        public ITrajectoryIntegrator Integrator { get; set; }

        public IReadOnlyList<ITrajectoryStepProvider> AccelerationProviders { get; set; }

        public bool IsAttractor { get; set; }

        public bool TrajectoryNeedsUpdating()
        {
            return false;
        }
    }

    public class TrajectorySimulator2Tests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

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

        static ITrajectoryTransform CreateObject( ISceneReferenceFrameProvider provider )
        {
            GameObject go = new GameObject();

            var trans = go.AddComponent<KinematicReferenceFrameTransform>();
            trans.SceneReferenceFrameProvider = provider;
            var trajTrans = go.AddComponent<DummyTrajectoryTransform>();
            trajTrans.ReferenceFrameTransform = trans;
            trajTrans.PhysicsTransform = trans;
            trajTrans.Integrator = new EulerIntegrator();
            trajTrans.AccelerationProviders = new ITrajectoryStepProvider[] { new NBodyAccelerationProvider() };
            trajTrans.IsAttractor = true;

            return trajTrans;
        }

        [UnityTest]
        public IEnumerator SimpleTest()
        {
            yield return new WaitForFixedUpdate();

            // Arrange
            var (manager, timeManager, refFrameManager, assertMonoBeh) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider() );
            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );

            yield return new WaitForFixedUpdate();

            sut.TryRemoveBody( obj );
            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );


            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
        }
    }
}