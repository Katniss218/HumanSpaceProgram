using HSP.Core;
using HSP.UI;
using HSP.UI.HUDs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vanilla.UI.Scenes
{
    public static class OnStartup
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.csite_huds" )]
        private static void ConstructionSiteHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<ConstructionSiteHUDManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.spawn_navball" )]
        private static void OnGameplayEnter()
        {
            var manager = GameplaySceneManager.GameObject.AddComponent<NavballRenderTextureManager>();

            NavballRenderTextureManager.ResetAttitudeIndicatorRT();
            NavballRenderTextureManager.CreateNavball();
            NavballRenderTextureManager.CreateNavballCamera();
        }
    }
}