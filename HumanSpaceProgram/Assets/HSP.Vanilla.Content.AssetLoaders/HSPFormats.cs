using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    /// <summary>
    /// Definitions for custom binary and text formats used by Human Space Program.
    /// </summary>
    public static class HSPFormats
    {
        /// <summary>HSP Mesh (Binary). Signature: "HSPM"</summary>
        public static readonly AssetFormat Hspm = new AssetFormat( "HSPM" );

        /// <summary>HSP Animation (Binary). Signature: "HSPA"</summary>
        public static readonly AssetFormat Hspa = new AssetFormat( "HSPA" );

        /// <summary>HSP Material Definition (JSON). Extension: .jsonmat</summary>
        public static readonly AssetFormat JsonMat = new AssetFormat( "JSONMAT" );

        /// <summary>Virtual Format for resizing textures on load.</summary>
        public static readonly AssetFormat VirtualResizeOp = new AssetFormat( "VIRT_RESIZE" );

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, "a" )]
        private static void Bootstrap()
        {
            // Register HSPM (0x4D505348)
            AssetFormatRegistry.RegisterExtension( ".hspm", Hspm );
            AssetFormatRegistry.RegisterSignature( Hspm, new byte[] { 0x48, 0x53, 0x50, 0x4D } ); // "HSPM" in bytes

            // Register HSPA (0x41505348)
            AssetFormatRegistry.RegisterExtension( ".hspa", Hspa );
            AssetFormatRegistry.RegisterSignature( Hspa, new byte[] { 0x48, 0x53, 0x50, 0x41 } ); // "HSPA" in bytes

            // Register JSON types
            // Note: Since these are normal JSON, they rely entirely on the file extension.
            AssetFormatRegistry.RegisterExtension( ".jsonmat", JsonMat );

            // Virtual operations
            AssetFormatRegistry.RegisterExtension( ".virtual_resize_op", VirtualResizeOp );
        }
    }
}