using HSP.SceneManagement;
using HSP.Trajectories;
using HSP.Trajectories.Components;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class Vessel_Ex
    {
        public const string ADD_PHYSICS_OBJECT = HSPEvent.NAMESPACE_HSP + ".add_physics_object";
        public const string TRY_PIN_PHYSICS_OBJECT = HSPEvent.NAMESPACE_HSP + ".try_pin_physics_object";

        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, ADD_PHYSICS_OBJECT )]
        private static void AddGameplayPhysicsObject( Vessel v )
        {
            if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
            {
                var comp = v.gameObject.AddComponent<FreePhysicsObject>();
                v.PhysicsTransform = comp;
                v.ReferenceFrameTransform = comp;
            }
            else if( SceneLoader.IsSceneLoaded( DesignSceneManager.SCENE_NAME ) )
            {
                var comp = v.gameObject.AddComponent<FixedPhysicsObject>();
                v.PhysicsTransform = comp;
                v.ReferenceFrameTransform = comp;
            }
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_HIERARCHY_CHANGED.ID, TRY_PIN_PHYSICS_OBJECT )]
        private static void TryPinPhysicsObject( (Vessel v, Transform oldRootPart, Transform newRootPart) e )
        {
            if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
            {
                if( e.oldRootPart == null )
                    return;

                if( FAnchor.IsAnchored( e.v.RootPart ) )
                {
                    PinnedPhysicsObject ppo = e.oldRootPart.GetVessel().GetComponent<PinnedPhysicsObject>();
                    e.v.Pin( ppo.ReferenceBody, ppo.ReferencePosition, ppo.ReferenceRotation );
                }
            }
        }
    }
}