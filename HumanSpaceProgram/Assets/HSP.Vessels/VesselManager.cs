using HSP.Core.SceneManagement;
using HSP.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Core
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
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
        
        [HSPEventListener( HSPEvent.TIMELINE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_vessels" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );

            int i = 0;
            foreach( var vessel in GetAllRootGameObjects() )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( vessel, TimelineManager.RefStore );
                _vesselsDataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent.TIMELINE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_vessels" )]
        private static void OnLoad( TimelineManager.LoadEventData e )
        {
            string path = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" );
            Directory.CreateDirectory( path );

            foreach( var dir in Directory.GetDirectories( path ) )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = _vesselsDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }
    }
}