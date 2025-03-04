
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
        /// Applies this page as the current working page.
        /// </summary>
        void Apply();
    }
}