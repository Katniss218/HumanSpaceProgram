using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Descriptors
{
    /// <summary>
    /// Base class for MemberwiseDescriptor to allow sharing members across the parent type chain.
    /// </summary>
    public abstract class MemberwiseDescriptorBase : CompositeDescriptor
    {
        internal readonly List<MemberDefinition> _members = new List<MemberDefinition>();
        internal readonly List<IMethodInfo> _methods = new List<IMethodInfo>();

        // Factories
        internal Func<object> _simpleFactory;
        internal Func<SerializedData, SerializationContext, object> _rawFactory;
        internal Delegate _constructor;
        internal (string name, Type type)[] _constructorParams;

        internal class MemberDefinition : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1;
            public ContextKey Context { get; }
            public Func<object, object> Getter;
            public Action<object, object> Setter;
            public RefSetter<object, object> RefSetter;
            public Type DeclaredType { get; }
            public bool RequiresWriteBack => DeclaredType.IsValueType;

            public Predicate<object> ShouldSerialize;
            public Func<object, SerializationContext, bool> ShouldSerializeWithContext;

            private IDescriptor _cachedDescriptor;

            public IDescriptor TypeDescriptor
            {
                get
                {
                    if( _cachedDescriptor == null )
                    {
                        _cachedDescriptor = TypeDescriptorRegistry.GetDescriptor( DeclaredType, Context );
                    }
                    return _cachedDescriptor;
                }
            }

            public MemberDefinition( string name, ContextKey context, Func<object, object> getter, Action<object, object> setter, RefSetter<object, object> refSetter, Type memberType )
            {
                Name = name;
                Context = context;
                Getter = getter;
                Setter = setter;
                RefSetter = refSetter;
                DeclaredType = memberType;
            }

            public MemberDefinition Clone()
            {
                var clone = new MemberDefinition( Name, Context, Getter, Setter, RefSetter, DeclaredType );
                clone.ShouldSerialize = ShouldSerialize;
                clone.ShouldSerializeWithContext = ShouldSerializeWithContext;
                return clone;
            }

            public ContextKey GetContext( object target ) => Context;

            public object GetValue( object target ) => Getter( target );

            public void SetValue( ref object target, object value )
            {
                if( RefSetter != null )
                {
                    RefSetter( ref target, value );
                }
                else if( Setter != null )
                {
                    Setter( target, value );
                }
            }
        }
    }
}
