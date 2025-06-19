using UnityEngine;

namespace HSP
{
    /// <summary>
    /// The currently active primary camera in the scene.
    /// </summary>
    public abstract class SceneCamera : SingletonMonoBehaviour<SceneCamera>
    {
        new public Camera camera { get; set; }

        /// <summary>
        /// Use this if you need to do camera-related transformations in the current scene.
        /// </summary>
        public static Camera Camera => instance.camera;
    }
}