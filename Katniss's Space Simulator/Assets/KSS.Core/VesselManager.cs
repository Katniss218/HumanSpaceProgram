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
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Strategies;

namespace KSS.Core
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    [RequireComponent( typeof( PreexistingReference ) )]
    public class VesselManager : SingletonMonoBehaviour<VesselManager>, IPersistsData
    {
        private List<Vessel> _vessels = new List<Vessel>();

        /// <summary>
        /// Gets all vessels that are currently loaded into memory.
        /// </summary>
        public static IEnumerable<Vessel> LoadedVessels
        {
            get
            {
                if( !exists )
                    throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

                return instance._vessels;
            }
        }

        public static int LoadedVesselCount
        {
            get
            {
                if( !exists )
                    throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

                return instance._vessels.Count;
            }
        }

        internal static void Register( Vessel vessel )
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

            instance._vessels.Add( vessel );
            HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_VESSEL_REGISTERED, vessel );
        }

        internal static void Unregister( Vessel vessel )
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

            instance._vessels.Remove( vessel );
            HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_VESSEL_UNREGISTERED, vessel );
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

        private static readonly JsonSeparateFileSerializedDataHandler _vesselsDataHandler = new JsonSeparateFileSerializedDataHandler();
        private static readonly ExplicitHierarchyGameObjectsStrategy _vesselsStrat = new ExplicitHierarchyGameObjectsStrategy( _vesselsDataHandler, GetAllRootGameObjects );

        private static GameObject[] GetAllRootGameObjects()
        {
            GameObject[] gos = new GameObject[instance._vessels.Count];
            for( int i = 0; i < instance._vessels.Count; i++ )
            {
                gos[i] = instance._vessels[i].gameObject;
            }
            return gos;
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_vessels" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );
            _vesselsDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "objects.json" );
            _vesselsDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "data.json" );
            e.objectActions.Add( _vesselsStrat.SaveAsync_Object );
            e.dataActions.Add( _vesselsStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_vessels" )]
        private static void OnBeforeLoad( TimelineManager.LoadEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );
            _vesselsDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "objects.json" );
            _vesselsDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "data.json" );
            e.objectActions.Add( _vesselsStrat.LoadAsync_Object );
            e.dataActions.Add( _vesselsStrat.LoadAsync_Data );
        }
    }
}