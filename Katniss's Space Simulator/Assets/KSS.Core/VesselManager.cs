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

namespace KSS.Core
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    [RequireComponent( typeof( PreexistingReference ) )]
    public class VesselManager : SingletonMonoBehaviour<VesselManager>
    {
        private List<Vessel> _vessels = new List<Vessel>();

        /// <summary>
        /// Gets all vessels that are currently loaded into memory.
        /// </summary>
        public static IEnumerable<Vessel> LoadedVessels
        {
            get
            {
                if( !instanceExists )
                    throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

                return instance._vessels;
            }
        }

        public static int LoadedVesselCount
        {
            get
            {
                if( !instanceExists )
                    throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

                return instance._vessels.Count;
            }
        }

        internal static void Register( Vessel vessel )
        {
            if( !instanceExists )
                throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

            instance._vessels.Add( vessel );
            HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_VESSEL_REGISTERED, vessel );
        }

        internal static void Unregister( Vessel vessel )
        {
            if( !instanceExists )
                throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

            instance._vessels.Remove( vessel );
            HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_VESSEL_UNREGISTERED, vessel );
        }

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
            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler();

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );
            _vesselsDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "objects.json" );
            _vesselsDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "data.json" );

            SerializationUnit _vesselsStrat = SerializationUnit.FromObjects( _vesselsDataHandler, GetAllRootGameObjects() );
            e.objectActions.Add( _vesselsStrat.SaveAsync_Object );
            e.dataActions.Add( _vesselsStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_vessels" )]
        private static void OnBeforeLoad( TimelineManager.LoadEventData e )
        {
            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler();

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );
            _vesselsDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "objects.json" );
            _vesselsDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "data.json" );

            SerializationUnit _vesselsStrat = SerializationUnit.FromData( _vesselsDataHandler, GetAllRootGameObjects );
            e.objectActions.Add( _vesselsStrat.LoadAsync_Object );
            e.dataActions.Add( _vesselsStrat.LoadAsync_Data );
        }
    }
}