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
            {
                ("graphics_format", new Member<Texture2DMetadata, GraphicsFormat>( o => o.GraphicsFormat )),
                ("filter_mode", new Member<Texture2DMetadata, FilterMode>( o => o.FilterMode )),
                ("wrap_mode", new Member<Texture2DMetadata, TextureWrapMode>( o => o.WrapMode )),
                ("mipmap_count", new Member<Texture2DMetadata, int>( o => o.MipMapCount )),
                ("aniso_level", new Member<Texture2DMetadata, int>( o => o.AnisoLevel )),
                ("readable", new Member<Texture2DMetadata, bool>( o => o.Readable ))
            };
        }
    }
}