using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Content.AssetLoaders.Metadata
{
    public class SpriteMetadata
    {
        const int CONTEXT_BORDER = -377894935;

        public Vector4 SliceBorder { get; set; } = Vector4.zero;

        /*[MapsInheritingFrom( typeof( SpriteMetadata ) )]
        public static SerializationMapping SpriteMetadataMapping()
        {
        // NEEDS GOOD SUPPORT FOR IMMUTABLE TYPES VIA MEMBERS FIRST - Sprite.CreateSprite constructor
            return new MemberwiseSerializationMapping<SpriteMetadata>()
            {
                ("slice_border", new Member<SpriteMetadata, Vector4>( CONTEXT_BORDER, o => o.SliceBorder ))
            };
        }*/

        [MapsInheritingFrom( typeof( Vector4 ), Context = CONTEXT_BORDER )]
        public static SerializationMapping Vector4BorderMapping()
        {
            return new MemberwiseSerializationMapping<Vector4>()
            {
                ("left", new Member<Vector4, float>( o => o.x )),
                ("right", new Member<Vector4, float>( o => o.z )),
                ("top", new Member<Vector4, float>( o => o.w )),
                ("bottom", new Member<Vector4, float>( o => o.y ))
            };
        }
    }
}