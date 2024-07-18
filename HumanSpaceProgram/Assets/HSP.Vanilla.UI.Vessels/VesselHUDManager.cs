using HSP.ReferenceFrames;
using HSP.UI;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib;

namespace HSP.Vanilla.UI.Vessels
{
    public class VesselHUDManager : SingletonMonoBehaviour<VesselHUDManager>
    {
        List<VesselHUD> _huds = new List<VesselHUD>();

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.vessel_huds" )]
        private static void OnStartup()
        {
            GameplaySceneManager.GameObject.AddComponent<VesselHUDManager>();
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_VESSEL_REGISTERED, "vanilla.vessel_huds" )]
        private static void OnVesselRegistered( Vessel vessel )
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                var hud = CanvasManager.Get( CanvasName.BACKGROUND ).AddVesselHUD( vessel );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_VESSEL_UNREGISTERED, "vanilla.vessel_huds" )]
        private static void OnVesselUnregistered( Vessel vessel )
        {
            foreach( var hud in instance._huds.ToArray() )
            {
                if( hud == null ) // hud can be null if exiting a scene - it doesn't affect anything, but gives ugly warnings.
                    return;

                if( hud.Vessel == vessel )
                {
                    Destroy( hud.gameObject );
                    instance._huds.Remove( hud );
                }
            }
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, "vanilla.vessel_huds" )]
        private static void OnActiveObjectChanged()
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                foreach( var vessel in VesselManager.LoadedVessels )
                {
                    var hud = CanvasManager.Get( CanvasName.BACKGROUND ).AddVesselHUD( vessel );
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