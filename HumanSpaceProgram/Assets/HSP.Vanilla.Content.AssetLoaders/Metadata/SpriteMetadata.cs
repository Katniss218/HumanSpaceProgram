using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Content.AssetLoaders.Metadata
{
    public class SpriteMetadata
    {
        const int CONTEXT_BORDER = -377894935;

        public Rect Rect { get; set; } // refault to full size
        public Vector2 Pivot { get; set; } // default to center
        public Vector4 Border { get; set; } = Vector4.zero;

        [MapsInheritingFrom( typeof( SpriteMetadata ) )]
        public static SerializationMapping SpriteMetadataMapping()
        {
            return new MemberwiseSerializationMapping<SpriteMetadata>()
            {
                ("rect", new Member<SpriteMetadata, Rect>( o => o.Rect )),
                ("pivot", new Member<SpriteMetadata, Vector2>( o => o.Pivot )),
                ("border", new Member<SpriteMetadata, Vector4>( CONTEXT_BORDER, o => o.Border ))
            };
        }

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
        [MapsInheritingFrom( typeof( Rect ) )]
        public static SerializationMapping RectMapping()
        {
            return new MemberwiseSerializationMapping<Rect>()
            {
                ("x", new Member<Rect, float>( o => o.x )),
                ("y", new Member<Rect, float>( o => o.y )),
                ("width", new Member<Rect, float>( o => o.width )),
                ("height", new Member<Rect, float>( o => o.height ))
            };
        }
    }
}