using UnityPlus.Serialization;

namespace HSP.Settings
{
    /// <summary>
    /// An arbitrary settings page. <br/>
    /// Inherit from this class like this: `CustomPage : SettingsPage<![CDATA[<]]>CustomPage<![CDATA[>]]>` to have a custom singleton settings page.
    /// </summary>
    /// <remarks>
    /// REMEMBER TO IMPLEMENT THE SPECIFIC ISETTINGSPAGE FOR YOUR SETTINGS PAGE'S PROVIDER!!!
    /// </remarks>
    /// <typeparam name="T">The inheriting singleton type.</typeparam>
    public abstract class SettingsPage<T> : ISettingsPage where T : SettingsPage<T>
    {
        // 'Reset to default' can be accomplished via the C# default values (instantiate a new instance of the page).

        /// <summary>
        /// Gets the currently used settings values for this page.
        /// </summary>
        public static T Current { get; protected set; }

        internal static SerializedData data;

        /// <summary>
        /// Applies this page as the current working page.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS: <br/>
        /// - This method should only apply members that need to be set somewhere externally, like Unity graphics settings, keybind mappings, etc.
        /// </remarks>
        /// <returns>The instance `this` that was being invoked.</returns>
        protected abstract T OnApply();

        public void Apply()
        {
            T page = OnApply();
            Current = page;
            data = SerializationUnit.Serialize<T>( page );
            SettingsManager.SaveSettings(); // We can only save a specific provider to disk, not individual pages themselves, and this is okay.
                                            // It would be nice to be able to tell if the provider for this page is available though.
                                            // - This can be done by checking which types are returned by which provider.
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