using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Content.AssetLoaders.Metadata
{
    public class Texture2DMetadata
    {
        public GraphicsFormat GraphicsFormat { get; set; } = GraphicsFormat.R8G8B8A8_SRGB;

        public FilterMode FilterMode { get; set; } = FilterMode.Bilinear;

        public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.Repeat;

        public int MipMapCount { get; set; } = -1;

        public int AnisoLevel { get; set; } = 1;

        public bool Readable { get; set; } = true;

        [MapsInheritingFrom( typeof( Texture2DMetadata ) )]
        public static SerializationMapping Texture2DMetadataMapping()
        {
            return new MemberwiseSerializationMapping<Texture2DMetadata>()
                .WithMember( "graphics_format", o => o.GraphicsFormat )
                .WithMember( "filter_mode", o => o.FilterMode )
                .WithMember( "wrap_mode", o => o.WrapMode )
                .WithMember( "mipmap_count", o => o.MipMapCount )
                .WithMember( "aniso_level", o => o.AnisoLevel )
                .WithMember( "readable", o => o.Readable );
        }
    }
}