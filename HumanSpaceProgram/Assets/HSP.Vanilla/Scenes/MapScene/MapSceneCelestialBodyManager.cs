using HSP.CelestialBodies;
using HSP.SceneManagement;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vessels;
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

    public class MapSceneCelestialBodyManager : SingletonMonoBehaviour<MapSceneCelestialBodyManager>
    {
        Dictionary<CelestialBody, MapCelestialBody> _mapCelestialBodies = new();

        public const string ADD_MAP_SCENE_CELESTIAL_BODY_MANAGER = HSPEvent.NAMESPACE_HSP + ".vanilla.mapscenecelestialbodymanager.add";

        public const string CREATE_MAP_CELESTIAL_BODY = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmapcb";
        public const string DESTROY_MAP_CELESTIAL_BODY = HSPEvent.NAMESPACE_HSP + ".vanilla.destroymapcb";

        public const string CREATE_MAP_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmapcbs";
        public const string DESTROY_MAP_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".vanilla.destroymapcbs";

        public static bool TryGet( string id, out MapCelestialBody mapCelestialBody )
        {
            var celestialBody = CelestialBodyManager.Get( id );
            if( celestialBody == null )
            {
                mapCelestialBody = default;
                return false;
            }
            return instance._mapCelestialBodies.TryGetValue( celestialBody, out mapCelestialBody );
        }
        public static bool TryGet( CelestialBody celestialBody, out MapCelestialBody mapCelestialBody )
        {
            return instance._mapCelestialBodies.TryGetValue( celestialBody, out mapCelestialBody );
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_MAP_SCENE_CELESTIAL_BODY_MANAGER )]
        public static void AddMapSceneCelestialBodyManager()
        {
            MapSceneM.Instance.gameObject.AddComponent<MapSceneCelestialBodyManager>();
        }

        [HSPEventListener( HSPEvent_AFTER_CELESTIAL_BODY_CREATED.ID, CREATE_MAP_CELESTIAL_BODY )]
        public static void OnCelestialBodyCreated( CelestialBody body )
        {
            if( !HSPSceneManager.IsLoaded<MapSceneM>() )
                return;

            CreateMapCelestialBody( body );
        }

        [HSPEventListener( HSPEvent_AFTER_CELESTIAL_BODY_DESTROYED.ID, DESTROY_MAP_CELESTIAL_BODY )]
        public static void OnCelestialBodyDestroyed( CelestialBody body )
        {
            if( !HSPSceneManager.IsLoaded<MapSceneM>() )
                return;

            DestroyMapCelestialBody( body );
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, CREATE_MAP_CELESTIAL_BODIES )]
        public static void OnMapSceneActivate()
        {
            foreach( var body in CelestialBodyManager.CelestialBodies )
            {
                CreateMapCelestialBody( body );
            }
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_DEACTIVATE.ID, DESTROY_MAP_CELESTIAL_BODIES )]
        public static void OnMapSceneDeactivate()
        {
            foreach( var body in instance._mapCelestialBodies.Keys.ToArray() )
            {
                DestroyMapCelestialBody( body );
            }
        }

        /// <summary>
        /// Creates a map celestial body for the given source celestial body.
        /// </summary>
        private static void CreateMapCelestialBody( CelestialBody source )
        {
            if( !instanceExists )
                return; // scene was unloaded.

            GameObject go = new GameObject( $"map celestialbody - {source.ID}" );
            var t = go.AddComponent<MapCelestialBody>();
            t.Source = source;
            var trans = go.AddComponent<FollowingDifferentReferenceFrameTransform>();
            trans.SceneReferenceFrameProvider = new MapSceneReferenceFrameProvider();
            trans.TargetTransform = source.ReferenceFrameTransform;
            t.ReferenceFrameTransform = trans;
            t.PhysicsTransform = source.PhysicsTransform;
            MapCelestialBody target = t;

            instance._mapCelestialBodies.Add( source, target );

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
            if( !instanceExists )
                return; // scene was unloaded.

            if( instance._mapCelestialBodies.TryGetValue( source, out MapCelestialBody target ) )
            {
                // destroy the gameobject stub
                if( target != null )
                    UnityEngine.Object.Destroy( target.gameObject );

                instance._mapCelestialBodies.Remove( source );
            }
        }
    }
}