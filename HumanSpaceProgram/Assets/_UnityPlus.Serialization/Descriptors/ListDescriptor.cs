using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class ListDescriptor<T> : CollectionDescriptor, ICollectionDescriptorWithContext
    {
        public override Type MappedType => typeof( List<T> );

        public IContextSelector ElementSelector { get; set; } = new UniformSelector( ContextIDs.Default );

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int capacity = 0;
            var arr = SerializationHelpers.GetValueNode( data, ctx.Config.ForceStandardJson );
            if( arr != null )
                capacity = arr.Count;

            return new List<T>( capacity );
        }

        public override object Resize( object target, int newSize )
        {
            List<T> list = (List<T>)target;
            list.Clear();

            if( list.Capacity < newSize )
            {
                list.Capacity = newSize;
            }

            for( int i = 0; i < newSize; i++ )
            {
                list.Add( default );
            }

            return list;
        }

        public override int GetStepCount( object target )
        {
            return ((List<T>)target).Count;
        }

        private IDescriptor _cachedElementDescriptor;

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            if( _cachedElementDescriptor == null && ElementSelector is UniformSelector uniform )
            {
                _cachedElementDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), uniform.Select( default ) );
            }
            return new ListMemberInfo( stepIndex, ElementSelector, _cachedElementDescriptor );
        }

        private readonly struct ListMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type MemberType => typeof( T );
            public bool RequiresWriteBack => typeof( T ).IsValueType;

            private readonly int _index;
            private readonly IContextSelector _selector;
            private readonly IDescriptor _cachedDescriptor;

            public ListMemberInfo( int index, IContextSelector selector, IDescriptor cachedDescriptor )
            {
                _index = index;
                _selector = selector;
                _cachedDescriptor = cachedDescriptor;
            }

            public ContextKey GetContext( object target )
            {
                if( _selector is UniformSelector uniform )
                    return uniform.Select( default );

                var args = new ContextSelectionArgs( _index, typeof( T ), typeof( T ), ((List<T>)target).Count );
                return _selector.Select( args );
            }

            public IDescriptor TypeDescriptor
            {
                get
                {
                    if( _cachedDescriptor != null )
                        return _cachedDescriptor;

                    if( _selector is UniformSelector uniform )
                        return TypeDescriptorRegistry.GetDescriptor( typeof( T ), uniform.Select( default ) );
                    return null;
                }
            }

            public object GetValue( object target ) => ((List<T>)target)[_index];
            public void SetValue( ref object target, object value ) => ((List<T>)target)[_index] = (T)value;
        }
    }
}