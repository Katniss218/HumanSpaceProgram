using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Extensions;
using UnityEngine.Rendering;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_UnityComponents
    {
        [SerializationMappingProvider( typeof( Behaviour ) )]
        public static SerializationMapping BehaviourMapping()
        {
            return new MemberwiseSerializationMapping<Behaviour>()
            {
                ("is_enabled", new Member<Behaviour, bool>( o => o.enabled ))
            };
        }

        [SerializationMappingProvider( typeof( Component ) )]
        public static SerializationMapping ComponentMapping()
        {
            return new MemberwiseSerializationMapping<Component>()
                .WithFactory( ( data, l ) =>
                {
                    Guid id = data[KeyNames.ID].DeserializeGuid();

                    Component c = (Component)l.RefMap.GetObj( id );

                    return c;
                } );
        }

        [SerializationMappingProvider( typeof( GameObject ) )]
        public static SerializationMapping GameObjectMapping()
        {
            return new MemberwiseSerializationMapping<GameObject>()
            {
                ("name", new Member<GameObject, string>( o => o.name )),
                ("layer", new Member<GameObject, int>( o => o.layer )),
                ("is_active", new Member<GameObject, bool>( o => o.activeSelf, (o, value) => o.SetActive(value) )),
                ("is_static", new Member<GameObject, bool>( o => o.isStatic )),
                ("tag", new Member<GameObject, string>( o => o.tag )),
                ("children", new Member<GameObject, GameObject[]>( o =>
                {
                    return o.transform.Children().Select( child => child.gameObject ).ToArray();
                }, (o, value) =>
                {
                    foreach( var child in value )
                    {
                        child.transform.SetParent( o.transform );
                    }
                } )),
                ("components", new Member<GameObject, Component[]>( o => {
                return o.GetComponents();
                }, (o, value) =>
                {
                    // Do nothing, since the instantiated components are already part of the gameobject.
                    // This is very much a hack, but it's how Unity works :shrug:.
                } ))
            }.WithFactory( ( data, l ) =>
            {
                GameObject obj = new GameObject();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.RefMap.SetObj( id.DeserializeGuid(), obj );
                }
                // Instantiate components along the gameobject.
                // The component base class factory will then look up the component in the refmap ('$id'), instead of instantiating and setting it.
                if( data.TryGetValue<SerializedArray>( "components", out var components ) )
                {
                    foreach( var compData in components.OfType<SerializedObject>() )
                    {
                        try
                        {
                            Guid id2 = compData[KeyNames.ID].DeserializeGuid();
                            Type type = compData[KeyNames.TYPE].DeserializeType();

                            Component component = obj.GetTransformOrAddComponent( type );
                            if( component is Behaviour behaviour )
                            {
                                // Disable the behaviour to prevent 'start' from firing prematurely if deserializing over multiple frames.
                                // It will be re-enabled by SetData.
                                behaviour.enabled = false;
                            }

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

        [SerializationMappingProvider( typeof( Renderer ) )]
        public static SerializationMapping RendererMapping()
        {
            return new MemberwiseSerializationMapping<Renderer>()
            {
                ("is_enabled", new Member<Renderer, bool>( o => o.enabled ))
            };
        }

        [SerializationMappingProvider( typeof( Transform ) )]
        public static SerializationMapping TransformMapping()
        {
            return new MemberwiseSerializationMapping<Transform>()
            {
                ("local_position", new Member<Transform, Vector3>( o => o.localPosition )),
                ("local_rotation", new Member<Transform, Quaternion>( o => o.localRotation )),
                ("local_scale", new Member<Transform, Vector3>( o => o.localScale ))
            };
        }

        [SerializationMappingProvider( typeof( MeshFilter ) )]
        public static SerializationMapping MeshFilterMapping()
        {
            return new MemberwiseSerializationMapping<MeshFilter>()
            {
                ("shared_mesh", new Member<MeshFilter, Mesh>( ObjectContext.Asset, o => o.sharedMesh ))
            };
        }

        [SerializationMappingProvider( typeof( MeshRenderer ) )]
        public static SerializationMapping MeshRendererMapping()
        {
            return new MemberwiseSerializationMapping<MeshRenderer>()
            {
                ("shared_materials", new Member<MeshRenderer, Material[]>( ArrayContext.Assets, o => o.sharedMaterials )),
                ("shadow_casting_mode", new Member<MeshRenderer, ShadowCastingMode>( o => o.shadowCastingMode )),
                ("receive_shadows", new Member<MeshRenderer, bool>( o => o.receiveShadows ))
            };
        }

        [SerializationMappingProvider( typeof( Rigidbody ) )]
        public static SerializationMapping RigidbodyMapping()
        {
            return new MemberwiseSerializationMapping<Rigidbody>()
            {
                ("is_kinematic", new Member<Rigidbody, bool>( o => o.isKinematic ))
            };
        }

        [SerializationMappingProvider( typeof( Collider ) )]
        public static SerializationMapping ColliderMapping()
        {
            return new MemberwiseSerializationMapping<Collider>()
            {
                ("is_enabled", new Member<Collider, bool>( o => o.enabled )),
                ("is_trigger", new Member<Collider, bool>( o => o.isTrigger ))
            };
        }

        [SerializationMappingProvider( typeof( BoxCollider ) )]
        public static SerializationMapping BoxColliderMapping()
        {
            return new MemberwiseSerializationMapping<BoxCollider>()
            {
                ("size", new Member<BoxCollider, Vector3>( o => o.size )),
                ("center", new Member<BoxCollider, Vector3>( o => o.center ))
            };
        }

        [SerializationMappingProvider( typeof( SphereCollider ) )]
        public static SerializationMapping SphereColliderMapping()
        {
            return new MemberwiseSerializationMapping<SphereCollider>()
            {
                ("radius", new Member<SphereCollider, float>( o => o.radius )),
                ("center", new Member<SphereCollider, Vector3>( o => o.center ))
            };
        }

        [SerializationMappingProvider( typeof( CapsuleCollider ) )]
        public static SerializationMapping CapsuleColliderMapping()
        {
            return new MemberwiseSerializationMapping<CapsuleCollider>()
            {
                ("radius", new Member<CapsuleCollider, float>( o => o.radius )),
                ("height", new Member<CapsuleCollider, float>( o => o.height )),
                ("direction", new Member<CapsuleCollider, int>( o => o.direction )),
                ("center", new Member<CapsuleCollider, Vector3>( o => o.center ))
            };
        }

        [SerializationMappingProvider( typeof( MeshCollider ) )]
        public static SerializationMapping MeshColliderMapping()
        {
            return new MemberwiseSerializationMapping<MeshCollider>()
            {
                ("shared_mesh", new Member<MeshCollider, Mesh>( ObjectContext.Asset, o => o.sharedMesh )),
                ("is_convex", new Member<MeshCollider, bool>( o => o.convex ))
            };
        }

        [SerializationMappingProvider( typeof( LOD ) )]
        public static SerializationMapping LODMapping()
        {
            return new MemberwiseSerializationMapping<LOD>()
            {
                ("fade_width", new Member<LOD, float>( o => o.fadeTransitionWidth )),
                ("percent", new Member<LOD, float>( o => o.screenRelativeTransitionHeight )),
                ("renderers", new Member<LOD, Renderer[]>( ArrayContext.Refs, o => o.renderers ))
            };
        }

        [SerializationMappingProvider( typeof( LODGroup ) )]
        public static SerializationMapping LODGroupMapping()
        {
            return new MemberwiseSerializationMapping<LODGroup>()
            {
                ("size", new Member<LODGroup, float>( o => o.size )),
                ("lods", new Member<LODGroup, LOD[]>( o => o.GetLODs(), (o, value) => o.SetLODs( value ) ))
            };
        }
    }
}