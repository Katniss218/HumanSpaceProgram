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
            return new DirectSerializationMapping<bool>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (bool)data
            };
        }

        [SerializationMappingProvider( typeof( byte ) )]
        public static SerializationMapping ByteMapping()
        {
            return new DirectSerializationMapping<byte>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (byte)data
            };
        }

        [SerializationMappingProvider( typeof( sbyte ) )]
        public static SerializationMapping SByteMapping()
        {
            return new DirectSerializationMapping<sbyte>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (sbyte)data
            };
        }

        [SerializationMappingProvider( typeof( short ) )]
        public static SerializationMapping Int16Mapping()
        {
            return new DirectSerializationMapping<short>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (short)data
            };
        }

        [SerializationMappingProvider( typeof( ushort ) )]
        public static SerializationMapping UInt16Mapping()
        {
            return new DirectSerializationMapping<ushort>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (ushort)data
            };
        }

        [SerializationMappingProvider( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new DirectSerializationMapping<int>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (int)data
            };
        }

        [SerializationMappingProvider( typeof( uint ) )]
        public static SerializationMapping UInt32Mapping()
        {
            return new DirectSerializationMapping<uint>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (uint)data
            };
        }

        [SerializationMappingProvider( typeof( long ) )]
        public static SerializationMapping Int64Mapping()
        {
            return new DirectSerializationMapping<long>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (long)data
            };
        }

        [SerializationMappingProvider( typeof( ulong ) )]
        public static SerializationMapping UInt64Mapping()
        {
            return new DirectSerializationMapping<ulong>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (ulong)data
            };
        }

        [SerializationMappingProvider( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new DirectSerializationMapping<float>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (float)data
            };
        }

        [SerializationMappingProvider( typeof( double ) )]
        public static SerializationMapping DoubleMapping()
        {
            return new DirectSerializationMapping<double>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (double)data
            };
        }

        [SerializationMappingProvider( typeof( decimal ) )]
        public static SerializationMapping DecimalMapping()
        {
            return new DirectSerializationMapping<decimal>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (decimal)data
            };
        }

        [SerializationMappingProvider( typeof( char ) )]
        public static SerializationMapping CharMapping()
        {
            return new DirectSerializationMapping<char>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)(o.ToString()),
                LoadFunc = ( data, l ) => ((string)data)[0]
            };
        }

        [SerializationMappingProvider( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new DirectSerializationMapping<string>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (string)data
            };
        }

        [SerializationMappingProvider( typeof( DateTime ) )]
        public static SerializationMapping DateTimeMapping()
        {
            // DateTime is saved as an ISO-8601 string.
            return new DirectSerializationMapping<DateTime>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o.ToString( "s", CultureInfo.InvariantCulture ),
                LoadFunc = ( data, l ) => DateTime.ParseExact( (string)data, "s", CultureInfo.InvariantCulture )
            };
        }

        [SerializationMappingProvider( typeof( TimeSpan ) )]
        public static SerializationMapping TimeSpanMapping()
        {
            // TimeSpan is saved as `[-][d'.']hh':'mm':'ss['.'fffffff]`.
            return new DirectSerializationMapping<TimeSpan>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o.ToString( "c", CultureInfo.InvariantCulture ),
                LoadFunc = ( data, l ) => TimeSpan.ParseExact( (string)data, "c", CultureInfo.InvariantCulture )
            };
        }



        [SerializationMappingProvider( typeof( Vector2 ) )]
        public static SerializationMapping Vector2Mapping()
        {
            return new DirectSerializationMapping<Vector2>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                LoadFunc = ( data, l ) => new Vector2( (float)data[0], (float)data[1] )
            };
        }
        
        [SerializationMappingProvider( typeof( Vector2Int ) )]
        public static SerializationMapping Vector2IntMapping()
        {
            return new DirectSerializationMapping<Vector2Int>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                LoadFunc = ( data, l ) => new Vector2Int( (int)data[0], (int)data[1] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new DirectSerializationMapping<Vector3>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                LoadFunc = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }
        
        [SerializationMappingProvider( typeof( Vector3Int ) )]
        public static SerializationMapping Vector3IntMapping()
        {
            return new DirectSerializationMapping<Vector3Int>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                LoadFunc = ( data, l ) => new Vector3Int( (int)data[0], (int)data[1], (int)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3Dbl ) )]
        public static SerializationMapping Vector3DblMapping()
        {
            return new DirectSerializationMapping<Vector3Dbl>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                LoadFunc = ( data, l ) => new Vector3Dbl( (double)data[0], (double)data[1], (double)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector4 ) )]
        public static SerializationMapping Vector4Mapping()
        {
            return new DirectSerializationMapping<Vector4>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                LoadFunc = ( data, l ) => new Vector4( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new DirectSerializationMapping<Quaternion>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                LoadFunc = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( QuaternionDbl ) )]
        public static SerializationMapping QuaternionDblMapping()
        {
            return new DirectSerializationMapping<QuaternionDbl>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                LoadFunc = ( data, l ) => new QuaternionDbl( (double)data[0], (double)data[1], (double)data[2], (double)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Enum ) )]
        public static SerializationMapping EnumMapping<T>() where T : struct, Enum
        {
            return new DirectSerializationMapping<T>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o.ToString( "G" ),
                LoadFunc = ( data, l ) => Enum.Parse<T>( (string)data )
            };
        }

        [SerializationMappingProvider( typeof( Array ) )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new DirectSerializationMapping<T[]>()
            {
                SaveFunc = ( o, s ) =>
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
                LoadFunc = ( data, l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return new T[] { };

                    T[] o = new T[serializedArray.Count];

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        Type elementType = typeof( T );
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( elementType );

                        var element = (T)mapping.Load( elementData, l );
                        o[i] = element;
                    }

                    return o;
                },
                // LoadReferencesFunc is for pass-through to the elements. Not needed otherwise.
                LoadReferencesFunc = ( ref T[] o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return;

                    for( int i = 0; i < o.Length; i++ )
                    {
                        T element = o[i];
                        SerializedData elementData = serializedArray[i];

                        Type elementType = typeof( T );
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( elementType );

                        object elem = element;
                        mapping.LoadReferences( ref elem, elementData, l );

                        o[i] = (T)elem;
                    }
                }
            };
        }

        [SerializationMappingProvider( typeof( Dictionary<,> ) )]
        public static SerializationMapping Dictionary_TKey_TValue_Mapping<TKey, TValue>()
        {
            // Assume the dictionary is saved as a mapping from references to values.

#warning TODO - we might want to save the dict as a mapping from references to references.

            return new DirectSerializationMapping<Dictionary<TKey, TValue>>()
            {
                SaveFunc = ( o, s ) =>
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
                LoadFunc = ( data, l ) =>
                {
                    return new Dictionary<TKey, TValue>();
                },
                LoadReferencesFunc = ( ref Dictionary<TKey, TValue> o, SerializedData data, IForwardReferenceMap l ) =>
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

                        object elem = mapping.Load( elementData, l );

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
            return new DirectSerializationMapping<Delegate>()
            {
                SaveFunc = ( o, s ) =>
                {
                    return Persistent_Delegate.GetData( o, s );
                },
                LoadReferencesFunc = ( ref Delegate o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    // This is kinda non-standard, but since we need the reference to the `target` to even create the delegate, we can only create it here.
                    o = Persistent_Delegate.ToDelegate( data, l );
                }
            };
        }
    }
}