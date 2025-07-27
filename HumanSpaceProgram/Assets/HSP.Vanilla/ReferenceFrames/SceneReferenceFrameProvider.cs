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

        public void SubscribeIfNotSubscribed( IReferenceFrameSwitchResponder responder )
        {
            GameplaySceneReferenceFrameManager.Instance.Subscribe( responder );
        }
        public void UnsubscribeIfSubscribed( IReferenceFrameSwitchResponder responder )
        {
            GameplaySceneReferenceFrameManager.Instance.Unsubscribe( responder );
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

        public void SubscribeIfNotSubscribed( IReferenceFrameSwitchResponder responder )
        {
            MapSceneReferenceFrameManager.Instance.Subscribe( responder );
        }
        public void UnsubscribeIfSubscribed( IReferenceFrameSwitchResponder responder )
        {
            MapSceneReferenceFrameManager.Instance.Unsubscribe( responder );
        }

        [MapsInheritingFrom( typeof( MapSceneReferenceFrameProvider ) )]
        public static SerializationMapping MapSceneReferenceFrameProviderMapping()
        {
            return new MemberwiseSerializationMapping<MapSceneReferenceFrameProvider>();
        }
    }
}