using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.Content.AssetLoaders.Metadata
{
    public class SpriteMetadata
    {
        public static class Ctx
        {
            public interface Border : IContext { }
        }

        public Rect Rect { get; set; } // refault to full size
        public Vector2 Pivot { get; set; } // default to center
        public Vector4 Border { get; set; } = Vector4.zero;

        [MapsInheritingFrom( typeof( SpriteMetadata ) )]
        public static IDescriptor SpriteMetadataMapping()
        {
            return new MemberwiseDescriptor<SpriteMetadata>()
                .WithMember( "rect", o => o.Rect )
                .WithMember( "pivot", o => o.Pivot )
                .WithMember( "border", typeof( Ctx.Border ), o => o.Border );
        }

        [MapsInheritingFrom( typeof( Vector4 ), ContextType = typeof( Ctx.Border ) )]
        public static IDescriptor Vector4BorderMapping()
        {
            return new MemberwiseDescriptor<Vector4>()
                .WithMember( "left", o => o.x )
                .WithMember( "right", o => o.z )
                .WithMember( "top", o => o.w )
                .WithMember( "bottom", o => o.y );
        }
    }
}