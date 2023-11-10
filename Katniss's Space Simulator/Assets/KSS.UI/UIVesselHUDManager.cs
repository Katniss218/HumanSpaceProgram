using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;

namespace KSS.UI
{
    public class UIVesselHUDManager : SingletonMonoBehaviour<UIVesselHUDManager>
    {
        List<UIVesselHUD> _huds = new List<UIVesselHUD>();

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.vessel_huds" )]
        static void OnStartup( object e )
        {
            GameplaySceneManager.GameObject.AddComponent<UIVesselHUDManager>(); // add to the scene. This could be done in the editor.
                                                                                // Mods don't have editor though, so I've done it like this here too.

            VesselManager.OnAfterVesselCreated += ( vessel ) =>
            {
                var hud = UIVesselHUD.Create( CanvasManager.Get( CanvasName.BACKGROUND ), new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), vessel );
                instance._huds.Add( hud );
            };
            VesselManager.OnAfterVesselDestroyed += ( vessel ) =>
            {
                foreach( var hud in instance._huds )
                {
                    if( hud.Vessel == vessel )
                    {
#warning TODO - hud is a UI thing, add proper destroy.
                        Destroy( hud.gameObject ); // hud can be null if exiting a scene - it doesn't affect anything, but gives ugly warnings.
                    }
                }
            };
        }
    }
}