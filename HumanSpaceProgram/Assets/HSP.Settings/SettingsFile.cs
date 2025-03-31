using System.Collections.Generic;
using UnityPlus.Serialization;

namespace HSP.Settings
{
    /// <summary>
    /// The object representation of a settings file.
    /// </summary>
    public class SettingsFile
    {
        /// <summary>
        /// The settings page instances in this file, with their current values.
        /// </summary>
        public List<ISettingsPage> Pages;

        [MapsInheritingFrom( typeof( SettingsFile ) )]
        public static SerializationMapping SettingsFileMapping()
        {
            return new MemberwiseSerializationMapping<SettingsFile>()
                .WithMember( "pages", o => o.Pages );
        }
    }
}