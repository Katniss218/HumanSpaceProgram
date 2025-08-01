﻿using HSP.Vanilla.Scenes.MapScene;

namespace HSP.Vanilla.UI.Scenes.GameplayMapScene
{
    internal class MapSceneUIFactory
    {
        public const string CREATE_UI = HSPEvent.NAMESPACE_HSP + ".map_scene.ui.create";
        public const string DESTROY_UI = HSPEvent.NAMESPACE_HSP + ".map_scene.ui.destroy";

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, CREATE_UI )]
        private static void Create()
        {

        }

        [HSPEventListener( HSPEvent_MAP_SCENE_DEACTIVATE.ID, DESTROY_UI )]
        private static void Destroy()
        {

        }
    }
}
