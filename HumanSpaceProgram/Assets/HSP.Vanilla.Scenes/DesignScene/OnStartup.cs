using HSP.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Scenes.DesignScene
{
    public class OnStartup : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_HSP + ".add_vessel_manager" )]
        private static void VesselManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<VesselManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_HSP + ".add_design_scene_tool_manager" )]
        private static void AddDesignSceneToolManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<DesignSceneToolManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_HSP + ".add_design_vessel_manager" )]
        private static void AddDesignVesselManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<DesignVesselManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_HSP + ".add_scene_reference_frame_manager" )]
        private static void AddSceneReferenceFrameManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<SceneReferenceFrameManager>();
        }
    }
}