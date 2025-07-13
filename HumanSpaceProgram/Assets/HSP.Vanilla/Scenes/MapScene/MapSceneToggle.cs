using HSP.Input;
using HSP.SceneManagement;
using HSP.Vanilla.Scenes.GameplayScene;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.MapScene
{
    public class MapSceneToggle : SingletonMonoBehaviour<MapSceneToggle>
    {
        public const string ADD_MAP_TOGGLE = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.add_maptoggle";
        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_MAP_TOGGLE )]
        private static void AddMapToggle()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<MapSceneToggle>();
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( InputChannel.GAMEPLAY_TOGGLE_MAP_VIEW, InputChannelPriority.MEDIUM, Input_ToggleMap );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( InputChannel.GAMEPLAY_TOGGLE_MAP_VIEW, Input_ToggleMap );
        }

        private static bool Input_ToggleMap( float value )
        {
            // Map view sets gameplay scene as background (so it can still process), and loads the map scene on top.

            if( HSPSceneManager.IsForeground<GameplaySceneM>() )
            {
                HSPSceneManager.SetAsBackground<GameplaySceneM>();
                HSPSceneManager.LoadAsync<MapSceneM>();
            }
            else
            {
                HSPSceneManager.UnloadAsync<MapSceneM>();
                HSPSceneManager.SetAsForeground<GameplaySceneM>();
            }

            return false;
        }
    }
}