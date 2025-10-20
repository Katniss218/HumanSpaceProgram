using System;
using System.Collections.Generic;
using System.Linq;
using Version = HSP.Content.Version;

namespace HSP.Content.Mods
{
    /// <summary>
    /// Provides utilities for querying and managing loaded mods.
    /// </summary>
    public static class ModManager
    {
        /// <summary>
        /// Gets the metadata for a loaded mod.
        /// </summary>
        /// <param name="modId">The ID of the mod to retrieve</param>
        /// <returns>The mod metadata, or null if not loaded</returns>
        public static ModMetadata GetLoadedMod( string modId )
        {
            if( string.IsNullOrEmpty( modId ) )
                return null;

            return HumanSpaceProgramModLoader.LoadedMods.TryGetValue( modId, out ModMetadata mod ) ? mod : null;
        }

        /// <summary>
        /// Checks if a mod is currently loaded.
        /// </summary>
        /// <param name="modId">The ID of the mod to check</param>
        /// <returns>True if the mod is loaded</returns>
        public static bool IsModLoaded( string modId )
        {
            return !string.IsNullOrEmpty( modId ) && HumanSpaceProgramModLoader.LoadedMods.ContainsKey( modId );
        }

        /// <summary>
        /// Gets the current versions of all loaded mods.
        /// </summary>
        /// <returns>Dictionary mapping mod IDs to their current versions</returns>
        public static Dictionary<string, Version> GetCurrentModVersions()
        {
            return HumanSpaceProgramModLoader.LoadedMods.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.ModVersion );
        }

        /// <summary>
        /// Validates that all required mod versions are loaded and compatible.
        /// </summary>
        /// <param name="required">Dictionary of required mod versions</param>
        /// <returns>True if all required mods are loaded with compatible versions</returns>
        public static bool ValidateModVersions( Dictionary<string, Version> required )
        {
            if( required == null )
                return true;

            foreach( var kvp in required )
            {
                string modId = kvp.Key;
                Version requiredVersion = kvp.Value;

                if( !IsModLoaded( modId ) )
                {
                    return false;
                }

                ModMetadata loadedMod = GetLoadedMod( modId );
                if( loadedMod == null || loadedMod.ModVersion != requiredVersion )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets detailed information about mod version mismatches.
        /// </summary>
        /// <param name="required">Dictionary of required mod versions</param>
        /// <returns>List of mod version issues</returns>
        public static List<ModDependencyIssue> GetModCompatibilityIssues( Dictionary<string, Version> required )
        {
            List<ModDependencyIssue> issues = new List<ModDependencyIssue>();

            if( required == null )
                return issues;

            foreach( var kvp in required )
            {
                string modId = kvp.Key;
                Version requiredVersion = kvp.Value;

                if( !IsModLoaded( modId ) )
                {
                    issues.Add( new ModDependencyIssue( modId, ModDependencyIssueType.Missing, requiredVersion, null ) );
                    continue;
                }

                ModMetadata loadedMod = GetLoadedMod( modId );
                if( loadedMod.ModVersion != requiredVersion )
                {
                    issues.Add( new ModDependencyIssue( modId, ModDependencyIssueType.VersionMismatch, requiredVersion, loadedMod.ModVersion ) );
                }
            }

            return issues;
        }
    }
}