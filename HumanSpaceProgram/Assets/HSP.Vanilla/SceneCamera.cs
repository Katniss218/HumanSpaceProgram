using HSP.SceneManagement;
using UnityEngine;
using UnityPlus.SceneManagement;

namespace HSP.Vanilla
{
    public sealed class SceneCamera : SingletonPerSceneMonoBehaviour<SceneCamera>
    {
#warning TODO - kinda ugly needing to be assigned from the whatever creates it.
        // Ultimately, this should return the camera that represents the position/orientation of the viewport (is currently drawing to the screen).
        // Should also be separate and not on the manager, because I want it to return the camera that is currently drawing. Not the camera for some specific scene.

        new public Camera camera { get; set; }

        /// <summary>
        /// Gets the camera for the specified HSP scene. <br/>
        /// The scene must currently be loaded.
        /// </summary>
        /// <typeparam name="TLoadedScene">The type specifying the scene to use</typeparam>
        public static Camera GetCamera<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            return GetInstance( HSPSceneManager.UnityScene<TLoadedScene>() ).camera;
        }

        /// <summary>
        /// Gets the camera for the specified HSP scene. <br/>
        /// The scene must currently be loaded.
        /// </summary>
        public static Camera GetCamera( IHSPScene loadedScene )
        {
            return GetInstance( loadedScene.UnityScene ).camera;
        }
    }
}