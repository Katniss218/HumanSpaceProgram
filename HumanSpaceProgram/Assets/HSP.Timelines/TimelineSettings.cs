using HSP.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization;
using HSP.Timelines.Serialization;

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

        public static IEnumerable<ISettingsPage> GetDefaultPages()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<ITimelineSettingsPage>( t ) );

            return types.Select( t => ISettingsPage.CreateDefaultPage( t ) );
        }

        public IEnumerable<Type> GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<ITimelineSettingsPage>( t ) );
        }

        private static string GetSettingsFilePath( TimelineMetadata timeline )
        {
            return Path.Combine( timeline.GetRootDirectory(), SETTINGS_FILENAME );
        }

        public bool IsAvailable()
        {
            return TimelineManager.CurrentTimeline != null;
        }

        public static SettingsFile LoadSettings( TimelineMetadata timeline )
        {
            string path = GetSettingsFilePath( timeline );

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

        public SettingsFile LoadSettings()
        {
            return LoadSettings( TimelineManager.CurrentTimeline );
        }

        public static void SaveSettings( TimelineMetadata timeline, SettingsFile settings )
        {
            SerializedData data = SerializationUnit.Serialize( settings );

            new JsonSerializedDataHandler( GetSettingsFilePath( timeline ) )
                .Write( data );
        }

        public void SaveSettings( SettingsFile settings )
        {
            SaveSettings( TimelineManager.CurrentTimeline, settings );
        }
    }
}