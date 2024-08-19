using HSP.CelestialBodies;

namespace HSP.Vanilla
{
    public static class CelestialBody_Ex
    {
        public const string ADD_PHYSICS_OBJECT = HSPEvent.NAMESPACE_HSP + ".add_physics_object";
        public const string ADD_SURFACE = HSPEvent.NAMESPACE_HSP + ".add_surface";

        [HSPEventListener( HSPEvent_ON_CELESTIAL_BODY_CREATED.ID, ADD_PHYSICS_OBJECT )]
        private static void AddGameplayPhysicsObject( CelestialBody cb )
        {
            //if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
            //{
            var comp = cb.gameObject.AddComponent<KinematicPhysicsTransform>();
            cb.PhysicsTransform = comp;
            cb.ReferenceFrameTransform = comp;
            //}
        }
    }
}