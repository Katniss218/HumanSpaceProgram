using HSP.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.MapScene;
using UnityPlus.Serialization;

namespace HSP.Vanilla.ReferenceFrames
{
    public sealed class GameplaySceneReferenceFrameProvider : ISceneReferenceFrameProvider
    {
        public IReferenceFrame GetSceneReferenceFrame()
        {
            return GameplaySceneReferenceFrameManager.ReferenceFrame;
        }

        [MapsInheritingFrom( typeof( GameplaySceneReferenceFrameProvider ) )]
        public static SerializationMapping GameplaySceneReferenceFrameProviderMapping()
        {
            return new MemberwiseSerializationMapping<GameplaySceneReferenceFrameProvider>();
        }
    }

    public sealed class MapSceneReferenceFrameProvider : ISceneReferenceFrameProvider
    {
        public IReferenceFrame GetSceneReferenceFrame()
        {
            return MapSceneReferenceFrameManager.ReferenceFrame;
        }

        [MapsInheritingFrom( typeof( MapSceneReferenceFrameProvider ) )]
        public static SerializationMapping MapSceneReferenceFrameProviderMapping()
        {
            return new MemberwiseSerializationMapping<MapSceneReferenceFrameProvider>();
        }
    }
}