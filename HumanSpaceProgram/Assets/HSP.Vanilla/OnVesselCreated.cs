using HSP.SceneManagement;
using HSP.Trajectories.AccelerationProviders;
using HSP.Trajectories.Components;
using HSP.Trajectories.TrajectoryIntegrators;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Trajectories;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class OnVesselCreated
    {
        public const float VESSEL_POSITION_RANGE = 1e5f;
        public const float VESSEL_VELOCITY_RANGE = 1e4f;
        public const float VESSEL_MAX_TIMESCALE = 64f;

        public const string ADD_REFERENCE_FRAME_TRANSFORM = HSPEvent.NAMESPACE_HSP + ".add_reference_frame_transform";
        public const string ADD_TRAJECTORY_TRANSFORM = HSPEvent.NAMESPACE_HSP + ".add_trajectory_transform";
        public const string TRY_PIN_PHYSICS_OBJECT = HSPEvent.NAMESPACE_HSP + ".try_pin_physics_object";

        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, ADD_REFERENCE_FRAME_TRANSFORM )]
        private static void AddGameplayReferenceFrameTransform( Vessel v )
        {
            if( HSPSceneManager.IsLoaded<GameplaySceneM>() )
            {
                var comp = v.gameObject.AddComponent<HybridReferenceFrameTransform>();
                comp.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
                comp.PositionRange = VESSEL_POSITION_RANGE;
                comp.VelocityRange = VESSEL_VELOCITY_RANGE;
                comp.MaxTimeScale = VESSEL_MAX_TIMESCALE;
                comp.AllowSceneSimulation = true;
            }
            else if( HSPSceneManager.IsLoaded<DesignSceneM>() )
            {
                var comp = v.gameObject.AddComponent<FixedReferenceFrameTransform>();
                comp.SceneReferenceFrameProvider = new GameplaySceneReferenceFrameProvider();
            }
        }

        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, ADD_TRAJECTORY_TRANSFORM )]
        private static void AddGameplayTrajectoryTransform( Vessel v )
        {
            if( HSPSceneManager.IsLoaded<GameplaySceneM>() )
            {
                TrajectoryTransform comp = v.gameObject.AddComponent<TrajectoryTransform>();
                comp.Integrator = new EulerIntegrator();
                comp.SetAccelerationProviders( new NBodyAccelerationProvider() );
                comp.IsAttractor = false;
                // no need to recalculate the mass of the vessel because it's not an attractor.
            }
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_HIERARCHY_CHANGED.ID, TRY_PIN_PHYSICS_OBJECT )]
        private static void TryPinPhysicsObject( (Vessel v, Transform oldRootPart, Transform newRootPart) e )
        {
            if( HSPSceneManager.IsLoaded<GameplaySceneM>() )
            {
                if( e.oldRootPart == null )
                    return;

                if( FAnchor.IsAnchored( e.v.RootPart ) )
                {
                    PinnedCelestialBodyReferenceFrameTransform ppo = e.oldRootPart.GetVessel().GetComponent<PinnedCelestialBodyReferenceFrameTransform>();
                    e.v.Pin( ppo.ReferenceBody, ppo.ReferencePosition, ppo.ReferenceRotation );
                }
            }
        }
    }
}