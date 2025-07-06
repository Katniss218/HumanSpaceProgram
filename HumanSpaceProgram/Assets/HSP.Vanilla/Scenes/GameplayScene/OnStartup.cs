using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Trajectories;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string UNPAUSE = HSPEvent.NAMESPACE_HSP + ".unpause";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, UNPAUSE )]
        private static void Unpause()
        {
            TimeManager.Unpause();
        }

        public const string ADD_TIMESCALE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_timescale_input_controller";
        public const string ADD_VESSEL_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_vessel_manager";
        public const string ADD_CELESTIAL_BODY_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_celestial_body_manager";
        public const string ADD_GAMEPLAY_SCENE_TOOL_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_gameplay_scene_tool_manager";
        public const string ADD_SCENE_REFERENCE_FRAME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_scene_reference_frame_manager";
        public const string ADD_ACTIVE_VESSEL_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_active_vessel_manager";
        public const string ADD_SELECTED_CONTROL_FRAME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_selected_control_frame_manager";
        public const string ADD_ESCAPE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller";
        public const string REMOVE_ESCAPE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".remove_escape_icontroller";
        public const string ADD_TRAJECTORY_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_trajectory_manager";
        public const string ADD_ATMOSPHERE_RENDERER = HSPEvent.NAMESPACE_HSP + ".add_atmosphere_renderer";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_TIMESCALE_INPUT_CONTROLLER )]
        private static void AddTimescaleInputController()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<TimeScaleInputController>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_VESSEL_MANAGER )]
        private static void AddVesselManager()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<VesselManager>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_CELESTIAL_BODY_MANAGER )]
        private static void AddCelestialBodyManager()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<CelestialBodyManager>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_ACTIVE_VESSEL_MANAGER )]
        private static void AddActiveVesselManager()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<ActiveVesselManager>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_SELECTED_CONTROL_FRAME_MANAGER )]
        private static void AddSelectedControlFrameManager()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<SelectedControlFrameManager>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_GAMEPLAY_SCENE_TOOL_MANAGER )]
        private static void AddGameplaySceneToolManager()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<GameplaySceneToolManager>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_SCENE_REFERENCE_FRAME_MANAGER )]
        private static void AddSceneReferenceFrameManager()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<SceneReferenceFrameManager>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, ADD_ESCAPE_INPUT_CONTROLLER )]
        private static void AddEscapeInputController()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<GameplaySceneEscapeInputController>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID, REMOVE_ESCAPE_INPUT_CONTROLLER )]
        private static void RemoveEscapeInputController()
        {
            var comp = GameplaySceneM.Instance.gameObject.GetComponent<GameplaySceneEscapeInputController>();
            if( comp != null )
            {
                Destroy( comp );
            }
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_TRAJECTORY_MANAGER )]
        private static void AddTrajectoryManager()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<TrajectoryManager>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, ADD_ATMOSPHERE_RENDERER,
            After = new[] { GameplaySceneCameraManager.CREATE_CAMERA } )]
        private static void AddAtmosphereRenderer()
        {
            AtmosphereRenderer atmosphereRenderer = GameplaySceneCameraManager.EffectCamera.gameObject.GetOrAddComponent<AtmosphereRenderer>();
            atmosphereRenderer.light = GameObject.Find( "CBLight" ).GetComponent<Light>();
            atmosphereRenderer.ColorRenderTextureGetter = () => GameplaySceneCameraManager.ColorRenderTexture;
            atmosphereRenderer.DepthRenderTextureGetter = () => GameplaySceneDepthBufferCombiner.CombinedDepthRenderTexture;
        }

        public const string RESET_UT = HSPEvent.NAMESPACE_HSP + ".reset_ut";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, RESET_UT )]
        private static void ResetUT()
        {
#warning TODO - don't. save in scenario instead.
            TimeManager.SetUT( 0 );
        }
    }
}