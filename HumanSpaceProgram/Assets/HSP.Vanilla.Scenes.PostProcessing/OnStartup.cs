using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.DesignScene.Cameras;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vanilla.Scenes.MainMenuScene;
using HSP.Vanilla.Scenes.MainMenuScene.Cameras;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Scenes.PostProcessing
{
    public class OnStartup : MonoBehaviour
    {
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, "vanilla.pppvolume" )]
        private static void AddPPPVolume()
        {
            GameObject ppp = new GameObject( "PPP" );
            ppp.SetLayer( (int)Layer.POST_PROCESSING );

            PostProcessVolume ppv = ppp.AddComponent<PostProcessVolume>();
            ppv.isGlobal = true;
            ppv.weight = 1.0f;
            ppv.priority = 0.0f;
            ppv.profile = AssetRegistry.Get<PostProcessProfile>( "builtin::Resources/PPP" );
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, "vanilla.gameplayscene_postprocessing", After = new[] { "vanilla.gameplayscene_camera" } )]
        private static void CreatePostProcessingLayers()
        {
            void SetupPPL( PostProcessLayer layer )
            {
                layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                layer.temporalAntialiasing.jitterSpread = 0.65f;
                layer.temporalAntialiasing.stationaryBlending = 0.99f;
                layer.temporalAntialiasing.motionBlending = 0.25f;
                layer.temporalAntialiasing.sharpness = 0.1f;
                layer.volumeLayer = Layer.POST_PROCESSING.ToMask();
                layer.volumeTrigger = layer.transform;
                layer.stopNaNPropagation = true;

                // This is required, for some stupid reason.
                var postProcessResources = AssetRegistry.Get<PostProcessResources>( "builtin::com.unity.postprocessing/PostProcessing/PostProcessResources" );
                layer.Init( postProcessResources );
                layer.InitBundles();
            }

            PostProcessLayer farPPL = GameplaySceneCameraManager.FarCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( farPPL );

            PostProcessLayer nearPPL = GameplaySceneCameraManager.NearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL );

            PostProcessLayer uiPPL = GameplaySceneCameraManager.UICamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }

        [HSPEventListener( HSPEvent_STARTUP_MAIN_MENU.ID, "vanilla.mainmenuscene_postprocessing", After = new[] { "vanilla.mainmenuscene_camera" } )]
        private static void CreatePostProcessingLayers2()
        {
            void SetupPPL( PostProcessLayer layer )
            {
                layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                layer.temporalAntialiasing.jitterSpread = 0.65f;
                layer.temporalAntialiasing.stationaryBlending = 0.99f;
                layer.temporalAntialiasing.motionBlending = 0.25f;
                layer.temporalAntialiasing.sharpness = 0.1f;
                layer.volumeLayer = Layer.POST_PROCESSING.ToMask();
                layer.volumeTrigger = layer.transform;
                layer.stopNaNPropagation = true;

                // This is required, for some stupid reason.
                var postProcessResources = AssetRegistry.Get<PostProcessResources>( "builtin::com.unity.postprocessing/PostProcessing/PostProcessResources" );
                layer.Init( postProcessResources );
                layer.InitBundles();
            }

            PostProcessLayer nearPPL = MainMenuSceneCameraManager.NearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL );

            PostProcessLayer uiPPL = MainMenuSceneCameraManager.UICamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }

        [HSPEventListener( HSPEvent_STARTUP_DESIGN.ID, "vanilla.designscene_postprocessing", After = new[] { "vanilla.designscene_camera" } )]
        private static void CreatePostProcessingLayers3()
        {
            void SetupPPL( PostProcessLayer layer )
            {
                layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                layer.temporalAntialiasing.jitterSpread = 0.65f;
                layer.temporalAntialiasing.stationaryBlending = 0.99f;
                layer.temporalAntialiasing.motionBlending = 0.25f;
                layer.temporalAntialiasing.sharpness = 0.1f;
                layer.volumeLayer = Layer.POST_PROCESSING.ToMask();
                layer.volumeTrigger = layer.transform;
                layer.stopNaNPropagation = true;

                // This is required, for some stupid reason.
                var postProcessResources = AssetRegistry.Get<PostProcessResources>( "builtin::com.unity.postprocessing/PostProcessing/PostProcessResources" );
                layer.Init( postProcessResources );
                layer.InitBundles();
            }

            PostProcessLayer nearPPL = DesignSceneCameraManager.NearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL );

            PostProcessLayer uiPPL = DesignSceneCameraManager.UICamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }
    }
}