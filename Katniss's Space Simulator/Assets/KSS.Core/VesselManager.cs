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
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    public class VesselManager : SerializedManager, IPersistent
    {
        public static Vessel ActiveVessel { get; set; }

        static List<Vessel> _vessels;

        public static Vessel[] GetLoadedVessels()
        {
            return _vessels.ToArray();
        }

        internal static void Register( Vessel vessel )
        {
            if( _vessels == null )
                _vessels = new List<Vessel>();

            _vessels.Add( vessel );
        }

        internal static void Unregister( Vessel vessel )
        {
            if( _vessels != null )
                _vessels.Remove( vessel );
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {
                { "active_vessel", s.WriteObjectReference( ActiveVessel ) }
            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "active_vessel", out var activeVessel ) )
                ActiveVessel = (Vessel)l.ReadObjectReference( activeVessel );
        }




        // move below to separate class "BuildingSerializer" or something.
        private static readonly JsonExplicitHierarchyGameObjectsStrategy _vesselsStrat = new JsonExplicitHierarchyGameObjectsStrategy( GetAllRootGameObjects );

        private static GameObject[] GetAllRootGameObjects()
        {
            GameObject[] gos = new GameObject[_vessels.Count];
            for( int i = 0; i < _vessels.Count; i++ )
            {
                gos[i] = _vessels[i].gameObject;
            }
            return gos;
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_vessels" )]
        private static void OnBeforeSave( object ee )
        {
            var e = (TimelineManager.SaveEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );
            _vesselsStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "objects.json" );
            _vesselsStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "data.json" );
            e.objectActions.Add( _vesselsStrat.Save_Object );
            e.dataActions.Add( _vesselsStrat.Save_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_vessels" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );
            _vesselsStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "objects.json" );
            _vesselsStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", "data.json" );
            e.objectActions.Add( _vesselsStrat.Load_Object );
            e.dataActions.Add( _vesselsStrat.Load_Data );
        }
    }
}