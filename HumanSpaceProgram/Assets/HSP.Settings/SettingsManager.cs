﻿using System;
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
        private class Entry
        {
            public ISettingsProvider provider;
            public bool wasAvailableDuringLastReload;
        }

        private static Entry[] _settingsProviders;

        private static Type[] GetProviderTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => !t.IsAbstract && !t.IsInterface && typeof( ISettingsProvider ).IsAssignableFrom( t ) ).ToArray();
        }

        private static void ReloadProviders()
        {
            Type[] types = GetProviderTypes();

            _settingsProviders = new Entry[types.Length];

            for( int i = 0; i < types.Length; i++ )
            {
                var provider = new Entry()
                {
                    provider = (ISettingsProvider)Activator.CreateInstance( types[i] ),
                    wasAvailableDuringLastReload = false
                };

                _settingsProviders[i] = provider;
            }
        }

        private static IEnumerable<Entry> GetProviders()
        {
            if( _settingsProviders == null )
            {
                ReloadProviders();
            }

            return _settingsProviders;
        }

        /// <summary>
        /// Reloads the current settings using the available settings providers.
        /// </summary>
        public static void ReloadSettings()
        {
            foreach( var provider in GetProviders() )
            {
                ReloadSettings( provider );
            }
        }

        /// <summary>
        /// Reloads the current settings of a given settings provider from disk.
        /// </summary>
        private static void ReloadSettings( Entry providerEntry )
        {
            ISettingsProvider provider = providerEntry.provider;

            Type[] pageTypes = provider.GetPageTypes().ToArray();

            SettingsFile settingsFile;
            bool saveToDisk = false;
            if( provider.IsAvailable() )
            {
                try
                {
                    settingsFile = provider.LoadSettings();
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Exception occurred while trying to load the settings file for provider '{provider}'." );
                    Debug.LogException( ex );

                    settingsFile = SettingsFile.FromPageTypes( pageTypes );
                    saveToDisk = true;
                }

                providerEntry.wasAvailableDuringLastReload = true;

                if( settingsFile == null || settingsFile.Pages == null )
                {
                    settingsFile = SettingsFile.FromPageTypes( pageTypes );
                    saveToDisk = true;
                }
            }
            else
            {
                // Force apply default values for providers that are no longer available.
                // Ideally providers would be ensure this is called as soon as the provider goes unavailable.

                // This is done to ensure that after leaving a scenario, the settings from it don't linger in the game.

                settingsFile = SettingsFile.FromPageTypes( pageTypes );
            }

            settingsFile.FillMissingTypes( pageTypes );

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

            if( saveToDisk ) // if something went wrong, save a default version of the file.
            {
                SaveSettings( providerEntry );
            }
        }

        /// <summary>
        /// Saves the current settings to disk.
        /// </summary>
        public static void SaveSettings()
        {
            foreach( var provider in GetProviders() )
            {
                SaveSettings( provider );
            }
        }

        /// <summary>
        /// Saves the provider associated with the given page type.
        /// </summary>
        /// <typeparam name="TSettingsPage">The type of the settings page to save.</typeparam>
        public static void SaveSettings<TSettingsPage>() where TSettingsPage : ISettingsPage
        {
            foreach( var provider in GetProviders() )
            {
                if( provider.provider.GetPageTypes().Contains( typeof( TSettingsPage ) ) )
                {
                    SaveSettings( provider, true );
                    return;
                }
            }
        }

        /// <summary>
        /// Saves the current settings of a given settings provider to disk.
        /// </summary>
        private static void SaveSettings( Entry providerEntry, bool force = false )
        {
            if( !force && !providerEntry.wasAvailableDuringLastReload )
                return;

            ISettingsProvider provider = providerEntry.provider;
            if( !provider.IsAvailable() )
                return;

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