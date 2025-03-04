using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Settings
{
    public static class SettingsManager
    {
        private static SettingsFile CreateNewSettings( Type[] types )
        {
            SettingsFile arr = new();
            arr.Pages = new List<ISettingsPage>( types.Length );
            for( int i = 0; i < types.Length; i++ )
            {
                var page = (ISettingsPage)Activator.CreateInstance( types[i] );
                arr.Pages.Add( page );
            }
            return arr;
        }

        private static Type[] GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => !t.IsAbstract && t != typeof( ISettingsPage ) && typeof( ISettingsPage ).IsAssignableFrom( t ) ).ToArray();
        }

        /// <summary>
        /// Reloads the current settings from disk.
        /// </summary>
        public static void ReloadSettings()
        {
            string path = HumanSpaceProgramSettings.GetSettingsFilePath();
            SettingsFile arr;

            Type[] pageTypes = GetPageTypes();

            bool errorOrMissingFile = false;
            if( File.Exists( path ) )
            {
                try
                {
                    var data = new JsonSerializedDataHandler( path ).Read();
                    arr = SerializationUnit.Deserialize<SettingsFile>( data );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Exception occurred while trying to load the settings file '{path}'." );
                    Debug.LogException( ex );

                    File.Copy( path, path.Replace( ".json", "_loadingfailed.json" ), true );

                    arr = CreateNewSettings( pageTypes );
                    errorOrMissingFile = true;
                }
            }
            else
            {
                arr = CreateNewSettings( pageTypes );
                errorOrMissingFile = true;
            }
            if( arr == null || arr.Pages == null )
                arr = CreateNewSettings( pageTypes );

            // Add any pages that were missing in the settings.json.
            foreach( var type in pageTypes )
            {
                if( !arr.Pages.Select( p => p.GetType() ).Contains( type ) )
                {
                    var page = (ISettingsPage)Activator.CreateInstance( type );
                    arr.Pages.Add( page );
                }
            }

            // apply the final settings.
            foreach( var page in arr.Pages )
            {
                try
                {
                    page.Apply();
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Exception occurred while trying to apply the settings page '{page.GetType().AssemblyQualifiedName}'." );
                    Debug.LogException( ex );
                }
            }

            if( errorOrMissingFile ) // if something went wrong, save a default version of the file.
            {
                SaveSettings();
            }
        }

        /// <summary>
        /// Saves the current settings to disk.
        /// </summary>
        public static void SaveSettings()
        {
            Type[] pageTypes = GetPageTypes();

            SettingsFile arr = new SettingsFile();
            arr.Pages = new List<ISettingsPage>( pageTypes.Length );
            foreach( var type in pageTypes )
            {
                PropertyInfo instanceProperty = type.GetProperty( "Current", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy );
                ISettingsPage page = (ISettingsPage)instanceProperty.GetValue( null );
                arr.Pages.Add( page );
            }

            JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( HumanSpaceProgramSettings.GetSettingsFilePath() );

            var data = SerializationUnit.Serialize( arr );
            dataHandler.Write( data );
        }
    }
}