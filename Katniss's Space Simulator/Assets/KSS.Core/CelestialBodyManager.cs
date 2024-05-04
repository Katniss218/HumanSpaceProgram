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
    [RequireComponent( typeof( PreexistingReference ) )]
    public class CelestialBodyManager : SingletonMonoBehaviour<CelestialBodyManager>, IPersistsData
    {
        private Dictionary<string, CelestialBody> _celestialBodies = new Dictionary<string, CelestialBody>();

        public static CelestialBody Get( string id )
        {
            if( !instanceExists )
                throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            if( instance._celestialBodies.TryGetValue( id, out CelestialBody body ) )
            {
                return body;
            }

            return null;
        }

        /// <summary>
        /// Gets all celestial bodies that are currently loaded into memory.
        /// </summary>
        public static IEnumerable<CelestialBody> CelestialBodies
        {
            get
            {
                if( !instanceExists )
                    throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

                return instance._celestialBodies.Values;
            }
        }

        public static int CelestialBodyCount
        {
            get
            {
                if( !instanceExists )
                    throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

                return instance._celestialBodies.Count;
            }
        }

        internal static void Register( CelestialBody celestialBody )
        {
            if( !instanceExists )
                throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            instance._celestialBodies[celestialBody.ID] = celestialBody;
        }

        internal static void Unregister( string id )
        {
            if( !instanceExists )
                throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            instance._celestialBodies.Remove( id );
        }


        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            //ret.AddAll( new SerializedObject()

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            //
        }

        private static readonly JsonSeparateFileSerializedDataHandler _celestialBodiesDataHandler = new JsonSeparateFileSerializedDataHandler();
        private static readonly PreexistingGameObjectsStrategy _celestialBodiesStrat = new PreexistingGameObjectsStrategy( _celestialBodiesDataHandler, GetAllRootGameObjects );

        private static GameObject[] GetAllRootGameObjects()
        {
            return instance._celestialBodies.Values.Select( cb => cb.gameObject ).ToArray();
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_celestial_bodies" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            e.objectActions.Add( _celestialBodiesStrat.SaveAsync_Object );
            e.dataActions.Add( _celestialBodiesStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_celestial_bodies" )]
        private static void OnBeforeLoad( TimelineManager.LoadEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            e.objectActions.Add( _celestialBodiesStrat.LoadAsync_Object );
            e.dataActions.Add( _celestialBodiesStrat.LoadAsync_Data );
        }
    }
}