using KSS.Core.SceneManagement;
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
    [RequireComponent( typeof( PreexistingReference ) )]
    public class BuildingManager : SingletonMonoBehaviour<BuildingManager>, IPersistent
    {
        private List<Building> _loadedBuildings = new List<Building>();

        public static Building[] GetLoadedBuildings()
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( BuildingManager )} is only available in the gameplay scene." );

            return instance._loadedBuildings.ToArray();
        }

        internal static void Register( Building vessel )
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( BuildingManager )} is only available in the gameplay scene." );

            instance._loadedBuildings.Add( vessel );
        }

        internal static void Unregister( Building vessel )
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( BuildingManager )} is only available in the gameplay scene." );

            instance._loadedBuildings.Remove( vessel );
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {

        }




        // move below to separate class "BuildingSerializer" or something.
        private static readonly JsonSeparateFileSerializedDataHandler _buildingDataHandler = new JsonSeparateFileSerializedDataHandler();
        private static readonly JsonExplicitHierarchyGameObjectsStrategy _buildingsStrat = new JsonExplicitHierarchyGameObjectsStrategy( _buildingDataHandler, GetAllRootGameObjects );

        private static GameObject[] GetAllRootGameObjects()
        {
            GameObject[] gos = new GameObject[instance._loadedBuildings.Count];
            for( int i = 0; i < instance._loadedBuildings.Count; i++ )
            {
                gos[i] = instance._loadedBuildings[i].gameObject;
            }
            return gos;
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_buildings" )]
        private static void OnBeforeSave( object ee )
        {
            var e = (TimelineManager.SaveEventData)ee;

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings" ) );
            _buildingDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "objects.json" );
            _buildingDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "data.json" );
            e.objectActions.Add( _buildingsStrat.SaveAsync_Object );
            e.dataActions.Add( _buildingsStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_buildings" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings" ) );
            _buildingDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "objects.json" );
            _buildingDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Buildings", "data.json" );
            e.objectActions.Add( _buildingsStrat.LoadAsync_Object );
            e.dataActions.Add( _buildingsStrat.LoadAsync_Data );
        }
    }
}