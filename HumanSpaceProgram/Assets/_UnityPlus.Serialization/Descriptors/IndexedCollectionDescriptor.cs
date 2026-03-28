using System;

namespace UnityPlus.Serialization.Descriptors
{
    public class IndexedCollectionDescriptor<TCollection, TElement> : CollectionDescriptor, ICollectionDescriptorWithContext
    {
        public override Type MappedType => typeof( TCollection );

        public IContextSelector ElementSelector { get; set; }

        public Func<int, TCollection> Factory { get; set; }
        public Func<TCollection, int, TCollection> ResizeMethod { get; set; }
        public Func<TCollection, int> GetCountMethod { get; set; }
        public Func<TCollection, int, TElement> GetElementMethod { get; set; }
        public Action<TCollection, int, TElement> SetElementMethod { get; set; }

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
            if( ResizeMethod != null )
                return ResizeMethod( (TCollection)target, newSize );
            return target;
        }

        public override int GetStepCount( object target )
        {
            if( GetCountMethod != null )
                return GetCountMethod( (TCollection)target );
            return 0;
        }

        private IDescriptor _cachedElementDescriptor;

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            if( _cachedElementDescriptor == null && ElementSelector is UniformSelector uniform )
            {
                _cachedElementDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TElement ), uniform.Select( default ) );
            }
            return new IndexedMemberInfo( stepIndex, ElementSelector, _cachedElementDescriptor, GetElementMethod, SetElementMethod, GetCountMethod );
        }

        private readonly struct IndexedMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type DeclaredType => typeof( TElement );
            public bool RequiresWriteBack => typeof( TElement ).IsValueType;

            private readonly int _index;
            private readonly IContextSelector _selector;
            private readonly IDescriptor _cachedDescriptor;
            private readonly Func<TCollection, int, TElement> _getElement;
            private readonly Action<TCollection, int, TElement> _setElement;
            private readonly Func<TCollection, int> _getCount;

            public IndexedMemberInfo( int index, IContextSelector selector, IDescriptor cachedDescriptor, Func<TCollection, int, TElement> getElement, Action<TCollection, int, TElement> setElement, Func<TCollection, int> getCount )
            {
                _index = index;
                _selector = selector;
                _cachedDescriptor = cachedDescriptor;
                _getElement = getElement;
                _setElement = setElement;
                _getCount = getCount;
            }

            public ContextKey GetContext( object target )
            {
                if( _selector is UniformSelector uniform )
                    return uniform.Select( default );

                int count = _getCount != null ? _getCount( (TCollection)target ) : 0;
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

            public object GetValue( object target ) => _getElement != null ? _getElement( (TCollection)target, _index ) : default;
            public void SetValue( ref object target, object value )
            {
                if( _setElement != null )
                {
                    _setElement( (TCollection)target, _index, (TElement)value );
                }
            }
        }
    }
}