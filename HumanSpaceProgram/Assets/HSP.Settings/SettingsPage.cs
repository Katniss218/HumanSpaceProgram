using UnityPlus.Serialization;

namespace HSP.Settings
{
    public abstract class SettingsPage<T> : ISettingsPage where T : SettingsPage<T>
    {
        // reset to default is accomplished via the default values in each page (instantiates a new instance of the page).

        public static T Instance { get; protected set; }
        internal static SerializedData data;

        // each page needs to be able to apply - change the underlying unity variables, as well as apply - make itself the singleton

        /// <summary>
        /// Implement this method to send the settings to an external member, like e.g. UnityEngine.Application.targetFramerate.
        /// </summary>
        /// <returns>The instance that was being invoked.</returns>
        protected abstract T OnApply();

        /// <summary>
        /// Applies the current page as the working page.
        /// </summary>
        public void Apply()
        {
            T page = OnApply();
            Instance = page;
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