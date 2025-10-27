using UnityEngine;

namespace HSP.SceneManagement
{
    public static class IHSPScene_Ex
    {
        public static IHSPScene GetHSPScene( this GameObject gameObject )
        {
            return HSPSceneManager.GetScene( gameObject );
        }
    }
}