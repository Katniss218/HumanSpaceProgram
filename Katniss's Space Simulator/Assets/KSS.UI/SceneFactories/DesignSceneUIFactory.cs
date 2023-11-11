using KSS.Core;
using KSS.Core.DesignScene;
using KSS.Core.DesignScene.Tools;
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
            CreateWindowToggleList( canvas );
        }

        const float PART_LIST_WIDTH = 285f;

        private static void CreatePartList( UICanvas canvas )
        {
            PartListUI partListUI = PartListUI.Create( canvas, UILayoutInfo.FillVertical( 30, 0, 0f, 0, PART_LIST_WIDTH ) );
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
            UIPanel topPanel = canvas.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, 1f, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );
            UIPanel p1 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 0f, 20, 110 ), null );
            UIButton newBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), null );
            UIButton openBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 40, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_open" ), () =>
            {
                CanvasManager.Get( CanvasName.WINDOWS ).AddTextSelectionWindow( "Load ...", "vessel name/ID", ( text ) =>
                {
                    DesignObjectManager.LoadVessel( IOHelper.SanitizeFileName( text ) );
                } );
            } );
            UIButton saveBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 80, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_save" ), () =>
            {
                CanvasManager.Get( CanvasName.WINDOWS ).AddConfirmationWindow( "Save ...", $"Confirm saving '{DesignObjectManager.CurrentVesselMetadata.ID}'.", () =>
                {
                    DesignObjectManager.SaveVessel();
                } );
            } );
            UIPanel p2 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 0.5f, 0, 300 ), null );
            _vesselNameIF = p2.AddInputField( UILayoutInfo.FillVertical( 0, 0, 0.5f, 0, 300 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) )
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
            UIPanel p3 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 0.5f, 150 + 30 + 10, 60 ), null );
            UIButton undoBtn = p3.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_undo" ), null );
            UIButton redoBtn = p3.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 32, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_redo" ), null );
            UIPanel p4 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 1f, -20, 30 ), null );
            UIButton exitBtn = p4.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_leave" ), () =>
            {
                SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null ) );
            } );
        }

        private static void CreateToolSelector( UICanvas canvas )
        {
            UIPanel uiPanel = canvas.AddPanel( new UILayoutInfo( new Vector2( 0, 1 ), new Vector2( PART_LIST_WIDTH + 10, -32 ), new Vector2( (30 + 2) * 4, 30 ) ), null );

            UIButton pickButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_pick" ), () =>
            {
                DesignSceneToolManager.UseTool<PickTool>();
            } );
            UIButton moveButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 32, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_move" ), () =>
            {
                DesignSceneToolManager.UseTool<TranslateTool>();
            } );
            UIButton rotateButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 64, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_rotate" ), () =>
            {
                DesignSceneToolManager.UseTool<RotateTool>();
            } );
            UIButton rerootButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 96, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_reroot" ), () =>
            {
                DesignSceneToolManager.UseTool<RerootTool>();
            } );
        }

        [HSPEventListener( HSPEvent.DESIGN_TOOL_CHANGED, HSPEvent.NAMESPACE_VANILLA + ".tool_changed_ui" )]
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
                _toolOptionsPanel = canvas.AddPanel( new UILayoutInfo( new Vector2( 0, 1 ), new Vector2( PART_LIST_WIDTH + 10 + ((30 + 2) * 4) + 10, -32 ), new Vector2( 62, 30 ) ), null );

                if( toolType == typeof( PickTool ) )
                {
                    UIButton buttonSnap = _toolOptionsPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_snap" ), null );
                    UIButton buttonSnapNum = _toolOptionsPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 32, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
                }
                if( toolType == typeof( TranslateTool ) )
                {
                    UIText t = _toolOptionsPanel.AddText( UILayoutInfo.Fill(), "translate" );
                }
                if( toolType == typeof( RotateTool ) )
                {
                    UIText t = _toolOptionsPanel.AddText( UILayoutInfo.Fill(), "rotate" );
                }
                if( toolType == typeof( RerootTool ) )
                {
                    UIText t = _toolOptionsPanel.AddText( UILayoutInfo.Fill(), "reroot" );
                }
            }
        }

        private static void CreateWindowToggleList( UICanvas canvas )
        {
            UIPanel uiPanel = canvas.AddPanel( new UILayoutInfo( new Vector2( 1, 0 ), Vector2.zero, new Vector2( 100, 30 ) ), null );
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