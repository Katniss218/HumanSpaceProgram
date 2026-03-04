using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class UnityComponentDescriptors
    {
        [MapsInheritingFrom( typeof( GameObject ) )]
        private static IDescriptor ProvideGameObject() => new GameObjectDescriptor();

        // --- BASE ---

        [MapsInheritingFrom( typeof( Transform ) )]
        public static IDescriptor Transform() => new MemberwiseDescriptor<Transform>()
            .WithMember( "localPosition", t => t.localPosition )
            .WithMember( "localRotation", t => t.localRotation )
            .WithMember( "localScale", t => t.localScale );

        // --- PHYSICS ---

        [MapsInheritingFrom( typeof( BoxCollider ) )]
        public static IDescriptor BoxCollider() => new MemberwiseDescriptor<BoxCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "size", c => c.size );

        [MapsInheritingFrom( typeof( SphereCollider ) )]
        public static IDescriptor SphereCollider() => new MemberwiseDescriptor<SphereCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "radius", c => c.radius );

        [MapsInheritingFrom( typeof( CapsuleCollider ) )]
        public static IDescriptor CapsuleCollider() => new MemberwiseDescriptor<CapsuleCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "radius", c => c.radius )
            .WithMember( "height", c => c.height )
            .WithMember( "direction", c => c.direction );

        [MapsInheritingFrom( typeof( MeshCollider ) )]
        public static IDescriptor MeshCollider() => new MemberwiseDescriptor<MeshCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "convex", c => c.convex )
            .WithMember( "sharedMesh", typeof( Ctx.Asset ), c => c.sharedMesh );

        // --- RENDERING ---

        [MapsInheritingFrom( typeof( MeshFilter ) )]
        public static IDescriptor MeshFilter() => new MemberwiseDescriptor<MeshFilter>()
            .WithMember( "sharedMesh", typeof( Ctx.Asset ), m => m.sharedMesh );

        [MapsInheritingFrom( typeof( MeshRenderer ) )]
        public static IDescriptor MeshRenderer() => new MemberwiseDescriptor<MeshRenderer>()
            .WithMember( "enabled", r => r.enabled )
            .WithMember( "shadowCastingMode", r => r.shadowCastingMode )
            .WithMember( "receiveShadows", r => r.receiveShadows )
            .WithMember( "sharedMaterials", typeof( Ctx.Asset ), r => r.sharedMaterials );

        [MapsInheritingFrom( typeof( Camera ) )]
        public static IDescriptor Camera() => new MemberwiseDescriptor<Camera>()
            .WithMember( "enabled", c => c.enabled )
            .WithMember( "clearFlags", c => c.clearFlags )
            .WithMember( "backgroundColor", c => c.backgroundColor )
            .WithMember( "cullingMask", c => c.cullingMask )
            .WithMember( "orthographic", c => c.orthographic )
            .WithMember( "orthographicSize", c => c.orthographicSize )
            .WithMember( "fieldOfView", c => c.fieldOfView )
            .WithMember( "nearClipPlane", c => c.nearClipPlane )
            .WithMember( "farClipPlane", c => c.farClipPlane )
            .WithMember( "depth", c => c.depth );

        [MapsInheritingFrom( typeof( Light ) )]
        public static IDescriptor Light() => new MemberwiseDescriptor<Light>()
            .WithMember( "enabled", l => l.enabled )
            .WithMember( "type", l => l.type )
            .WithMember( "color", l => l.color )
            .WithMember( "intensity", l => l.intensity )
            .WithMember( "range", l => l.range )
            .WithMember( "spotAngle", l => l.spotAngle )
            .WithMember( "shadows", l => l.shadows );

        [MapsInheritingFrom( typeof( LODGroup ) )]
        public static IDescriptor LODGroup() => new MemberwiseDescriptor<LODGroup>()
            .WithMember( "localReferencePoint", l => l.localReferencePoint )
            .WithMember( "size", l => l.size )
            .WithMember( "fadeMode", l => l.fadeMode )
            .WithMember( "animateCrossFading", l => l.animateCrossFading )
            .WithMember( "lods", l => l.GetLODs(), ( l, v ) => l.SetLODs( v ) );

        [MapsInheritingFrom( typeof( LOD ) )]
        public static IDescriptor LOD() => new MemberwiseDescriptor<LOD>()
            .WithMember( "screenRelativeTransitionHeight", l => l.screenRelativeTransitionHeight )
            .WithMember( "fadeTransitionWidth", l => l.fadeTransitionWidth )
            .WithMember( "renderers", typeof( Ctx.Ref ), l => l.renderers );
    }
}