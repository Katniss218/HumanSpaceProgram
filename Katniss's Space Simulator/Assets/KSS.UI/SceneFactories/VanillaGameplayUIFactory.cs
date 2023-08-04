using KSS.Core;
using KSS.Core.Mods;
using KSS.Core.Scenes;
using KSS.UI;
using System;
using System.Collections.Generic;
using UILib;
using UILib.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.KSS.UI.SceneFactories
{
    public static class VanillaGameplayUIFactory
    {
        [OverridableEventListener( HSPOverridableEvent.STARTUP_GAMEPLAY, HSPOverridableEvent.NAMESPACE_VANILLA + ".gameplay_ui" )]
        public static void Create( object obj )
        {
            (_, UIStyle style) = ((UIStyle, UIStyle))obj;

            RectTransform canvasTransform = (RectTransform)CanvasManager.GetCanvas( CanvasManager.STATIC ).transform;

#warning todo complete the mainmenu factory.
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreateMainMenuButton( RectTransform parent, UIStyle style )
        {

        }

        private static void CreateSaveButton( RectTransform parent, UIStyle style )
        {

        }

        private static void CreateLoadButton( RectTransform parent, UIStyle style )
        {

        }
    }
}
