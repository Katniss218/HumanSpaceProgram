using KSS.Core;
using KSS.DesignScene;
using KSS.DesignScene.Tools;
using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using KSS.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.SceneFactories
{
    /// <summary>
    /// Creates Design Scene UI elements.
    /// </summary>
    public static class DesignSceneUIFactory
    {
        public static UIPanel _toolOptionsPanel;
        public static UIInputField _vesselNameIF;

        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_VANILLA + ".design_scene_ui" )]
        public static void Create( object e )
        {
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            CreatePartList( canvas );
            CreateTopPanel( canvas );
            CreateToolSelector( canvas );
            CreateToggleButtonList( canvas );
        }

        const float PART_LIST_WIDTH = 285f;

        private static void CreatePartList( UICanvas canvas )
        {
            PartListUI partListUI = PartListUI.Create( canvas, new UILayoutInfo( UIAnchor.Left, UIFill.Vertical( 30, 0 ), 0, PART_LIST_WIDTH ) );
        }

        [HSPEventListener( HSPEvent.DESIGN_AFTER_LOAD, HSPEvent.NAMESPACE_VANILLA + ".update_current" )]
        private static void OnAfterVesselLoad( object e )
        {
            _vesselNameIF.Text = DesignObjectManager.CurrentVesselMetadata.Name;
        }

        private static void CreateTopPanel( UICanvas canvas )
        {
            if( !_vesselNameIF.IsNullOrDestroyed() )
            {
                _vesselNameIF.Destroy();
            }
            UIPanel topPanel = canvas.AddPanel( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );
            UIPanel p1 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 20, 110 ), null );
            UIButton newBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_new" ), null );
            UIButton openBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (40, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_open" ), () =>
            {
                CanvasManager.Get( CanvasName.WINDOWS ).AddPromptWindow( "Load ...", "vessel name/ID", ( text ) =>
                {
                    DesignObjectManager.LoadVessel( IOHelper.SanitizeFileName( text ) );
                } );
            } );
            UIButton saveBtn = p1.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (80, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_save" ), () =>
            {
                CanvasManager.Get( CanvasName.WINDOWS ).AddConfirmWindow( "Save ...", $"Confirm saving '{DesignObjectManager.CurrentVesselMetadata.ID}'.", () =>
                {
                    DesignObjectManager.SaveVessel();
                } );
            } );
            UIPanel p2 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Center, UIFill.Vertical(), 0, 300 ), null );
            _vesselNameIF = p2.AddInputField( new UILayoutInfo( UIAnchor.Center, UIFill.Vertical(), 0, 300 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) )
                .WithMargins( 5, 5, 5, 5 )
                .WithPlaceholder( "vessel's name..." );
            _vesselNameIF.SetOnTextChange( s =>
            {
                DesignObjectManager.CurrentVesselMetadata = new VesselMetadata( IOHelper.SanitizeFileName( s ) )
                {
                    Name = s,
                    Description = DesignObjectManager.CurrentVesselMetadata?.Description,
                    Author = DesignObjectManager.CurrentVesselMetadata?.Author
                };
            } );
            UIPanel p3 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Center, UIFill.Vertical(), 190, 60 ), null );
            UIButton undoBtn = p3.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_undo" ), null );
            UIButton redoBtn = p3.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (32, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_redo" ), null );
            UIPanel p4 = topPanel.AddPanel( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical(), -20, 30 ), null );
            UIButton exitBtn = p4.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_leave" ), () =>
            {
                SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null ) );
            } );
        }

        private static void CreateToolSelector( UICanvas canvas )
        {
            UIPanel uiPanel = canvas.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, ((PART_LIST_WIDTH + 10), -32), (((30 + 2) * 4), 30) ), null );

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

        [HSPEventListener( HSPEvent.DESIGN_AFTER_TOOL_CHANGED, HSPEvent.NAMESPACE_VANILLA + ".tool_changed_ui" )]
        private static void CreateCurrentToolOptions( object e )
        {
            if( !_toolOptionsPanel.IsNullOrDestroyed() )
            {
                _toolOptionsPanel.Destroy();
            }
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            Type toolType = DesignSceneToolManager.ActiveToolType;
            if( toolType != null )
            {
                _toolOptionsPanel = canvas.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, ((PART_LIST_WIDTH + 10 + ((30 + 2) * 4) + 10), -32), (62, 30) ), null );

                if( toolType == typeof( PickTool ) )
                {
                    UIButton buttonSnap = _toolOptionsPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_snap" ), null );
                    UIButton buttonSnapNum = _toolOptionsPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (32, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
                }
                if( toolType == typeof( TranslateTool ) )
                {
                    UIText t = _toolOptionsPanel.AddText( new UILayoutInfo( UIFill.Fill() ), "translate" );
                }
                if( toolType == typeof( RotateTool ) )
                {
                    UIText t = _toolOptionsPanel.AddText( new UILayoutInfo( UIFill.Fill() ), "rotate" );
                }
                if( toolType == typeof( RerootTool ) )
                {
                    UIText t = _toolOptionsPanel.AddText( new UILayoutInfo( UIFill.Fill() ), "reroot" );
                }
            }
        }

        private static void CreateToggleButtonList( UICanvas canvas )
        {
            UIPanel buttonListPanel = canvas.AddPanel( new UILayoutInfo( UIAnchor.BottomRight, (0, 0), (100, 30) ), null );
            buttonListPanel.LayoutDriver = new HorizontalLayoutDriver()
            {
                Dir = HorizontalLayoutDriver.Direction.RightToLeft,
                Spacing = 2f,
                FitToSize = true
            };

            UIButton deltaVAnalysisWindow = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
            UIButton controlSetupWindow = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), () =>
            {
                UIControlSetupWindow.Create( DesignObjectManager.DesignObject.transform );
            } );
            UIButton button3 = buttonListPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            UILayoutManager.BroadcastLayoutUpdate( buttonListPanel );
        }
    }
}