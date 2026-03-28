using UnityEngine;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    public static class UnityComponentDescriptors
    {
        [MapsInheritingFrom( typeof( GameObject ) )]
        private static IDescriptor ProvideGameObject() => new GameObjectDescriptor();

        // --- BASE ---

        [MapsInheritingFrom( typeof( Transform ) )]
        public static IDescriptor Transform() => new MemberwiseDescriptor<Transform>()
            .WithMember( "local_position", t => t.localPosition )
            .WithMember( "local_rotation", t => t.localRotation )
            .WithMember( "local_scale", t => t.localScale );

        // --- PHYSICS ---

        [MapsInheritingFrom( typeof( BoxCollider ) )]
        public static IDescriptor BoxCollider() => new MemberwiseDescriptor<BoxCollider>()
            .WithMember( "is_trigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "size", c => c.size );

        [MapsInheritingFrom( typeof( SphereCollider ) )]
        public static IDescriptor SphereCollider() => new MemberwiseDescriptor<SphereCollider>()
            .WithMember( "is_trigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "radius", c => c.radius );

        [MapsInheritingFrom( typeof( CapsuleCollider ) )]
        public static IDescriptor CapsuleCollider() => new MemberwiseDescriptor<CapsuleCollider>()
            .WithMember( "is_trigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "radius", c => c.radius )
            .WithMember( "height", c => c.height )
            .WithMember( "direction", c => c.direction );

        [MapsInheritingFrom( typeof( MeshCollider ) )]
        public static IDescriptor MeshCollider() => new MemberwiseDescriptor<MeshCollider>()
            .WithMember( "is_trigger", c => c.isTrigger )
            .WithMember( "convex", c => c.convex )
            .WithMember( "shared_mesh", typeof( Ctx.Asset ), c => c.sharedMesh );

        // --- RENDERING ---

        [MapsInheritingFrom( typeof( MeshFilter ) )]
        public static IDescriptor MeshFilter() => new MemberwiseDescriptor<MeshFilter>()
            .WithMember( "shared_mesh", typeof( Ctx.Asset ), m => m.sharedMesh );

        [MapsInheritingFrom( typeof( MeshRenderer ) )]
        public static IDescriptor MeshRenderer() => new MemberwiseDescriptor<MeshRenderer>()
            .WithMember( "enabled", r => r.enabled )
            .WithMember( "shadow_casting_mode", r => r.shadowCastingMode )
            .WithMember( "receive_shadows", r => r.receiveShadows )
            .WithMember( "shared_materials", typeof( Ctx.Array<Ctx.Asset> ), r => r.sharedMaterials );

        [MapsInheritingFrom( typeof( Camera ) )]
        public static IDescriptor Camera() => new MemberwiseDescriptor<Camera>()
            .WithMember( "enabled", c => c.enabled )
            .WithMember( "clear_flags", c => c.clearFlags )
            .WithMember( "background_color", c => c.backgroundColor )
            .WithMember( "culling_mask", c => c.cullingMask )
            .WithMember( "orthographic", c => c.orthographic )
            .WithMember( "orthographic_size", c => c.orthographicSize )
            .WithMember( "field_of_view", c => c.fieldOfView )
            .WithMember( "near_clip_plane", c => c.nearClipPlane )
            .WithMember( "far_clip_plane", c => c.farClipPlane )
            .WithMember( "depth", c => c.depth );

        [MapsInheritingFrom( typeof( Light ) )]
        public static IDescriptor Light() => new MemberwiseDescriptor<Light>()
            .WithMember( "enabled", l => l.enabled )
            .WithMember( "type", l => l.type )
            .WithMember( "color", l => l.color )
            .WithMember( "intensity", l => l.intensity )
            .WithMember( "range", l => l.range )
            .WithMember( "spot_angle", l => l.spotAngle )
            .WithMember( "shadows", l => l.shadows );

        [MapsInheritingFrom( typeof( LODGroup ) )]
        public static IDescriptor LODGroup() => new MemberwiseDescriptor<LODGroup>()
            .WithMember( "local_reference_point", l => l.localReferencePoint )
            .WithMember( "size", l => l.size )
            .WithMember( "fade_mode", l => l.fadeMode )
            .WithMember( "animate_cross_fading", l => l.animateCrossFading )
            .WithMember( "lods", l => l.GetLODs(), ( l, v ) => l.SetLODs( v ) );

        [MapsInheritingFrom( typeof( LOD ) )]
        public static IDescriptor LOD() => new MemberwiseDescriptor<LOD>()
            .WithMember( "screen_relative_transition_height", l => l.screenRelativeTransitionHeight )
            .WithMember( "fade_transition_width", l => l.fadeTransitionWidth )
            .WithMember( "renderers", typeof( Ctx.Array<Ctx.Ref> ), l => l.renderers );
    }
}