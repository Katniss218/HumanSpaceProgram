using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Scenes.DesignScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string UNPAUSE = HSPEvent.NAMESPACE_HSP + ".unpause";

        [HSPEventListener( HSPEvent_SCENELOAD_DESIGN.ID, UNPAUSE )]
        private static void Unpause()
        {
            TimeManager.Unpause();
        }

        public const string ADD_VESSEL_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_vessel_manager";
        public const string ADD_DESIGN_SCENE_TOOL_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_design_scene_tool_manager";
        public const string ADD_DESIGN_VESSEL_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_design_vessel_manager";
        public const string ADD_SCENE_REFERENCE_FRAME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_scene_reference_frame_manager";
        public const string ADD_ACTIVE_OBJECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_active_object_manager";
        public const string ADD_ESCAPE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller";

        [HSPEventListener( HSPEvent_SCENELOAD_DESIGN.ID, ADD_VESSEL_MANAGER )]
        private static void VesselManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<VesselManager>();
        }

        [HSPEventListener( HSPEvent_SCENELOAD_DESIGN.ID, ADD_DESIGN_SCENE_TOOL_MANAGER )]
        private static void AddDesignSceneToolManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<DesignSceneToolManager>();
        }

        [HSPEventListener( HSPEvent_SCENELOAD_DESIGN.ID, ADD_DESIGN_VESSEL_MANAGER )]
        private static void AddDesignVesselManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<DesignVesselManager>();
        }

        [HSPEventListener( HSPEvent_SCENELOAD_DESIGN.ID, ADD_SCENE_REFERENCE_FRAME_MANAGER )]
        private static void AddSceneReferenceFrameManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<SceneReferenceFrameManager>();
        }

        [HSPEventListener( HSPEvent_SCENELOAD_DESIGN.ID, ADD_ACTIVE_OBJECT_MANAGER )]
        private static void AddActiveObjectManager()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<ActiveVesselManager>();
        }

        [HSPEventListener( HSPEvent_SCENELOAD_DESIGN.ID, ADD_ESCAPE_INPUT_CONTROLLER )]
        private static void CreateInstanceInScene()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<DesignSceneEscapeInputController>();
        }
    }
}