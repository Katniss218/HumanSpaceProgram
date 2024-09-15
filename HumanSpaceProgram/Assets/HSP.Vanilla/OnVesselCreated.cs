using HSP.SceneManagement;
using HSP.Trajectories;
using HSP.Trajectories.Components;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class OnVesselCreated
    {
        public const string ADD_REFERENCE_FRAME_TRANSFORM = HSPEvent.NAMESPACE_HSP + ".add_reference_frame_transform";
        public const string ADD_TRAJECTORY_TRANSFORM = HSPEvent.NAMESPACE_HSP + ".add_trajectory_transform";
        public const string TRY_PIN_PHYSICS_OBJECT = HSPEvent.NAMESPACE_HSP + ".try_pin_physics_object";

        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, ADD_REFERENCE_FRAME_TRANSFORM )]
        private static void AddGameplayReferenceFrameTransform( Vessel v )
        {
            if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
            {
                var comp = v.gameObject.AddComponent<FreeReferenceFrameTransform>();
            }
            else if( SceneLoader.IsSceneLoaded( DesignSceneManager.SCENE_NAME ) )
            {
                var comp = v.gameObject.AddComponent<FixedReferenceFrameTransform>();
            }
        }
        
        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, ADD_TRAJECTORY_TRANSFORM )]
        private static void AddGameplayTrajectoryTransform( Vessel v )
        {
            if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
            {
                TrajectoryTransform comp = v.gameObject.AddComponent<TrajectoryTransform>();
                comp.Trajectory = new NewtonianOrbit( Time.TimeManager.UT, ..., ..., ... );
                comp.IsAttractor = false;
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
                    PinnedReferenceFrameTransform ppo = e.oldRootPart.GetVessel().GetComponent<PinnedReferenceFrameTransform>();
                    e.v.Pin( ppo.ReferenceBody, ppo.ReferencePosition, ppo.ReferenceRotation );
                }
            }
        }
    }
}