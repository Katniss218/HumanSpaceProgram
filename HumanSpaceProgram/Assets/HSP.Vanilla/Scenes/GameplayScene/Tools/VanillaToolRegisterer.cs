﻿
namespace HSP.Vanilla.Scenes.GameplayScene.Tools
{
    public static class VanillaToolRegisterer
    {
        public const string REGISTER_TOOLS = HSPEvent.NAMESPACE_HSP + ".register_gameplayscene_tools";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, REGISTER_TOOLS )]
        private static void RegisterTools()
        {
            GameplaySceneToolManager.RegisterTool<DefaultTool>();
            GameplaySceneToolManager.RegisterTool<ConstructTool>();
        }
    }
}