using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Core
{
    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `design` scene.
    /// </summary>
    public class DesignSceneManager : SingletonMonoBehaviour<DesignSceneManager>
    {
        public const string SCENE_NAME = "Design";

        public static DesignSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_DESIGN );
        }
    }
}