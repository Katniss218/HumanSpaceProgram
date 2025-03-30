using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HSP.Settings
{
    public interface IGameSettingsPage : ISettingsPage
    {
        // global settings - this is not in HSP.Settings either.
        // move HumanSpaceProgramGameSettings with it.
    }

    /// <summary>
    /// Provides the 'game settings' - a global set of settings that are the same across the game installation.
    /// </summary>
    public sealed class GameSettingsProvider : ISettingsProvider
    {
        public const string SETTINGS_FILENAME = "settings.json";

        public IEnumerable<Type> GetPageTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                .Where( t => ISettingsProvider.IsValidSettingsPage<IGameSettingsPage>( t ) );
        }

        public string GetSettingsFilePath()
        {
            return Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), SETTINGS_FILENAME );
        }
    }
}