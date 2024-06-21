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
        [SerializationMappingProvider( typeof( object ), Context = ObjectContext.Ref )]
        public static SerializationMapping ObjectRefMapping<T>() where T : class
        {
            return new PrimitiveObjectSerializationMapping<T>()
            {
                OnSave = ( o, s ) => s.RefMap.WriteObjectReference<T>( o ),
                OnInstantiate = ( data, l ) => l.ReadObjectReference<T>( data )
            };
        }

        [SerializationMappingProvider( typeof( Array ), Context = ObjectContext.Ref )]
        public static SerializationMapping ArrayReferenceMapping<T>() where T : class
        {
            return new PrimitiveObjectSerializationMapping<T[]>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray();
                    for( int i = 0; i < o.Length; i++ )
                    {
                        var data = s.RefMap.WriteObjectReference<T>( o[i] );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                OnInstantiate = ( data, l ) =>
                {
                    SerializedArray serializedArray = (SerializedArray)data;

                    T[] array = new T[serializedArray.Count];

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        var element = l.ReadObjectReference<T>( elementData );
                        array[i] = element;
                    }

                    return array;
                }
            };
        }

#warning TODO - generic mappings might want to be used on different types of things, kind of like the generic constraints. This method here is currently not safe because it can be invoked on a struct.
        
        [SerializationMappingProvider( typeof( object ), Context = ObjectContext.Asset )]
        public static SerializationMapping ObjectAssetMapping<T>() where T : class
        {
            return new PrimitiveObjectSerializationMapping<T>()
            {
                OnSave = ( o, s ) => s.RefMap.WriteAssetReference<T>( o ),
                OnInstantiate = ( data, l ) => l.ReadAssetReference<T>( data )
            };
        }

        [SerializationMappingProvider( typeof( Array ), Context = ObjectContext.Asset )]
        public static SerializationMapping ArrayAssetMapping<T>() where T : class
        {
            return new PrimitiveObjectSerializationMapping<T[]>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray();
                    for( int i = 0; i < o.Length; i++ )
                    {
                        var data = s.RefMap.WriteAssetReference<T>( o[i] );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                OnInstantiate = ( data, l ) =>
                {
                    SerializedArray serializedArray = (SerializedArray)data;

                    T[] array = new T[serializedArray.Count];

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        var element = l.ReadAssetReference<T>( elementData );
                        array[i] = element;
                    }

                    return array;
                }
            };
        }

        [SerializationMappingProvider( typeof( SerializedPrimitive ) )]
        public static SerializationMapping SerializedPrimitiveMapping()
        {
            return new PrimitiveObjectSerializationMapping<SerializedPrimitive>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedPrimitive
            };
        }

        [SerializationMappingProvider( typeof( SerializedObject ) )]
        public static SerializationMapping SerializedObjectMapping()
        {
            return new PrimitiveObjectSerializationMapping<SerializedObject>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedObject
            };
        }

        [SerializationMappingProvider( typeof( SerializedArray ) )]
        public static SerializationMapping SerializedArrayMapping()
        {
            return new PrimitiveObjectSerializationMapping<SerializedArray>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedArray
            };
        }
        
        [SerializationMappingProvider( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new PrimitiveObjectSerializationMapping<bool>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (bool)data
            };
        }

        [SerializationMappingProvider( typeof( byte ) )]
        public static SerializationMapping ByteMapping()
        {
            return new PrimitiveObjectSerializationMapping<byte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (byte)data
            };
        }

        [SerializationMappingProvider( typeof( sbyte ) )]
        public static SerializationMapping SByteMapping()
        {
            return new PrimitiveObjectSerializationMapping<sbyte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (sbyte)data
            };
        }

        [SerializationMappingProvider( typeof( short ) )]
        public static SerializationMapping Int16Mapping()
        {
            return new PrimitiveObjectSerializationMapping<short>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (short)data
            };
        }

        [SerializationMappingProvider( typeof( ushort ) )]
        public static SerializationMapping UInt16Mapping()
        {
            return new PrimitiveObjectSerializationMapping<ushort>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ushort)data
            };
        }

        [SerializationMappingProvider( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new PrimitiveObjectSerializationMapping<int>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (int)data
            };
        }

        [SerializationMappingProvider( typeof( uint ) )]
        public static SerializationMapping UInt32Mapping()
        {
            return new PrimitiveObjectSerializationMapping<uint>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (uint)data
            };
        }

        [SerializationMappingProvider( typeof( long ) )]
        public static SerializationMapping Int64Mapping()
        {
            return new PrimitiveObjectSerializationMapping<long>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (long)data
            };
        }

        [SerializationMappingProvider( typeof( ulong ) )]
        public static SerializationMapping UInt64Mapping()
        {
            return new PrimitiveObjectSerializationMapping<ulong>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ulong)data
            };
        }

        [SerializationMappingProvider( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new PrimitiveObjectSerializationMapping<float>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (float)data
            };
        }

        [SerializationMappingProvider( typeof( double ) )]
        public static SerializationMapping DoubleMapping()
        {
            return new PrimitiveObjectSerializationMapping<double>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (double)data
            };
        }

        [SerializationMappingProvider( typeof( decimal ) )]
        public static SerializationMapping DecimalMapping()
        {
            return new PrimitiveObjectSerializationMapping<decimal>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (decimal)data
            };
        }

        [SerializationMappingProvider( typeof( char ) )]
        public static SerializationMapping CharMapping()
        {
            return new PrimitiveObjectSerializationMapping<char>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)(o.ToString()),
                OnInstantiate = ( data, l ) => ((string)data)[0]
            };
        }

        [SerializationMappingProvider( typeof( string ) )]
        public static SerializationMapping StringMapping()
        { 
            return new PrimitiveObjectSerializationMapping<string>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (string)data
            };
        }

        [SerializationMappingProvider( typeof( DateTime ) )]
        public static SerializationMapping DateTimeMapping()
        {
            // DateTime is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602Z`

            return new PrimitiveObjectSerializationMapping<DateTime>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => DateTime.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [SerializationMappingProvider( typeof( DateTimeOffset ) )]
        public static SerializationMapping DateTimeOffsetMapping()
        {
            // DateTimeOffset is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602+00:00`

            return new PrimitiveObjectSerializationMapping<DateTimeOffset>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => DateTimeOffset.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [SerializationMappingProvider( typeof( TimeSpan ) )]
        public static SerializationMapping TimeSpanMapping()
        {
            // TimeSpan is saved as `[-][dd'.']hh':'mm':'ss['.'fffffff]`.
            // `-3962086.01:03:44.2452523`

            return new PrimitiveObjectSerializationMapping<TimeSpan>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "c", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => TimeSpan.ParseExact( (string)data, "c", CultureInfo.InvariantCulture )
            };
        }



        [SerializationMappingProvider( typeof( Vector2 ) )]
        public static SerializationMapping Vector2Mapping()
        {
            return new PrimitiveObjectSerializationMapping<Vector2>()
            {
                OnSave = ( o, s ) => new SerializedArray( 2 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnInstantiate = ( data, l ) => new Vector2( (float)data[0], (float)data[1] )
            };
        }

        [SerializationMappingProvider( typeof( Vector2Int ) )]
        public static SerializationMapping Vector2IntMapping()
        {
            return new PrimitiveObjectSerializationMapping<Vector2Int>()
            {
                OnSave = ( o, s ) => new SerializedArray( 2 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnInstantiate = ( data, l ) => new Vector2Int( (int)data[0], (int)data[1] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new PrimitiveObjectSerializationMapping<Vector3>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3Int ) )]
        public static SerializationMapping Vector3IntMapping()
        {
            return new PrimitiveObjectSerializationMapping<Vector3Int>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3Int( (int)data[0], (int)data[1], (int)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3Dbl ) )]
        public static SerializationMapping Vector3DblMapping()
        {
            return new PrimitiveObjectSerializationMapping<Vector3Dbl>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3Dbl( (double)data[0], (double)data[1], (double)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector4 ) )]
        public static SerializationMapping Vector4Mapping()
        {
            return new PrimitiveObjectSerializationMapping<Vector4>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new Vector4( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new PrimitiveObjectSerializationMapping<Quaternion>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( QuaternionDbl ) )]
        public static SerializationMapping QuaternionDblMapping()
        {
            return new PrimitiveObjectSerializationMapping<QuaternionDbl>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new QuaternionDbl( (double)data[0], (double)data[1], (double)data[2], (double)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Enum ) )]
        public static SerializationMapping EnumMapping<T>() where T : struct, Enum
        {
            return new PrimitiveObjectSerializationMapping<T>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "G" ),
                OnInstantiate = ( data, l ) => Enum.Parse<T>( (string)data )
            };
        }

