using HSP.UI;
using HSP.UI.Windows;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.GameplayScene.Tools;
using HSP.Vanilla.UI.Components;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Scenes.GameplayScene
{
    public static class GameplaySceneUIFactory
    {
        static UIPanel _mainPanel;

        public const string CREATE_UI = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.ui.create";
        public const string DESTROY_UI = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.ui.destroy";

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID, CREATE_UI )]
        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, CREATE_UI, After = new[] { HSP.Vanilla.Scenes.GameplayScene.OnStartup.ADD_ACTIVE_VESSEL_MANAGER } )]
        private static void Create()
        {
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            if( !_mainPanel.IsNullOrDestroyed() )
            {
                _mainPanel.Destroy();
            }
            _mainPanel = canvas.AddPanel( new UILayoutInfo( UIFill.Fill() ), null );

            if( ActiveVesselManager.ActiveObject == null )
            {
                CreateUIActiveObjectNull( _mainPanel );
            }
            else
            {
                CreateUIActiveObjectExists( _mainPanel );
            }
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID, DESTROY_UI )]
        private static void Destroy()
        {
            if( !_mainPanel.IsNullOrDestroyed() )
            {
                _mainPanel.Destroy();
            }
        }

        public static void CreateUIActiveObjectNull( IUIElementContainer parent )
        {
            CreateTopPanel( parent );
            CreateFPSPanel( parent );
            CreateBottomPanelInactive( parent );

            CreateToggleButtonList( parent );
        }

        public static void CreateUIActiveObjectExists( IUIElementContainer parent )
        {
            CreateTopPanel( parent );
            CreateFPSPanel( parent );

            CreateToggleButtonList( parent );

            parent.AddPanel( new UILayoutInfo( UIAnchor.BottomLeft, (75, 0), (150, 25) ), AssetRegistry.Get<Sprite>( "builtin::Background" ) )
                .WithTint( Color.gray )
                .AddTextReadout_Acceleration( new UILayoutInfo( UIFill.Fill() ) )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            parent.AddNavball( new UILayoutInfo( UIAnchor.Bottom, (0, 0), (222, 202) ) );
        }

        static void CreateFPSPanel( IUIElementContainer parent )
        {
            UIPanel fpsPanel = parent.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, (5, -35), (80, 30) ), null );
            
            fpsPanel.AddTextReadout_FPS( new UILayoutInfo( UIFill.Fill() ) )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Left )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );
        }

        private static void CreateTopPanel( IUIElementContainer parent )
        {
            if( ActiveVesselManager.ActiveObject == null )
            {
                UIPanel topPanel = parent.AddPanel( new UILayoutInfo( UIFill.Horizontal( -15, -15 ), UIAnchor.Top, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel" ) );

                UIPanel p1 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 35, 110 ), null );
                UIButton newBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_new" ), null );
                UIButton openBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (40, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_open" ), () =>
                {
                } );
                UIButton saveBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (80, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_save" ), () =>
                {
                } );

                topPanel.AddTextReadout_ElapsedUT( new UILayoutInfo( UIAnchor.BottomLeft, (150, 0), (80, 30) ) )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                UITimewarpSelector selector = topPanel.AddTimewarpSelector( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 230, 110 ), new float[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256 } );
            }
            else
            {
                UIPanel topLeftPanel = parent.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, (-15, 0), (416, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel" ) );

                UIPanel p1 = topLeftPanel.AddPanel( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 35, 110 ), null );
                UIButton newBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_new" ), null );
                UIButton openBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (40, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_open" ), () =>
                {
                } );
                UIButton saveBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (80, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_save" ), () =>
                {
                } );

                topLeftPanel.AddTextReadout_ElapsedUT( new UILayoutInfo( UIAnchor.BottomLeft, (150, 0), (80, 30) ) )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                UITimewarpSelector selector = topLeftPanel.AddTimewarpSelector( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 230, 110 ), new float[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256 } );

                UIPanel topRightPanel = parent.AddPanel( new UILayoutInfo( UIAnchor.TopRight, (15, 0), (100, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel" ) );

                UIPanel p4 = topRightPanel.AddPanel( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical(), -35, 30 ), null );
                UIButton deselectActive = p4.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_leave" ), () =>
                {
                    ActiveVesselManager.ActiveObject = null;
                } );
            }
        }

        private static void CreateBottomPanelInactive( IUIElementContainer parent )
        {
            UIPanel bottomPanel = parent.AddPanel( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 30 ), null );

            UIButton defaultButton = bottomPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (-48, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {
            } )
                .WithText( new UILayoutInfo( UIFill.Fill() ), "A", out _ );

            UIButton constructButton = bottomPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (-16, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {
                CanvasManager.Get( CanvasName.WINDOWS ).AddPromptWindow( "Vessel to create...", "id here", vesselId =>
                {
                    ConstructTool tool = GameplaySceneToolManager.UseTool<ConstructTool>();
                    tool.SpawnVesselAndSetGhost( vesselId );
                } );
            } )
                .WithText( new UILayoutInfo( UIFill.Fill() ), "C", out _ );

            UIButton deconstructButton = bottomPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (16, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {

            } )
                .WithText( new UILayoutInfo( UIFill.Fill() ), "D", out _ );
        }

        private static void CreateToggleButtonList( IUIElementContainer parent )
        {
            UIPanel buttonListPanel = parent.AddPanel( new UILayoutInfo( UIAnchor.BottomRight, (0, 0), (100, 30) ), null );
            buttonListPanel.LayoutDriver = new HorizontalLayoutDriver()
            {
                Dir = HorizontalLayoutDriver.Direction.RightToLeft,
                Spacing = 2f,
                FitToSize = true
            };

            UIButton deltaVAnalysisWindow = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton controlSetupWindow = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton button3 = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            UILayoutManager.ForceLayoutUpdate( buttonListPanel );
        }
    }
}