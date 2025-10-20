using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HSP.Content.Mods
{
    /// <summary>
    /// Handles loading and validation of mods for HSP.
    /// </summary>
    public static class HumanSpaceProgramModLoader
    {
        private static Dictionary<string, ModMetadata> _loadedMods = new Dictionary<string, ModMetadata>();

        /// <summary>
        /// Gets all currently loaded mods.
        /// </summary>
        public static IReadOnlyDictionary<string, ModMetadata> LoadedMods => _loadedMods;

        private static void LoadAssembliesRecursive( string path )
        {
            foreach( var dllPath in Directory.GetFiles( path, "*.dll" ) )
            {
                try
                {
                    byte[] assemblyBytes = File.ReadAllBytes( dllPath );
                    Assembly.Load( assemblyBytes );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load assembly '{dllPath}': {ex.Message}" );
                    Debug.LogException( ex );
                }
            }

            foreach( var subfolder in Directory.GetDirectories( path ) )
            {
                LoadAssembliesRecursive( subfolder );
            }
        }

        /// <summary>
        /// Loads all mods and their assemblies from the GameData directory.
        /// </summary>
        [HSPEventListener( HSPEvent_STARTUP_LOAD_MOD_ASSEMBLIES.ID, HSPEvent.NAMESPACE_HSP + ".load_mod_assemblies" )]
        private static void LoadModAssemblies()
        {
            string gameDataDirectory = HumanSpaceProgramContent.GetContentDirectoryPath();

            if( !Directory.Exists( gameDataDirectory ) )
                Directory.CreateDirectory( gameDataDirectory );

            Debug.Log( $"The content directory is: '{gameDataDirectory}'" );

            // Discover and load all mod metadata
            Dictionary<string, ModMetadata> discoveredMods = new Dictionary<string, ModMetadata>();
            List<string> modDirectories = new List<string>();

            foreach( var directory in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string modId = HumanSpaceProgramContent.GetModID( directory );
                modDirectories.Add( directory );

                try
                {
                    ModMetadata metadata = ModMetadata.LoadFromDisk( directory );
                    discoveredMods[modId] = metadata;
                    Debug.Log( $"Discovered mod: {metadata.Name} v{metadata.ModVersion} ({modId})" );
                }
                catch( FileNotFoundException )
                {
                    Debug.LogWarning( $"Mod directory '{directory}' is missing a mod manifest file, skipping." );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load mod metadata from '{directory}': {ex.Message}" );
                    Debug.LogException( ex );
                }
            }

            // Topologically sort mods by dependencies.
            List<string> sortedModIds = TopologicalSort( discoveredMods );
            if( sortedModIds == null )
            {
#warning TODO - log which mods are involved in the circular dependency.
                Debug.LogError( "Circular dependency detected in mods, aborting mod loading." );
                return;
            }

            // Load mods in sorted order.
            foreach( string modId in sortedModIds )
            {
                if( !discoveredMods.TryGetValue( modId, out ModMetadata mod ) )
                    continue;

                var unsatisfiedDeps = mod.GetUnsatisfiedDependencies( _loadedMods );
                if( unsatisfiedDeps.Count > 0 )
                {
                    Debug.LogError( $"Mod '{modId}' has unsatisfied dependencies: {string.Join( ", ", unsatisfiedDeps )}" );
                    continue;
                }

                string modDirectory = modDirectories.First( d => HumanSpaceProgramContent.GetModID( d ) == modId );
                LoadAssembliesRecursive( modDirectory );

                _loadedMods[modId] = mod;
                Debug.Log( $"Loaded mod: {mod.Name} v{mod.ModVersion} ({modId})" );
            }

            Debug.Log( $"Loaded {_loadedMods.Count} mods successfully." );

        }

        /// <summary>
        /// Performs topological sort of mods based on their dependencies.
        /// Returns null if a circular dependency is detected.
        /// </summary>
        private static List<string> TopologicalSort( Dictionary<string, ModMetadata> mods )
        {
            List<string> result = new List<string>();
            HashSet<string> visited = new HashSet<string>();
            HashSet<string> visiting = new HashSet<string>();

            foreach( string modId in mods.Keys )
            {
                if( !visited.Contains( modId ) )
                {
                    if( !VisitMod( modId, mods, visited, visiting, result ) )
                    {
                        return null; // Circular dependency detected
                    }
                }
            }

            return result;
        }

        private static bool VisitMod( string modId, Dictionary<string, ModMetadata> mods, HashSet<string> visited, HashSet<string> visiting, List<string> result )
        {
#warning TODO - move to use the topological sort by delegates later.
            if( visiting.Contains( modId ) )
                return false; // Circular dependency

            if( visited.Contains( modId ) )
                return true; // Already processed

            visiting.Add( modId );

            if( mods.TryGetValue( modId, out ModMetadata mod ) )
            {
                foreach( var dependency in mod.Dependencies )
                {
                    if( !dependency.IsOptional && mods.ContainsKey( dependency.ID ) )
                    {
                        if( !VisitMod( dependency.ID, mods, visited, visiting, result ) )
                        {
                            return false; // Circular dependency in dependencies
                        }
                    }
                }
            }

            visiting.Remove( modId );
            visited.Add( modId );
            result.Add( modId );

            return true;
        }
    }
}