using HSP.CelestialBodies;
using HSP.Vanilla.ReferenceFrames;

namespace HSP.Vanilla
{
    public static class OnCelestialBodyCreated
    {
        public const string ADD_PHYSICS_OBJECT = HSPEvent.NAMESPACE_HSP + ".add_physics_object";
        public const string ADD_TRAJECTORY_TRANSFORM = HSPEvent.NAMESPACE_HSP + ".add_trajectory_transform";
        public const string ADD_SURFACE = HSPEvent.NAMESPACE_HSP + ".add_surface";

        [HSPEventListener( HSPEvent_ON_CELESTIAL_BODY_CREATED.ID, ADD_PHYSICS_OBJECT )]
        private static void AddGameplayPhysicsObject( CelestialBody cb )
        {
            var comp = cb.gameObject.AddComponent<KinematicReferenceFrameTransform>();
            comp.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            //var comp = cb.gameObject.AddComponent<HybridReferenceFrameTransform>();
            //comp.AllowCollisionResponse = false;
            comp.Mass = (float)cb.Mass;
        }

        /*[HSPEventListener( HSPEvent_ON_CELESTIAL_BODY_CREATED.ID, ADD_TRAJECTORY_TRANSFORM )]
        private static void AddGameplayTrajectoryTransform( CelestialBody cb )
        {
            if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
            {
                TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
                comp.Trajectory = new KeplerianOrbit( Time.TimeManager.UT, ..., ..., cb.Mass );
#warning parent body
                comp.IsAttractor = true;
            }
        THIS SHOULD BE SET UP BY THE PLANETARY SYSTEM FACTORY.
        }
        */
    }
}