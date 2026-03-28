using System;
using System.Reflection;

namespace UnityPlus.Serialization.Descriptors
{
    public class ReflectionPropertyInfo : IMemberInfo
    {
        public string Name { get; }
        public int Index => -1;
        public Type DeclaredType { get; }
        public bool RequiresWriteBack { get; }

        private readonly ContextKey _context;
        public ContextKey GetContext( object target ) => _context;

        private readonly Getter<object, object> _getter;
        private readonly Setter<object, object> _setter;
        private readonly RefSetter<object, object> _refSetter;

        private IDescriptor _cachedDesc;
        public IDescriptor TypeDescriptor
        {
            get
            {
                if( _cachedDesc == null )
                    _cachedDesc = TypeDescriptorRegistry.GetDescriptor( DeclaredType, _context );
                return _cachedDesc;
            }
        }

        public ReflectionPropertyInfo( PropertyInfo property, ContextKey context = default )
        {
            Name = property.Name;
            DeclaredType = property.PropertyType;
            RequiresWriteBack = DeclaredType.IsValueType;
            _context = context;

            _getter = AccessorUtils.CreateUntypedGetter( property );

            if( property.DeclaringType.IsValueType )
            {
                _refSetter = AccessorUtils.CreateUntypedStructSetter( property );
            }
            else
            {
                _setter = AccessorUtils.CreateUntypedSetter( property );
            }
        }

        public object GetValue( object target ) => _getter( target );

        public void SetValue( ref object target, object value )
        {
            if( _setter != null )
            {
                _setter( target, value );
            }
            else
            {
                _refSetter( ref target, value );
            }
        }
    }
}