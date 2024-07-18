using HSP.Core;
using HSP.GameplayScene;
using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class OnStartup : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, HSPEvent.NAMESPACE_VANILLA + ".reframe_active" )]
        private static void OnActiveObjectChanged()
        {
            SceneReferenceFrameManager.TryFixActiveObjectOutOfBounds();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".add_timescale_icontroller" )]
        private static void CreateInstanceInScene()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<TimeScaleInputController>();
        }
    }
}