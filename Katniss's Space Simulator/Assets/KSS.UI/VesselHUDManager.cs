using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;

namespace KSS.UI
{
    public class VesselHUDManager : SingletonMonoBehaviour<VesselHUDManager>
    {
        List<VesselHUD> _huds = new List<VesselHUD>();

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.vessel_huds" )]
        static void OnStartup( object e )
        {
            GameplaySceneManager.GameObject.AddComponent<VesselHUDManager>(); // add to the scene. This could be done in the editor.
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_VESSEL_REGISTERED, "vanilla.vessel_huds" )]
        static void OnVesselRegistered( Vessel vessel )
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                var hud = VesselHUD.Create( CanvasManager.Get( CanvasName.BACKGROUND ), new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), vessel );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_VESSEL_UNREGISTERED, "vanilla.vessel_huds" )]
        static void OnVesselUnregistered( Vessel vessel )
        {
            foreach( var hud in instance._huds.ToArray() )
            {
                if( hud.Vessel == vessel )
                {
                    Destroy( hud.gameObject ); // hud can be null if exiting a scene - it doesn't affect anything, but gives ugly warnings.
                    instance._huds.Remove( hud );
                }
            }
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, "vanilla.vessel_huds" )]
        static void OnActiveObjectChanged()
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                foreach( var vessel in VesselManager.GetLoadedVessels() )
                {
                    var hud = VesselHUD.Create( CanvasManager.Get( CanvasName.BACKGROUND ), new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), vessel );
                    instance._huds.Add( hud );
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
        }
    }
}