using KSS.Core;
using KSS.GameplayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;

namespace KSS.UI.HUDs
{
    public class ConstructionSiteHUDManager : SingletonMonoBehaviour<ConstructionSiteHUDManager>
    {
        List<ConstructionSiteHUD> _huds = new List<ConstructionSiteHUD>();

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.csite_huds" )]
        private static void OnStartup()
        {
            GameplaySceneManager.GameObject.AddComponent<ConstructionSiteHUDManager>();
        }

        [HSPEventListener( HSPEventVanilla.GAMEPLAY_AFTER_CONSTRUCTION_SITE_CREATED, "vanilla.csite_huds" )]
        private static void OnConstructionSiteCreated( FConstructionSite constructionSite )
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                var hud = CanvasManager.Get( CanvasName.BACKGROUND ).AddConstructionSiteHUD( constructionSite );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEventVanilla.GAMEPLAY_AFTER_CONSTRUCTION_SITE_DESTROYED, "vanilla.csite_huds" )]
        private static void OnConstructionSiteDestroyed( FConstructionSite constructionSite )
        {
            if( !exists ) // Can be null if exiting a scene - it doesn't affect anything, but gives ugly warnings.
                return;

            foreach( var hud in instance._huds.ToArray() )
            {
                if( hud == null ) // if was destroyed by scene change.
                    continue;

                if( hud.ConstructionSite == constructionSite )
                {
                    Destroy( hud.gameObject );
                    instance._huds.Remove( hud );
                }
            }
        }
    }
}