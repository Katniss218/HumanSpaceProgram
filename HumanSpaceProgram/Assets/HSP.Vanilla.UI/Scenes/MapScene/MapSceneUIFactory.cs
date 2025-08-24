using HSP.Vanilla.Scenes.MapScene;
using UnityPlus.AssetManagement;
using UnityPlus.UILib.UIElements;
using UnityPlus.UILib;
using HSP.UI;
using UnityEngine;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    internal class MapSceneUIFactory
    {
        public const string CREATE_UI = HSPEvent.NAMESPACE_HSP + ".map_scene.ui.create";
        public const string DESTROY_UI = HSPEvent.NAMESPACE_HSP + ".map_scene.ui.destroy";

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, CREATE_UI )]
        private static void Create()
        {
#warning TODO - sorting, ephemeris behind huds.
            MapSceneM.GameObject.AddComponent<EphemerisDrawer>();
            CreateFPSPanel( MapSceneM.Instance.GetBackgroundCanvas() );
        }
        static void CreateFPSPanel( IUIElementContainer parent )
        {
            UIPanel fpsPanel = parent.AddPanel( new UILayoutInfo( UIAnchor.TopLeft, (5, -35), (80, 30) ), null );

            fpsPanel.AddTextReadout_FPS( new UILayoutInfo( UIFill.Fill() ) )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Left )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_DEACTIVATE.ID, DESTROY_UI )]
        private static void Destroy()
        {

            UnityEngine.Object.Destroy( MapSceneM.GameObject.GetComponent<EphemerisDrawer>());
        }
    }
}
