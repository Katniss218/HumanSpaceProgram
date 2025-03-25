using HSP.Content;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Timelines.Serialization
{
    /// <summary>
    /// Represents the 'scenario.json' file.
    /// </summary>
    public sealed class ScenarioMetadata
    {
        /// <summary>
        /// The name of the file that stores the timeline metadata.
        /// </summary>
        public const string SCENARIO_FILENAME = "_scenario.json";

        public readonly NamespacedID ScenarioID; // modId::scenarioId

        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }

        /// <summary>
        /// Gets the thumbnail icon of this scenario.
        /// </summary>
        public Sprite Icon => AssetRegistry.Get<Sprite>( $"{ScenarioID.ModID}::Scenarios/{ScenarioID.ContentID}/_scenario_sprite" );

        public ScenarioMetadata( NamespacedID scenarioId )
        {
            this.ScenarioID = scenarioId;
        }

        /// <summary>
        /// Returns the path to the (root) directory of the scenario.
        /// </summary>
        /// <remarks>
        /// Root directory is the directory that contains the _scenario.json file.
        /// </remarks>
        public static string GetRootDirectory( NamespacedID scenarioId )
        {
            // GameData/<mod_id>/Scenarios/<scenario_id>/...
            return Path.Combine( HumanSpaceProgramContent.GetContentDirectoryPath(), scenarioId.ModID, "Scenarios", scenarioId.ContentID );
        }

        /// <summary>
        /// Returns the path to the (root) directory of the scenario.
        /// </summary>
        /// <remarks>
        /// Root directory is the directory that contains the _scenario.json file.
        /// </remarks>
        public string GetRootDirectory()
        {
            return GetRootDirectory( this.ScenarioID );
        }

        /// <summary>
        /// Reads all timelines from disk and returns a list of their metadata.
        /// </summary>
        public static IEnumerable<ScenarioMetadata> ReadAllScenarios()
        {
            List<string> potentialScenarios = new();

            foreach( var modDirectory in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string modScenariosDirectory = Path.Combine( modDirectory, "Scenarios" );
                if( Directory.Exists( modScenariosDirectory ) )
                {
                    try
                    {
                        string[] scenariosInMod = Directory.GetDirectories( modScenariosDirectory );
                        potentialScenarios.AddRange( scenariosInMod );
                    }
                    catch
                    {
                        Debug.LogWarning( $"Couldn't open `{modScenariosDirectory}` directory." );

                        continue;
                    }
                }
            }

            List<ScenarioMetadata> scenarios = new List<ScenarioMetadata>();

            foreach( var scenarioDirPath in potentialScenarios )
            {
                try
                {
                    ScenarioMetadata scenarioMetadata = ScenarioMetadata.LoadFromDisk( NamespacedID.FromContentPath( scenarioDirPath, out _ ) );
                    scenarios.Add( scenarioMetadata );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"Couldn't load scenario `{scenarioDirPath}`." );
                    Debug.LogException( ex );
                }
            }

            return scenarios;
        }

        public static ScenarioMetadata LoadFromDisk( NamespacedID scenarioId )
        {
            string filePath = Path.Combine( GetRootDirectory( scenarioId ), SCENARIO_FILENAME );

            ScenarioMetadata scenarioMetadata = new ScenarioMetadata( scenarioId );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( filePath );
            var data = handler.Read();
            SerializationUnit.Populate( scenarioMetadata, data );
            return scenarioMetadata;
        }

        public void SaveToDisk()
        {
            string filePath = Path.Combine( GetRootDirectory(), SCENARIO_FILENAME );

            var data = SerializationUnit.Serialize( this );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( filePath );
            handler.Write( data );
        }

        [MapsInheritingFrom( typeof( ScenarioMetadata ) )]
        public static SerializationMapping ScenarioMetadataMapping()
        {
            return new MemberwiseSerializationMapping<ScenarioMetadata>()
                .WithMember( "name", o => o.Name )
                .WithMember( "description", o => o.Description )
                .WithMember( "author", o => o.Author );
        }
    }
}