namespace HSP.UI
{
    /// <summary>
    /// A class containing IDs to use with <see cref="UnityPlus.UILib.CanvasManager.Get(string)"/>.
    /// </summary>
    public static class CanvasName
    {
        /// <summary>
        /// The (special) canvas for the console overlay. Renders on top of everything else.
        /// </summary>
        public const string CONSOLE = "sp.console";

        /// <summary>
        /// The (special) canvas for objects held by the cursor. <br />
        /// Renders on top of <see cref="CONTEXT_MENUS"/>.
        /// </summary>
        public const string CURSOR = "sp.cursor";

        /// <summary>
        /// The canvas for context menus/windows. <br />
        /// Renders on top of <see cref="WINDOWS"/>.
        /// </summary>
        public const string CONTEXT_MENUS = "context_menus";

        /// <summary>
        /// The primary canvas for popup windows. <br />
        /// Renders on top of <see cref="STATIC"/>.
        /// </summary>
        public const string WINDOWS = "windows";

        /// <summary>
        /// The primary canvas for most static UI elements. <br />
        /// Renders on top of <see cref="BACKGROUND"/>.
        /// </summary>
        public const string STATIC = "static";

        /// <summary>
        /// The primary canvas for background UI elements. <br />
        /// Renders behind everything else.
        /// </summary>
        public const string BACKGROUND = "background";
    }
}