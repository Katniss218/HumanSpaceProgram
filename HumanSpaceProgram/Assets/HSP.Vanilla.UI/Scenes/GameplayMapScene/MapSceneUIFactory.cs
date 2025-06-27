using HSP.Vanilla.Scenes.MapScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSP.Vanilla.UI.Scenes.GameplayMapScene
{
    internal class MapSceneUIFactory
    {




        public const string CREATE_UI = HSPEvent.NAMESPACE_HSP + ".mapscene_ui";

        public static void DestroyGameplayUI()
        {

        }

        public static void RestoreGameplayUI()
        {
#warning TODO - we don't know what UI elements exist, so we should call an event to set it up.
            // this event has to be a different event from the gameplay startup evt, because it will need to be invoked at other times.
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, CREATE_UI )]
        public static void CreateUI()
        {

        }
    }
}
