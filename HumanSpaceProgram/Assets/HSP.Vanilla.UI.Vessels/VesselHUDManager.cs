using HSP.ReferenceFrames;
using HSP.SceneManagement;
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

        [HSPEventListener( HSPEvent_AFTER_VESSEL_REGISTERED.ID, HSPEvent.NAMESPACE_HSP + ".vessel_hud_manager" )]
        private static void OnVesselRegistered( Vessel vessel )
        {
            if( !instanceExists )
                return;

            if( ActiveObjectManager.ActiveObject == null )
            {
                var hud = CanvasManager.Get( CanvasName.BACKGROUND ).AddVesselHUD( vessel );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_UNREGISTERED.ID, HSPEvent.NAMESPACE_HSP + ".vessel_hud_manager" )]
        private static void OnVesselUnregistered( Vessel vessel )
        {
            if( !instanceExists )
                return;

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

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID, HSPEvent.NAMESPACE_HSP + ".vessel_hud_manager" )]
        private static void OnActiveObjectChanged()
        {
            if( !instanceExists )
                return;

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