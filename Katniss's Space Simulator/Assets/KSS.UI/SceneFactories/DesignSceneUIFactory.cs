using KSS.Core;
using KSS.Core.SceneManagement;
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
        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_VANILLA + ".design_scene_ui" )]
        public static void Create( object e )
        {
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            CreatePartList( canvas );
            CreateTopPanel( canvas );
            CreateToolSelector( canvas );
            CreatePickToolOptions( canvas );
        }

        const float PART_LIST_WIDTH = 285f;

        private static void CreatePartList( UICanvas canvas )
        {
            PartListUI partListUI = PartListUI.Create( canvas, UILayoutInfo.FillVertical( 30, 0, 0f, 0, PART_LIST_WIDTH ) );
        }

        private static void CreateTopPanel( UICanvas canvas )
        {
            UIPanel topPanel = canvas.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, 1f, 0, 30 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/top_panel_background" ) );
            UIPanel p1 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 0f, 20, 110 ), null );
            UIButton newBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), null );
            UIButton openBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 40, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_open" ), null );
            UIButton saveBtn = p1.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 80, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_save" ), null );
            UIPanel p2 = topPanel.AddPanel( UILayoutInfo.FillVertical( 0, 0, 0.5f, 0, 300 ), null );
            UIInputField nameInputField = p2.AddInputField( UILayoutInfo.FillVertical( 0, 0, 0.5f, 0, 300 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) )
                .WithMargins( 5, 5, 5, 5 )
                .WithPlaceholder( "vessel's name..." );
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

            UIButton pickButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_pick" ), null );
            UIButton moveButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 32, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_move" ), null );
            UIButton rotateButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 64, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_rotate" ), null );
            UIButton rerootButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 96, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/toolicon_reroot" ), null );
        }

        private static void CreatePickToolOptions( UICanvas canvas )
        {
            UIPanel uiPanel = canvas.AddPanel( new UILayoutInfo( new Vector2( 0, 1 ), new Vector2( PART_LIST_WIDTH + 10 + ((30 + 2) * 4) + 10, -32 ), new Vector2( 62, 30 ) ), null );

            UIButton buttonSnap = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_snap" ), null );
            UIButton buttonSnapNum = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 32, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );
        }
    }
}