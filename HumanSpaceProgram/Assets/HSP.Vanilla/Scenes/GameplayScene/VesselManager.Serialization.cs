using HSP.SceneManagement;
using HSP.Timelines;
using HSP.Vessels;
using HSP.Vessels.Serialization;
using System;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public static class VesselManager_Serialization
    {
        public const string SERIALIZE_VESSELS = HSPEvent.NAMESPACE_HSP + ".serialize_vessels";
        public const string DESERIALIZE_VESSELS = HSPEvent.NAMESPACE_HSP + ".deserialize_vessels";

        [HSPEventListener( HSPEvent_ON_TIMELINE_SAVE.ID, SERIALIZE_VESSELS )]
        [HSPEventListener( HSPEvent_ON_SCENARIO_SAVE.ID, SERIALIZE_VESSELS )]
        private static void SerializeVessels( IMessageEventData e )
        {
            string vesselsPath;
            if( e is TimelineSaveEventData e2 )
                vesselsPath = Path.Combine( e2.save.GetRootDirectory(), "Vessels" );
            else if( e is ScenarioSaveEventData e3 )
                vesselsPath = Path.Combine( e3.scenario.GetRootDirectory(), "Vessels" );
            else
                throw new ArgumentException();

            Directory.CreateDirectory( vesselsPath );

            int i = 0;
            foreach( var vessel in VesselManager.LoadedVessels )
            {
                string vesselDir = Path.Combine( vesselsPath, $"{i}" );
                try
                {
                    VesselSerializationUtils.Save( vessel, vesselDir, TimelineManager.RefStore );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to serialize vessel '{vessel.name}': {ex.Message}" );
                    Debug.LogException( ex );
                    e.AddMessage( LogType.Error, $"Failed to serialize vessel '{vessel.name}': {ex.Message}" );
                }
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_VESSELS, After = new[] { CelestialBodyManager_Serialization.DESERIALIZE_CELESTIAL_BODIES } )]
        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_VESSELS, After = new[] { CelestialBodyManager_Serialization.DESERIALIZE_CELESTIAL_BODIES } )]
        private static void DeserializeVessels( IMessageEventData e )
        {
            string vesselsPath;
            if( e is TimelineLoadEventData e2 )
                vesselsPath = Path.Combine( e2.save.GetRootDirectory(), "Vessels" );
            else if( e is TimelineNewEventData e3 )
                vesselsPath = Path.Combine( e3.scenario.GetRootDirectory(), "Vessels" );
            else
                throw new ArgumentException();

            if( !Directory.Exists( vesselsPath ) )
                return;

            foreach( var dir in Directory.GetDirectories( vesselsPath ) )
            {
                try
                {
                    Vessel vessel = VesselSerializationUtils.Load<Vessel>( GameplaySceneM.Instance, dir, TimelineManager.RefStore );
                    //if( vessel != null )
                    //{
                    //    HSPSceneManager.MoveGameObjectToScene<GameplaySceneM>( vessel.gameObject );
                    //}
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to deserialize vessel from '{dir}': {ex.Message}" );
                    Debug.LogException( ex );
                    e.AddMessage( LogType.Error, $"Failed to deserialize vessel from '{dir}': {ex.Message}" );
                }
            }
        }
    }
}