using KSS.Core;
using KSS.Core.Mods;
using KSS.Core.SceneManagement;
using KSS.UI;
using System;
using System.Collections.Generic;
using UnityPlus.UILib;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;
using UnityPlus.AssetManagement;

namespace KSS.UI.SceneFactories
{
    public static class VanillaGameplayUIFactory
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".gameplay_ui" )]
        public static void Create( object obj )
        {
            Canvas canvas = CanvasManager.Get( CanvasName.STATIC );


            UIText text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Velocity: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            VelocityReadoutUI ui = text.gameObject.AddComponent<VelocityReadoutUI>();
            ui.Text = text;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 25 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Acceleration: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AccelerationReadoutUI ui2 = text.gameObject.AddComponent<AccelerationReadoutUI>();
            ui2.Text = text;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 50 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Altitude: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AltitudeReadoutUI ui3 = text.gameObject.AddComponent<AltitudeReadoutUI>();
            ui3.Text = text;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 75 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Warp Rate: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            TimewarpReadoutUI ui4 = text.gameObject.AddComponent<TimewarpReadoutUI>();
            ui4.Text = text;


            text = canvas.AddPanel( new UILayoutInfo( Vector2.up, new Vector2( 0, 0 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "FPS: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            FPSCounterUI ui5 = text.gameObject.AddComponent<FPSCounterUI>();
            ui5.Text = text;


            UIPanel navball = canvas.AddPanel( new UILayoutInfo( new Vector2( 0.5f, 0 ), Vector2.zero, new Vector2( 222, 202 ) ), null );

            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( navball.rectTransform, "mask", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 190, 190 ) ) );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.maskable = true;
            imageComponent.sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/std0/ui_navball" );
            imageComponent.type = Image.Type.Simple;

            Mask mask = rootGameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject rawGameObject, RectTransform rawTransform) = UIElement.CreateUI( rootTransform, "raw", UILayoutInfo.Fill() );
            RawImage rawImage = rawGameObject.AddComponent<RawImage>();
            rawImage.texture = null;

            UIIcon velocityIcon = navball.AddIcon( UILayoutInfo.Fill(), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/attitude_indicator" ) )
                .AddIcon( new UILayoutInfo( new Vector2( 0.5f, 1f ), new Vector2( 0, 15 ), new Vector2( 167.5f, 40 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/velocity_indicator" ) );

            velocityIcon.AddButton( new UILayoutInfo( new Vector2( 0, 0.5f ), new Vector2( 2, 0 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_list_gold" ) );
            velocityIcon.AddButton( new UILayoutInfo( new Vector2( 1, 0.5f ), new Vector2( -2, 0 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_down_gold" ) );

            UIText velText = velocityIcon.AddText( UILayoutInfo.Fill( 31.5f, 31.5f, 0, 0 ), "Velocity" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            VelocityReadoutUI vel = velocityIcon.gameObject.AddComponent<VelocityReadoutUI>();
            vel.Text = velText;
        }
    }
}
