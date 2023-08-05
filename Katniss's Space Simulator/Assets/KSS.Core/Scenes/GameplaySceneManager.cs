using UnityEngine;

namespace KSS.Core
{
    public class GameplaySceneManager : MonoBehaviour
    {
        void Awake()
        {
            HSPOverridableEvent.EventManager.TryInvoke( HSPOverridableEvent.STARTUP_GAMEPLAY );
        }
    }
}