#warning TODO - add tuple mappings (analogous to the KeyValuePair<TKey, TValue>).

        [SerializationMappingProvider( typeof( Array ) )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new NonPrimitiveSerializationMapping<T[]>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray( o.Length );
                    for( int i = 0; i < o.Length; i++ )
                    {
                        T value = o[i];

                        var mapping = SerializationMappingRegistry.GetMapping<T>( ObjectContext.Default, value );

                        var data = MappingHelper.DoSave<T>( mapping, value, s );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                OnInstantiate = ( data, l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return new T[] { };

                    return data == null ? default : new T[serializedArray.Count];
                },
                OnLoad = ( ref T[] o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return;

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        Type elementType = elementData != null && elementData.TryGetValue( KeyNames.TYPE, out var elementType2 )
                            ? elementType2.DeserializeType()
                            : typeof( T );

                        T element = default;
                        var mapping = SerializationMappingRegistry.GetMapping<T>( ObjectContext.Default, elementType );
                        if( MappingHelper.DoLoad( mapping, ref element, elementData, l ) )
                        {
                            o[i] = element;
                        }
                    }

                    //return o;
                },
                OnLoadReferences = ( ref T[] o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return;

                    for( int i = 0; i < o.Length; i++ )
                    {
                        SerializedData elementData = serializedArray[i]; // Since objects will be instantiated in OnLoad, this should be safe.
                        /*
                        Type elementType = typeof( T );
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMapping<T>( elementType );
                        */

                        T element = o[i];
                        var mapping = SerializationMappingRegistry.GetMapping<T>( ObjectContext.Default, element );
                        if( MappingHelper.DoLoadReferences( mapping, ref element, elementData, l ) )
                        {
                            o[i] = element;
                        }
                    }
                }
            };
        }



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
                var obj = new GameObject();
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

        [SerializationMappingProvider( typeof( Delegate ) )]
        public static SerializationMapping DelegateMapping()
        {
            return new PrimitiveObjectSerializationMapping<Delegate>()
            {
                OnSave = ( o, s ) =>
                {
                    return Persistent_Delegate.GetData( o, s.RefMap );
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