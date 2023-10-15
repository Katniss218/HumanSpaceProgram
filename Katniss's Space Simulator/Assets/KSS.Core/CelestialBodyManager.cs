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
    public class CelestialBodyManager : SerializedManager, IPersistent
    {
        public static Dictionary<string, CelestialBody> CelestialBodies { get; private set; }

        public static CelestialBody Get( string id )
        {
            if( CelestialBodies != null
             && CelestialBodies.TryGetValue( id, out CelestialBody body ) )
            {
                return body;
            }

            return null;
        }

        internal static void Register( CelestialBody celestialBody )
        {
            if( CelestialBodies == null )
                CelestialBodies = new Dictionary<string, CelestialBody>();

            CelestialBodies[celestialBody.ID] = celestialBody;
        }

        internal static void Unregister( string id )
        {
            if( CelestialBodies != null )
                CelestialBodies.Remove( id );
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
            return CelestialBodies.Values.Select( cb => cb.gameObject ).ToArray();
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_managers" )]
        private static void OnBeforeSave( object ee )
        {
            var e = (TimelineManager.SaveEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            // INFO - preexisting objects strat doesn't have Save_Objects method.
            e.dataActions.Add( _celestialBodiesStrat.Save_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_managers" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );
            e.objectActions.Add( _celestialBodiesStrat.Load_Object );
            e.dataActions.Add( _celestialBodiesStrat.Load_Data );
        }
    }
}