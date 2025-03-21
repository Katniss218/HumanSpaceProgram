using System.Collections.Generic;
using UnityPlus.Serialization;

namespace HSP.Settings
{
    /// <summary>
    /// The object representation of a settings file.
    /// </summary>
    internal class SettingsFile
    {
        public List<ISettingsPage> Pages;

        [MapsInheritingFrom( typeof( SettingsFile ) )]
        public static SerializationMapping SettingsPage_GraphicsMapping()
        {
            return new MemberwiseSerializationMapping<SettingsFile>()
                .WithMember( "pages", o => o.Pages );
        }
    }
}