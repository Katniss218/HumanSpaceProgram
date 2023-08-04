namespace KSS.UI
{
    public static class CanvasManager
    {
        /// <summary>
        /// The canvas for the console overlay. Always on top.
        /// </summary>
        public const string CONSOLE = "sp.console";

        /// <summary>
        /// The canvas for objects held by the cursor. Rendered above the static canvases.
        /// </summary>
        public const string CURSOR = "sp.cursor";

        /// <summary>
        /// The canvas for context menus/windows. <br/>
        /// Rendered above the <see cref="WINDOWS"/>.
        /// </summary>
        public const string CONTEXT_MENUS = "context_menus";

        /// <summary>
        /// The primary canvas for windows, popups, panels, etc. <br/>
        /// Rendered above the <see cref="STATIC"/>.
        /// </summary>
        public const string WINDOWS = "windows";

        /// <summary>
        /// The primary canvas for background UI elements.
        /// </summary>
        public const string STATIC = "static";
    }
}