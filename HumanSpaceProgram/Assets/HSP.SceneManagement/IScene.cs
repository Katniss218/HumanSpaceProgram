using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.SceneManagement
{
    public interface IScene
    {

    }

    public abstract class SceneManager<T> : SingletonMonoBehaviour<T>, IScene where T : SceneManager<T>
    {
        /// <summary>
        /// Override this to tell the loader to load a custom scene (included in an asset bundle or in the project).
        /// </summary>
        public virtual string UNITY_SCENE_NAME => "empty_scene";

        /// <summary>
        /// Override this to perform any initialisation logic that should be run when the scene is loaded.
        /// </summary>
        protected abstract void OnLoad();
        /// <summary>
        /// Override this to perform any cleanup logic that should be run when the scene is unloaded.
        /// </summary>
        protected abstract void OnUnload();

        /// <summary>
        /// Override this to perform any initialisation logic that should be run when the scene becomes the 'main' scene.
        /// </summary>
        protected abstract void OnActivate();
        /// <summary>
        /// Override this to perform any cleanup logic that should be run when the scene is no longer the 'main' scene.
        /// </summary>
        protected abstract void OnDeactivate();

        internal T GetOrCreateSceneManagerInActiveScene()
        {
            if( instance == null )
            {
                // create
            }
            return instance;
        }
    }
}