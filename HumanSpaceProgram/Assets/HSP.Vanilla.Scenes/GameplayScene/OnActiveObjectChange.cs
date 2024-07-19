using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnActiveObjectChangeListener : MonoBehaviour
    {
        [HSPEventListener( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID, HSPEvent.NAMESPACE_HSP + ".try_set_scene_reference_frame" )]
        private static void OnActiveObjectChange()
        {
            SceneReferenceFrameManager.TryFixActiveObjectOutOfBounds();
        }
    }
}