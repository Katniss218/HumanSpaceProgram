using Assets.HSP.Trajectories;
using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vessels;
using System.Linq;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string UNPAUSE = HSPEvent.NAMESPACE_HSP + ".unpause";

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, UNPAUSE )]
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
        public const string ADD_TRAJECTORY_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_trajectory_manager";
        public const string ADD_ATMOSPHERE_RENDERER = HSPEvent.NAMESPACE_HSP + ".add_atmosphere_renderer";

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_TIMESCALE_INPUT_CONTROLLER )]
        private static void AddTimescaleInputController()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<TimeScaleInputController>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_VESSEL_MANAGER )]
        private static void AddVesselManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<VesselManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_CELESTIAL_BODY_MANAGER )]
        private static void AddCelestialBodyManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<CelestialBodyManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_ACTIVE_VESSEL_MANAGER )]
        private static void AddActiveVesselManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<ActiveVesselManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_SELECTED_CONTROL_FRAME_MANAGER )]
        private static void AddSelectedControlFrameManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<SelectedControlFrameManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_GAMEPLAY_SCENE_TOOL_MANAGER )]
        private static void AddGameplaySceneToolManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplaySceneToolManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_SCENE_REFERENCE_FRAME_MANAGER )]
        private static void AddSceneReferenceFrameManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<SceneReferenceFrameManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_ESCAPE_INPUT_CONTROLLER )]
        private static void AddEscapeInputController()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplaySceneEscapeInputController>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_TRAJECTORY_MANAGER )]
        private static void AddTrajectoryManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<TrajectoryManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_ATMOSPHERE_RENDERER,
            After = new[] { GameplaySceneCameraManager.CREATE_GAMEPLAY_CAMERA } )]
        private static void AddAtmosphereRenderer()
        {
            AtmosphereRenderer atmosphereRenderer = GameplaySceneCameraManager.EffectCamera.gameObject.AddComponent<AtmosphereRenderer>();
            atmosphereRenderer.light = GameObject.Find( "CBLight" ).GetComponent<Light>();
            atmosphereRenderer.ColorRenderTextureGetter = () => GameplaySceneCameraManager.ColorRenderTexture;
            atmosphereRenderer.DepthRenderTextureGetter = () => GameplaySceneDepthBufferCombiner.CombinedDepthRenderTexture;

        }

        public const string RESET_UT = HSPEvent.NAMESPACE_HSP + ".reset_ut";

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, RESET_UT )]
        private static void ResetUT()
        {
            TimeManager.SetUT( 0 );
        }

        [HSPEventListener( HSPEvent_ON_CELESTIAL_BODY_CREATED.ID, ADD_ATMOSPHERE_RENDERER )]
        private static void SetBody( CelestialBody body )
        {
#warning TODO - configurable bodies need their own atmospheres. sorted by distance (*hopefully* bodies won't intersect each other so it's enough to draw atmos farthest to nearest).
            if( body.ID == "main" )
                AtmosphereRenderer.Body = CelestialBodyManager.CelestialBodies.First();
        }
    }
}