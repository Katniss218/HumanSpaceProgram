using HSP.CelestialBodies;
using HSP.Vanilla.Scenes.MapScene;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class MapCelestialBodyHUDManager : SingletonMonoBehaviour<MapCelestialBodyHUDManager>
    {
        List<MapCelestialBodyHUD> _huds = new List<MapCelestialBodyHUD>();

        public const string CREATE_CELESTIAL_BODY_HUD = HSPEvent.NAMESPACE_HSP + ".create_celestial_body_hud";
        public const string DESTROY_CELESTIAL_BODY_HUD = HSPEvent.NAMESPACE_HSP + ".destroy_celestial_body_hud";
        public const string CREATE_OR_DESTROY_CELESTIAL_BODY_HUDS = HSPEvent.NAMESPACE_HSP + ".c_or_d_celestial_body_huds";

        [HSPEventListener( HSPEvent_AFTER_MAP_CELESTIAL_BODY_CREATED.ID, CREATE_CELESTIAL_BODY_HUD )]
        private static void AfterCelestialBodyCreated( MapCelestialBody celestialBody )
        {
            if( !instanceExists )
                return;

            var hud = MapSceneM.Instance.GetBackgroundCanvas().AddMapCelestialBodyHUD( celestialBody );
            instance._huds.Add( hud );
        }

        [HSPEventListener( HSPEvent_AFTER_MAP_CELESTIAL_BODY_DESTROYED.ID, DESTROY_CELESTIAL_BODY_HUD )]
        private static void AfterCelestialBodyDestroyed( MapCelestialBody celestialBody )
        {
            if( !instanceExists )
                return;

            foreach( var hud in instance._huds.ToArray() )
            {
                if( hud == null ) // hud can be null if exiting a scene - it doesn't affect anything, but gives ugly warnings.
                    return;

                if( hud.CelestialBody == celestialBody )
                {
                    Destroy( hud.gameObject );
                    instance._huds.Remove( hud );
                }
            }
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, CREATE_CELESTIAL_BODY_HUD, After = new[] { MapSceneCelestialBodyManager.CREATE_MAP_CELESTIAL_BODIES } )]
        private static void OnMapSceneActivate()
        {
            if( !instanceExists )
                return;

            var canvas = MapSceneM.Instance.GetBackgroundCanvas();
            foreach( var celestialBody in CelestialBodyManager.CelestialBodies )
            {
                if( !MapSceneCelestialBodyManager.TryGet( celestialBody, out var mapCelestialBody ) )
                    continue;
                var hud = canvas.AddMapCelestialBodyHUD( mapCelestialBody );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_DEACTIVATE.ID, DESTROY_CELESTIAL_BODY_HUD )]
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