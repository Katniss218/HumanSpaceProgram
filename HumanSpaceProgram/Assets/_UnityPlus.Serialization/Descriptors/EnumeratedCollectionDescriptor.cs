using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Descriptors
{
    public class EnumeratedCollectionDescriptor<TCollection, TElement> : CollectionDescriptor, ICollectionDescriptorWithContext
    {
        public override Type MappedType => typeof( TCollection );

        public IContextSelector ElementSelector { get; set; }

        public Func<int, TCollection> Factory { get; set; }
        public Action<TCollection, TElement> AddMethod { get; set; }
        public Func<TCollection, IEnumerable<TElement>> GetEnumerable { get; set; }
        public Action<TCollection> ClearMethod { get; set; }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int capacity = 0;
            var arr = SerializationHelpers.GetValueNode( data );
            if( arr != null )
                capacity = arr.Count;

            if( Factory != null )
                return Factory( capacity );

            return Activator.CreateInstance( typeof( TCollection ) );
        }

        public override object Resize( object target, int newSize )
        {
            if( ClearMethod != null )
            {
                ClearMethod( (TCollection)target );
            }
            else if( target is ICollection<TElement> coll )
            {
                coll.Clear();
            }
            return target;
        }

        public override int GetStepCount( object target )
        {
            if( target is ICollection<TElement> coll )
                return coll.Count;

            int count = 0;
            var enumerable = GetEnumerable != null ? GetEnumerable( (TCollection)target ) : (IEnumerable<TElement>)target;
            foreach( var item in enumerable )
                count++;
            return count;
        }

        public override IEnumerator<IMemberInfo> GetMemberEnumerator( object target )
        {
            var enumerable = GetEnumerable != null ? GetEnumerable( (TCollection)target ) : (IEnumerable<TElement>)target;
            int index = 0;
            foreach( var item in enumerable )
            {
                yield return new EnumeratedMemberInfo( index, ElementSelector, null, item, AddMethod );
                index++;
            }
        }

        private IDescriptor _cachedElementDescriptor;

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            if( _cachedElementDescriptor == null && ElementSelector is UniformSelector uniform )
            {
                _cachedElementDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TElement ), uniform.Select( default ) );
            }
            return new EnumeratedMemberInfo( stepIndex, ElementSelector, _cachedElementDescriptor, default, AddMethod );
        }

        private readonly struct EnumeratedMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type DeclaredType => typeof( TElement );
            public bool RequiresWriteBack => typeof( TElement ).IsValueType;

            private readonly int _index;
            private readonly IContextSelector _selector;
            private readonly IDescriptor _cachedDescriptor;
            private readonly TElement _value;
            private readonly Action<TCollection, TElement> _addMethod;

            public EnumeratedMemberInfo( int index, IContextSelector selector, IDescriptor cachedDescriptor, TElement value, Action<TCollection, TElement> addMethod )
            {
                _index = index;
                _selector = selector;
                _cachedDescriptor = cachedDescriptor;
                _value = value;
                _addMethod = addMethod;
            }

            public ContextKey GetContext( object target )
            {
                if( _selector is UniformSelector uniform )
                    return uniform.Select( default );

                int count = 0;
                if( target is ICollection<TElement> coll )
                    count = coll.Count;

                var args = new ContextSelectionArgs( _index, typeof( TElement ), typeof( TElement ), count );
                return _selector.Select( args );
            }

            public IDescriptor TypeDescriptor
            {
                get
                {
                    if( _cachedDescriptor != null )
                        return _cachedDescriptor;

                    if( _selector is UniformSelector uniform )
                        return TypeDescriptorRegistry.GetDescriptor( typeof( TElement ), uniform.Select( default ) );
                    return null;
                }
            }

            public object GetValue( object target ) => _value;
            public void SetValue( ref object target, object value )
            {
                if( _addMethod != null )
                {
                    _addMethod( (TCollection)target, (TElement)value );
                }
                else if( target is ICollection<TElement> coll )
                {
                    coll.Add( (TElement)value );
                }
            }
        }
    }
}