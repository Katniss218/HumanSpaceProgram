using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization.Descriptors
{
    public readonly struct ComponentCollection
    {
        public readonly GameObject GameObject;
        public readonly Component[] CachedComponents;

        public ComponentCollection( GameObject go )
        {
            GameObject = go;
            CachedComponents = go.GetComponents<Component>();
        }

        [MapsInheritingFrom( typeof( ComponentCollection ) )]
        private static IDescriptor ProvideString() => new ComponentSequenceDescriptor();
    }

    public readonly struct ChildCollection
    {
        public readonly GameObject GameObject;
        public readonly Transform[] CachedChildren;

        public ChildCollection( GameObject go )
        {
            GameObject = go;
            int count = go.transform.childCount;
            CachedChildren = new Transform[count];
            for( int i = 0; i < count; i++ )
            {
                CachedChildren[i] = go.transform.GetChild( i );
            }
        }

        [MapsInheritingFrom( typeof( ChildCollection ) )]
        private static IDescriptor ProvideString() => new ChildSequenceDescriptor();
    }

    public class GameObjectDescriptor : CompositeDescriptor
    {
        public override Type MappedType => typeof( GameObject );

        private readonly IMemberInfo[] _members;

        public GameObjectDescriptor()
        {
            _members = new IMemberInfo[]
            {
                new PropertyMember( "name", typeof( string ), ( t ) => ((GameObject)t).name, ( ref object t, object v ) => ((GameObject)t).name = (string)v ),
                new PropertyMember( "layer", typeof( int ), ( t ) => ((GameObject)t).layer, ( ref object t, object v ) => ((GameObject)t).layer = (int)v ),
                new PropertyMember( "tag", typeof( string ), ( t ) => ((GameObject)t).tag, ( ref object t, object v ) => ((GameObject)t).tag = (string)v ),
                new PropertyMember( "is_static", typeof( bool ), ( t ) => ((GameObject)t).isStatic, ( ref object t, object v ) => ((GameObject)t).isStatic = (bool)v ),
                // Virtual Containers
                new VirtualListMember( KeyNames.COMPONENTS, typeof( ComponentCollection ), new ComponentSequenceDescriptor(), t => new ComponentCollection( (GameObject)t ) ),
                new VirtualListMember( KeyNames.CHILDREN, typeof( ChildCollection ), new ChildSequenceDescriptor(), t => new ChildCollection( (GameObject)t ) ),
                // Activation (Must be last)
                new PropertyMember( "is_active", typeof( bool ), ( t ) => ((GameObject)t).activeSelf, ( ref object t, object v ) => ((GameObject)t).SetActive( (bool)v ) ),
            };
        }

        // Steps:
        // 0: Name
        // 1: Layer
        // 2: Tag
        // 3: IsStatic
        // 4: Components (Sequence)
        // 5: Children (Sequence)
        // 6: Active (Applied last to trigger Awake/OnEnable after full population)
        public override int GetStepCount( object target ) => _members.Length;

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            if( stepIndex >= 0 && stepIndex < _members.Length )
                return _members[stepIndex];
            throw new IndexOutOfRangeException();
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            var go = new GameObject();
            go.SetActive( false ); // Ensure it's inactive during population

            SerializedArray componentsArray = null;
            if( data is SerializedObject objRoot && objRoot.TryGetValue( KeyNames.COMPONENTS, out var compNode ) )
            {
                componentsArray = SerializationHelpers.GetValueNode( compNode );
            }

            if( componentsArray != null )
            {
                // Track usage of existing components (e.g. Transform, or those added by RequireComponent)
                // to prevent duplicates or double-counting.
                var usedCounts = new Dictionary<Type, int>();

                foreach( var cNode in componentsArray )
                {
                    if( cNode is SerializedObject cObj && Persistent_Type.TryReadTypeName( cObj, out string typeName ) )
                    {
                        Type type = ctx.Config.TypeResolver.ResolveType( typeName );

                        if( type != null )
                        {
                            // Check if we already have an available component of this type
                            var existing = go.GetComponents( type );
                            int used = 0;
                            if( usedCounts.TryGetValue( type, out int u ) )
                                used = u;

                            if( existing.Length > used )
                            {
                                // Claim the existing one
                                usedCounts[type] = used + 1;
                            }
                            else
                            {
                                // Add a new one
                                go.GetTransformOrAddComponent( type );
                                usedCounts[type] = used + 1;
                            }
                        }
                    }
                }
            }

            return go;
        }

        // --- Helpers ---

        private struct PropertyMember : IMemberInfo
        {
            public string Name { get; }
            public readonly int Index => -1;
            public Type DeclaredType { get; }
            public IDescriptor TypeDescriptor { get; }
            public readonly bool RequiresWriteBack => false;

            private Func<object, object> _getter;
            private RefSetter<object, object> _setter;

            public PropertyMember( string name, Type type, Func<object, object> getter, RefSetter<object, object> setter )
            {
                Name = name;
                DeclaredType = type;
                TypeDescriptor = TypeDescriptorRegistry.GetDescriptor( type, ContextKey.Default );
                _getter = getter;
                _setter = setter;
            }

            public ContextKey GetContext( object target ) => ContextKey.Default;

            public object GetValue( object target ) => _getter( target );
            public void SetValue( ref object target, object value ) => _setter( ref target, value );
        }

        private struct VirtualListMember : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1;
            public Type DeclaredType { get; }
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => false;

            private Func<object, object> _getter;

            public VirtualListMember( string name, Type type, IDescriptor descriptor, Func<object, object> getter )
            {
                Name = name;
                DeclaredType = type;
                TypeDescriptor = descriptor;
                _getter = getter;
            }

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target ) => _getter( target );
            public void SetValue( ref object target, object value ) { /* No-op */ }
        }
    }

    /// <summary>
    /// Iterates over components on a GameObject.
    /// </summary>
    public class ComponentSequenceDescriptor : CollectionDescriptor
    {
        public override Type MappedType => typeof( ComponentCollection );

        private IDescriptor _componentDescriptor;

        public ComponentSequenceDescriptor()
        {
        }

        private IDescriptor GetComponentDescriptor()
        {
            if( _componentDescriptor == null )
                _componentDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( Component ) );
            return _componentDescriptor;
        }

        public override int GetStepCount( object target )
        {
            Debug.Log( "B" );
            return ((ComponentCollection)target).CachedComponents.Length;
        }

        public override object Resize( object target, int newSize )
        {
            return target;
        }

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            return new InstanceMemberInfo( stepIndex, GetComponentDescriptor() );
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return null;
        }

        private struct InstanceMemberInfo : IMemberInfo
        {
            public string Name => null; // Array element
            public int Index => _index;
            public Type DeclaredType => typeof( Component );
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => false;

            private int _index;

            public InstanceMemberInfo( int index, IDescriptor descriptor )
            {
                _index = index;
                TypeDescriptor = descriptor;
            }

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target )
            {
                var components = ((ComponentCollection)target).CachedComponents;
                if( _index < components.Length )
                    return components[_index];
                return null;
            }
            public void SetValue( ref object target, object value ) { }
        }
    }

    /// <summary>
    /// Iterates over children of a GameObject.
    /// </summary>
    public class ChildSequenceDescriptor : CollectionDescriptor
    {
        public override Type MappedType => typeof( ChildCollection );

        private IDescriptor _gameObjectDescriptor;

        public ChildSequenceDescriptor()
        {
        }

        private IDescriptor GetGameObjectDescriptor()
        {
            if( _gameObjectDescriptor == null )
                _gameObjectDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( GameObject ) );
            return _gameObjectDescriptor;
        }

        public override int GetStepCount( object target )
        {
            return ((ChildCollection)target).CachedChildren.Length;
        }

        public override object Resize( object target, int newSize )
        {
            Transform t = ((ChildCollection)target).GameObject.transform;
            for( int i = t.childCount - 1; i >= 0; i-- )
            {
                UnityEngine.Object.DestroyImmediate( t.GetChild( i ).gameObject );
            }
            return new ChildCollection( ((ChildCollection)target).GameObject );
        }

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            return new ChildMemberInfo( stepIndex, GetGameObjectDescriptor() );
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return null;
        }

        private struct ChildMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type DeclaredType => typeof( GameObject );
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => false;

            private int _index;

            public ChildMemberInfo( int index, IDescriptor descriptor )
            {
                _index = index;
                TypeDescriptor = descriptor;
            }

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target )
            {
                var children = ((ChildCollection)target).CachedChildren;
                if( _index < children.Length )
                    return children[_index].gameObject;
                return null;
            }

            public void SetValue( ref object target, object value )
            {
                if( value is GameObject childGO && target is ChildCollection parentWrapper )
                {
                    childGO.transform.SetParent( parentWrapper.GameObject.transform, false );
                }
            }
        }
    }
}