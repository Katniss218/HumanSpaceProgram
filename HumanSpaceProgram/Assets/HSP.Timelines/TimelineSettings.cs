using HSP.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public string GetSettingsFilePath()
        {
            return Path.Combine( TimelineManager.CurrentTimeline.GetRootDirectory(), SETTINGS_FILENAME );
        }
    }
}