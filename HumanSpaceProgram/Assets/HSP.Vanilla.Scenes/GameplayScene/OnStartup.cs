using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnStartup : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_timescale_input_controller" )]
        private static void AddTimescaleInputController()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<TimeScaleInputController>();
        }
        
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_vessel_manager" )]
        private static void AddVesselManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<VesselManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_celestial_body_manager" )]
        private static void AddCelestialBodyManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<CelestialBodyManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_active_object_manager" )]
        private static void AddActiveObjectManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<ActiveObjectManager>();
        }
        
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_gameplay_scene_tool_manager" )]
        private static void AddGameplaySceneToolManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplaySceneToolManager>();
        }
        
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_scene_reference_frame_manager" )]
        private static void AddSceneReferenceFrameManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<SceneReferenceFrameManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "add_atmosphere_renderer", After = new[] { "vanilla.gameplayscene_camera" } )]
        private static void AddAtmosphereRenderer()
        {
            AtmosphereRenderer atmosphereRenderer = GameplaySceneCameraManager.EffectCamera.gameObject.AddComponent<AtmosphereRenderer>();
            atmosphereRenderer.light = GameObject.Find( "CBLight" ).GetComponent<Light>();
            atmosphereRenderer.ColorRenderTextureGetter = () => GameplaySceneCameraManager.ColorRenderTexture;
            atmosphereRenderer.DepthRenderTextureGetter = () => GameplaySceneDepthBufferCombiner.CombinedDepthRenderTexture;
        }
    }
}