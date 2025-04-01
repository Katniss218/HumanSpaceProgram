using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HSP.Settings
{
    /// <summary>
    /// A class that can manage the settings from many different sources.
    /// </summary>
    public static class SettingsManager
    {
        private static ISettingsProvider[] _settingsProviders = null;

        private static void ReloadProviders()
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => !t.IsAbstract && !t.IsInterface && typeof( ISettingsProvider ).IsAssignableFrom( t ) ).ToArray();

            _settingsProviders = new ISettingsProvider[types.Length];

            for( int i = 0; i < types.Length; i++ )
            {
                var provider = (ISettingsProvider)Activator.CreateInstance( types[i] );
                _settingsProviders[i] = provider;
            }
        }

        private static IEnumerable<ISettingsProvider> GetAvailableProviders()
        {
            if( _settingsProviders == null )
            {
                ReloadProviders();
            }

            return _settingsProviders.Where( p => p.IsAvailable() );
        }

        /// <summary>
        /// Reloads the current settings using the available settings providers.
        /// </summary>
        public static void ReloadSettings()
        {
            foreach( var provider in GetAvailableProviders() )
            {
                ReloadSettings( provider );
            }
        }

        /// <summary>
        /// Reloads the current settings of a given settings provider from disk.
        /// </summary>
        private static void ReloadSettings( ISettingsProvider provider )
        {
            SettingsFile settingsFile;

            Type[] pageTypes = provider.GetPageTypes().ToArray();

            bool errorOrMissingFile = false;
            try
            {
                settingsFile = provider.LoadSettings();
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Exception occurred while trying to load the settings file for provider '{provider}'." );
                Debug.LogException( ex );

                //File.Copy( path, path.Replace( ".json", "_loadingfailed.json" ), true );

                settingsFile = SettingsFile.FromPageTypes( pageTypes );
                errorOrMissingFile = true;
            }

            if( settingsFile == null || settingsFile.Pages == null )
            {
                settingsFile = SettingsFile.FromPageTypes( pageTypes );
                errorOrMissingFile = true;
            }

            // Add any pages that were missing in the settings.json.
            foreach( var type in pageTypes )
            {
                if( !settingsFile.Pages.Select( p => p.GetType() ).Contains( type ) )
                {
                    var page = (ISettingsPage)Activator.CreateInstance( type );
                    settingsFile.Pages.Add( page );
                }
            }

            // apply the final settings.
            foreach( var page in settingsFile.Pages )
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
            foreach( var provider in GetAvailableProviders() )
            {
                SaveSettings( provider );
            }
        }

        /// <summary>
        /// Saves the current settings of a given settings provider to disk.
        /// </summary>
        private static void SaveSettings( ISettingsProvider provider )
        {
            Type[] pageTypes = provider.GetPageTypes().ToArray();

            SettingsFile settingsFile = new SettingsFile();
            settingsFile.Pages = new List<ISettingsPage>( pageTypes.Length );
            foreach( var type in pageTypes )
            {
                PropertyInfo instanceProperty = type.GetProperty( "Current", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy );
                ISettingsPage page = (ISettingsPage)instanceProperty.GetValue( null );
                settingsFile.Pages.Add( page );
            }

            provider.SaveSettings( settingsFile );
        }
    }
}