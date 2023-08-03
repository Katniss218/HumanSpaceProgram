using KSS.Core;
using KSS.Core.Mods;
using KSS.Core.Scenes;
using System;
using System.Collections.Generic;
using UILib;
using UILib.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace KSS.UI.SceneFactories
{
    /// <summary>
    /// Creates the Main Menu UI elements.
    /// </summary>
    public static class VanillaMainMenuUIFactory
    {
        [OverridableEventListener( HSPOverridableEvent.STARTUP_MAINMENU, HSPOverridableEvent.NAMESPACE_VANILLA + ".mainmenu_ui" )]
        public static void Create( object obj )
        {
            (_, UIStyle style) = ((Canvas, UIStyle))obj;

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