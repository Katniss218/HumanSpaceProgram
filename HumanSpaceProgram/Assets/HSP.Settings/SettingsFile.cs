using System.Collections.Generic;
using System.Linq;
using UnityPlus.Serialization;

namespace HSP.Settings
{
    /// <summary>
    /// data object representation of the settings file.
    /// </summary>
    internal class SettingsFile
    {
        public List<ISettingsPage> Pages;

        [MapsInheritingFrom( typeof( SettingsFile ) )]
        public static SerializationMapping SettingsPage_GraphicsMapping()
        {
            return new MemberwiseSerializationMapping<SettingsFile>()
            {
                ("pages", new Member<SettingsFile, ISettingsPage[]>( o => o.Pages?.ToArray(), (o, value) => o.Pages = value?.ToList() )),
            };
        }
    }
}