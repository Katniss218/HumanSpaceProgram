using HSP.Core;
using HSP.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;

namespace HSP.UI.HUDs
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
            //if( ActiveObjectManager.ActiveObject == null )
            //{
                var hud = CanvasManager.Get( CanvasName.BACKGROUND ).AddConstructionSiteHUD( constructionSite );
                instance._huds.Add( hud );
            //}
        }

        [HSPEventListener( HSPEventVanilla.GAMEPLAY_AFTER_CONSTRUCTION_SITE_DESTROYED, "vanilla.csite_huds" )]
        private static void OnConstructionSiteDestroyed( FConstructionSite constructionSite )
        {
            if( !instanceExists ) // Can be null if exiting a scene - it doesn't affect anything, but gives ugly warnings.
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
        /*
        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, "vanilla.csite_huds" )]
        private static void OnActiveObjectChanged()
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                foreach( var vessel in VesselManager.LoadedVessels )
                {
                    foreach( var constructionSite in vessel.GetComponentsInChildren<FConstructionSite>() )
                    {
                        var hud = CanvasManager.Get( CanvasName.BACKGROUND ).AddConstructionSiteHUD( constructionSite );
                        instance._huds.Add( hud );
                    }
                }
            }
            else
            {
                foreach( var hud in instance._huds )
                {
                    Destroy( hud.gameObject );
                }
                instance._huds.Clear();
            }
        }*/
    }
}