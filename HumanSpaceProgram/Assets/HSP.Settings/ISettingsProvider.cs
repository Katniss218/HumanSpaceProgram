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
        /// Gets the types of all of the settings pages of this provider.
        /// </summary>
        public IEnumerable<Type> GetPageTypes();

#warning TODO - replace by loadsettings / savesettings that returns a SettingsFile ?? - this would allow saving to network and stuff.
        /// <summary>
        /// Gets path to the file containing the settings.
        /// </summary>
        public string GetSettingsFilePath();

        public static bool IsValidSettingsPage<T>( Type type ) where T : ISettingsPage
        {
            return !type.IsAbstract && !type.IsInterface && typeof( T ).IsAssignableFrom( type );
        }
    }
}