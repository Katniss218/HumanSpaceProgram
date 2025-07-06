using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels.Construction;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vanilla.UI.Vessels.Construction
{
    public class GameplayConstructionSiteHUDManager : SingletonMonoBehaviour<GameplayConstructionSiteHUDManager>
    {
        List<ConstructionSiteHUD> _huds = new List<ConstructionSiteHUD>();

        public const string CREATE_CONSTRUCTION_SITE_HUD = HSPEvent.NAMESPACE_HSP + ".create_csite_hud";
        public const string DESTROY_CONSTRUCTION_SITE_HUD = HSPEvent.NAMESPACE_HSP + ".destroy_csite_hud";

        [HSPEventListener( HSPEvent_AFTER_CONSTRUCTION_SITE_CREATED.ID, CREATE_CONSTRUCTION_SITE_HUD )]
        private static void OnConstructionSiteCreated( FConstructionSite constructionSite )
        {
            //if( ActiveObjectManager.ActiveObject == null )
            //{
            var hud = GameplaySceneM.Instance.GetBackgroundCanvas().AddConstructionSiteHUD( constructionSite );
            instance._huds.Add( hud );
            //}
        }

        [HSPEventListener( HSPEvent_AFTER_CONSTRUCTION_SITE_DESTROYED.ID, DESTROY_CONSTRUCTION_SITE_HUD )]
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

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, CREATE_CONSTRUCTION_SITE_HUD )]
        private static void OnGameplaySceneActivate()
        {
            if( !instanceExists )
                return;

            var canvas = GameplaySceneM.Instance.GetBackgroundCanvas();

            foreach( var vessel in FindObjectsOfType<FConstructionSite>() )
            {
                var hud = canvas.AddConstructionSiteHUD( vessel );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID, DESTROY_CONSTRUCTION_SITE_HUD )]
        private static void OnGameplaySceneDeactivate()
        {
            if( !instanceExists )
                return;

            foreach( var hud in instance._huds )
            {
                Destroy( hud.gameObject );
            }
            instance._huds.Clear();
        }
    }
}