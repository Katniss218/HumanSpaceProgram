using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnSctiveObjectChange : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, HSPEvent.NAMESPACE_HSP + ".reframe_active" )]
        private static void OnActiveObjectChange()
        {
            SceneReferenceFrameManager.TryFixActiveObjectOutOfBounds();
        }
    }
}