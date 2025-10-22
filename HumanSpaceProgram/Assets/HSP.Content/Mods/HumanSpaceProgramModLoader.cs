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

        /// <summary>
        /// Gets the metadata for a loaded mod.
        /// </summary>
        /// <param name="modId">The ID of the mod to retrieve.</param>
        /// <returns>The mod metadata, or null if not loaded.</returns>
        public static ModMetadata GetLoadedMod( string modId )
        {
            if( string.IsNullOrEmpty( modId ) )
                return null;

            return HumanSpaceProgramModLoader.LoadedMods.TryGetValue( modId, out ModMetadata mod ) ? mod : null;
        }

        /// <summary>
        /// Checks if a mod is currently loaded.
        /// </summary>
        /// <param name="modId">The ID of the mod to check.</param>
        /// <returns>True if the mod is loaded.</returns>
        public static bool IsModLoaded( string modId )
        {
            return !string.IsNullOrEmpty( modId ) && HumanSpaceProgramModLoader.LoadedMods.ContainsKey( modId );
        }

        /// <summary>
        /// Gets the current versions of all loaded mods.
        /// </summary>
        /// <returns>Dictionary mapping mod IDs to their current versions.</returns>
        public static Dictionary<string, Version> GetCurrentModVersions()
        {
            return HumanSpaceProgramModLoader.LoadedMods.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.ModVersion );
        }

        /// <summary>
        /// Gets the current versions of all loaded mods.
        /// </summary>
        /// <returns>Dictionary mapping mod IDs to their current versions.</returns>
        public static Dictionary<string, Version> GetCurrentSaveModVersions()
        {
            return HumanSpaceProgramModLoader.LoadedMods.Where( kvp => !kvp.Value.ExcludeFromSaves ).ToDictionary( kvp => kvp.Key, kvp => kvp.Value.ModVersion );
        }

        /// <summary>
        /// Validates that all required mod versions are loaded and compatible.
        /// </summary>
        /// <param name="required">Dictionary of required mod versions.</param>
        /// <returns>True if all required mods are loaded with compatible versions.</returns>
        public static bool AreRequiredModsLoaded( Dictionary<string, Version> required )
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

        public const string LOAD_MODS = HSPEvent.NAMESPACE_HSP + ".load_mods";

        /// <summary>
        /// Loads all mods and their assemblies from the GameData directory.
        /// </summary>
        [HSPEventListener( HSPEvent_STARTUP_LOAD_MOD_ASSEMBLIES.ID, LOAD_MODS )]
        private static void LoadMods()
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
                Debug.Log( $"Discovered mod: {metadata.Name} v{metadata.ModVersion} ({directory})" );
            }

            // Topologically sort mods by dependencies.
            List<ModMetadata> sortedMods = modsToLoad.SortDependencies(
                mod => mod.ModID,
                mod => null,
                mod => mod.Dependencies?.Select( d => d.ID ),
                out IEnumerable<ModMetadata> circularDependencies );
            if( circularDependencies.Any() )
            {
                foreach( var cycle in circularDependencies )
                {
                    Debug.LogError( $"Circular dependency detected in mod '{cycle}'" );
                }
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
                    LoadAssemblies( modDirectory );
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

        private static void LoadAssemblies( string path )
        {
            var dlls = Directory.GetFiles( path, "*.dll", SearchOption.AllDirectories );
            if( dlls.Length == 0 )
                return;
            Debug.Log( $"Found {dlls.Length} {(dlls.Length == 1 ? "DLL" : "DLLs")} in mod directory '{path}'." );
            foreach( var dllPath in dlls )
            {
                AssemblyName assemblyName;
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName( dllPath );
                }
                catch( BadImageFormatException )
                {
                    // Unmanaged DLL (or corrupted/couldn't be read).
                    Debug.Log( $"Skipping unmanaged DLL: '{dllPath}'." );
                    continue;
                }
                catch( FileLoadException ex )
                {
                    Debug.LogWarning( $"Failed to inspect assembly '{dllPath}': {ex.Message}." );
                    Debug.LogException( ex );
                    continue;
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"Unexpected exception inspecting '{dllPath}': {ex.Message}." );
                    Debug.LogException( ex );
                    continue;
                }

                try
                {
                    // Don't try to re-load an assembly with the same identity that's already loaded in the AppDomain.
                    Assembly alreadyLoadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault( a =>
                        {
                            try { return AssemblyName.ReferenceMatchesDefinition( a.GetName(), assemblyName ); }
                            catch { return false; }
                        } );
                    if( alreadyLoadedAssembly != null )
                    {
                        Debug.LogWarning( $"Duplicate assembly '{assemblyName.FullName}' already loaded (from {alreadyLoadedAssembly.Location ?? "in-memory"}), skipping." );
                        continue;
                    }

                    byte[] assemblyBytes = File.ReadAllBytes( dllPath );
                    Assembly.Load( assemblyBytes );
                }
                catch( BadImageFormatException )
                {
                    Debug.LogError( $"Failed to load managed assembly '{dllPath}'. The assembly is corrupted or unreadable." );
                    continue;
                }
                catch( FileLoadException ex )
                {
                    Debug.LogError( $"Failed to load managed assembly '{dllPath}': {ex.Message}" );
                    Debug.LogException( ex );
                    continue;
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Unexpected exception loading managed assembly '{dllPath}': {ex.Message}" );
                    Debug.LogException( ex );
                    continue;
                }
            }
        }
    }
}