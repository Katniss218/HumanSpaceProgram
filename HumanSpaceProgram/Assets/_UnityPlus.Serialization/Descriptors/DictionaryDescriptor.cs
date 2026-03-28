using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization.Descriptors
{
    public class DictionaryDescriptor<TDict, TKey, TValue> : CollectionDescriptor, ICollectionDescriptorWithContext where TDict : IDictionary<TKey, TValue>, new()
    {
        public override Type MappedType => typeof( TDict );
        public IContextSelector ElementSelector { get; set; }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return new TDict();
        }

        public override object Resize( object target, int newSize )
        {
            ((TDict)target).Clear();
            return target;
        }

        public override int GetStepCount( object target )
        {
            return ((TDict)target).Count;
        }

        private IDescriptor _cachedKvpDescriptor;

        private void EnsureCachedDescriptor()
        {
            if( _cachedKvpDescriptor != null ) return;

            // Resolve context for KeyValuePair based on ElementSelector
            // We assume index 0 = Key, index 1 = Value for the selector
            var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );
            var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );

            ContextKey keyCtx = ElementSelector?.Select( args1 ) ?? ContextKey.Default;
            ContextKey valCtx = ElementSelector?.Select( args2 ) ?? ContextKey.Default;

            if( keyCtx == ContextKey.Default && valCtx == ContextKey.Default )
            {
                _cachedKvpDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( KeyValuePair<TKey, TValue> ) );
            }
            else
            {
                ContextKey kvpCtx = ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
                _cachedKvpDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( KeyValuePair<TKey, TValue> ), kvpCtx );
            }
        }

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            EnsureCachedDescriptor();
            // Random Access Mode (Thin)
            return new DictionaryEntryMemberInfo( stepIndex, default, false, _cachedKvpDescriptor );
        }

        public override IEnumerator<IMemberInfo> GetMemberEnumerator( object target )
        {
            EnsureCachedDescriptor();
            var dict = (TDict)target;
            int index = 0;
            foreach( var kvp in dict )
            {
                // Enumeration Mode (Fat)
                yield return new DictionaryEntryMemberInfo( index, kvp, true, _cachedKvpDescriptor );
                index++;
            }
        }

        private readonly struct DictionaryEntryMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type DeclaredType => typeof( KeyValuePair<TKey, TValue> );
            public bool RequiresWriteBack => true;

            public IDescriptor TypeDescriptor { get; }

            private readonly int _index;
            private readonly KeyValuePair<TKey, TValue> _kvp; // Only used during enumeration
            private readonly bool _isExisting;

            public DictionaryEntryMemberInfo( int index, KeyValuePair<TKey, TValue> kvp, bool isExisting, IDescriptor descriptor )
            {
                _index = index;
                _kvp = kvp;
                _isExisting = isExisting;
                TypeDescriptor = descriptor;
            }

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target )
            {
                if( !_isExisting )
                {
                    // Random Access Mode: Find element
                    var dict = (TDict)target;
                    if( _index < dict.Count )
                        return (object)dict.ElementAt( _index );
                    return null;
                }

                // Enumeration Mode: Use cached value
                return (object)_kvp;
            }

            public void SetValue( ref object target, object value )
            {
                var dict = (TDict)target;
                var pair = (KeyValuePair<TKey, TValue>)value;

                if( pair.Key == null )
                    return;

                if( dict.ContainsKey( pair.Key ) )
                    dict[pair.Key] = pair.Value;
                else
                    dict.Add( pair.Key, pair.Value );
            }
        }
    }
}