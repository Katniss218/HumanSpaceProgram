using HSP.Content.Vessels.Serialization;
using HSP.SceneManagement;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Formats;
using UnityPlus.Serialization.ReferenceMaps;

namespace HSP.Vessels.Serialization
{
    /// <summary>
    /// Static utility for saving and loading vessels in the 3-file multi-body format (_vessel.json, parts.json, graph.json).
    /// </summary>
    public static class VesselSerializationUtils
    {
        public const string METADATA_FILENAME = VesselMetadata.VESSEL_METADATA_FILENAME;
        public const string PARTS_FILENAME = "parts.json";
        public const string GRAPH_FILENAME = "graph.json";

        /// <summary>
        /// Saves a vessel into the multi-file format at the specified directory path.
        /// </summary>
        public static void Save<T>( T vessel, string directoryPath, IReverseReferenceMap refStore = null, VesselMetadata metadata = null ) where T : IVessel
        {
            if( vessel == null )
                throw new ArgumentNullException( nameof( vessel ) );
            if( string.IsNullOrEmpty( directoryPath ) )
                throw new ArgumentException( "Directory path cannot be null or empty.", nameof( directoryPath ) );

            Directory.CreateDirectory( directoryPath );

            refStore ??= new BidirectionalReferenceStore();

            // 1. Gather parts
            VesselPart[] parts = vessel.Parts?.ToArray();
            GameObject[] partObjects = parts
                .Where( p => p != null )
                .Select( p => p.gameObject )
                .ToArray();

            // 2. Save parts.json
            var partsDataHandler = new FileSerializedDataHandler( Path.Combine( directoryPath, PARTS_FILENAME ), JsonFormat.Instance );
            var partsData = SerializationUnit.Serialize( partObjects, refStore );
            partsDataHandler.Write( partsData );

            // 3. Save graph.json
            var graphDataHandler = new FileSerializedDataHandler( Path.Combine( directoryPath, GRAPH_FILENAME ), JsonFormat.Instance );
            var graphData = SerializationUnit.Serialize( vessel.Attachments, refStore );
            graphDataHandler.Write( graphData );
        }

        public static bool TrySave<T>()
        {

        }

        /// <summary>
        /// Loads a vessel from the multi-file format at the specified directory path.
        /// </summary>
        public static T Load<T>( IHSPScene scene, string directoryPath, IForwardReferenceMap refStore = null ) where T : IVessel
        {
            if( string.IsNullOrEmpty( directoryPath ) )
                throw new ArgumentException( "Directory path cannot be null or empty.", nameof( directoryPath ) );
            if( !Directory.Exists( directoryPath ) )
                return default;

            refStore ??= new BidirectionalReferenceStore();

            // 1. Load _vessel.json
            VesselMetadata metadata = null;
            string metadataPath = Path.Combine( directoryPath, METADATA_FILENAME );
            if( File.Exists( metadataPath ) )
            {
                var vesselDataHandler = new FileSerializedDataHandler( metadataPath, JsonFormat.Instance );
                var vesselData = vesselDataHandler.Read();
                metadata = SerializationUnit.Deserialize<VesselMetadata>( vesselData, refStore );
            }

            // 2. Load parts.json
            string partsPath = Path.Combine( directoryPath, PARTS_FILENAME );
            GameObject[] partObjects = null;
            if( File.Exists( partsPath ) )
            {
                var partsDataHandler = new FileSerializedDataHandler( partsPath, JsonFormat.Instance );
                var partsData = partsDataHandler.Read();
                partObjects = SerializationUnit.Deserialize<GameObject[]>( partsData, refStore );
            }

            VesselPart[] parts = partObjects != null
                ? partObjects.Where( go => go != null ).Select( go => go.GetComponent<VesselPart>() ).Where( p => p != null ).ToArray()
                : Array.Empty<VesselPart>();

            // 3. Load graph.json
            VesselAttachmentGraph graph = null;
            string graphPath = Path.Combine( directoryPath, GRAPH_FILENAME );
            if( File.Exists( graphPath ) )
            {
                var graphDataHandler = new FileSerializedDataHandler( graphPath, JsonFormat.Instance );
                var graphData = graphDataHandler.Read();
                graph = SerializationUnit.Deserialize<VesselAttachmentGraph>( graphData, refStore );
            }

            if( graph == null || graph.Nodes.Count == 0 )
            {
                if( parts.Length > 0 )
                {
                    graph = VesselAttachmentGraph.Create( parts[0] );
                }
            }
            else
            {
                foreach( var part in parts )
                {
                    if( part != null )
                    {
                        if(!graph.HasNode( part ) )
                            graph.AddNode( part ); // ?
                    }
                }
            }

            // 4. Instantiate Vessel
            var vessel = VesselFactory.CreatePartless<T>( scene, pos, rot, vel, angVel );

            return vessel;
        }

        public static bool TryLoad()
        {

        }

        public static void SaveMetadata( VesselMetadata metadata, string directoryPath, IReverseReferenceMap refStore = null )
        {

        }

        /// <summary>
        /// Reads metadata from _vessel.json without loading parts or graph.
        /// </summary>
        public static VesselMetadata LoadMetadata( string directoryPath, IForwardReferenceMap refStore = null )
        {
            if( string.IsNullOrEmpty( directoryPath ) )
                return null;

            string metadataPath = Path.Combine( directoryPath, METADATA_FILENAME );
            if( !File.Exists( metadataPath ) )
                return null;

            var vesselDataHandler = new FileSerializedDataHandler( metadataPath, JsonFormat.Instance );
            var vesselData = vesselDataHandler.Read();
            return SerializationUnit.Deserialize<VesselMetadata>( vesselData, refStore );
        }
    }
}
