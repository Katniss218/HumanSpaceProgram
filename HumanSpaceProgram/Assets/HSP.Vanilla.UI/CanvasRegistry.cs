using HSP.SceneManagement;
using HSP.UI.Canvases;
using UnityPlus.UILib;

namespace HSP.Vanilla.UI
{
    public static class CanvasRegistry
    {
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

        public static UIConsoleCanvas GetConsoleCanvas()
        {
            return CanvasManager.GetOrCreate<UIConsoleCanvas>( "sp.console" );
        }
        public static UIConsoleCanvas GetCursorCanvas()
        {
            return CanvasManager.GetOrCreate<UIConsoleCanvas>( "sp.cursor" );
        }
        public static UIConsoleCanvas GetContextMenuCanvas()
        {
            return CanvasManager.GetOrCreate<UIConsoleCanvas>( "context_menus" );
        }
    }
}