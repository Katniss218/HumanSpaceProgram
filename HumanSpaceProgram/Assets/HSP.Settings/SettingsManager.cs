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

        public static bool IsValidSettingsPage<T>( Type t ) where T : ISettingsPage
        {
            return !t.IsAbstract && !t.IsInterface && typeof( T ).IsAssignableFrom( t );
        }
    }



    public interface IGameSettingsPage : ISettingsPage
    {
        // global settings - this is not in HSP.Settings either.
        // move HumanSpaceProgramGameSettings with it.
    }

    public sealed class GameSettingsProvider : ISettingsProvider
    {
        public const string SettingsFileName = "settings.json";

        public IEnumerable<Type> GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<IGameSettingsPage>( t ) );
        }

        public string GetSettingsFilePath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), SettingsFileName );

            return path;
        }
    }



    public interface IScenarioSettingsPage : ISettingsPage
    {
        // HSP.Timelines is not basal, we can include settings in it.
    }

    public interface ITimelineSettingsPage : ISettingsPage
    {
        // HSP.Timelines is not basal, we can include settings in it.
    }

    /// <summary>
    /// Fixed scenario settings
    /// </summary>
    public sealed class ScenarioSettingsProvider : ISettingsProvider
    {
        public IEnumerable<Type> GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<IScenarioSettingsPage>( t ) );
        }

        public string GetSettingsFilePath()
        {
            // more complex, use the current scenario and take the default settings from it.
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 'scenario' settings that are editable after the scenario has started.
    /// </summary>
    public sealed class TimelineSettingsProvider : ISettingsProvider
    {
        public IEnumerable<Type> GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<ITimelineSettingsPage>( t ) );
        }

        public string GetSettingsFilePath()
        {
            // more complex, use the current timeline and take the settings from it.
            throw new NotImplementedException();
        }
    }



    public static class SettingsManager
    {
        private static List<ISettingsProvider> _settingsProviders = new();

        public static bool TryRegisterProvider( ISettingsProvider provider )
        {
            Type providerType = provider.GetType();

            if( _settingsProviders.Any( p => p.GetType() == providerType ) )
                return false;

            _settingsProviders.Add( provider );
            return true;
        }

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

        /// <summary>
        /// Reloads the current settings from disk.
        /// </summary>
        public static void ReloadSettings()
        {
            foreach( var provider in _settingsProviders )
            {
                ReloadSettings( provider );
            }
        }

        /// <summary>
        /// Reloads the current settings from disk.
        /// </summary>
        private static void ReloadSettings( ISettingsProvider provider )
        {
            string path = provider.GetSettingsFilePath();
            SettingsFile arr;

            Type[] pageTypes = provider.GetPageTypes().ToArray();

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
            foreach( var provider in _settingsProviders )
            {
                SaveSettings( provider );
            }
        }

        /// <summary>
        /// Saves the current settings to disk.
        /// </summary>
        private static void SaveSettings( ISettingsProvider provider )
        {
            Type[] pageTypes = provider.GetPageTypes().ToArray();

            SettingsFile arr = new SettingsFile();
            arr.Pages = new List<ISettingsPage>( pageTypes.Length );
            foreach( var type in pageTypes )
            {
                PropertyInfo instanceProperty = type.GetProperty( "Current", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy );
                ISettingsPage page = (ISettingsPage)instanceProperty.GetValue( null );
                arr.Pages.Add( page );
            }

            JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( provider.GetSettingsFilePath() );

            var data = SerializationUnit.Serialize( arr );
            dataHandler.Write( data );
        }
    }
}