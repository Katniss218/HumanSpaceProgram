using HSP.Timelines;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.MainMenuScene;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;

namespace HSP._DevUtils
{
    /// <summary>
    /// Game manager for testing.
    /// </summary>
    public class DevUtilsMainMenuManager : SingletonMonoBehaviour<DevUtilsMainMenuManager>
    {
        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_LOAD.ID, "test" )]
        private static void OnLoad()
        {
            var oldD = AssetRegistry.Get<SerializedData>( "Debug::Data/old" );
            var newD = AssetRegistry.Get<SerializedData>( "Debug::Data/gameobjects" );

            var diffs = SerializedDataDiff.Diff( oldD, newD, new SerializedDataDiffConfig()
            {
                IgnoreKeys = new() { "$type", "$id", "$ref" },
            } );

            foreach( var diff in diffs )
            {
                Debug.Log( diff.ToString() );
            }
        }

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_LOAD.ID, "tesfesfsf" )]
        private static void AddDevUtilsMainMenuManager()
        {
            MainMenuSceneM.Instance.gameObject.AddComponent<DevUtilsMainMenuManager>();
        }

        void Update()
        {
            if( UnityEngine.Input.GetKeyDown( KeyCode.F5 ) )
            {
                DevDefaultScenarioCreator.CreateScenario();
                //CreateVessel( launchSite );
            }
        }
    }
}