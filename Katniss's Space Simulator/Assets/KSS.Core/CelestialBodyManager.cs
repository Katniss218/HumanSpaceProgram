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
    public class CelestialBodyManager : SingletonMonoBehaviour<CelestialBodyManager>, IPersistent
    {
        private Dictionary<string, CelestialBody> _celestialBodies = new Dictionary<string, CelestialBody>();

        public static CelestialBody Get( string id )
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            if( instance._celestialBodies.TryGetValue( id, out CelestialBody body ) )
            {
                return body;
            }

            return null;
        }

        public static CelestialBody[] GetAll()
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            return instance._celestialBodies.Values.ToArray();
        }

        internal static void Register( CelestialBody celestialBody )
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            instance._celestialBodies[celestialBody.ID] = celestialBody;
        }

        internal static void Unregister( string id )
        {
            if( !exists )
                throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            instance._celestialBodies.Remove( id );
        }


        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            // nothing yet.
        }

        private static readonly JsonPreexistingGameObjectsStrategy _celestialBodiesStrat = new JsonPreexistingGameObjectsStrategy( GetAllRootGameObjects );

        private static GameObject[] GetAllRootGameObjects()
        {
            return instance._celestialBodies.Values.Select( cb => cb.gameObject ).ToArray();
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_celestial_bodies" )]
        private static void OnBeforeSave( object ee )
        {
            var e = (TimelineManager.SaveEventData)ee;

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            e.objectActions.Add( _celestialBodiesStrat.SaveAsync_Object );
            e.dataActions.Add( _celestialBodiesStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_celestial_bodies" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            e.objectActions.Add( _celestialBodiesStrat.LoadAsync_Object );
            e.dataActions.Add( _celestialBodiesStrat.LoadAsync_Data );
        }
    }
}