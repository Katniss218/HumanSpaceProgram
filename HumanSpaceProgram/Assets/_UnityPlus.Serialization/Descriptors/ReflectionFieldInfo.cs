using System;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// An IMemberInfo implementation that uses compiled Expression Trees for fast field access via AccessorUtils.
    /// </summary>
    public class ReflectionFieldInfo : IMemberInfo
    {
        public string Name { get; }
        public int Index => -1; // Fields are named, not indexed
        public Type MemberType { get; }
        public bool RequiresWriteBack { get; }

        private readonly ContextKey _context;
        public ContextKey GetContext( object target ) => _context;

        private readonly Getter<object, object> _getter;
        private readonly Setter<object, object> _setter; // For classes
        private readonly RefSetter<object, object> _refSetter; // For structs

        private IDescriptor _cachedDesc;
        public IDescriptor TypeDescriptor
        {
            get
            {
                if( _cachedDesc == null )
                    _cachedDesc = TypeDescriptorRegistry.GetDescriptor( MemberType, _context );
                return _cachedDesc;
            }
        }

        public ReflectionFieldInfo( FieldInfo field, ContextKey context = default )
        {
            Name = field.Name;
            MemberType = field.FieldType;
            RequiresWriteBack = MemberType.IsValueType;
            _context = context;

            // Use AccessorUtils to generate optimized delegates
            _getter = AccessorUtils.CreateUntypedGetter( field );

            if( field.DeclaringType.IsValueType )
            {
                // We use a generated RefSetter which handles Unbox -> Modify -> Rebox.
                // This ensures that the boxed instance passed as 'ref object target' is updated to the new box.
                // This triggers the Write-Back chain in ExecutionStack.
                _refSetter = AccessorUtils.CreateUntypedStructSetter( field );
            }
            else
            {
                _setter = AccessorUtils.CreateUntypedSetter( field );
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
                // Boxed struct set
                _refSetter( ref target, value );
            }
        }
    }
}