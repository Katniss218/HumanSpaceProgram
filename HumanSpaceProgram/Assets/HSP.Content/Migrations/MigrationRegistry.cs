using HSP.Content.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Version = HSP.Content.Version;

namespace HSP.Content.Migrations
{
    /// <summary>
    /// Manages save file migrations between different mod versions.
    /// </summary>
    public static class MigrationRegistry
    {
#warning TODO - add a MigrationUtility static class with helpers for bulk operations, filters, renames, etc.
#warning TODO - separate APIVersion and Version in mod files. API version is for compatibility.

        private static Dictionary<string, List<StructuralMigration>> _structuralMigrations = new();
        private static Dictionary<string, List<DataMigration>> _dataMigrations = new();

        public const string DISCOVER_SAVE_MIGRATIONS = HSPEvent.NAMESPACE_HSP + ".discover_save_migrations";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, DISCOVER_SAVE_MIGRATIONS )]
        private static void DiscoverMigrations()
        {
            DiscoverMigrations( AppDomain.CurrentDomain.GetAssemblies() );
        }

        /// <summary>
        /// Discovers and registers migration methods from the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for migration methods</param>
        public static void DiscoverMigrations( IEnumerable<Assembly> assemblies )
        {
            _structuralMigrations.Clear();
            _dataMigrations.Clear();

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
                                DataMigrationAttribute attr = method.GetCustomAttribute<DataMigrationAttribute>();
                                StructuralMigrationAttribute attr2 = method.GetCustomAttribute<StructuralMigrationAttribute>();
                                if( attr == null && attr2 == null )
                                    continue;

                                if( attr != null )
                                {
                                    if( !DataMigrationAttribute.IsValidMethodSignature( method ) )
                                        throw new InvalidOperationException( $"Data migration method '{method.Name}' in type '{type.Name}' has invalid signature. Expected `static void Method(ref SerializedData data)`" );

                                    if( !HumanSpaceProgramModLoader.IsModLoaded( attr.ModID ) )
                                        throw new InvalidOperationException( $"Cannot register migration for mod '{attr.ModID}' because the mod is not loaded. Mods shouldn't register migrations for other mods." );

                                    RegisterMigration( attr, method );
                                }
                                else if( attr2 != null )
                                {
                                    if( !StructuralMigrationAttribute.IsValidMethodSignature( method ) )
                                        throw new InvalidOperationException( $"Structural migration method '{method.Name}' in type '{type.Name}' has invalid signature. Expected `static void Method(IMigrationContext context)`" );

                                    if( !HumanSpaceProgramModLoader.IsModLoaded( attr2.ModID ) )
                                        throw new InvalidOperationException( $"Cannot register migration for mod '{attr2.ModID}' because the mod is not loaded. Mods shouldn't register migrations for other mods." );

                                    RegisterMigration( attr2, method );
                                }
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

            Debug.Log( $"Discovered {_structuralMigrations.Values.Sum( list => list.Count )} structural migration(s) across {_structuralMigrations.Count} mods." );
            Debug.Log( $"Discovered {_dataMigrations.Values.Sum( list => list.Count )} data migration(s) across {_dataMigrations.Count} mods." );
        }

        /// <summary>
        /// Applies migrations to serialized data.
        /// </summary>
        /// <param name="filePath">The path to the file/directory to migrate. <br/> If migrating a directory, it will apply both the structural and data migrations migrations. If migrating a file, it will apply only the data migrations.</param>
        /// <param name="fromVersions">The mod versions when the file was created.</param>
        /// <param name="toVersions">The current mod versions.</param>
        /// <param name="force">If true, forces the migration even if there are mod version mismatches or other issues.</param>
        /// <returns>True if migration was successful.</returns>
        public static void Migrate( string filePath, Dictionary<string, Version> fromVersions, Dictionary<string, Version> toVersions, bool force )
        {
            if( filePath == null || fromVersions == null || toVersions == null )
                return;

            if( File.Exists( filePath ) == false )
                throw new MigrationException( $"Save file '{filePath}' does not exist." );

            foreach( var kvp in fromVersions )
            {
                string modId = kvp.Key;
                Version fromVersion = kvp.Value;

                if( !toVersions.TryGetValue( modId, out Version toVersion ) && !force )
                {
                    throw new MigrationException( $"Mod '{modId}' was present when save was created but is not currently loaded." );
                }

                if( fromVersion > toVersion && !force )
                {
                    throw new MigrationException( $"Mod '{modId}' has a newer version ({fromVersion}) in the save than the currently loaded version ({toVersion}). Downgrades are not supported." );
                }

                if( fromVersion == toVersion )
                    continue; // No migration needed

                if( !TryGetMigrationChain( modId, fromVersion, toVersion, out var migrationChain ) && !force )
                {
                    throw new MigrationException( $"No save migration chain found for mod '{modId}' for versions {fromVersion} -> {toVersion}." );
                }

                IMigrationContext context = new MigrationContext( filePath );
                migrationChain.Migrate( context );
            }
        }

        private static void RegisterMigration( DataMigrationAttribute attr, MethodInfo method )
        {
            if( !_dataMigrations.TryGetValue( attr.ModID, out List<DataMigration> modMigrations ) )
            {
                modMigrations = new List<DataMigration>();
                _dataMigrations[attr.ModID] = modMigrations;
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

            DataMigrationFunc migrationFunc = (DataMigrationFunc)Delegate.CreateDelegate( typeof( DataMigrationFunc ), method );

            var migration = new DataMigration( fromVersion, toVersion, migrationFunc, attr.Description );
            modMigrations.Add( migration );

            Debug.Log( $"Registered migration for mod '{attr.ModID}': {migration}" );
        }

        private static void RegisterMigration( StructuralMigrationAttribute attr2, MethodInfo method )
        {
            if( !HumanSpaceProgramModLoader.IsModLoaded( attr2.ModID ) )
            {
                throw new InvalidOperationException( $"Cannot register migration for mod '{attr2.ModID}' because the mod is not loaded. Mods shouldn't register migrations for other mods." );
            }

            if( !_structuralMigrations.TryGetValue( attr2.ModID, out List<StructuralMigration> modMigrations ) )
            {
                modMigrations = new List<StructuralMigration>();
                _structuralMigrations[attr2.ModID] = modMigrations;
            }

            if( !Version.TryParse( attr2.FromVersion, out Version fromVersion ) )
            {
                Debug.LogError( $"Invalid FromVersion '{attr2.FromVersion}' in migration for mod '{attr2.ModID}'." );
                return;
            }
            if( !Version.TryParse( attr2.ToVersion, out Version toVersion ) )
            {
                Debug.LogError( $"Invalid ToVersion '{attr2.ToVersion}' in migration for mod '{attr2.ModID}'." );
                return;
            }

            StructuralMigrationFunc migrationFunc = (StructuralMigrationFunc)Delegate.CreateDelegate( typeof( StructuralMigrationFunc ), method );

            var migration = new StructuralMigration( fromVersion, toVersion, migrationFunc, attr2.Description );
            modMigrations.Add( migration );

            Debug.Log( $"Registered migration for mod '{attr2.ModID}': {migration}" );
        }

        /// <summary>
        /// Gets a migration chain from one version to another for a specific mod.
        /// </summary>
        /// <param name="modId">The mod ID</param>
        /// <param name="fromVersion">The version to migrate from</param>
        /// <param name="toVersion">The version to migrate to</param>
        public static bool TryGetMigrationChain( string modId, Version fromVersion, Version toVersion, out MigrationChain migrations )
        {
            if( fromVersion == toVersion )
            {
                migrations = default;
                return false;
            }

            migrations = default;

            bool hasDataMigrations = _dataMigrations.TryGetValue( modId, out List<DataMigration> modDataMigrations );
            bool hasStructuralMigrations = _structuralMigrations.TryGetValue( modId, out List<StructuralMigration> modStructuralMigrations );
            if( !hasDataMigrations && !hasStructuralMigrations )
                return false;

            Dictionary<Version, List<DataMigration>> dataGraph = BuildDataMigrationGraph( modDataMigrations );
            Dictionary<Version, List<StructuralMigration>> structuralGraph = BuildStructuralMigrationGraph( modStructuralMigrations );
            bool dataPath = TryFindDataMigrationPath( dataGraph, fromVersion, toVersion, out var dataMigrations );
            bool structuralPath = TryFindStructuralMigrationPath( structuralGraph, fromVersion, toVersion, out var structuralMigrations );
            if( !dataPath && !structuralPath )
                return false;

            migrations = new MigrationChain( dataMigrations ?? Enumerable.Empty<DataMigration>(), structuralMigrations ?? Enumerable.Empty<StructuralMigration>() );
            return true;
        }

        private static Dictionary<Version, List<DataMigration>> BuildDataMigrationGraph( List<DataMigration> migrations )
        {
            Dictionary<Version, List<DataMigration>> graph = new();

            foreach( var migration in migrations )
            {
                if( !graph.TryGetValue( migration.FromVersion, out List<DataMigration> migrationsForVersion ) )
                {
                    migrationsForVersion = new List<DataMigration>();
                    graph[migration.FromVersion] = migrationsForVersion;
                }

                migrationsForVersion.Add( migration );
            }

            return graph;
        }

        private static Dictionary<Version, List<StructuralMigration>> BuildStructuralMigrationGraph( List<StructuralMigration> migrations )
        {
            Dictionary<Version, List<StructuralMigration>> graph = new();

            foreach( var migration in migrations )
            {
                if( !graph.TryGetValue( migration.FromVersion, out List<StructuralMigration> migrationsForVersion ) )
                {
                    migrationsForVersion = new List<StructuralMigration>();
                    graph[migration.FromVersion] = migrationsForVersion;
                }

                migrationsForVersion.Add( migration );
            }

            return graph;
        }

        private static bool TryFindDataMigrationPath( Dictionary<Version, List<DataMigration>> graph, Version fromVersion, Version toVersion, out IEnumerable<DataMigration> migrations )
        {
            Queue<Version> queue = new();
            HashSet<Version> visited = new();
            Dictionary<Version, DataMigration> parent = new();

            queue.Enqueue( fromVersion );
            visited.Add( fromVersion );

            // BFS

            while( queue.Count > 0 )
            {
                Version current = queue.Dequeue();

                if( current == toVersion )
                {
                    List<DataMigration> path = new();
                    Version pathVersion = toVersion;

                    while( parent.TryGetValue( pathVersion, out DataMigration migration ) )
                    {
                        path.Insert( 0, migration );
                        pathVersion = migration.FromVersion;
                    }

                    migrations = path;
                    return true;
                }

                if( graph.TryGetValue( current, out List<DataMigration> outgoing ) )
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
        private static bool TryFindStructuralMigrationPath( Dictionary<Version, List<StructuralMigration>> graph, Version fromVersion, Version toVersion, out IEnumerable<StructuralMigration> migrations )
        {
            Queue<Version> queue = new();
            HashSet<Version> visited = new();
            Dictionary<Version, StructuralMigration> parent = new();

            queue.Enqueue( fromVersion );
            visited.Add( fromVersion );

            // BFS

            while( queue.Count > 0 )
            {
                Version current = queue.Dequeue();

                if( current == toVersion )
                {
                    List<StructuralMigration> path = new();
                    Version pathVersion = toVersion;

                    while( parent.TryGetValue( pathVersion, out StructuralMigration migration ) )
                    {
                        path.Insert( 0, migration );
                        pathVersion = migration.FromVersion;
                    }

                    migrations = path;
                    return true;
                }

                if( graph.TryGetValue( current, out List<StructuralMigration> outgoing ) )
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