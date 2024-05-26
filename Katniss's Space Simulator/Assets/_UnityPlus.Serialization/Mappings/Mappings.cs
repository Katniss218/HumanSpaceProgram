using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Extensions;
using UnityEngine.Rendering;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings
    {
        [SerializationMappingProvider( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new PrimitiveStructSerializationMapping<bool>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (bool)data
            };
        }

        [SerializationMappingProvider( typeof( byte ) )]
        public static SerializationMapping ByteMapping()
        {
            return new PrimitiveStructSerializationMapping<byte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (byte)data
            };
        }

        [SerializationMappingProvider( typeof( sbyte ) )]
        public static SerializationMapping SByteMapping()
        {
            return new PrimitiveStructSerializationMapping<sbyte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (sbyte)data
            };
        }

        [SerializationMappingProvider( typeof( short ) )]
        public static SerializationMapping Int16Mapping()
        {
            return new PrimitiveStructSerializationMapping<short>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (short)data
            };
        }

        [SerializationMappingProvider( typeof( ushort ) )]
        public static SerializationMapping UInt16Mapping()
        {
            return new PrimitiveStructSerializationMapping<ushort>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ushort)data
            };
        }

        [SerializationMappingProvider( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new PrimitiveStructSerializationMapping<int>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (int)data
            };
        }

        [SerializationMappingProvider( typeof( uint ) )]
        public static SerializationMapping UInt32Mapping()
        {
            return new PrimitiveStructSerializationMapping<uint>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (uint)data
            };
        }

        [SerializationMappingProvider( typeof( long ) )]
        public static SerializationMapping Int64Mapping()
        {
            return new PrimitiveStructSerializationMapping<long>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (long)data
            };
        }

        [SerializationMappingProvider( typeof( ulong ) )]
        public static SerializationMapping UInt64Mapping()
        {
            return new PrimitiveStructSerializationMapping<ulong>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ulong)data
            };
        }

        [SerializationMappingProvider( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new PrimitiveStructSerializationMapping<float>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (float)data
            };
        }

        [SerializationMappingProvider( typeof( double ) )]
        public static SerializationMapping DoubleMapping()
        {
            return new PrimitiveStructSerializationMapping<double>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (double)data
            };
        }

        [SerializationMappingProvider( typeof( decimal ) )]
        public static SerializationMapping DecimalMapping()
        {
            return new PrimitiveStructSerializationMapping<decimal>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (decimal)data
            };
        }

        [SerializationMappingProvider( typeof( char ) )]
        public static SerializationMapping CharMapping()
        {
            return new PrimitiveStructSerializationMapping<char>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)(o.ToString()),
                OnInstantiate = ( data, l ) => ((string)data)[0]
            };
        }

        [SerializationMappingProvider( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new PrimitiveStructSerializationMapping<string>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (string)data
            };
        }

        [SerializationMappingProvider( typeof( DateTime ) )]
        public static SerializationMapping DateTimeMapping()
        {
            // DateTime is saved as an ISO-8601 string.
            return new PrimitiveStructSerializationMapping<DateTime>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "s", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => DateTime.ParseExact( (string)data, "s", CultureInfo.InvariantCulture )
            };
        }

        [SerializationMappingProvider( typeof( TimeSpan ) )]
        public static SerializationMapping TimeSpanMapping()
        {
            // TimeSpan is saved as `[-][d'.']hh':'mm':'ss['.'fffffff]`.
            return new PrimitiveStructSerializationMapping<TimeSpan>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "c", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => TimeSpan.ParseExact( (string)data, "c", CultureInfo.InvariantCulture )
            };
        }



        [SerializationMappingProvider( typeof( Vector2 ) )]
        public static SerializationMapping Vector2Mapping()
        {
            return new PrimitiveStructSerializationMapping<Vector2>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnInstantiate = ( data, l ) => new Vector2( (float)data[0], (float)data[1] )
            };
        }

        [SerializationMappingProvider( typeof( Vector2Int ) )]
        public static SerializationMapping Vector2IntMapping()
        {
            return new PrimitiveStructSerializationMapping<Vector2Int>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnInstantiate = ( data, l ) => new Vector2Int( (int)data[0], (int)data[1] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new PrimitiveStructSerializationMapping<Vector3>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3Int ) )]
        public static SerializationMapping Vector3IntMapping()
        {
            return new PrimitiveStructSerializationMapping<Vector3Int>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3Int( (int)data[0], (int)data[1], (int)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3Dbl ) )]
        public static SerializationMapping Vector3DblMapping()
        {
            return new PrimitiveStructSerializationMapping<Vector3Dbl>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3Dbl( (double)data[0], (double)data[1], (double)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector4 ) )]
        public static SerializationMapping Vector4Mapping()
        {
            return new PrimitiveStructSerializationMapping<Vector4>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new Vector4( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new PrimitiveStructSerializationMapping<Quaternion>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( QuaternionDbl ) )]
        public static SerializationMapping QuaternionDblMapping()
        {
            return new PrimitiveStructSerializationMapping<QuaternionDbl>()
            {
                OnSave = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new QuaternionDbl( (double)data[0], (double)data[1], (double)data[2], (double)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Enum ) )]
        public static SerializationMapping EnumMapping<T>() where T : struct, Enum
        {
            return new PrimitiveStructSerializationMapping<T>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "G" ),
                OnInstantiate = ( data, l ) => Enum.Parse<T>( (string)data )
            };
        }

        [SerializationMappingProvider( typeof( Array ) )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new NonPrimitiveSerializationMapping<T[]>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray();
                    for( int i = 0; i < o.Length; i++ )
                    {
                        T value = o[i];

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( value );

                        var data = mapping.Save( value, s );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                OnInstantiate = ( data, l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return new T[] { };

                    return new T[serializedArray.Count];
                },
                OnLoad = ( ref T[] o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return;

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        Type elementType = typeof( T );
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( elementType );

                        // Parity with Member.
                        T element;
                        switch( mapping.SerializationStyle )
                        {
                            default:
                                continue;
                            case SerializationStyle.PrimitiveStruct:
                                element = (T)mapping.Instantiate( elementData, l );
                                break;
                            case SerializationStyle.NonPrimitive:
                                object refmember = (T)mapping.Instantiate( elementData, l );
                                mapping.Load( ref refmember, elementData, l );
                                element = (T)refmember;
                                break;
                        }

                        o[i] = element;
                    }

                    //return o;
                },
                OnLoadReferences = ( ref T[] o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return;

                    for( int i = 0; i < o.Length; i++ )
                    {
                        T element = o[i];
                        SerializedData elementData = serializedArray[i];
                        /*
                        Type elementType = typeof( T );
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( elementType );
                        */
                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( element );

                        // Parity with Member.
                        switch( mapping.SerializationStyle )
                        {
                            default:
                                continue;
                            case SerializationStyle.PrimitiveObject:
                                element = (T)mapping.Instantiate( elementData, l );
                                break;
                            case SerializationStyle.NonPrimitive:
                                object refmember = element;
                                mapping.LoadReferences( ref refmember, elementData, l );
                                element = (T)refmember;
                                break;
                        }

                        o[i] = element;
                    }
                }
            };
        }

        [SerializationMappingProvider( typeof( Dictionary<,> ) )]
        public static SerializationMapping Dictionary_TKey_TValue_Mapping<TKey, TValue>()
        {
            // Assume the dictionary is saved as a mapping from references to values.

#warning TODO - we might want to save the dict as a mapping from references to references.

            return new NonPrimitiveSerializationMapping<Dictionary<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedObject obj = new SerializedObject();

                    foreach( var (key, value) in o )
                    {
                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( value );

                        var data = mapping.Save( value, s );

                        string keyName = s.GetID( key ).SerializeGuidAsKey();
                        obj[keyName] = data;
                    }

                    return obj;
                },
                OnInstantiate = ( data, l ) =>
                {
                    return new Dictionary<TKey, TValue>();
                },
                OnLoad = null,
                OnLoadReferences = ( ref Dictionary<TKey, TValue> o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    if( data is not SerializedObject dataObj )
                        return;

                    foreach( var (key, value) in dataObj )
                    {
                        SerializedData elementData = value;

                        Type elementType = typeof( TValue );
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( elementType );

#warning TODO - fix this mess.
                        // Calling `mapping.Load` inside LoadReferences makes the objects inside the dict unable to be referenced by other external objects.
                        object elem = mapping.Instantiate( elementData, l );

                        mapping.Load( ref elem, elementData, l );
                        mapping.LoadReferences( ref elem, elementData, l );

                        TKey keyObj = (TKey)l.GetObj( key.DeserializeGuidAsKey() );

                        o[keyObj] = (TValue)elem;
                    }
                }
            };
        }



        [SerializationMappingProvider( typeof( Behaviour ) )]
        public static SerializationMapping BehaviourMapping()
        {
            return new CompoundSerializationMapping<Behaviour>()
            {
                ("is_enabled", new Member<Behaviour, bool>( o => o.enabled ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( Component ) )]
        public static SerializationMapping ComponentMapping()
        {
            return new CompoundSerializationMapping<Component>()
                .WithFactory( ( data, l ) =>
                {
                    Guid id = data[KeyNames.ID].DeserializeGuid();

                    Component c = (Component)l.GetObj( id );

                    return c;
                } );
        }

        [SerializationMappingProvider( typeof( GameObject ) )]
        public static SerializationMapping GameObjectMapping()
        {
            return new CompoundSerializationMapping<GameObject>()
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
                var obj = new GameObject();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.SetObj( id.DeserializeGuid(), obj );
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

                            l.SetObj( id2, component );
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
            return new CompoundSerializationMapping<Renderer>()
            {
                ("is_enabled", new Member<Renderer, bool>( o => o.enabled ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( Transform ) )]
        public static SerializationMapping TransformMapping()
        {
            return new CompoundSerializationMapping<Transform>()
            {
                ("local_position", new Member<Transform, Vector3>( o => o.localPosition )),
                ("local_rotation", new Member<Transform, Quaternion>( o => o.localRotation )),
                ("local_scale", new Member<Transform, Vector3>( o => o.localScale ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( MeshFilter ) )]
        public static SerializationMapping MeshFilterMapping()
        {
            return new CompoundSerializationMapping<MeshFilter>()
            {
                ("shared_mesh", new MemberAsset<MeshFilter, Mesh>( o => o.sharedMesh ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( MeshRenderer ) )]
        public static SerializationMapping MeshRendererMapping()
        {
            return new CompoundSerializationMapping<MeshRenderer>()
            {
                ("shared_materials", new MemberAssetArray<MeshRenderer, Material>( o => o.sharedMaterials )),
                ("shadow_casting_mode", new Member<MeshRenderer, ShadowCastingMode>( o => o.shadowCastingMode )),
                ("receive_shadows", new Member<MeshRenderer, bool>( o => o.receiveShadows ))
            }
            .UseBaseTypeFactory()
            .IncludeMembers<Renderer>();
        }

        [SerializationMappingProvider( typeof( Rigidbody ) )]
        public static SerializationMapping RigidbodyMapping()
        {
            return new CompoundSerializationMapping<Rigidbody>()
            {
                ("is_kinematic", new Member<Rigidbody, bool>( o => o.isKinematic ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( BoxCollider ) )]
        public static SerializationMapping BoxColliderMapping()
        {
            return new CompoundSerializationMapping<BoxCollider>()
            {
                ("size", new Member<BoxCollider, Vector3>( o => o.size )),
                ("center", new Member<BoxCollider, Vector3>( o => o.center )),
                ("is_trigger", new Member<BoxCollider, bool>( o => o.isTrigger ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( SphereCollider ) )]
        public static SerializationMapping SphereColliderMapping()
        {
            return new CompoundSerializationMapping<SphereCollider>()
            {
                ("radius", new Member<SphereCollider, float>( o => o.radius )),
                ("center", new Member<SphereCollider, Vector3>( o => o.center )),
                ("is_trigger", new Member<SphereCollider, bool>( o => o.isTrigger ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( CapsuleCollider ) )]
        public static SerializationMapping CapsuleColliderMapping()
        {
            return new CompoundSerializationMapping<CapsuleCollider>()
            {
                ("radius", new Member<CapsuleCollider, float>( o => o.radius )),
                ("height", new Member<CapsuleCollider, float>( o => o.height )),
                ("direction", new Member<CapsuleCollider, int>( o => o.direction )),
                ("center", new Member<CapsuleCollider, Vector3>( o => o.center )),
                ("is_trigger", new Member<CapsuleCollider, bool>( o => o.isTrigger ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( MeshCollider ) )]
        public static SerializationMapping MeshColliderMapping()
        {
            return new CompoundSerializationMapping<MeshCollider>()
            {
                ("shared_mesh", new MemberAsset<MeshCollider, Mesh>( o => o.sharedMesh )),
                ("is_convex", new Member<MeshCollider, bool>( o => o.convex )),
                ("is_trigger", new Member<MeshCollider, bool>( o => o.isTrigger ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( LOD ) )]
        public static SerializationMapping LODMapping()
        {
            return new CompoundSerializationMapping<LOD>()
            {
                ("fade_width", new Member<LOD, float>( o => o.fadeTransitionWidth )),
                ("percent", new Member<LOD, float>( o => o.screenRelativeTransitionHeight )),
                //("renderers", new MemberReference<LOD, Renderer[]>( o => o.renderers ))
                ("renderers", new MemberReferenceArray<LOD, Renderer>( o => o.renderers ))
            };
        }

        [SerializationMappingProvider( typeof( LODGroup ) )]
        public static SerializationMapping LODGroupMapping()
        {
            return new CompoundSerializationMapping<LODGroup>()
            {
                ("size", new Member<LODGroup, float>( o => o.size )),
                ("lods", new Member<LODGroup, LOD[]>( o => o.GetLODs(), (o, value) => o.SetLODs( value ) ))
            }
            .UseBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( Delegate ) )]
        public static SerializationMapping DelegateMapping()
        {
            return new PrimitiveObjectSerializationMapping<Delegate>()
            {
                OnSave = ( o, s ) =>
                {
                    return Persistent_Delegate.GetData( o, s );
                },
                OnInstantiate = ( SerializedData data, IForwardReferenceMap l ) =>
                {
                    // This is kinda non-standard, but since we need the reference to the `target` to even create the delegate, we can only create it here.
                    return Persistent_Delegate.ToDelegate( data, l );
                }
            };
        }
    }
}