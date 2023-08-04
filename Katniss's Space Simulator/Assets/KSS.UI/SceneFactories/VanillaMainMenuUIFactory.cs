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
            RectTransform canvasTransform = (RectTransform)CanvasManager.GetCanvas( CanvasManager.STATIC ).transform;

            CreatePlayButton( canvasTransform, style );
            CreateSettingsButton( canvasTransform, style );
            CreateExitButton( canvasTransform, style );

            (_, var bar) = UIValueBarEx.CreateEmptyHorizontal( canvasTransform, "bar", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, 35 ), new Vector2( 200, 30 ) ), style );

            var seg = bar.AddSegment( 0.25f );
            seg.Sprite = style.Bar;
            seg.Color = Color.blue;

            seg = bar.InsertSegment( 0, 0.5f );
            seg.Sprite = style.Bar;
            seg.Color = Color.red;
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreatePlayButton( RectTransform parent )
        {
            (_, Button btn) = ButtonFactory.CreateTextXY( parent, "play button", "PLAY TEST", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 200, 30 ) ), style );

            PlayButtonSwitcher pbs = btn.gameObject.AddComponent<PlayButtonSwitcher>();
            btn.onClick.AddListener( pbs.StartGame );
        }

        private static void CreateSettingsButton( RectTransform parent )
        {
            (_, Button btn) = ButtonFactory.CreateTextXY( parent, "settings button", "SETTINGS", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -35 ), new Vector2( 200, 30 ) ), style );

            btn.interactable = false;
        }

        private static void CreateExitButton( RectTransform parent )
        {
            (_, Button btn) = ButtonFactory.CreateTextXY( parent, "exit button", "EXIT", new UILayoutInfo( new Vector2( 0.5f, 0 ), new Vector2( 0, 50 ), new Vector2( 200, 30 ) ), style );
        }
    }
}