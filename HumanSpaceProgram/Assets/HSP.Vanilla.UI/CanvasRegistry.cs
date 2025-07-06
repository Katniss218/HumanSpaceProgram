using HSP.SceneManagement;
using HSP.UI.Canvases;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI
{
    public static class CanvasRegistry
    {
        public static UICanvas GetBackgroundCanvas( this IHSPScene scene )
        {
            return CanvasManager.GetOrCreate<UIBackgroundCanvas>( scene.UnityScene, "background" );
        }
        public static UICanvas GetStaticCanvas( this IHSPScene scene )
        {
            return CanvasManager.GetOrCreate<UIStaticCanvas>( scene.UnityScene, "static" );
        }
        public static UICanvas GetWindowCanvas( this IHSPScene scene )
        {
            return CanvasManager.GetOrCreate<UIWindowCanvas>( scene.UnityScene, "windows" );
        }

        public static UICanvas GetConsoleCanvas()
        {
            return CanvasManager.GetOrCreate<UIConsoleCanvas>( "sp.console" );
        }
        public static UICanvas GetCursorCanvas()
        {
            return CanvasManager.GetOrCreate<UIConsoleCanvas>( "sp.cursor" );
        }
        public static UICanvas GetContextMenuCanvas()
        {
            return CanvasManager.GetOrCreate<UIConsoleCanvas>( "context_menus" );
        }
    }
}