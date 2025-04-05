using HSP.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization;

namespace HSP.Timelines
{
    public interface ITimelineSettingsPage : ISettingsPage
    {

    }

    /// <summary>
    /// Provides the editable 'scenario settings' - with values that are editable after starting a timeline using the scenario. <br/>
    /// These settings are the same across a timeline.
    /// </summary>
    public sealed class TimelineSettingsProvider : ISettingsProvider
    {
        public const string SETTINGS_FILENAME = "timeline_settings.json";

        public IEnumerable<Type> GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<ITimelineSettingsPage>( t ) );
        }

        private string GetSettingsFilePath()
        {
            return Path.Combine( TimelineManager.CurrentTimeline.GetRootDirectory(), SETTINGS_FILENAME );
        }

        public bool IsAvailable()
        {
            return TimelineManager.CurrentTimeline != null;
        }

        public SettingsFile LoadSettings()
        {
            string path = GetSettingsFilePath();

            if( File.Exists( path ) )
            {
                try
                {
                    SerializedData data = new JsonSerializedDataHandler( path )
                        .Read();

                    return SerializationUnit.Deserialize<SettingsFile>( data );
                }
                catch( Exception ex )
                {
                    File.Copy( path, path.Replace( ".json", "_loadingfailed.json" ), true );
                    throw ex;
                }
            }

            return SettingsFile.Empty;
        }

        public void SaveSettings( SettingsFile settings )
        {
            SerializedData data = SerializationUnit.Serialize( settings );

            new JsonSerializedDataHandler( GetSettingsFilePath() )
                .Write( data );
        }
    }
}