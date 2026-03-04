using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public readonly struct ComponentCollection
    {
        public readonly GameObject GameObject;
        public ComponentCollection( GameObject go ) => GameObject = go;
    }

    public readonly struct ChildCollection
    {
        public readonly GameObject GameObject;
        public ChildCollection( GameObject go ) => GameObject = go;
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
                new PropertyMember( "isStatic", typeof( bool ), ( t ) => ((GameObject)t).isStatic, ( ref object t, object v ) => ((GameObject)t).isStatic = (bool)v ),
                // Virtual Containers
                new VirtualListMember( KeyNames.COMPONENTS, typeof( ComponentCollection ), new ComponentSequenceDescriptor(), t => new ComponentCollection( (GameObject)t ) ),
                new VirtualListMember( KeyNames.CHILDREN, typeof( ChildCollection ), new ChildSequenceDescriptor(), t => new ChildCollection( (GameObject)t ) ),
                // Activation (Must be last)
                new PropertyMember( "active", typeof( bool ), ( t ) => ((GameObject)t).activeSelf, ( ref object t, object v ) => ((GameObject)t).SetActive( (bool)v ) ),
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
            // Pre-scan for RectTransform to determine creation method
            bool hasRectTransform = false;

            SerializedArray componentsArray = null;
            if( data is SerializedObject objRoot && objRoot.TryGetValue( KeyNames.COMPONENTS, out var compNode ) )
            {
                componentsArray = SerializationHelpers.GetValueNode( compNode, ctx.Config.ForceStandardJson );
            }

            if( componentsArray != null )
            {
                foreach( var cNode in componentsArray )
                {
                    if( cNode is SerializedObject cObj && Persistent_Type.TryReadTypeName( cObj, out string typeName ) )
                    {
                        if( typeName.Contains( "RectTransform" ) )
                        {
                            hasRectTransform = true;
                            break;
                        }
                    }
                }
            }

            var go = hasRectTransform
                ? new GameObject( "New Game Object", typeof( RectTransform ) )
                : new GameObject();

            go.SetActive( false ); // Ensure it's inactive during population

            // Pre-instantiate components based on data types
            if( componentsArray != null )
            {
                foreach( var cNode in componentsArray )
                {
                    if( cNode is SerializedObject cObj && Persistent_Type.TryReadTypeName( cObj, out string typeName ) )
                    {
                        Type type = ctx.Config.TypeResolver.ResolveType( typeName );

                        if( type != null )
                        {
                            // Don't add duplicates of Transform/RectTransform which are auto-created
                            if( go.GetComponent( type ) == null )
                            {
                                go.AddComponent( type );
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
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor { get; }
            public readonly bool RequiresWriteBack => false;

            private Func<object, object> _getter;
            private RefSetter<object, object> _setter;

            public PropertyMember( string name, Type type, Func<object, object> getter, RefSetter<object, object> setter )
            {
                Name = name;
                MemberType = type;
                TypeDescriptor = TypeDescriptorRegistry.GetDescriptor( type );
                _getter = getter;
                _setter = setter;
            }

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target ) => _getter( target );
            public void SetValue( ref object target, object value ) => _setter( ref target, value );
        }

        private struct VirtualListMember : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1;
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => false;

            private Func<object, object> _getter;

            public VirtualListMember( string name, Type type, IDescriptor descriptor, Func<object, object> getter )
            {
                Name = name;
                MemberType = type;
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
            return ((ComponentCollection)target).GameObject.GetComponents<Component>().Length;
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
            public Type MemberType => typeof( Component );
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
                var components = ((ComponentCollection)target).GameObject.GetComponents<Component>();
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
            return ((ChildCollection)target).GameObject.transform.childCount;
        }

        public override object Resize( object target, int newSize )
        {
            Transform t = ((ChildCollection)target).GameObject.transform;
            for( int i = t.childCount - 1; i >= 0; i-- )
            {
                UnityEngine.Object.DestroyImmediate( t.GetChild( i ).gameObject );
            }
            return target;
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
            public Type MemberType => typeof( GameObject );
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
                Transform t = ((ChildCollection)target).GameObject.transform;
                if( _index < t.childCount )
                    return t.GetChild( _index ).gameObject;
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