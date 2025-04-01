using System;
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

        public static SettingsFile FromPageTypes( Type[] types )
        {
            SettingsFile settingsFile = new();

            settingsFile.Pages = new List<ISettingsPage>( types.Length );
            foreach( var type in types )
            {
                if( !typeof( ISettingsPage ).IsAssignableFrom( type ) )
                {
                    throw new ArgumentException( $"All types must derive from '{nameof( ISettingsPage )}'.", nameof( type ) );
                }

                var page = (ISettingsPage)Activator.CreateInstance( type );
                settingsFile.Pages.Add( page );
            }

            return settingsFile;
        }

        [MapsInheritingFrom( typeof( SettingsFile ) )]
        public static SerializationMapping SettingsFileMapping()
        {
            return new MemberwiseSerializationMapping<SettingsFile>()
                .WithMember( "pages", o => o.Pages );
        }
    }
}