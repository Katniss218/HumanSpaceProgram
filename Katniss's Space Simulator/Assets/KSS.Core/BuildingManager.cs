using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Strategies;

namespace KSS.Core
{
    public class BuildingManager : HSPManager, IPersistent
    {
        private static List<Building> _loadedBuildings = new List<Building>();

        public static Building[] GetLoadedBuildings()
        {
            return _loadedBuildings.ToArray();
        }

        internal static void Register( Building vessel )
        {
            _loadedBuildings.Add( vessel );
        }

        internal static void Unregister( Building vessel )
        {
            _loadedBuildings.Remove( vessel );
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetData( ILoader l, SerializedData data )
        {

        }




        // move below to separate class "BuildingSerializer" or something.
        private static readonly JsonExplicitHierarchyGameObjectsStrategy _buildingsStrat = new JsonExplicitHierarchyGameObjectsStrategy( GetAllRootGameObjects );

        private static GameObject[] GetAllRootGameObjects()
        {
            GameObject[] gos = new GameObject[_loadedBuildings.Count];
            for( int i = 0; i < _loadedBuildings.Count; i++ )
            {
                gos[i] = _loadedBuildings[i].gameObject;
            }
            return gos;
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_buildings" )]
        private static void OnBeforeSave( object ee )
        {
            var e = (TimelineManager.SaveEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings" ) );
            _buildingsStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "objects.json" );
            _buildingsStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "data.json" );
            e.objectActions.Add( _buildingsStrat.Save_Object );
            e.dataActions.Add( _buildingsStrat.Save_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_buildings" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings" ) );
            _buildingsStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "objects.json" );
            _buildingsStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "data.json" );
            e.objectActions.Add( _buildingsStrat.Load_Object );
            e.dataActions.Add( _buildingsStrat.Load_Data );
        }
    }
}