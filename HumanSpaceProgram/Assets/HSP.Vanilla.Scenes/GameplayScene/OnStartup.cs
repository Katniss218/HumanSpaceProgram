using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnStartup : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, HSPEvent.NAMESPACE_HSP + ".reframe_active" )]
        private static void OnActiveObjectChanged()
        {
            SceneReferenceFrameManager.TryFixActiveObjectOutOfBounds();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_timescale_icontroller" )]
        private static void CreateInstanceInScene()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<TimeScaleInputController>();
        }
        
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_vessel_manager" )]
        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_HSP + ".add_vessel_manager" )]
        private static void VesselManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<VesselManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_HSP + ".add_celestial_body_manager" )]
        private static void CelestialBodyManagerManager()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<CelestialBodyManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.gameplayscene_atmospheres", After = new[] { "vanilla.gameplayscene_camera" } )]
        private static void CreateAtmosphereRenderer()
        {
            AtmosphereRenderer atmosphereRenderer = GameplaySceneCameraManager.EffectCamera.gameObject.AddComponent<AtmosphereRenderer>();
            atmosphereRenderer.light = GameObject.Find( "CBLight" ).GetComponent<Light>();
            atmosphereRenderer.ColorRenderTextureGetter = () => GameplaySceneCameraManager.ColorRenderTexture;
            atmosphereRenderer.DepthRenderTextureGetter = () => GameplaySceneDepthBufferCombiner.CombinedDepthRenderTexture;
        }
    }
}