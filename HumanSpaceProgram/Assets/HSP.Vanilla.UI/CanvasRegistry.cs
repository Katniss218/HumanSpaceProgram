using HSP.SceneManagement;
using HSP.UI.Canvases;
using UnityPlus.UILib;

namespace HSP.Vanilla.UI
{
    public static class CanvasRegistry
    {
        // Main (general-purpose) canvases

        public static UIBackgroundCanvas GetBackgroundCanvas( this IHSPScene scene )
        {
            return CanvasManager.GetOrCreate<UIBackgroundCanvas>( scene.UnityScene, "background" );
        }
        public static UIStaticCanvas GetStaticCanvas( this IHSPScene scene )
        {
            return CanvasManager.GetOrCreate<UIStaticCanvas>( scene.UnityScene, "static" );
        }
        public static UIWindowCanvas GetWindowCanvas( this IHSPScene scene )
        {
            return CanvasManager.GetOrCreate<UIWindowCanvas>( scene.UnityScene, "windows" );
        }

        // Special canvases

        public static UIConsoleCanvas GetConsoleCanvas()
        {
            return CanvasManager.GetOrCreate<UIConsoleCanvas>( "sp.console" );
        }
        public static UICursorCanvas GetCursorCanvas()
        {
            return CanvasManager.GetOrCreate<UICursorCanvas>( "sp.cursor" );
        }
        public static UIContextMenuCanvas GetContextMenuCanvas()
        {
            return CanvasManager.GetOrCreate<UIContextMenuCanvas>( "context_menus" );
        }
    }
}