using KSS.Core;
using KSS.Core.Mods;
using KSS.Core.Scenes;
using KSS.UI;
using System;
using System.Collections.Generic;
using UnityPlus.UILib;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;
using UnityPlus.AssetManagement;

namespace Assets.KSS.UI.SceneFactories
{
    public static class VanillaGameplayUIFactory
    {
        [OverridableEventListener( HSPOverridableEvent.STARTUP_GAMEPLAY, HSPOverridableEvent.NAMESPACE_VANILLA + ".gameplay_ui" )]
        public static void Create( object obj )
        {
            Canvas canvas = CanvasManager.Get( CanvasName.STATIC );


            UIText text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint(Color.gray)
                .AddText( UILayoutInfo.Fill(), "Velocity: <missing>" );

            VelocityReadoutUI ui = text.gameObject.AddComponent<VelocityReadoutUI>();
            ui.TextBox = text.textComponent;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 25 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Acceleration: <missing>" );

            AccelerationReadoutUI ui2 = text.gameObject.AddComponent<AccelerationReadoutUI>();
            ui2.TextBox = text.textComponent;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 50 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Altitude: <missing>" );

            AltitudeReadoutUI ui3 = text.gameObject.AddComponent<AltitudeReadoutUI>();
            ui3.TextBox = text.textComponent;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 75 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Warp Rate: <missing>" );

            TimewarpReadoutUI ui4 = text.gameObject.AddComponent<TimewarpReadoutUI>();
            ui4.TextBox = text.textComponent;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.up, new Vector2( 0, 0 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "FPS: <missing>" );

            FPSCounterUI ui5 = text.gameObject.AddComponent<FPSCounterUI>();
            ui5.TextBox = text.textComponent;


#warning todo complete the mainmenu factory.
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreateMainMenuButton( RectTransform parent )
        {

        }

        private static void CreateSaveButton( RectTransform parent )
        {

        }

        private static void CreateLoadButton( RectTransform parent )
        {

        }
    }
}
