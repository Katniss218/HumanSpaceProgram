using HSP.CelestialBodies;
using HSP.SceneManagement;
using HSP.Vanilla.ReferenceFrames;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    /// <summary>
    /// Invoked to build the map celestial body from some source 'real' celestial body.
    /// </summary>
    public static class HSPEvent_MAP_SCENE_CELESTIAL_BODY_BUILDER
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mapscenecb.builder";

        public class Data
        {
            public CelestialBody source;
            public MapCelestialBody target;
        }
    }

    public class MapSceneCelestialBodyManager
    {
        static Dictionary<CelestialBody, MapCelestialBody> _mapCelestialBodies = new();

        public const string CREATE_MAP_CELESTIAL_BODY = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmapcb";
        public const string DESTROY_MAP_CELESTIAL_BODY = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmapcb";

        public static MapCelestialBody Get( string id )
        {
            return _mapCelestialBodies[CelestialBodyManager.Get( id )];
        }

        [HSPEventListener( HSPEvent_AFTER_CELESTIAL_BODY_CREATED.ID, CREATE_MAP_CELESTIAL_BODY )]
        public static void OnCelestialBodyCreated( CelestialBody body )
        {
            if( !HSPSceneManager.IsForeground<MapSceneM>() )
                return;

            CreateMapCelestialBody( body );
        }

        [HSPEventListener( HSPEvent_AFTER_CELESTIAL_BODY_DESTROYED.ID, DESTROY_MAP_CELESTIAL_BODY )]
        public static void OnCelestialBodyDestroyed( CelestialBody body )
        {
            if( !HSPSceneManager.IsForeground<MapSceneM>() )
                return;

            DestroyMapCelestialBody( body );
        }

        public const string CREATE_MAP_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmapcbs";
        public const string DESTROY_MAP_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmapcbs";

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, CREATE_MAP_CELESTIAL_BODIES, After = new[] { OnStartup.ADD_SCENE_REFERENCE_FRAME_MANAGER } )]
        public static void OnMapSceneLoad()
        {
            foreach( var body in CelestialBodyManager.CelestialBodies )
            {
                CreateMapCelestialBody( body );
            }
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_UNLOAD.ID, DESTROY_MAP_CELESTIAL_BODIES )]
        public static void OnMapSceneUnload()
        {
            foreach( var body in _mapCelestialBodies.Keys.ToArray() )
            {
                DestroyMapCelestialBody( body );
            }
        }

        /// <summary>
        /// Creates a map celestial body for the given source celestial body.
        /// </summary>
        private static void CreateMapCelestialBody( CelestialBody source )
        {
            GameObject go = new GameObject( "dummy" );
            var t = go.AddComponent<MapCelestialBody>();
            t.Source = source;
            var trans = go.AddComponent<FollowingDifferentReferenceFrameTransform>();
            trans.SceneReferenceFrameProvider = new MapSceneReferenceFrameProvider();
            trans.TargetTransform = source.ReferenceFrameTransform;
            t.ReferenceFrameTransform = trans;
            t.PhysicsTransform = source.PhysicsTransform;
            MapCelestialBody target = t;

            _mapCelestialBodies.Add( source, target );

            var data = new HSPEvent_MAP_SCENE_CELESTIAL_BODY_BUILDER.Data
            {
                source = source,
                target = target
            };
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAP_SCENE_CELESTIAL_BODY_BUILDER.ID, data );
        }

        /// <summary>
        /// Destroys the map celestial body associated with the given source celestial body.
        /// </summary>
        private static void DestroyMapCelestialBody( CelestialBody source )
        {
            if( _mapCelestialBodies.TryGetValue( source, out MapCelestialBody target ) )
            {
                // destroy the gameobject stub
                if( target.gameObject != null )
                    UnityEngine.Object.Destroy( target.gameObject );

                _mapCelestialBodies.Remove( source );
            }
        }
    }
}