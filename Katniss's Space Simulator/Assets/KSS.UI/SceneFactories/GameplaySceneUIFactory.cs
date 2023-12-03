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
using KSS.UI.Windows;
using KSS.GameplayScene;
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Strategies;
using KSS.Components;

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
            CreateFPSPanel();
            CreateBottomPanelInactive();

            CreateToggleButtonList();
        }

        public static void CreateUIActiveObjectExists()
        {
            CreateTopPanel();
            CreateFPSPanel();

            CreateToggleButtonList();

            var text = _mainPanel.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Acceleration: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AccelerationReadoutUI ui2 = text.gameObject.AddComponent<AccelerationReadoutUI>();
            ui2.Text = text;


            text = _mainPanel.AddPanel( new UILayoutInfo( Vector2.zero, new Vector2( 0, 25 ), new Vector2( 150, 25 ) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( UILayoutInfo.Fill(), "Altitude: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AltitudeReadoutUI ui3 = text.gameObject.AddComponent<AltitudeReadoutUI>();
            ui3.Text = text;

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

        static void CreateFPSPanel()
        {
            UIPanel fpsPanel = _mainPanel.AddPanel( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 5, -35 ), new Vector2( 80, 30 ) ), null );
            UIText fpsCounter = fpsPanel.AddText( UILayoutInfo.Fill(), "FPS: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Left )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            FPSCounterUI ui5 = fpsCounter.gameObject.AddComponent<FPSCounterUI>();
            ui5.Text = fpsCounter;
        }

        private static void CreateTopPanel()
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                UIPanel topPanel = _mainPanel.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, 1f, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );

                UIPanel p1 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 0f, 20, 110 ), null );
                UIButton newBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), null );
                UIButton openBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 40, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_open" ), () =>
                {
                } );
                UIButton saveBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 80, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_save" ), () =>
                {
                } );

                UIText utText = topPanel.AddText( new UILayoutInfo( Vector2.zero, new Vector2( 150, 0 ), new Vector2( 80, 30 ) ), "" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                UTReadoutUI ui2 = utText.gameObject.AddComponent<UTReadoutUI>();
                ui2.Text = utText;

                TimewarpSelectorUI selector = TimewarpSelectorUI.Create( topPanel, UILayoutInfo.FillVertical( 0, 0, 0f, 230, 110 ), new float[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256 } );
            }
            else
            {
                UIPanel topLeftPanel = _mainPanel.AddPanel( new UILayoutInfo( UILayoutInfo.TopLeft, Vector2.zero, new Vector2( 400, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );

                UIPanel p1 = topLeftPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 0f, 20, 110 ), null );
                UIButton newBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), null );
                UIButton openBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 40, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_open" ), () =>
                {
                } );
                UIButton saveBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 80, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_save" ), () =>
                {
                } );

                UIText utText = topLeftPanel.AddText( new UILayoutInfo( Vector2.zero, new Vector2( 150, 0 ), new Vector2( 80, 30 ) ), "" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                UTReadoutUI ui2 = utText.gameObject.AddComponent<UTReadoutUI>();
                ui2.Text = utText;

                TimewarpSelectorUI selector = TimewarpSelectorUI.Create( topLeftPanel, UILayoutInfo.FillVertical( 0, 0, 0f, 230, 110 ), new float[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256 } );

                UIPanel topRightPanel = _mainPanel.AddPanel( new UILayoutInfo( UILayoutInfo.TopRight, Vector2.zero, new Vector2( 70, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );

                UIPanel p4 = topRightPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 1f, -20, 30 ), null );
                UIButton deselectActive = p4.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
                {
                    ActiveObjectManager.ActiveObject = null;
                } );
            }
        }

        private static void CreateBottomPanelInactive()
        {
            UIPanel bottomPanel = _mainPanel.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, 0f, 0, 30 ), null );

            UIButton constructButton = bottomPanel.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( -16, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {
                CanvasManager.Get( CanvasName.WINDOWS ).AddTextSelectionWindow( "Vessel to create...", "id here", s =>
                {
                    (Transform root, Dictionary<FConstructible, ConstructionSite.ConstructibleData> de, BidirectionalReferenceStore refMap) = ConstructionSite.SpawnGhost( s );
                    root.position = Cameras.GameplayCameraController.MainCamera.transform.position;
                    ConstructionSite.AddGhostToConstruction( root, de, VesselManager.GetLoadedVessels()[1].RootPart, refMap );
                } );
            } )
                .WithText( UILayoutInfo.Fill(), "C", out _ );

            UIButton deconstructButton = bottomPanel.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 16, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {

            } )
                .WithText( UILayoutInfo.Fill(), "D", out _ );
        }

        private static void CreateToggleButtonList()
        {
            UIPanel buttonListPanel = _mainPanel.AddPanel( new UILayoutInfo( new Vector2( 1, 0 ), Vector2.zero, new Vector2( 100, 30 ) ), null );
            buttonListPanel.LayoutDriver = new HorizontalLayoutDriver()
            {
                Dir = HorizontalLayoutDriver.Direction.RightToLeft,
                Spacing = 2f,
                FitToSize = true
            };

            UIButton deltaVAnalysisWindow = buttonListPanel.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton controlSetupWindow = buttonListPanel.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton button3 = buttonListPanel.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            UILayout.BroadcastLayoutUpdate( buttonListPanel );
        }
    }
}