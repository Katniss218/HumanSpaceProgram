using HSP.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string ADD_SCENE_REFERENCE_FRAME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_scene_reference_frame_manager";
        private static void ReferenceFrameSwitch_Responders( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            foreach( var obj in MapSceneM.Instance.UnityScene.GetRootGameObjects() ) // This is a map scene manager, so we only get from map scene.
                                                                                     // Everything registering to this event should be in the map scene.
            {
                if( obj.TryGetComponent<IReferenceFrameSwitchResponder>( out var referenceFrameSwitch ) )
                {
                    referenceFrameSwitch.OnSceneReferenceFrameSwitch( data );
                }
            }
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_SCENE_REFERENCE_FRAME_MANAGER )]
        private static void AddSceneReferenceFrameManager()
        {
            MapSceneReferenceFrameManager.Instance = MapSceneM.Instance.gameObject.AddComponent<MapSceneReferenceFrameManager>();
            GameplaySceneReferenceFrameManager.Instance.MaxRelativePosition = 1e8f;
            GameplaySceneReferenceFrameManager.Instance.MaxRelativeVelocity = float.MaxValue;
            MapSceneReferenceFrameManager.Instance.OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Responders;
        }
    }
}