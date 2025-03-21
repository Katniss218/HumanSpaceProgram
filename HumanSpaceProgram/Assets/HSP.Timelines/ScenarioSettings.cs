using HSP.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.Timelines
{
    public interface IScenarioSettingsPage : ISettingsPage
    {

    }

    /// <summary>
    /// Provides the immutable 'scenario settings' - with values defined by the scenario and unchangeable later.
    /// These settings are the same for any timeline started from the same scenario.
    /// </summary>
    public sealed class ScenarioSettingsProvider// : ISettingsProvider
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
}