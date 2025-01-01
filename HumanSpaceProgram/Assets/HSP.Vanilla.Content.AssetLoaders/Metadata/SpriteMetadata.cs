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
                .WithMember( "rect", o => o.Rect )
                .WithMember( "pivot", o => o.Pivot )
                .WithMember( "border", CONTEXT_BORDER, o => o.Border );
        }

        [MapsInheritingFrom( typeof( Vector4 ), Context = CONTEXT_BORDER )]
        public static SerializationMapping Vector4BorderMapping()
        {
            return new MemberwiseSerializationMapping<Vector4>()
                .WithMember( "left", o => o.x )
                .WithMember( "right", o => o.z )
                .WithMember( "top", o => o.w )
                .WithMember( "bottom", o => o.y );
        }
        [MapsInheritingFrom( typeof( Rect ) )]
        public static SerializationMapping RectMapping()
        {
            return new MemberwiseSerializationMapping<Rect>()
                .WithMember( "x", o => o.x )
                .WithMember( "y", o => o.y )
                .WithMember( "width", o => o.width )
                .WithMember( "height", o => o.height );
        }
    }
}