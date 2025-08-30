using HSP.SceneManagement;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vessels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    public static class HSPEvent_MAP_SCENE_VESSEL_BUILDER
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_scene.vessel.builder";

        public class Data
        {
            public Vessel source;
            public MapVessel target;
        }
    }

    public class MapSceneVesselManager : SingletonMonoBehaviour<MapSceneVesselManager>
    {
        // implement an IVesselFilter that will check whether or not to display a given vessel in the map, or for other purposes.


        Dictionary<Vessel, MapVessel> _mapVessels = new();


        public const string ADD_MAP_SCENE_VESSEL_MANAGER = HSPEvent.NAMESPACE_HSP + ".vanilla.mapscenevesselmanager.add";

        public const string CREATE_MAP_VESSEL = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmap_vessel";
        public const string DESTROY_MAP_VESSEL = HSPEvent.NAMESPACE_HSP + ".vanilla.destroymap_vessel";

        public const string CREATE_MAP_VESSELS = HSPEvent.NAMESPACE_HSP + ".vanilla.spawnmap_vessels";
        public const string DESTROY_MAP_VESSELS = HSPEvent.NAMESPACE_HSP + ".vanilla.destroymap_vessels";

        public static bool TryGet( Vessel vessel, out MapVessel mapVessel )
        {
            return instance._mapVessels.TryGetValue( vessel, out mapVessel );
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_MAP_SCENE_VESSEL_MANAGER )]
        public static void AddMapSceneVesselManager()
        {
            MapSceneM.Instance.gameObject.AddComponent<MapSceneVesselManager>();
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_CREATED.ID, CREATE_MAP_VESSEL )]
        public static void OnVesselCreated( Vessel vessel )
        {
            if( !HSPSceneManager.IsLoaded<MapSceneM>() )
                return;

            CreateMapVessel( vessel );
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_DESTROYED.ID, DESTROY_MAP_VESSEL )]
        public static void OnVesselDestroyed( Vessel vessel )
        {
            if( !HSPSceneManager.IsLoaded<MapSceneM>() )
                return;

            DestroyMapVessel( vessel );
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, CREATE_MAP_VESSELS )]
        public static void OnMapSceneActivate()
        {
            foreach( var body in VesselManager.LoadedVessels )
            {
                CreateMapVessel( body );
            }
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_DEACTIVATE.ID, DESTROY_MAP_VESSELS )]
        public static void OnMapSceneDeactivate()
        {
            foreach( var body in instance._mapVessels.Keys.ToArray() )
            {
                DestroyMapVessel( body );
            }
        }

        /// <summary>
        /// Creates a map celestial body for the given source celestial body.
        /// </summary>
        private static void CreateMapVessel( Vessel source )
        {
            if( !instanceExists )
                return; // scene was unloaded.

            GameObject go = new GameObject( $"map vessel - {source.name}" );
            var t = go.AddComponent<MapVessel>();
           // t.Source = source;
            var trans = go.AddComponent<FollowingDifferentReferenceFrameTransform>();
            trans.SceneReferenceFrameProvider = new MapSceneReferenceFrameProvider();
            trans.TargetTransform = source.ReferenceFrameTransform;
            MapVessel target = t;

            instance._mapVessels.Add( source, target );

            var data = new HSPEvent_MAP_SCENE_VESSEL_BUILDER.Data
            {
                source = source,
                target = target
            };
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAP_SCENE_VESSEL_BUILDER.ID, data );
        }

        /// <summary>
        /// Destroys the map celestial body associated with the given source celestial body.
        /// </summary>
        private static void DestroyMapVessel( Vessel source )
        {
            if( !instanceExists )
                return; // scene was unloaded.

            if( instance._mapVessels.TryGetValue( source, out MapVessel target ) )
            {
                // destroy the gameobject stub
                if( target != null )
                    UnityEngine.Object.Destroy( target.gameObject );

                instance._mapVessels.Remove( source );
            }
        }
    }
}