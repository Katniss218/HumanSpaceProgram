using System;

namespace HSP.Settings
{
    /// <summary>
    /// Represents an arbitrary settings page.
    /// </summary>
    /// <remarks>
    /// NOTE TO MODDERS/IMPLEMENTERS: <br/>
    /// Use the <see cref="SettingsPage{T}"/> class as a base class for custom settings pages, as it auto-sets the singleton instance when applied (making it easy to retrieve the current settings).
    /// </remarks>
    public interface ISettingsPage
    {
        /// <summary>
        /// Applies this settings page's parameters to external members. Effectively sets this page as the 'in use' page.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS: <br/>
        /// - This method should only apply members that need to be set somewhere externally, like Unity graphics settings, keybind mappings, etc.
        /// </remarks>
        void Apply();

        public static ISettingsPage CreateDefaultPage( Type type )
        {
            return (ISettingsPage)Activator.CreateInstance( type );
        }

        public static T CreateDefaultPage<T>() where T : ISettingsPage
        {
            return (T)CreateDefaultPage( typeof( T ) );
        }
    }
}