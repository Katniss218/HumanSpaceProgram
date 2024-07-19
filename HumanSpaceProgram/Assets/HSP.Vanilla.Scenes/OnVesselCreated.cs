using HSP.SceneManagement;
using HSP.Trajectories;
using HSP.Trajectories.Components;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Scenes
{
    public static class Vessel_Ex
    {
        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, "add_physics_object" )]
        private static void AddGameplayPhysicsObject( Vessel v )
        {
            if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
                v.PhysicsObject = v.gameObject.AddComponent<FreePhysicsObject>();
            else if( SceneLoader.IsSceneLoaded( DesignSceneManager.SCENE_NAME ) )
                v.PhysicsObject = v.gameObject.AddComponent<FixedPhysicsObject>();
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_HIERARCHY_CHANGED.ID, "try_pin_physics_object" )]
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