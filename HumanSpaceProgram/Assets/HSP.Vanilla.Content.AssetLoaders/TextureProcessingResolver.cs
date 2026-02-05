using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class TextureProcessingResolver : IAssetResolver
    {
        public const string REGISTER_TEX_PROC_RESOLVER = HSPEvent.NAMESPACE_HSP + ".gdas.register_tex_proc_resolver";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, REGISTER_TEX_PROC_RESOLVER )]
        private static void RegisterTextureProcessingResolver()
        {
            AssetRegistry.RegisterResolver( new TextureProcessingResolver() );
        }

        public const string TOPOLOGICAL_ID = "hsp.textureresizeresolver";

        public string ID => TOPOLOGICAL_ID;

        public string[] Before => null;
        
        public string[] After => new[] { GameDataFileResolver.TOPOLOGICAL_ID }; // File resolvers must run after it, to intercept the query parameter.
        public string[] Blacklist => null;

        public bool CanResolve( AssetUri uri, Type targetType )
        {
            // If the user is requesting something that isn't a Texture2D (or base Texture, or object),
            // we should not intercept, even if "?resize" is present.
            // This allows requests like "mesh.obj?resize=x" to fall through to the file resolver 
            // which will ignore the query param and load the mesh normally.
            if( !targetType.IsAssignableFrom( typeof( Texture2D ) ) )
            {
                return false;
            }

            return uri.QueryParams != null && uri.QueryParams.ContainsKey( "resize" );
        }

        public Task<AssetDataHandle> ResolveAsync( AssetUri uri, Type targetType, CancellationToken ct )
        {
            if( uri.QueryParams.TryGetValue( "resize", out string sizeStr ) && int.TryParse( sizeStr, out int size ) )
            {
                // Construct the ID for the base asset (stripping the resize param).
                // TODO - Keep any other params in case the base loader needs them.

                string baseId = uri.BaseID;

                // Simple parser for "resize=128x64" could go here if needed.
                int width = size;
                int height = size;


                return Task.FromResult<AssetDataHandle>( new VirtualResizeAssetDataHandle( baseId, width, height ) );
            }

            return Task.FromResult<AssetDataHandle>( null );
        }
    }
}
