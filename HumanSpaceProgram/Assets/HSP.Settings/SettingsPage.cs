using UnityPlus.Serialization;

namespace HSP.Settings
{
    /// <summary>
    /// An arbitrary settings page. <br/>
    /// Inherit from this class like this: `CustomPage : SettingsPage<![CDATA[<]]>CustomPage<![CDATA[>]]>` to have a custom singleton settings page.
    /// </summary>
    /// <typeparam name="T">The inheriting singleton type.</typeparam>
    public abstract class SettingsPage<T> : ISettingsPage where T : SettingsPage<T>
    {
        // Reset to default values is accomplished via the default values (it instantiates a new instance of the page).

        /// <summary>
        /// Gets the currently used settings values for this page.
        /// </summary>
        public static T Current { get; protected set; }

        internal static SerializedData data;

        /// <summary>
        /// Implement this method to send the settings to an external member, like e.g. UnityEngine.Application.targetFramerate.
        /// </summary>
        /// <returns>The instance that was being invoked.</returns>
        protected abstract T OnApply();

        /// <summary>
        /// Applies this page as the current working page.
        /// </summary>
        public void Apply()
        {
            T page = OnApply();
            Current = page;
            data = SerializationUnit.Serialize<T>( page );
            SettingsManager.SaveSettings();
        }

        /// <summary>
        /// Gets a fresh settings page that hasn't been applied yet.
        /// </summary>
        /// <remarks>
        /// The fields in the page will reflect the past applied page.
        /// </remarks>
        public static T GetUnlinkedLastAppliedPage()
        {
            // gets a deep copy of the page.
            return SerializationUnit.Deserialize<T>( SettingsPage<T>.data );
        }
    }
}