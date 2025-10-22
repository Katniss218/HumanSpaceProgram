using System;
using System.Collections.Generic;
using System.IO;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Content.Migrations
{
    public struct MigrationChain
    {
        internal readonly IEnumerable<DataMigration> dataMigrations;
        internal readonly IEnumerable<StructuralMigration> structuralMigrations;

        public MigrationChain( IEnumerable<DataMigration> dataMigrations, IEnumerable<StructuralMigration> structuralMigrations )
        {
            this.dataMigrations = dataMigrations;
            this.structuralMigrations = structuralMigrations;
        }

        public void Migrate( IMigrationContext context )
        {
            if( Directory.Exists( context.RootPath ) )
            {
                foreach( var migration in structuralMigrations )
                {
                    try
                    {
                        migration.MigrationFunc.Invoke( context );
                    }
                    catch( Exception ex )
                    {
                        throw new MigrationException( $"Failed to apply structural migration from {migration.FromVersion} to {migration.ToVersion}: {ex.Message}", ex );
                    }
                }
            }

            var dataFilesToMigrate = Directory.Exists( context.RootPath )
                ? Directory.GetFiles( context.RootPath, "*.json", SearchOption.AllDirectories )
                : new string[] { context.RootPath };

            foreach( var migration in dataMigrations )
            {
                try
                {
                    foreach( var file in dataFilesToMigrate )
                    {
                        var handler = new JsonSerializedDataHandler( file );
                        var data = handler.Read();

                        migration.MigrationFunc( ref data );

                        handler.Write( data );
                    }
                }
                catch( Exception ex )
                {
                    throw new MigrationException( $"Failed to apply data migration from {migration.FromVersion} to {migration.ToVersion}: {ex.Message}", ex );
                }
            }
        }
    }
}