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
using KSS.GameplayScene.Tools;

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

            _mainPanel = canvas.AddPanel( new UILayoutInfo( UIFill.Fill() ), null );

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

            var text = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (150, 25) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Acceleration: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AccelerationReadoutUI ui2 = text.gameObject.AddComponent<AccelerationReadoutUI>();
            ui2.Text = text;


            text = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.BottomLeft, (0, 25), (150, 25) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Altitude: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            AltitudeReadoutUI ui3 = text.gameObject.AddComponent<AltitudeReadoutUI>();
            ui3.Text = text;

            UIPanel navball = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.Bottom, (0, 0), (222, 202) ), null );

            UIMask mask = navball.AddMask( new UILayoutInfo( UIAnchor.Center, (0, 0), (190, 190) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/std0/ui_navball" ) );

            (GameObject rawGameObject, RectTransform rawTransform) = UIElement.CreateUIGameObject( mask.rectTransform, "raw", new UILayoutInfo( UIFill.Fill() ) );
            RawImage rawImage = rawGameObject.AddComponent<RawImage>();
            rawImage.texture = NavballManager.AttitudeIndicatorRT;

            UIIcon attitudeIndicator = navball.AddIcon( new UILayoutInfo( UIFill.Fill() ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/attitude_indicator" ) );


            UIIcon prograde = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_prograde" ) );
            UIIcon retrograde = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_retrograde" ) );
            UIIcon normal = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_normal" ) );
            UIIcon antinormal = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_antinormal" ) );
            UIIcon antiradial = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_radial_out" ) );
            UIIcon radial = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_radial_in" ) );
            UIIcon maneuver = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_maneuver" ) );

            NavballUI nui = navball.gameObject.AddComponent<NavballUI>();
            nui.SetDirectionIcons( prograde, retrograde, normal, antinormal, antiradial, radial, maneuver );

            UIIcon horizon = navball.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (90, 32) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_horizon" ) );


            UIPanel velocityIndicator = navball.AddPanel( new UILayoutInfo( UIAnchor.Top, (0, 15), (167.5f, 40) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/velocity_indicator" ) );

            velocityIndicator.AddButton( new UILayoutInfo( UIAnchor.Left, (2, 0), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_list_gold" ), null );
            velocityIndicator.AddButton( new UILayoutInfo( UIAnchor.Right, (-2, 0), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_down_gold" ), null );

            UIText velText = velocityIndicator.AddText( new UILayoutInfo( UIFill.Fill( 31.5f, 31.5f, 0, 0 ) ), "Velocity" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            VelocityReadoutUI vel = velocityIndicator.gameObject.AddComponent<VelocityReadoutUI>();
            vel.Text = velText;
        }

        static void CreateFPSPanel()
        {
            UIPanel fpsPanel = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, (5, -35), (80, 30) ), null );
            UIText fpsCounter = fpsPanel.AddText( new UILayoutInfo( UIFill.Fill() ), "FPS: <missing>" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Left )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            FPSCounterUI ui5 = fpsCounter.gameObject.AddComponent<FPSCounterUI>();
            ui5.Text = fpsCounter;
        }

        private static void CreateTopPanel()
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                UIPanel topPanel = _mainPanel.AddPanel( new UILayoutInfo( UIFill.Horizontal( -15, -15 ), UIAnchor.Top, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel" ) );

                UIPanel p1 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 35, 110 ), null );
                UIButton newBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_new" ), null );
                UIButton openBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (40, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_open" ), () =>
                {
                } );
                UIButton saveBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (80, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_save" ), () =>
                {
                } );

                UIText utText = topPanel.AddText( new UILayoutInfo( UIAnchor.BottomLeft, (150, 0), (80, 30) ), "" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                UTReadoutUI ui2 = utText.gameObject.AddComponent<UTReadoutUI>();
                ui2.Text = utText;

                TimewarpSelectorUI selector = TimewarpSelectorUI.Create( topPanel, new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 230, 110 ), new float[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256 } );
            }
            else
            {
                UIPanel topLeftPanel = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, (-15, 0), (416, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel" ) );

                UIPanel p1 = topLeftPanel.AddPanel( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 35, 110 ), null );
                UIButton newBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_new" ), null );
                UIButton openBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (40, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_open" ), () =>
                {
                } );
                UIButton saveBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (80, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_save" ), () =>
                {
                } );

                UIText utText = topLeftPanel.AddText( new UILayoutInfo( UIAnchor.BottomLeft, (150, 0), (80, 30) ), "" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                UTReadoutUI ui2 = utText.gameObject.AddComponent<UTReadoutUI>();
                ui2.Text = utText;

                TimewarpSelectorUI selector = TimewarpSelectorUI.Create( topLeftPanel, new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 230, 110 ), new float[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256 } );

                UIPanel topRightPanel = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.TopRight, (15, 0), (100, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel" ) );

                UIPanel p4 = topRightPanel.AddPanel( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical(), -35, 30 ), null );
                UIButton deselectActive = p4.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_leave" ), () =>
                {
                    ActiveObjectManager.ActiveObject = null;
                } );
            }
        }

        private static void CreateBottomPanelInactive()
        {
            UIPanel bottomPanel = _mainPanel.AddPanel( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 30 ), null );

            UIButton defaultButton = bottomPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (-48, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {
            } )
                .WithText( new UILayoutInfo( UIFill.Fill() ), "A", out _ );

            UIButton constructButton = bottomPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (-16, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {
                CanvasManager.Get( CanvasName.WINDOWS ).AddPromptWindow( "Vessel to create...", "id here", vesselId =>
                {
                    Transform root = PartRegistry.Load( new NamespacedIdentifier( "Vessels", vesselId ) ).transform;

                    foreach( var fc in root.GetComponentsInChildren<FConstructible>() )
                        fc.BuildPoints = 0.0f;

                    ConstructTool tool = GameplaySceneToolManager.UseTool<ConstructTool>();
                    tool.SetGhostPart( root, Vector3.zero );
                } );
            } )
                .WithText( new UILayoutInfo( UIFill.Fill() ), "C", out _ );

            UIButton deconstructButton = bottomPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (16, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {

            } )
                .WithText( new UILayoutInfo( UIFill.Fill() ), "D", out _ );
        }

        private static void CreateToggleButtonList()
        {
            UIPanel buttonListPanel = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.BottomRight, (0, 0), (100, 30) ), null );
            buttonListPanel.LayoutDriver = new HorizontalLayoutDriver()
            {
                Dir = HorizontalLayoutDriver.Direction.RightToLeft,
                Spacing = 2f,
                FitToSize = true
            };

            UIButton deltaVAnalysisWindow = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton controlSetupWindow = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton button3 = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            UILayoutManager.BroadcastLayoutUpdate( buttonListPanel );
        }
    }
}