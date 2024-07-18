using HSP;
using HSP.Vanilla.Scenes.DesignScene.Cameras;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vanilla.Scenes.MainMenuScene.Cameras;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityPlus.AssetManagement;

public class AddPostProcessing : MonoBehaviour
{
    [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.gameplayscene_pppvolume", After = new[] { "vanilla.gameplayscene_camera" } )]
    private static void AddPPPVolume()
    {

    }

    [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.gameplayscene_postprocessing", After = new[] { "vanilla.gameplayscene_camera" } )]
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

    [HSPEventListener( HSPEvent.STARTUP_MAINMENU, "vanilla.mainmenuscene_postprocessing", After = new[] { "vanilla.mainmenuscene_camera" } )]
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

    [HSPEventListener( HSPEvent.STARTUP_DESIGN, "vanilla.designscene_postprocessing", After = new[] { "vanilla.designscene_camera" } )]
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