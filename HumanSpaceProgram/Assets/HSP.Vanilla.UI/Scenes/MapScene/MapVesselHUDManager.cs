using HSP.Vanilla.Scenes.MapScene;
using HSP.Vessels;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class MapVesselHUDManager : SingletonMonoBehaviour<MapVesselHUDManager>
    {
        List<MapVesselHUD> _huds = new List<MapVesselHUD>();

        public const string CREATE_VESSEL_HUD = HSPEvent.NAMESPACE_HSP + ".create_vessel_hud";
        public const string DESTROY_VESSEL_HUD = HSPEvent.NAMESPACE_HSP + ".destroy_vessel_hud";
        public const string CREATE_OR_DESTROY_VESSEL_HUDS = HSPEvent.NAMESPACE_HSP + ".c_or_d_vessel_huds";

        [HSPEventListener( HSPEvent_AFTER_MAP_VESSEL_CREATED.ID, CREATE_VESSEL_HUD )]
        private static void AfterVesselCreated( MapVessel vessel )
        {
            if( !instanceExists )
                return;

            var hud = MapSceneM.Instance.GetBackgroundCanvas().AddMapVesselHUD( vessel );
            instance._huds.Add( hud );
        }

        [HSPEventListener( HSPEvent_AFTER_MAP_VESSEL_DESTROYED.ID, DESTROY_VESSEL_HUD )]
        private static void AfterVesselDestroyed( MapVessel vessel )
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

        /*[HSPEventListener( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID, CREATE_OR_DESTROY_VESSEL_HUDS )]
        private static void AfterActiveObjectChanged()
        {
            if( !instanceExists )
                return;

            if( ActiveVesselManager.ActiveObject == null )
            {
                var canvas = MapSceneM.Instance.GetBackgroundCanvas();

                foreach( var vessel in VesselManager.LoadedVessels )
                {
                    if( !MapSceneVesselManager.TryGet( vessel, out var mapVessel ) )
                        continue;
                    var hud = canvas.AddMapVesselHUD( mapVessel );
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
        }*/

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, CREATE_VESSEL_HUD, After = new[] { MapSceneVesselManager.CREATE_MAP_VESSELS } )]
        private static void OnMapSceneActivate()
        {
            if( !instanceExists )
                return;

            var canvas = MapSceneM.Instance.GetBackgroundCanvas();
            foreach( var vessel in VesselManager.LoadedVessels )
            {
                if( !MapSceneVesselManager.TryGet( vessel, out var mapVessel ) )
                    continue;
                var hud = canvas.AddMapVesselHUD( mapVessel );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_DEACTIVATE.ID, DESTROY_VESSEL_HUD )]
        private static void OnMapSceneDeactivate()
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