using HSP.ReferenceFrames;
using HSP.UI;
using HSP.Vessels;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib;

namespace HSP.Vanilla.UI.Vessels
{
    public class VesselHUDManager : SingletonMonoBehaviour<VesselHUDManager>
    {
        List<VesselHUD> _huds = new List<VesselHUD>();

        public const string CREATE_VESSEL_HUD = HSPEvent.NAMESPACE_HSP + ".create_vessel_hud";
        public const string DESTROY_VESSEL_HUD = HSPEvent.NAMESPACE_HSP + ".destroy_vessel_hud";
        public const string CREATE_OR_DESTROY_VESSEL_HUDS = HSPEvent.NAMESPACE_HSP + ".c_or_d_vessel_huds";

        [HSPEventListener( HSPEvent_AFTER_VESSEL_CREATED.ID, CREATE_VESSEL_HUD )]
        private static void AfterVesselCreated( Vessel vessel )
        {
            if( !instanceExists )
                return;

            if( ActiveObjectManager.ActiveObject == null )
            {
                var hud = CanvasManager.Get( CanvasName.BACKGROUND ).AddVesselHUD( vessel );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_DESTROYED.ID, DESTROY_VESSEL_HUD )]
        private static void AfterVesselDestroyed( Vessel vessel )
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

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID, CREATE_OR_DESTROY_VESSEL_HUDS )]
        private static void AfterActiveObjectChanged()
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