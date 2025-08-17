using HSP.SceneManagement;
using System;
using UnityEngine;
using UnityPlus.SceneManagement;

namespace HSP.Vanilla
{
    //[RequireComponent( typeof( Camera ) )]
    public sealed class SceneCamera : SingletonPerSceneMonoBehaviour<SceneCamera>
    {
#warning TODO - use the per-scene singleton instead.
        // also could be hooked into some interface that all XYZCameraManager (camera stack for the HSPScene) would implement so it's automagically assigned.

        // use the active camera for a specific scene.
        // use the method to get the hspscene from a gameobject if it needs support for any scene.
        new public Camera camera { get; set; }
        // attach to a camera to have it automatically referenced.
        // The camera should have clipping planes, etc appropriate for converting between the screen and world spaces.

        public static Camera GetCamera<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            return GetInstance( HSPSceneManager.UnityScene<TLoadedScene>() ).camera;
            throw new NotImplementedException();
        }
        public static Camera GetCamera( IHSPScene loadedScene )
        {
            return GetInstance( loadedScene.UnityScene ).camera;
        }

        /*public static void GetForegroundCamera()
        {

        }*/

        /// <summary>
        /// Use this if you need to do camera-related transformations in the current scene.
        /// </summary>
        //public static Camera Camera => instanceExists ? instance.camera : null;

       /* void OnEnable()
        {
            if( instance != this && instance.isActiveAndEnabled )
            {
                throw new SingletonInstanceException( $"Too many (active) instances of {nameof( MonoBehaviour )} {typeof( SceneCamera ).Name}." );
            }

            if( instance == null || !instance.isActiveAndEnabled )
            {
                instance = this;
                return;
            }
        }*/
    }
}