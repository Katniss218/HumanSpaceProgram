using KSS.Core.Scenes;
using System.Collections;
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
    public static class MainMenuUIFactory
    {
        public static void Create( Canvas mainMenuCanvas, UIStyle style )
        {
            RectTransform canvasTransform = (RectTransform)mainMenuCanvas.transform;

            CreatePlayButton( canvasTransform, style );
            CreateSettingsButton( canvasTransform, style );
            CreateExitButton( canvasTransform, style );

            (_, var bar) = BarFactory.CreateEmptyHorizontal( canvasTransform, "bar", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, 35 ), new Vector2( 200, 30 ) ), style );

            var seg = bar.AddSegment( 0.25f );
            seg.Sprite = style.Bar;
            seg.Color = Color.blue;

            seg = bar.InsertSegment( 0, 0.5f );
            seg.Sprite = style.Bar;
            seg.Color = Color.red;
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreatePlayButton( RectTransform parent, UIStyle style )
        {
            (_, Button btn) = ButtonFactory.CreateTextXY( parent, "play button", "PLAY TEST", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 200, 30 ) ), style );

            PlayButtonSwitcher pbs = btn.gameObject.AddComponent<PlayButtonSwitcher>();
            btn.onClick.AddListener( pbs.StartGame );
        }

        private static void CreateSettingsButton( RectTransform parent, UIStyle style )
        {
            (_, Button btn) = ButtonFactory.CreateTextXY( parent, "settings button", "SETTINGS", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -35), new Vector2( 200, 30 ) ), style );
            
            btn.interactable = false;
        }

        private static void CreateExitButton( RectTransform parent, UIStyle style )
        {
            (_, Button btn) = ButtonFactory.CreateTextXY( parent, "exit button", "EXIT", new UILayoutInfo( new Vector2( 0.5f, 0 ), new Vector2( 0, 50), new Vector2( 200, 30 ) ), style );

        }
    }
}