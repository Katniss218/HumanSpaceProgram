using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Extensions;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_UnityComponents
    {
        [MapsInheritingFrom( typeof( ScriptableObject ) )]
        public static SerializationMapping ScriptableObjectMapping()
        {
            return new MemberwiseSerializationMapping<ScriptableObject>()
                .WithRawFactory( ( data, l ) =>
                {
                    if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                    {
                        Type soType = type.DeserializeType();

                        ScriptableObject obj = ScriptableObject.CreateInstance( soType );
                        if( data.TryGetValue( KeyNames.ID, out var id ) )
                        {
                            l.RefMap.SetObj( id.DeserializeGuid(), obj );
                        }
                        return obj;
                    }
                    return null;
                } );
        }

        [MapsInheritingFrom( typeof( Component ) )]
        public static SerializationMapping ComponentMapping()
        {
            return new MemberwiseSerializationMapping<Component>()
                .WithRawFactory( ( data, l ) =>
                {
                    Guid id = data[KeyNames.ID].DeserializeGuid();

                    Component c = (Component)l.RefMap.GetObj( id );

                    return c;
                } );
        }

        [MapsInheritingFrom( typeof( GameObject ) )]
        public static SerializationMapping GameObjectMapping()
        {
            return new GameObjectSerializationMapping()
                //.WithMember( "is_active", o => o.activeSelf, ( o, value ) => { /*o.SetActive( value )*/ } ) // handled by the GameObjectSerializationMapping itself.
                .WithMember( "is_static", o => o.isStatic )
                .WithMember( "layer", o => o.layer )
                .WithMember( "name", o => o.name )
                .WithMember( "tag", o => o.tag )
                .WithMember( "children", o =>
                    {
                        return o.transform.Children().Select( child => child.gameObject ).ToArray();
                    }, ( o, value ) =>
                    {
                        foreach( var child in value )           // The 'value' array here is a sort of 'virtual' array.
                        {
                            if( child == null ) // only true if setter invoked after failure.
                                continue;

                            child.transform.SetParent( o.transform, false );
                        }
                    } )
                .WithMember( "components", o =>
                    {
                        return o.GetComponents();
                    }, ( o, value ) =>
                    {
                        // Do nothing, since the instantiated components are already part of the gameobject.
                        // This is very much a hack, but it's how Unity works :shrug:.
                    } )
            .WithRawFactory( ( data, l ) =>
            {
                GameObject obj = new GameObject();
                obj.SetActive( false ); // Still needed because ne need to disable it before adding components.

                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.RefMap.SetObj( id.DeserializeGuid(), obj );
                }
                // Instantiate components along the gameobject.
                // The component base class factory will then look up the component in the refmap ('$id'), instead of instantiating and setting it.
                if( data.TryGetValue( "components", out var componentsO ) && componentsO.TryGetValue<SerializedArray>( "value", out var components ) )
                {
                    foreach( var compData in components.OfType<SerializedObject>() )
                    {
                        try
                        {
                            Guid id2 = compData[KeyNames.ID].DeserializeGuid();
                            Type type = compData[KeyNames.TYPE].DeserializeType();
                            if( type == null )
                                continue; // Skips adding to refmap

                            Component component = obj.GetTransformOrAddComponent( type );

                            l.RefMap.SetObj( id2, component );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Failed to deserialize a component with ID: `{(string)compData?[KeyNames.ID] ?? "<null>"}`." );
                            Debug.LogException( ex );
                        }
                    }
                }
                return obj;
            } );
        }

        [MapsInheritingFrom( typeof( Behaviour ) )]
        public static SerializationMapping BehaviourMapping()
        {
            return new MemberwiseSerializationMapping<Behaviour>()
                .WithMember( "is_enabled", o => o.enabled );
        }

        [MapsInheritingFrom( typeof( Renderer ) )]
        public static SerializationMapping RendererMapping()
        {
            return new MemberwiseSerializationMapping<Renderer>()
                .WithMember( "is_enabled", o => o.enabled );
        }

        [MapsInheritingFrom( typeof( Transform ) )]
        public static SerializationMapping TransformMapping()
        {
            return new MemberwiseSerializationMapping<Transform>()
                .WithMember( "local_position", o => o.localPosition )
                .WithMember( "local_rotation", o => o.localRotation )
                .WithMember( "local_scale", o => o.localScale );
        }

        [MapsInheritingFrom( typeof( MeshFilter ) )]
        public static SerializationMapping MeshFilterMapping()
        {
            return new MemberwiseSerializationMapping<MeshFilter>()
                .WithMember( "shared_mesh", ObjectContext.Asset, o => o.sharedMesh );
        }

        [MapsInheritingFrom( typeof( MeshRenderer ) )]
        public static SerializationMapping MeshRendererMapping()
        {
            return new MemberwiseSerializationMapping<MeshRenderer>()
                .WithMember( "shared_materials", ArrayContext.Assets, o => o.sharedMaterials )
                .WithMember( "shadow_casting_mode", o => o.shadowCastingMode )
                .WithMember( "receive_shadows", o => o.receiveShadows );
        }

        [MapsInheritingFrom( typeof( Rigidbody ) )]
        public static SerializationMapping RigidbodyMapping()
        {
            return new MemberwiseSerializationMapping<Rigidbody>()
                .WithMember( "is_kinematic", o => o.isKinematic );
        }

        [MapsInheritingFrom( typeof( Collider ) )]
        public static SerializationMapping ColliderMapping()
        {
            return new MemberwiseSerializationMapping<Collider>()
                .WithMember( "is_enabled", o => o.enabled )
                .WithMember( "is_trigger", o => o.isTrigger );
        }

        [MapsInheritingFrom( typeof( BoxCollider ) )]
        public static SerializationMapping BoxColliderMapping()
        {
            return new MemberwiseSerializationMapping<BoxCollider>()
                .WithMember( "size", o => o.size )
                .WithMember( "center", o => o.center );
        }

        [MapsInheritingFrom( typeof( SphereCollider ) )]
        public static SerializationMapping SphereColliderMapping()
        {
            return new MemberwiseSerializationMapping<SphereCollider>()
                .WithMember( "radius", o => o.radius )
                .WithMember( "center", o => o.center );
        }

        [MapsInheritingFrom( typeof( CapsuleCollider ) )]
        public static SerializationMapping CapsuleColliderMapping()
        {
            return new MemberwiseSerializationMapping<CapsuleCollider>()
                .WithMember( "radius", o => o.radius )
                .WithMember( "height", o => o.height )
                .WithMember( "direction", o => o.direction )
                .WithMember( "center", o => o.center );
        }

        [MapsInheritingFrom( typeof( MeshCollider ) )]
        public static SerializationMapping MeshColliderMapping()
        {
            return new MemberwiseSerializationMapping<MeshCollider>()
                .WithMember( "shared_mesh", ObjectContext.Asset, o => o.sharedMesh )
                .WithMember( "is_convex", o => o.convex );
        }

        [MapsInheritingFrom( typeof( LOD ) )]
        public static SerializationMapping LODMapping()
        {
            return new MemberwiseSerializationMapping<LOD>()
                .WithMember( "fade_width", o => o.fadeTransitionWidth )
                .WithMember( "percent", o => o.screenRelativeTransitionHeight )
                .WithMember( "renderers", ArrayContext.Refs, o => o.renderers );
        }

        [MapsInheritingFrom( typeof( LODGroup ) )]
        public static SerializationMapping LODGroupMapping()
        {
            return new MemberwiseSerializationMapping<LODGroup>()
                .WithMember( "size", o => o.size )
                .WithMember( "lods", o => o.GetLODs(), ( o, value ) => o.SetLODs( value ) );
        }
    }
}