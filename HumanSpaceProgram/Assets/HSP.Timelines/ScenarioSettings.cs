using HSP.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization;

namespace HSP.Timelines
{
    public interface IScenarioSettingsPage : ISettingsPage
    {

    }

    /// <summary>
    /// Provides the immutable 'scenario settings' - with values defined by the scenario and unchangeable later.
    /// These settings are the same for any timeline started from the same scenario.
    /// </summary>
    public sealed class ScenarioSettingsProvider : ISettingsProvider
    {
        public const string SETTINGS_FILENAME = "scenario_settings.json";

        public IEnumerable<Type> GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<IScenarioSettingsPage>( t ) );
        }

        private string GetSettingsFilePath()
        {
            return Path.Combine( TimelineManager.CurrentScenario.GetRootDirectory(), SETTINGS_FILENAME );
        }

        public bool IsAvailable()
        {
            return TimelineManager.CurrentScenario != null;
        }

        public SettingsFile LoadSettings()
        {
            string path = GetSettingsFilePath();

            if( File.Exists( path ) )
            {
                SerializedData data = new JsonSerializedDataHandler( path )
                    .Read();

                return SerializationUnit.Deserialize<SettingsFile>( data );
            }
            //File.Copy( path, path.Replace( ".json", "_loadingfailed.json" ), true );
            return null;
        }

        public void SaveSettings( SettingsFile settings )
        {
            SerializedData data = SerializationUnit.Serialize( settings );

            new JsonSerializedDataHandler( GetSettingsFilePath() )
                .Write( data );
        }
    }
}