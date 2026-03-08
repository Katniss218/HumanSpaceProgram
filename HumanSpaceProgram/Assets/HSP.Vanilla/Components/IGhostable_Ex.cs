using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using Ctx = UnityPlus.Serialization.Ctx;

namespace HSP.Vanilla.Components
{
    public static class IGhostable_Ex
    {
        [MapsInheritingFrom( typeof( Renderer ), ContextType = typeof( Ctx.Ghost ) )]
        public static IDescriptor RendererMapping()
        {
            return new MemberwiseDescriptor<Renderer>()
                .WithMember( "shared_materials", typeof( Ctx.Array<Ctx.Asset> ),
                    o => o.sharedMaterials.Select( m => AssetRegistry.Get<Material>( "builtin::Resources/Materials/ghost_wireframe" ) ).ToArray(),
                   ( o, value ) => o.sharedMaterials = value );
        }

        [MapsInheritingFrom( typeof( Collider ), ContextType = typeof( Ctx.Ghost ) )]
        public static IDescriptor ColliderMapping()
        {
            return new MemberwiseDescriptor<Collider>()
                .WithMember( "is_trigger", o => true, ( o, value ) => o.isTrigger = value );
        }

        [MapsInheritingFrom( typeof( FPointMass ), ContextType = typeof( Ctx.Ghost ) )]
        public static IDescriptor FPointMassMapping()
        {
            return new MemberwiseDescriptor<FPointMass>()
                .WithMember( "mass", o => 0.0f, ( o, value ) => o.Mass = value );
        }
    }
}