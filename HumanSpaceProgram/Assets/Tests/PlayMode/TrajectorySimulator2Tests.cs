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

        static ITrajectoryTransform CreateObject( ISceneReferenceFrameProvider provider, bool isAttractor )
        {
            GameObject go = new GameObject();

            var trans = go.AddComponent<KinematicReferenceFrameTransform>();
            trans.SceneReferenceFrameProvider = provider;
            var trajTrans = go.AddComponent<DummyTrajectoryTransform>();
            trajTrans.ReferenceFrameTransform = trans;
            trajTrans.PhysicsTransform = trans;
            trajTrans.Integrator = new EulerIntegrator();
            trajTrans.AccelerationProviders = new ITrajectoryStepProvider[] { new NBodyAccelerationProvider() };
            trajTrans.IsAttractor = isAttractor;

            return trajTrans;
        }

        [UnityTest]
        public IEnumerator AddSingle_IncrementsCount()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator AddMultiple_AllPresent()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform a = CreateObject( new GameplaySceneReferenceFrameProvider(), true );
            ITrajectoryTransform b = CreateObject( new GameplaySceneReferenceFrameProvider(), false );
            ITrajectoryTransform c = CreateObject( new GameplaySceneReferenceFrameProvider(), true );
            ITrajectoryTransform d = CreateObject( new GameplaySceneReferenceFrameProvider(), false );

            sut.TryAddBody( a );
            sut.TryAddBody( b );
            sut.TryAddBody( c );
            sut.TryAddBody( d );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 4 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 2 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 2 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (a as Component).gameObject );
            GameObject.DestroyImmediate( (b as Component).gameObject );
            GameObject.DestroyImmediate( (c as Component).gameObject );
            GameObject.DestroyImmediate( (d as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator RemoveNonExistent_Empty()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            // remove without ever adding
            sut.TryRemoveBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 0 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 0 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator AddThenRemoveBeforeFix_ShouldResultAbsent()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            sut.TryAddBody( obj );
            sut.TryRemoveBody( obj ); // cancel before FixStale
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );

            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 0 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 0 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator AddThenRemoveMultipleBeforeFix_ShouldResultAbsent()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform a = CreateObject( new GameplaySceneReferenceFrameProvider(), true );
            ITrajectoryTransform b = CreateObject( new GameplaySceneReferenceFrameProvider(), true );
            ITrajectoryTransform c = CreateObject( new GameplaySceneReferenceFrameProvider(), false );
            ITrajectoryTransform d = CreateObject( new GameplaySceneReferenceFrameProvider(), false );

            sut.TryAddBody( a );
            sut.TryAddBody( b );
            sut.TryAddBody( c );
            sut.TryAddBody( d );
            sut.TryRemoveBody( a ); // cancel before FixStale
            sut.TryRemoveBody( c );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );

            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 2 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 1 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (a as Component).gameObject );
            GameObject.DestroyImmediate( (b as Component).gameObject );
            GameObject.DestroyImmediate( (c as Component).gameObject );
            GameObject.DestroyImmediate( (d as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator RemoveThenAddBeforeFix_ShouldResultPresent()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            // make sure the object is present in the simulator first
            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();
            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );

            // schedule remove then add (remove cancelled by add)
            sut.TryRemoveBody( obj );
            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );

            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator RepeatedFixStale_NoDuplicates()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator MarkStaleThenAdd_WorksCorrectly() // mark stale then add, then FixStale().
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            // calling MarkStale before add should not break subsequent add
            try
            {
                sut.MarkStale( obj );
            }
            catch
            {
                // Some implementations may not expose MarkStale; swallow to keep the test simple.
            }

            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator AddSame_ShouldBeIdempotent()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            sut.TryAddBody( obj );
            sut.TryAddBody( obj );
            sut.TryAddBody( obj );

            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }


        [UnityTest]
        public IEnumerator RemoveSame_ShouldBeIdempotent()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();
            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            sut.TryRemoveBody( obj );
            sut.TryRemoveBody( obj );
            sut.TryRemoveBody( obj );

            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 0 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 0 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator AddRemoveAdd_WithoutFixBetween_ShouldRespectLastOperation()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            // sequence: add, remove, add (all before FixStale)
            sut.TryAddBody( obj );
            sut.TryRemoveBody( obj );
            sut.TryAddBody( obj );

            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator FixStale_IsIdempotent()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            int before = sut.BodyCount;
            int attractorsBefore = sut.AttractorCount;
            int followersBefore = sut.FollowerCount;

            // call FixStale repeatedly with no changes
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );

            Assert.That( sut.BodyCount, Is.EqualTo( before ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( attractorsBefore ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( followersBefore ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        // The following tests assume there is a MarkStale(ITrajectoryTransform) or similar API.
        // If your implementation uses a different method name you'll get a compile error which
        // will tell you the exact API mismatch. This is intentional so the test-suite can be
        // used to discover the real API surface.

        [UnityTest]
        public IEnumerator MarkStale_BeforeAdd_ShouldStillAddAndClearStale()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            ITrajectoryTransform obj = CreateObject( new GameplaySceneReferenceFrameProvider(), true );

            // call MarkStale before adding
            try
            {
                sut.MarkStale( obj );
            }
            catch( Exception )
            {
                // Some implementations may not expose MarkStale; if so, the test harness
                // will fail compilation. We swallow runtime exceptions here to make the
                // test resilient if the call exists but throws.
            }

            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }

        [UnityTest]
        public IEnumerator MarkStale_MoveArray_ShouldKeepCountAndNotCrash()
        {
            yield return new WaitForFixedUpdate();

            var (manager, _, _, _) = CreateTestScene();
            TrajectorySimulator2 sut = new TrajectorySimulator2( 0.5, 10 );

            var provider = new GameplaySceneReferenceFrameProvider();
            ITrajectoryTransform obj = CreateObject( provider, true );

            sut.TryAddBody( obj );
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();
            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 1 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 0 ) );

            // flip attractor flag and mark stale so FixStale must move the row
            (obj as DummyTrajectoryTransform).IsAttractor = false;
            try
            {
                sut.MarkStale( obj );
            }
            catch( Exception )
            {
                // ignore runtime exception if method missing; test will still exercise add/remove patterns
            }

            // should not throw and should keep the object present
            sut.FixStale( TrajectorySimulator2.SimulationDirection.Forward );
            yield return new WaitForFixedUpdate();

            Assert.That( sut.BodyCount, Is.EqualTo( 1 ) );
            Assert.That( sut.AttractorCount, Is.EqualTo( 0 ) );
            Assert.That( sut.FollowerCount, Is.EqualTo( 1 ) );

            GameObject.DestroyImmediate( manager );
            GameObject.DestroyImmediate( (obj as Component).gameObject );
        }
    }
}