using HSP.Content.Migrations;
using System.IO;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Migrations
{
    internal static partial class Migrations
    {
        [DataMigration( "Vanilla", "-999.0", "-998.0", Description = "Rename RocketEngine to LiquidEngine" )]
        internal static void Migration_1_1_to_1_2( ref SerializedData data )
        {
            // scans the serialized data and renames value of any "$type" key that matches the predicate.
            MigrationUtility.RenameType(
                ref data,
                "HSP.Content.Vessels.PartMetadata, HSP.Vessels, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                "HSP.Content.Vessels.Serialization.PartMetadata, HSP.Vessels, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            );
        }

        [StructuralMigration( "Vanilla", "-999.0", "-998.0", Description = "Reorganize to new folder structure" )]
        private static void Migration_1_8_to_2_0_Structural( IMigrationContext context )
        {
            // Move all engine files to new engines/ subdirectory
            var engineFiles = context.GetFiles( "*_engine.json", SearchOption.TopDirectoryOnly );

            var enginesDir = Path.Combine( context.RootPath, "engines" );
            context.CreateDirectory( enginesDir );

            foreach( var file in engineFiles )
            {
                var fileName = Path.GetFileName( file );
                var newPath = Path.Combine( enginesDir, fileName );
                context.MoveFile( file, newPath );
            }
        }
    }
}
