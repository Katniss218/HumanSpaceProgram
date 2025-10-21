using HSP.Content.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityPlus.Serialization;
using Version = HSP.Content.Version;

namespace HSP.Timelines
{
    /// <summary>
    /// Manages save file migrations between different mod versions.
    /// </summary>
    public static class SaveMigrationRegistry
    {
        private static Dictionary<string, List<SaveMigration>> _migrations = new Dictionary<string, List<SaveMigration>>();

        public const string DISCOVER_SAVE_MIGRATIONS = HSPEvent.NAMESPACE_HSP + ".discover_save_migrations";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, DISCOVER_SAVE_MIGRATIONS )]
        private static void DiscoverSaveMigrations()
        {
            DiscoverMigrations( AppDomain.CurrentDomain.GetAssemblies() );
        }

        private static bool IsValidMigrationMethod( MethodInfo method )
        {
            if( !method.IsStatic )
                return false;

            ParameterInfo[] parameters = method.GetParameters();
            if( parameters.Length != 1 )
                return false;

            if( parameters[0].ParameterType != typeof( SerializedData ).MakeByRefType() )
                return false;

            if( method.ReturnType != typeof( void ) )
                return false;

            return true;
        }

        /// <summary>
        /// Discovers and registers migration methods from the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for migration methods</param>
        public static void DiscoverMigrations( IEnumerable<Assembly> assemblies )
        {
            _migrations.Clear();

            foreach( var assembly in assemblies )
            {
                try
                {
                    Type[] assemblyTypes = assembly.GetTypes();
                    foreach( var type in assemblyTypes )
                    {
                        MethodInfo[] methods = type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                        foreach( var method in methods )
                        {
                            try
                            {
                                SaveMigrationAttribute attr = method.GetCustomAttribute<SaveMigrationAttribute>();
                                if( attr == null )
                                    continue;

                                if( !IsValidMigrationMethod( method ) )
                                {
                                    Debug.LogWarning( $"Migration method '{method.Name}' in type '{type.Name}' has invalid signature. Expected `static void Method(SerializedData data)`" );
                                    continue;
                                }

                                RegisterMigration( attr, method );
                            }
                            catch( Exception ex )
                            {
                                Debug.LogError( $"Error processing migration method '{method.Name}' in type '{type.Name}': {ex.Message}" );
                                Debug.LogException( ex );
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Error scanning assembly '{assembly.FullName}' for migrations: {ex.Message}" );
                    Debug.LogException( ex );
                }
            }

            Debug.Log( $"Discovered {_migrations.Values.Sum( list => list.Count )} save migrations across {_migrations.Count} mods." );
        }

#warning TODO - some migrations need to be able to use the file system (to change the structure of the save, those should run first)
        /// <summary>
        /// Applies migrations to save data.
        /// </summary>
        /// <param name="data">The save data to migrate</param>
        /// <param name="fromVersions">The mod versions when the save was created</param>
        /// <param name="toVersions">The current mod versions</param>
        /// <param name="force">If true, forces the migration even if there are mod version mismatches or other issues.</param>
        /// <returns>True if migration was successful</returns>
        public static void Migrate( ref SerializedData data, Dictionary<string, Version> fromVersions, Dictionary<string, Version> toVersions, bool force )
        {
            if( data == null || fromVersions == null || toVersions == null )
                return;

            foreach( var kvp in fromVersions )
            {
                string modId = kvp.Key;
                Version fromVersion = kvp.Value;

                if( !toVersions.TryGetValue( modId, out Version toVersion ) && !force )
                {
                    throw new SaveMigrationException( $"Mod '{modId}' was present when save was created but is not currently loaded." );
                }

                if( fromVersion > toVersion && !force )
                {
                    throw new SaveMigrationException( $"Mod '{modId}' has a newer version ({fromVersion}) in the save than the currently loaded version ({toVersion}). Downgrades are not supported." );
                }

                if( fromVersion == toVersion )
                    continue; // No migration needed

                if( !TryGetMigrationChain( modId, fromVersion, toVersion, out var migrationChain ) && !force )
                {
                    throw new SaveMigrationException( $"No save migration chain found for mod '{modId}' for versions {fromVersion} -> {toVersion}." );
                }

                foreach( var migration in migrationChain )
                {
                    try
                    {
                        migration.MigrationFunc( ref data );
                        Debug.Log( $"Applied save migration for mod '{modId}' from {migration.FromVersion} to {migration.ToVersion}" );
                    }
                    catch( Exception ex )
                    {
                        throw new SaveMigrationException( $"Failed to apply migration for mod '{modId}' from {migration.FromVersion} to {migration.ToVersion}: {ex.Message}", ex );
                    }
                }
            }
        }

        private static void RegisterMigration( SaveMigrationAttribute attr, MethodInfo method )
        {
            if( !ModManager.IsModLoaded( attr.ModID ) )
            {
                throw new InvalidOperationException( $"Cannot register migration for mod '{attr.ModID}' because the mod is not loaded. Mods shouldn't register migrations for other mods." );
            }

            if( !_migrations.TryGetValue( attr.ModID, out List<SaveMigration> modMigrations ) )
            {
                modMigrations = new List<SaveMigration>();
                _migrations[attr.ModID] = modMigrations;
            }

            if( !Version.TryParse( attr.FromVersion, out Version fromVersion ) )
            {
                Debug.LogError( $"Invalid FromVersion '{attr.FromVersion}' in migration for mod '{attr.ModID}'." );
                return;
            }
            if( !Version.TryParse( attr.ToVersion, out Version toVersion ) )
            {
                Debug.LogError( $"Invalid ToVersion '{attr.ToVersion}' in migration for mod '{attr.ModID}'." );
                return;
            }

            SaveMigrationFunc migrationFunc = (SaveMigrationFunc)Delegate.CreateDelegate( typeof( SaveMigrationFunc ), method );

            var migration = new SaveMigration( fromVersion, toVersion, migrationFunc, attr.Description );
            modMigrations.Add( migration );

            Debug.Log( $"Registered migration for mod '{attr.ModID}': {migration}" );
        }

        /// <summary>
        /// Gets a migration chain from one version to another for a specific mod.
        /// </summary>
        /// <param name="modId">The mod ID</param>
        /// <param name="fromVersion">The version to migrate from</param>
        /// <param name="toVersion">The version to migrate to</param>
        /// <returns>Ordered list of migrations, or null if no path exists</returns>
        public static bool TryGetMigrationChain( string modId, Version fromVersion, Version toVersion, out IEnumerable<SaveMigration> migrations )
        {
            migrations = null;

            if( !_migrations.TryGetValue( modId, out List<SaveMigration> modMigrations ) )
                return false;

            if( fromVersion == toVersion )
            {
                migrations = Enumerable.Empty<SaveMigration>();
                return true;
            }

            Dictionary<Version, List<SaveMigration>> graph = BuildMigrationGraph( modMigrations );

            return TryFindMigrationPath( graph, fromVersion, toVersion, out migrations );
        }

        private static Dictionary<Version, List<SaveMigration>> BuildMigrationGraph( List<SaveMigration> migrations )
        {
            Dictionary<Version, List<SaveMigration>> graph = new();

            foreach( var migration in migrations )
            {
                if( !graph.TryGetValue( migration.FromVersion, out List<SaveMigration> migrationsForVersion ) )
                {
                    migrationsForVersion = new List<SaveMigration>();
                    graph[migration.FromVersion] = migrationsForVersion;
                }

                migrationsForVersion.Add( migration );
            }

            return graph;
        }

        private static bool TryFindMigrationPath( Dictionary<Version, List<SaveMigration>> graph, Version fromVersion, Version toVersion, out IEnumerable<SaveMigration> migrations )
        {
            Queue<Version> queue = new();
            HashSet<Version> visited = new();
            Dictionary<Version, SaveMigration> parent = new();

            queue.Enqueue( fromVersion );
            visited.Add( fromVersion );

            // BFS

            while( queue.Count > 0 )
            {
                Version current = queue.Dequeue();

                if( current == toVersion )
                {
                    List<SaveMigration> path = new();
                    Version pathVersion = toVersion;

                    while( parent.TryGetValue( pathVersion, out SaveMigration migration ) )
                    {
                        path.Insert( 0, migration );
                        pathVersion = migration.FromVersion;
                    }

                    migrations = path;
                    return true;
                }

                if( graph.TryGetValue( current, out List<SaveMigration> outgoing ) )
                {
                    foreach( var migration in outgoing )
                    {
                        if( !visited.Contains( migration.ToVersion ) )
                        {
                            visited.Add( migration.ToVersion );
                            parent[migration.ToVersion] = migration;
                            queue.Enqueue( migration.ToVersion );
                        }
                    }
                }
            }

            migrations = null;
            return false;
        }
    }
}