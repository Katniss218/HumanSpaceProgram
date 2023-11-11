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
using UnityPlus.UILib.Layout;

namespace KSS.UI.SceneFactories
{
    public static class GameplaySceneUIFactory
    {
        static UIPanel _mainPanel;

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, HSPEvent.NAMESPACE_VANILLA + ".gameplay_ui" )]
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".gameplay_ui" )]
        public static void CreateUI( object e )
        {
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            if( !_mainPanel.IsNullOrDestroyed() )
            {
                _mainPanel.Destroy();
            }

            _mainPanel = canvas.AddPanel( UILayoutInfo.Fill(), null );

            if( ActiveObjectManager.ActiveObject == null )
            {
                CreateUIActiveObjectNull();
            }
            else
            {
                CreateUIActiveObjectExists();
            }
        }

        public static void CreateUIActiveObjectNull()
        {
            CreateTopPanel();

            CreateWindowToggleList();
        }

        public static void CreateUIActiveObjectExists()
        {
            CreateTopPanel();

            CreateWindowToggleList();

            UIText text = _mainPanel.AddPanel( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Velocity: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            VelocityReadoutUI ui = text.gameObject.AddComponent<VelocityReadoutUI>();
            ui.Text = text;


            text = _mainPanel.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 25 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Acceleration: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AccelerationReadoutUI ui2 = text.gameObject.AddComponent<AccelerationReadoutUI>();
            ui2.Text = text;


            text = _mainPanel.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 50 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Altitude: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AltitudeReadoutUI ui3 = text.gameObject.AddComponent<AltitudeReadoutUI>();
            ui3.Text = text;


            text = _mainPanel.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 75 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Warp Rate: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            TimewarpReadoutUI ui4 = text.gameObject.AddComponent<TimewarpReadoutUI>();
            ui4.Text = text;


            text = _mainPanel.AddPanel( new UILayoutInfo( Vector2.up, new Vector2( 0, 0 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "FPS: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            FPSCounterUI ui5 = text.gameObject.AddComponent<FPSCounterUI>();
            ui5.Text = text;


            UIPanel navball = _mainPanel.AddPanel( new UILayoutInfo( new Vector2( 0.5f, 0 ), Vector2.zero, new Vector2( 222, 202 ) ), null );

            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUIGameObject( navball.rectTransform, "mask", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 190, 190 ) ) );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.maskable = true;
            imageComponent.sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/std0/ui_navball" );
            imageComponent.type = Image.Type.Simple;

            Mask mask = rootGameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject rawGameObject, RectTransform rawTransform) = UIElement.CreateUIGameObject( rootTransform, "raw", UILayoutInfo.Fill() );
            RawImage rawImage = rawGameObject.AddComponent<RawImage>();
            rawImage.texture = null;

            UIIcon attitudeIndicator = navball.AddIcon( UILayoutInfo.Fill(), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/attitude_indicator" ) );
            UIPanel velocityIndicator = navball.AddPanel( new UILayoutInfo( new Vector2( 0.5f, 1f ), new Vector2( 0, 15 ), new Vector2( 167.5f, 40 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/velocity_indicator" ) );

            velocityIndicator.AddButton( new UILayoutInfo( new Vector2( 0, 0.5f ), new Vector2( 2, 0 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_list_gold" ), null );
            velocityIndicator.AddButton( new UILayoutInfo( new Vector2( 1, 0.5f ), new Vector2( -2, 0 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_down_gold" ), null );

            UIText velText = velocityIndicator.AddText( UILayoutInfo.Fill( 31.5f, 31.5f, 0, 0 ), "Velocity" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            VelocityReadoutUI vel = velocityIndicator.gameObject.AddComponent<VelocityReadoutUI>();
            vel.Text = velText;
        }

        private static void CreateTopPanel()
        {
            UIPanel topPanel = _mainPanel.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, 1f, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );
            UIPanel p4 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 1f, -20, 30 ), null );
            if( ActiveObjectManager.ActiveObject != null )
            {
                UIButton deselectActive = p4.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
                {
                    ActiveObjectManager.ActiveObject = null;
                } );
            }
        }

        private static void CreateWindowToggleList()
        {
            UIPanel uiPanel = _mainPanel.AddPanel( new UILayoutInfo( new Vector2( 1, 0 ), Vector2.zero, new Vector2( 100, 30 ) ), null );
            uiPanel.LayoutDriver = new HorizontalLayoutDriver()
            {
                Dir = HorizontalLayoutDriver.Direction.RightToLeft,
                Spacing = 2f,
                FitToSize = true
            };

            UIButton deltaVAnalysisWindow = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton controlSetupWindow = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton button3 = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            UILayout.BroadcastLayoutUpdate( uiPanel );
        }
    }
}
