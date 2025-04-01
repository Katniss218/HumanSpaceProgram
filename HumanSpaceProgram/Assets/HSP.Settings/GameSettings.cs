using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization;

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

        private string GetSettingsFilePath()
        {
            return Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), SETTINGS_FILENAME );
        }

        public bool IsAvailable() => true;

        public SettingsFile LoadSettings()
        {
            string path = GetSettingsFilePath();

            if( File.Exists( path ) )
            {
                SerializedData data = new JsonSerializedDataHandler( path )
                    .Read();

                return SerializationUnit.Deserialize<SettingsFile>( data );
            }
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