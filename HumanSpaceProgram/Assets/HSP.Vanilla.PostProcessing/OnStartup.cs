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
        public const string ADD_POST_PROCESS_VOLUME = HSPEvent.NAMESPACE_HSP + ".pppvolume";
        public const string ADD_POST_PROCESS_LAYER = HSPEvent.NAMESPACE_HSP + ".ppplayer";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_POST_PROCESS_VOLUME )]
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

        private static void SetupPPL( PostProcessLayer layer, bool onlyAA = false )
        {
            layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
            layer.temporalAntialiasing.jitterSpread = 0.65f;
            layer.temporalAntialiasing.stationaryBlending = 0.99f;
            layer.temporalAntialiasing.motionBlending = 0.25f;
            layer.temporalAntialiasing.sharpness = 0.1f;
            layer.volumeLayer = onlyAA
                ? 0
                : Layer.POST_PROCESSING.ToMask();
            layer.volumeTrigger = layer.transform;
            layer.stopNaNPropagation = true;

            // This is required, for some stupid reason.
            var postProcessResources = AssetRegistry.Get<PostProcessResources>( "builtin::com.unity.postprocessing/PostProcessing/PostProcessResources" );
            layer.Init( postProcessResources );
            layer.InitBundles();
        }

        [HSPEventListener( HSPEvent_SCENEACTIVATE_GAMEPLAY.ID, ADD_POST_PROCESS_LAYER, After = new[] { GameplaySceneCameraManager.CREATE_GAMEPLAY_CAMERA } )]
        private static void CreatePostProcessingLayers()
        {
            //PostProcessLayer farPPL = GameplaySceneCameraManager.FarCamera.gameObject.AddComponent<PostProcessLayer>(); Appears to not be needed, and for some reason, it takes a big performance hit.
            //SetupPPL( farPPL );

            PostProcessLayer nearPPL = GameplaySceneCameraManager.NearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL, true );

            PostProcessLayer uiPPL = GameplaySceneCameraManager.UICamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }

        [HSPEventListener( HSPEvent_SCENEACTIVATE_MAIN_MENU.ID, ADD_POST_PROCESS_LAYER, After = new[] { MainMenuSceneCameraManager.CREATE_MAIN_MENU_CAMERA } )]
        private static void CreatePostProcessingLayers2()
        {
            PostProcessLayer nearPPL = MainMenuSceneCameraManager.NearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL, true );

            PostProcessLayer uiPPL = MainMenuSceneCameraManager.UICamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }

        [HSPEventListener( HSPEvent_SCENEACTIVATE_DESIGN.ID, ADD_POST_PROCESS_LAYER, After = new[] { DesignSceneCameraManager.CREATE_DESIGN_CAMERA } )]
        private static void CreatePostProcessingLayers3()
        {
            PostProcessLayer nearPPL = DesignSceneCameraManager.NearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL, true );

            PostProcessLayer uiPPL = DesignSceneCameraManager.UICamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }
    }
}