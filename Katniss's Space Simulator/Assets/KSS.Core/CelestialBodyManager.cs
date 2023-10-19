using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public class CelestialBodyManager : HSPManager, IPersistent
    {
        private static Dictionary<string, CelestialBody> _celestialBodies = new Dictionary<string, CelestialBody>();

        public static CelestialBody Get( string id )
        {
            if( _celestialBodies.TryGetValue( id, out CelestialBody body ) )
            {
                return body;
            }

            return null;
        }

        internal static void Register( CelestialBody celestialBody )
        {
            _celestialBodies[celestialBody.ID] = celestialBody;
        }

        internal static void Unregister( string id )
        {
            _celestialBodies.Remove( id );
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
            return _celestialBodies.Values.Select( cb => cb.gameObject ).ToArray();
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_celestial_bodies" )]
        private static void OnBeforeSave( object ee )
        {
            var e = (TimelineManager.SaveEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            e.objectActions.Add( _celestialBodiesStrat.Save_Object );
            e.dataActions.Add( _celestialBodiesStrat.Save_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_celestial_bodies" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            e.objectActions.Add( _celestialBodiesStrat.Load_Object );
            e.dataActions.Add( _celestialBodiesStrat.Load_Data );
        }
    }
}