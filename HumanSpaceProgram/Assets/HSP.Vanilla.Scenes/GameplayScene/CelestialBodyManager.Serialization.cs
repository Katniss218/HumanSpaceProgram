
namespace HSP.Vanilla.Scenes.GameplayScene
{
    public static class CelestialBodyManager_Serialization
    {

        /*[HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_celestial_bodies" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            JsonSerializedDataHandler _celestialBodiesDataHandler = new JsonSerializedDataHandler();

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );

            SerializationUnit _celestialBodiesStrat = SerializationUnit.FromObjects( GetAllRootGameObjects() );
            e.objectActions.Add( _celestialBodiesStrat.SaveAsync_Object );
            e.dataActions.Add( _celestialBodiesStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_celestial_bodies" )]
        private static void OnBeforeLoad( TimelineManager.LoadEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies" ) );
            _celestialBodiesDataHandler.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "object.json" );
            _celestialBodiesDataHandler.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "CelestialBodies", "data.json" );

            SerializationUnit _vesselsStrat = SerializationUnit.FromData( GetAllRootGameObjects );
            e.objectActions.Add( _celestialBodiesStrat.LoadAsync_Object );
            e.dataActions.Add( _celestialBodiesStrat.LoadAsync_Data );
        }*/
    }
}