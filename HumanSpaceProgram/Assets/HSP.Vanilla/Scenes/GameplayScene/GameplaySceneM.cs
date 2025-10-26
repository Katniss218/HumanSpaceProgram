using HSP.SceneManagement;
using HSP.Timelines.Serialization;
using UnityEngine;
using static HSP.Vanilla.Scenes.GameplayScene.GameplaySceneM;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    /// <summary>
    /// Invoked immediately after loading the gameplay scene.
    /// </summary>
    public static class HSPEvent_GAMEPLAY_SCENE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the gameplay scene.
    /// </summary>
    public static class HSPEvent_GAMEPLAY_SCENE_UNLOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.unload";
    }
    
    /// <summary>
    /// Invoked immediately after the gameplay scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_GAMEPLAY_SCENE_ACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.activate";
    }

    /// <summary>
    /// Invoked immediately before the gameplay scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_GAMEPLAY_SCENE_DEACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `gameplay` scene.
    /// </summary>
    public sealed class GameplaySceneM : HSPScene<GameplaySceneM, LoadData>
    {
        public sealed class LoadData // TODO - maybe change to 2 subclasses?
        {
            public TimelineMetadata newTimeline;
            public string loadTimelineId;
            public string loadSaveId;
        }

        public static LoadData NewTimelineLoadData( TimelineMetadata newTimeline )
        {
            return new LoadData
            {
                newTimeline = newTimeline
            };
        }

        public static LoadData LoadSaveLoadData( string timelineId, string saveId = null )
        {
            return new LoadData
            {
                loadTimelineId = timelineId,
                loadSaveId = saveId
            };
        }

        public static new string UNITY_SCENE_NAME => "Testing And Shit"; // TODO - swap out for "Gameplay" when the part with creating and loading rockets is done.

        /// <summary>
        /// Returns the scene instance, if loaded. <br/>
        /// Throws an exception if the scene is not loaded.
        /// </summary>
        public static GameplaySceneM Instance => instance;
        /// <summary>
        /// Returns the manager gameobject associated with this scene instance, if the scene is loaded. <br/>
        /// Throws an exception if the scene is not loaded.
        /// </summary>
        public static GameObject GameObject => instance.gameObject;

        protected override void OnLoad( LoadData loadData )
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, loadData );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_GAMEPLAY_SCENE_UNLOAD.ID );
        }

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID );
        }
    }
}