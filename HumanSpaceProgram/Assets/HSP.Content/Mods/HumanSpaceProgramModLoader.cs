using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityPlus;

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

        public const string LOAD_MOD_ASSEMBLIES = HSPEvent.NAMESPACE_HSP + ".load_mod_assemblies";

        /// <summary>
        /// Loads all mods and their assemblies from the GameData directory.
        /// </summary>
        [HSPEventListener( HSPEvent_STARTUP_LOAD_MOD_ASSEMBLIES.ID, LOAD_MOD_ASSEMBLIES )]
        private static void LoadModAssemblies()
        {
            string gameDataDirectory = HumanSpaceProgramContent.GetContentDirectoryPath();

            if( !Directory.Exists( gameDataDirectory ) )
                Directory.CreateDirectory( gameDataDirectory );

            Debug.Log( $"The content directory is: '{gameDataDirectory}'" );

            // Discover and load all mod metadata
            HashSet<ModMetadata> modsToLoad = new();
            List<string> modDirectories = new();

            foreach( var directory in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string modId = HumanSpaceProgramContent.GetModID( directory );
                modDirectories.Add( directory );

                ModMetadata metadata = null;
                try
                {
                    metadata = ModMetadata.LoadFromDisk( directory );
                }
                catch( FileNotFoundException )
                {
                    Debug.LogWarning( $"Mod directory '{directory}' is missing a mod manifest file, skipping." );
                    continue;
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load mod metadata file from '{directory}': {ex.Message}" );
                    Debug.LogException( ex );
                    continue;
                }

                if( modsToLoad.Contains( metadata ) )
                {
                    throw new ModLoaderException( $"Duplicate mod ID '{modId}' found in directory '{directory}', aborting mod loading." );
                }

                modsToLoad.Add( metadata );
                Debug.Log( $"Discovered mod: {metadata.Name} v{metadata.ModVersion} ({modId})" );
            }

            // Topologically sort mods by dependencies.
            List<ModMetadata> sortedMods = ITopologicallySortable_Ex.SortDependencies<ModMetadata, string>( modsToLoad, mod => mod.ModID, mod => null, mod => mod.Dependencies?.Select( d => d.ID ), out bool hasCircularDependency );
            if( hasCircularDependency )
            {
#warning TODO - log which mods are involved in the circular dependency.
                throw new ModLoaderException( "Circular dependency detected among mods, aborting mod loading." );
            }

            // Load mods in sorted order.
            foreach( var mod in sortedMods )
            {
                IEnumerable<ModDependency> unsatisfiedDeps = mod.GetUnsatisfiedDependencies( _loadedMods );
                if( unsatisfiedDeps.Any() )
                {
                    Debug.LogError( $"Mod '{mod.ModID}' has unsatisfied dependencies: {string.Join( ", ", unsatisfiedDeps )}" );
                    continue;
                }

                string modDirectory = modDirectories.First( d => HumanSpaceProgramContent.GetModID( d ) == mod.ModID );
                try
                {
                    LoadAssembliesRecursive( modDirectory );
                }
                catch( Exception ex )
                {
                    // Abort because this could potentially cause a very bad state internally (failed dependencies, etc) if it were allowed to continue.
                    throw new ModLoaderException( $"Failed to load assemblies for mod '{mod.ModID}', aborting mod loading.", ex );
                }

                _loadedMods[mod.ModID] = mod;
                Debug.Log( $"Loaded mod: {mod.Name} v{mod.ModVersion} ({mod.ModID})" );
            }

            Debug.Log( $"Loaded {_loadedMods.Count} mods successfully." );
        }
    }
}