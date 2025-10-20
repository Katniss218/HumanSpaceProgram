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
        private static Dictionary<string, List<Migration>> _migrations = new Dictionary<string, List<Migration>>();

        public const string DISCOVER_SAVE_MIGRATIONS = HSPEvent.NAMESPACE_HSP + ".discover_save_migrations";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, DISCOVER_SAVE_MIGRATIONS )]
        private static void DiscoverSaveMigrations()
        {
            DiscoverMigrations( AppDomain.CurrentDomain.GetAssemblies() );
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

        /// <summary>
        /// Gets a migration chain from one version to another for a specific mod.
        /// </summary>
        /// <param name="modId">The mod ID</param>
        /// <param name="fromVersion">The version to migrate from</param>
        /// <param name="toVersion">The version to migrate to</param>
        /// <returns>Ordered list of migrations, or null if no path exists</returns>
        public static List<Migration> GetMigrationChain( string modId, Version fromVersion, Version toVersion )
        {
            if( !_migrations.TryGetValue( modId, out List<Migration> modMigrations ) )
                return null;

            if( fromVersion == toVersion )
                return new List<Migration>(); // No migration needed

            // Build a graph of migrations and find the shortest path
            var migrationGraph = BuildMigrationGraph( modMigrations );
            return FindMigrationPath( migrationGraph, fromVersion, toVersion );
        }

        /// <summary>
        /// Applies migrations to save data.
        /// </summary>
        /// <param name="data">The save data to migrate</param>
        /// <param name="fromVersions">The mod versions when the save was created</param>
        /// <param name="toVersions">The current mod versions</param>
        /// <returns>True if migration was successful</returns>
        public static bool ApplyMigrations( SerializedData data, Dictionary<string, Version> fromVersions, Dictionary<string, Version> toVersions )
        {
            if( data == null || fromVersions == null || toVersions == null )
                return true; // Nothing to migrate

            bool anyMigrationApplied = false;

            foreach( var kvp in fromVersions )
            {
                string modId = kvp.Key;
                Version fromVersion = kvp.Value;

                if( !toVersions.TryGetValue( modId, out Version toVersion ) )
                {
                    Debug.LogError( $"Mod '{modId}' was present when save was created but is not currently loaded." );
                    return false;
                }

                if( fromVersion == toVersion )
                    continue; // No migration needed

                var migrationChain = GetMigrationChain( modId, fromVersion, toVersion );
                if( migrationChain == null )
                {
                    Debug.LogError( $"No migration path found for mod '{modId}' from version {fromVersion} to {toVersion}." );
                    return false;
                }

                foreach( var migration in migrationChain )
                {
                    try
                    {
                        migration.MigrationFunc( data );
                        Debug.Log( $"Applied migration for mod '{modId}' from {migration.FromVersion} to {migration.ToVersion}" );
                        anyMigrationApplied = true;
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"Failed to apply migration for mod '{modId}' from {migration.FromVersion} to {migration.ToVersion}: {ex.Message}" );
                        Debug.LogException( ex );
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsValidMigrationMethod( MethodInfo method )
        {
            if( !method.IsStatic )
                return false;

            ParameterInfo[] parameters = method.GetParameters();
            if( parameters.Length != 1 )
                return false;

            if( parameters[0].ParameterType != typeof( SerializedData ) )
                return false;

            if( method.ReturnType != typeof( void ) )
                return false;

            return true;
        }

        private static void RegisterMigration( SaveMigrationAttribute attr, MethodInfo method )
        {
            if( !_migrations.TryGetValue( attr.ModID, out List<Migration> modMigrations ) )
            {
                modMigrations = new List<Migration>();
                _migrations[attr.ModID] = modMigrations;
            }

            Version fromVersion = Version.Parse( attr.FromVersion );
            Version toVersion = Version.Parse( attr.ToVersion );

            Action<SerializedData> migrationFunc = (Action<SerializedData>)Delegate.CreateDelegate( typeof( Action<SerializedData> ), method );

            var migration = new Migration( fromVersion, toVersion, migrationFunc, attr.Description );
            modMigrations.Add( migration );

            Debug.Log( $"Registered migration for mod '{attr.ModID}': {fromVersion} -> {toVersion}" );
        }

        private static Dictionary<Version, List<Migration>> BuildMigrationGraph( List<Migration> migrations )
        {
            var graph = new Dictionary<Version, List<Migration>>();

            foreach( var migration in migrations )
            {
                if( !graph.TryGetValue( migration.FromVersion, out List<Migration> outgoing ) )
                {
                    outgoing = new List<Migration>();
                    graph[migration.FromVersion] = outgoing;
                }
                outgoing.Add( migration );
            }

            return graph;
        }

        private static List<Migration> FindMigrationPath( Dictionary<Version, List<Migration>> graph, Version fromVersion, Version toVersion )
        {
            var queue = new Queue<Version>();
            var visited = new HashSet<Version>();
            var parent = new Dictionary<Version, Migration>();

            queue.Enqueue( fromVersion );
            visited.Add( fromVersion );

            while( queue.Count > 0 )
            {
                Version current = queue.Dequeue();

                if( current == toVersion )
                {
                    // Reconstruct path
                    var path = new List<Migration>();
                    Version pathVersion = toVersion;

                    while( parent.TryGetValue( pathVersion, out Migration migration ) )
                    {
                        path.Insert( 0, migration );
                        pathVersion = migration.FromVersion;
                    }

                    return path;
                }

                if( graph.TryGetValue( current, out List<Migration> outgoing ) )
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

            return null; // No path found
        }
    }

    /// <summary>
    /// Represents a single migration step.
    /// </summary>
    public struct Migration
    {
        public Version FromVersion { get; }
        public Version ToVersion { get; }
        public Action<SerializedData> MigrationFunc { get; }
        public string Description { get; }

        public Migration( Version fromVersion, Version toVersion, Action<SerializedData> migrationFunc, string description = null )
        {
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
            this.MigrationFunc = migrationFunc;
            this.Description = description;
        }

        public override string ToString()
        {
            return $"{FromVersion} -> {ToVersion}" + ( string.IsNullOrEmpty( Description ) ? "" : $" ({Description})" );
        }
    }
}
