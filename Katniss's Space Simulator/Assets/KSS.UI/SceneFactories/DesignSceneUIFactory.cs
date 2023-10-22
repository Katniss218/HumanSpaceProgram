using KSS.Core;
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
            CreateSaveLoadButtons( canvas );
            CreateGizmoToggles( canvas );
        }

        const float PART_LIST_WIDTH = 250f;

        private static void CreatePartList( UICanvas canvas )
        {
            PartListUI partListUI = PartListUI.Create( canvas, UILayoutInfo.FillVertical( 0, 0, 0f, 0, PART_LIST_WIDTH ) );
        }

        private static void CreateSaveLoadButtons( UICanvas canvas )
        {

        }

        private static void CreateGizmoToggles( UICanvas canvas )
        {
            UIPanel uiPanel = canvas.AddPanel( new UILayoutInfo( new Vector2( 0, 1 ), new Vector2( PART_LIST_WIDTH, -100 ), new Vector2( 160, 40 ) ), null );

            UIButton pickButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 0, 0 ), new Vector2( 40, 40 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_pick" ), null );
            UIButton moveButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 40, 0 ), new Vector2( 40, 40 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_move" ), null );
            UIButton rotateButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 80, 0 ), new Vector2( 40, 40 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_rotate" ), null );
            UIButton rerootButton = uiPanel.AddButton( new UILayoutInfo( Vector2.zero, new Vector2( 120, 0 ), new Vector2( 40, 40 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/icon_reroot" ), null );
        }
    }
}