using System;
using System.Collections.Generic;

namespace HSP.Settings
{
    /// <summary>
    /// Represents a class that provides some collection of settings pages.
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Gets the types of all of the settings pages that should be associated with this provider.
        /// </summary>
        /// <remarks>
        /// This method should always return the same set of types. <br/>
        /// A given page type should only be returned by a single <see cref="ISettingsProvider"/>.
        /// </remarks>
        public IEnumerable<Type> GetPageTypes();

        /// <summary>
        /// Checks whether the provider can be invoked.
        /// </summary>
        /// <remarks>
        /// This is useful for providers that are 'valid' only in specific circumstances, <br/>
        /// like timeline settings only being applicable when a timeline is loaded/active.
        /// </remarks>
        public bool IsAvailable(); // The provider must also not be saved before it is loaded (if the method is true for save but was false for preceeding load - that would force-overwrite everything with defaults),
                                   //   so the state when 'load' is called must be cached in the SettingsManager

        /// <summary>
        /// Loads the settings data.
        /// </summary>
        /// <returns>A structure containing the deserialized settings pages, excluding any missing ones.</returns>
        public SettingsFile LoadSettings();

        /// <summary>
        /// Saves the provided settings data.
        /// </summary>
        /// <param name="settings">A structure containing the settings pages to be serialized.</param>
        public void SaveSettings( SettingsFile settings );

        public static bool IsValidSettingsPage<T>( Type type ) where T : ISettingsPage
        {
            return !type.IsAbstract && !type.IsInterface && typeof( T ).IsAssignableFrom( type );
        }
    }
}