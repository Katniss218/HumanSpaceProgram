using HSP.Content.Vessels.Serialization;
using HSP.SceneManagement;
using HSP.UI;
using HSP.UI.Canvases;
using HSP.UI.Windows;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.DesignScene.Tools;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.UI.Components;
using System;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Scenes.DesignScene
{
    /// <summary>
    /// Creates Design Scene UI elements.
    /// </summary>
    public static class DesignSceneUIFactory
    {
        public static UIPanel _toolOptionsPanel;
        public static UIInputField<string> _vesselNameIF;

        static UIPanel _mainPanel;

        public const string CREATE_UI = HSPEvent.NAMESPACE_HSP + ".design_scene.ui.create";
        public const string DESTROY_UI = HSPEvent.NAMESPACE_HSP + ".design_scene.ui.destroy";
        public const string UPDATE_VESSEL_NAME = HSPEvent.NAMESPACE_HSP + ".update.vessel_name";
        public const string UPDATE_SELECTED_TOOL = HSPEvent.NAMESPACE_HSP + ".update.tool";

        [HSPEventListener( HSPEvent_DESIGN_SCENE_ACTIVATE.ID, CREATE_UI )]
        private static void Create()
        {
            UICanvas canvas = DesignSceneM.Instance.GetStaticCanvas();

            if( !_mainPanel.IsNullOrDestroyed() )
            {
                _mainPanel.Destroy();
            }
            _mainPanel = canvas.AddPanel( new UILayoutInfo( UIFill.Fill() ), null );

            CreatePartList( _mainPanel );
            CreateTopPanel( _mainPanel );
            CreateToolSelector( _mainPanel );
            CreateToggleButtonList( _mainPanel );
        }

        [HSPEventListener( HSPEvent_DESIGN_SCENE_DEACTIVATE.ID, DESTROY_UI )]
        private static void Destroy()
        {
            if( !_mainPanel.IsNullOrDestroyed() )
            {
                _mainPanel.Destroy();
            }
        }

        [HSPEventListener( HSPEvent_AFTER_DESIGN_SCENE_VESSEL_LOADED.ID, UPDATE_VESSEL_NAME )]
        private static void OnAfterVesselLoad()
        {
            _vesselNameIF.SetValue( DesignVesselManager.CurrentVesselMetadata.Name );
        }

        const float PART_LIST_WIDTH = 285f;

        private static void CreatePartList( IUIElementContainer parent )
        {
            UIPartList partListUI = parent.AddPartList( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical( 30, 0 ), 0, PART_LIST_WIDTH ) );
        }

        private static void CreateTopPanel( IUIElementContainer parent )
        {
            if( !_vesselNameIF.IsNullOrDestroyed() )
            {
                _vesselNameIF.Destroy();
            }
            UIPanel topPanel = parent.AddPanel( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );
            UIPanel p1 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 20, 110 ), null );
            UIButton newBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_new" ), null )
                .Disabled();
            UIButton openBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (40, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_open" ), () =>
            {
                DesignSceneM.Instance.GetWindowCanvas()
                    .AddPromptWindow( "Load ...", "vessel name/ID", ( text ) =>
                {
                    DesignVesselManager.LoadVessel( IOHelper.SanitizeFileName( text ) );
                } );
            } );
            UIButton saveBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (80, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_save" ), () =>
            {
                if( DesignVesselManager.CurrentVesselMetadata == null )
                {
                    DesignSceneM.Instance.GetWindowCanvas()
                        .AddAlertWindow( "Specify the name of the vessel before saving." );
                    return;
                }

                DesignSceneM.Instance.GetWindowCanvas()
                    .AddConfirmWindow( "Save ...", $"Confirm saving '{DesignVesselManager.CurrentVesselMetadata.ID}'.", () =>
                {
                    DesignVesselManager.SaveVessel();
                } );
            } );
            UIPanel p2 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Center, UIFill.Vertical(), 0, 300 ), null );
            _vesselNameIF = p2.AddStringInputField( new UILayoutInfo( UIAnchor.Center, UIFill.Vertical(), 0, 300 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) )
                .WithMargins( 5, 5, 5, 5 )
                .WithPlaceholder( "vessel's name..." );
            _vesselNameIF.OnValueChanged += e =>
            {
                DesignVesselManager.CurrentVesselMetadata = new VesselMetadata( IOHelper.SanitizeFileName( e.NewValue ) )
                {
                    Name = e.NewValue,
                    Description = DesignVesselManager.CurrentVesselMetadata?.Description,
                    Author = DesignVesselManager.CurrentVesselMetadata?.Author
                };
            };
            UIPanel p3 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Center, UIFill.Vertical(), 190, 60 ), null );
            UIButton undoBtn = p3.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_undo" ), null )
                .Disabled();
            UIButton redoBtn = p3.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (32, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_redo" ), null )
                .Disabled();
            UIPanel p4 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical(), -20, 30 ), null );
            UIButton exitBtn = p4.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_leave" ), () =>
            {
                HSPSceneManager.ReplaceForegroundScene<Vanilla.Scenes.MainMenuScene.MainMenuSceneM>();
            } );
        }

        private static void CreateToolSelector( IUIElementContainer parent )
        {
            UIPanel uiPanel = parent.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, ((PART_LIST_WIDTH + 10), -32), (((30 + 2) * 4), 30) ), null );

            UIButton pickButton = uiPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_pick" ), () =>
            {
                DesignSceneToolManager.UseTool<PickTool>();
            } );
            UIButton moveButton = uiPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (32, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_move" ), () =>
            {
                DesignSceneToolManager.UseTool<TranslateTool>();
            } );
            UIButton rotateButton = uiPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (64, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_rotate" ), () =>
            {
                DesignSceneToolManager.UseTool<RotateTool>();
            } );
            UIButton rerootButton = uiPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (96, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_reroot" ), () =>
            {
                DesignSceneToolManager.UseTool<RerootTool>();
            } );
        }

        [HSPEventListener( HSPEvent_AFTER_DESIGN_SCENE_TOOL_CHANGED.ID, UPDATE_SELECTED_TOOL )]
        private static void CreateCurrentToolOptions()
        {
            if( !_toolOptionsPanel.IsNullOrDestroyed() )
            {
                _toolOptionsPanel.Destroy();
            }

#warning TODO - this tool-specific ui belongs to where the tools are.
            Type toolType = DesignSceneToolManager.ActiveToolType;
            if( toolType != null )
            {
                _toolOptionsPanel = _mainPanel.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, ((PART_LIST_WIDTH + 10 + ((30 + 2) * 4) + 10), -32), (62, 30) ), null );

                if( toolType == typeof( PickTool ) )
                {
                    UIButton buttonSnap = _toolOptionsPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_snap" ), null );
                    UIButton buttonSnapNum = _toolOptionsPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (32, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
                }
                if( toolType == typeof( TranslateTool ) )
                {
                    UIText t = _toolOptionsPanel.AddStdText( new UILayoutInfo( UIFill.Fill() ), "translate" );
                }
                if( toolType == typeof( RotateTool ) )
                {
                    UIText t = _toolOptionsPanel.AddStdText( new UILayoutInfo( UIFill.Fill() ), "rotate" );
                }
                if( toolType == typeof( RerootTool ) )
                {
                    UIText t = _toolOptionsPanel.AddStdText( new UILayoutInfo( UIFill.Fill() ), "reroot" );
                }
            }
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
            UIButton controlSetupWindow = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {
                if( DesignVesselManager.DesignObject == null )
                    return;

                var wc = DesignSceneM.Instance.GetWindowCanvas();
                var cmc = CanvasRegistry.GetContextMenuCanvas();
                UIControlSetupWindow.Create( wc, cmc, DesignVesselManager.DesignObject.transform );
            } );
            UIButton button3 = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            UILayoutManager.ForceLayoutUpdate( buttonListPanel );
        }
    }
}