//using UnityEngine;
//using UnityPlus.SceneManagement;

//namespace HSP
//{
//    /// <summary>
//    /// The current active primary camera in whatever scene is currently active.
//    /// </summary>
//    public sealed class SceneCamera : SingletonPerSceneMonoBehaviour<SceneCamera>
//    {
//#warning TODO - use the per-scene singleton instead
//        // use the active camera for a specific scene.
//        // use the method to get the hspscene from a gameobject if it needs support for any scene.
//        new public Camera camera { get; set; }

//        /// <summary>
//        /// Use this if you need to do camera-related transformations in the current scene.
//        /// </summary>
//        public static Camera Camera => instanceExists ? instance.camera : null;

//        void OnEnable()
//        {
//            if( instance != this && instance.isActiveAndEnabled )
//            {
//                throw new SingletonInstanceException( $"Too many (active) instances of {nameof( MonoBehaviour )} {typeof( SceneCamera ).Name}." );
//            }

//            if( instance == null || !instance.isActiveAndEnabled )
//            {
//                instance = this;
//                return;
//            }
//        }
//    }
//}