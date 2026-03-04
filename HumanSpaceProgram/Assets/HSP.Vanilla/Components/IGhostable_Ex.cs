using HSP.Vessels.Construction;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public static class IGhostable_Ex
    {
        [MapsInheritingFrom( typeof( Renderer ), Context = GhostableContext.Ghost )]
        public static IDescriptor RendererMapping()
        {
            return new MemberwiseDescriptor<Renderer>()
                .WithMember( "shared_materials", ArrayContext.Assets,
                    o => o.sharedMaterials.Select( m => AssetRegistry.Get<Material>( "builtin::Resources/Materials/ghost_wireframe" ) ).ToArray(),
                   ( o, value ) => o.sharedMaterials = value );
        }

        [MapsInheritingFrom( typeof( Collider ), Context = GhostableContext.Ghost )]
        public static IDescriptor ColliderMapping()
        {
            return new MemberwiseDescriptor<Collider>()
                .WithMember( "is_trigger", o => true, ( o, value ) => o.isTrigger = value );
        }

        [MapsInheritingFrom( typeof( FPointMass ), Context = GhostableContext.Ghost )]
        public static IDescriptor FPointMassMapping()
        {
            return new MemberwiseDescriptor<FPointMass>()
                .WithMember( "mass", o => 0.0f, ( o, value ) => o.Mass = value );
        }
    }
}