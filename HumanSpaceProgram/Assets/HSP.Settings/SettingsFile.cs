using System;
using System.Collections.Generic;
using System.Linq;
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

        public static SettingsFile Empty => new SettingsFile();

        /// <summary>
        /// Creates a new settings file and populates it with default values.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when one or more of the types don't implement <see cref="ISettingsPage"/>.</exception>
        public static SettingsFile FromPageTypes( Type[] types )
        {
            SettingsFile settingsFile = new();

            settingsFile.Pages = new List<ISettingsPage>( types.Length );
            foreach( var type in types )
            {
                if( !typeof( ISettingsPage ).IsAssignableFrom( type ) )
                {
                    throw new ArgumentException( $"All types must implement '{nameof( ISettingsPage )}'.", nameof( type ) );
                }

                var page = (ISettingsPage)Activator.CreateInstance( type );
                settingsFile.Pages.Add( page );
            }

            return settingsFile;
        }

        public void FillMissingTypes( Type[] types )
        {
            foreach( var type in types )
            {
                if( !this.Pages.Select( p => p.GetType() ).Contains( type ) )
                {
                    var page = (ISettingsPage)Activator.CreateInstance( type );
                    this.Pages.Add( page );
                }
            }
        }

        [MapsInheritingFrom( typeof( SettingsFile ) )]
        public static SerializationMapping SettingsFileMapping()
        {
            return new MemberwiseSerializationMapping<SettingsFile>()
                .WithMember( "pages", o => o.Pages );
        }
    }
}