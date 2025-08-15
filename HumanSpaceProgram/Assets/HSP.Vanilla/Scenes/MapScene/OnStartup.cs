using HSP.Vanilla.Scenes.GameplayScene;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string ADD_SCENE_REFERENCE_FRAME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_scene_reference_frame_manager";

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_SCENE_REFERENCE_FRAME_MANAGER )]
        private static void AddSceneReferenceFrameManager()
        {
            MapSceneReferenceFrameManager.Instance = MapSceneM.Instance.gameObject.AddComponent<MapSceneReferenceFrameManager>();
            MapSceneReferenceFrameManager.Instance.MaxRelativePosition = 1e8f;
            MapSceneReferenceFrameManager.Instance.MaxRelativeVelocity = float.MaxValue;
        }
    }
}