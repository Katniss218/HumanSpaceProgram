﻿using System.Collections.Generic;
using System.IO;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Content.Vessels.Serialization
{
    /// <summary>
    /// Represents additional metadata of a saved vessel.
    /// </summary>
    public class VesselMetadata
    {
        /// <summary>
        /// The name of the file that stores the vessel metadata.
        /// </summary>
        public const string VESSEL_METADATA_FILENAME = "_vessel.json";

        /// <summary>
        /// The unique ID of this vessel.
        /// </summary>
        public readonly string ID; // Vessels don't have a namespace, they're player-created.

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The name of the author of the vessel.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The version of the vessel file.
        /// </summary>
        public Version FileVersion { get; set; }

        /// <summary>
        /// The versions of all the mods used when the vessel file was created.
        /// </summary>
        public Dictionary<string, Version> ModVersions { get; set; } = new Dictionary<string, Version>();

        public VesselMetadata( string id )
        {
            this.ID = id;
        }

        /// <summary>
        /// Root directory is the directory that contains the _timeline.json file.
        /// </summary>
        public static string GetSavedVesselsPath()
        {
            return HumanSpaceProgramContent.GetSavedVesselsDirectoryPath();
        }

        /// <summary>
        /// Root directory is the directory that contains the _vessel.json file.
        /// </summary>
        public static string GetRootDirectory( string vesselId )
        {
            return Path.Combine( GetSavedVesselsPath(), vesselId );
        }

        /// <summary>
        /// Root directory is the directory that contains the _vessel.json file.
        /// </summary>
        public string GetRootDirectory()
        {
            return GetRootDirectory( this.ID );
        }

        public static VesselMetadata LoadFromDisk( string id )
        {
            string saveFilePath = Path.Combine( GetRootDirectory( id ), VESSEL_METADATA_FILENAME );

            VesselMetadata vesselMetadata = new VesselMetadata( id );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( saveFilePath );
            var data = handler.Read();
            SerializationUnit.Populate( vesselMetadata, data );
            return vesselMetadata;
        }

        public void SaveToDisk()
        {
            string saveFilePath = Path.Combine( GetRootDirectory(), VESSEL_METADATA_FILENAME );

            var data = SerializationUnit.Serialize( this );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( saveFilePath );
            handler.Write( data );
        }

        [MapsInheritingFrom( typeof( VesselMetadata ) )]
        public static SerializationMapping VesselMetadataMapping()
        {
            return new MemberwiseSerializationMapping<VesselMetadata>()
                .WithMember( "name", o => o.Name )
                .WithMember( "description", o => o.Description )
                .WithMember( "author", o => o.Author )
                // .WithMember( "categories", o => o.Categories )
                // .WithMember( "filter", o => o.Filter )
                // .WithMember( "group", o => o.Group )
                .WithMember( "file_version", o => o.FileVersion )
                .WithMember( "mod_versions", o => o.ModVersions );
        }
    }
}