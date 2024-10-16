using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnActiveObjectChangeListener : MonoBehaviour
    {
        public const string TRY_SET_SCENE_REFERENCE_FRAME = HSPEvent.NAMESPACE_HSP + ".try_set_scene_reference_frame";

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID, TRY_SET_SCENE_REFERENCE_FRAME )]
        private static void OnActiveObjectChange()
        {
            SceneReferenceFrameManager.TryFixActiveObjectOutOfBounds();
        }
    }
}