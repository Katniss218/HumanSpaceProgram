using System;
using System.Collections.Generic;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    internal static class CollectionProviders
    {
        [MapsInheritingFrom( typeof( Array ) )]
        private static IDescriptor ProvideArray<T>( ContextKey context, Type targetType ) // context needs to be here because of the 'pass through' feature where one provider can handle families of contexts.
        {
            IDescriptor desc;
            if( targetType.IsArray && targetType.GetArrayRank() > 1 )
            {
                desc = new MultiDimensionalDescriptor<Array, T>()
                {
                    Factory = lengths => Array.CreateInstance( typeof( T ), lengths ),
                    GetLengths = arr =>
                    {
                        int[] lengths = new int[arr.Rank];
                        for( int i = 0; i < arr.Rank; i++ )
                            lengths[i] = arr.GetLength( i );
                        return lengths;
                    },
                    GetFlatValues = arr =>
                    {
                        T[] flat = new T[arr.Length];

                        int i = 0;
                        foreach( T value in arr )
                            flat[i++] = value;

                        return flat;
                    },
                    SetFlatValues = ( arr, flat ) =>
                    {
                        int count = Math.Min( arr.Length, flat.Length );

                        int i = 0;
                        foreach( var _ in arr )
                        {
                            if( i >= count )
                                break;

                            arr.SetValue( flat[i], i );
                            i++;
                        }
                    }
                };
                return desc;
            }

            desc = new IndexedCollectionDescriptor<T[], T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new T[capacity],
                ResizeMethod = ( arr, newSize ) =>
                {
                    if( arr == null || arr.Length != newSize )
                    {
                        if( arr != null )
                            Array.Resize( ref arr, newSize );
                        else
                            arr = new T[newSize];
                    }
                    return arr;
                },
                GetCountMethod = arr => arr.Length,
                GetElementMethod = ( arr, index ) => arr[index],
                SetElementMethod = ( arr, index, item ) => arr[index] = item
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentBag<> ) )]
        private static IDescriptor ProvideConcurrentBag<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentBag<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentBag<T>(),
                AddMethod = ( coll, item ) => coll.Add( item ),
                ClearMethod = coll => { while( coll.TryTake( out _ ) ) ; }
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentDictionary<,> ) )]
        private static IDescriptor ProvideConcurrentDictionary<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );
            var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );

            ContextKey keyCtx = selector?.Select( args1 ) ?? ContextKey.Default;
            ContextKey valCtx = selector?.Select( args2 ) ?? ContextKey.Default;
            ContextKey kvpCtx = ContextKey.Default;
            if( keyCtx != ContextKey.Default || valCtx != ContextKey.Default )
            {
                kvpCtx = ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
            }

            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( kvpCtx ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>(),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentQueue<> ) )]
        private static IDescriptor ProvideConcurrentQueue<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentQueue<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentQueue<T>(),
                AddMethod = ( coll, item ) => coll.Enqueue( item ),
                ClearMethod = coll => { while( coll.TryDequeue( out _ ) ) ; }
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentStack<> ) )]
        private static IDescriptor ProvideConcurrentStack<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentStack<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentStack<T>(),
                AddMethod = ( coll, item ) => coll.Push( item ),
                GetEnumerable = coll => { var arr = coll.ToArray(); Array.Reverse( arr ); return arr; },
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ) )]
        private static IDescriptor ProvideDictionary<TKey, TValue>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( context ),
                Factory = capacity => new Dictionary<TKey, TValue>( capacity ),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( HashSet<> ) )]
        private static IDescriptor ProvideHashSet<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<HashSet<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new HashSet<T>(),
                AddMethod = ( coll, item ) => coll.Add( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( LinkedList<> ) )]
        private static IDescriptor ProvideLinkedList<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<LinkedList<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new LinkedList<T>(),
                AddMethod = ( coll, item ) => coll.AddLast( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( List<> ) )]
        private static IDescriptor ProvideList<T>( ContextKey context )
        {
            var desc = new IndexedCollectionDescriptor<List<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new List<T>( capacity ),
                ResizeMethod = ( list, newSize ) =>
                {
                    list.Clear();
                    if( list.Capacity < newSize )
                        list.Capacity = newSize;
                    for( int i = 0; i < newSize; i++ )
                        list.Add( default );
                    return list;
                },
                GetCountMethod = list => list.Count,
                GetElementMethod = ( list, index ) => list[index],
                SetElementMethod = ( list, index, item ) => list[index] = item
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.ObjectModel.ObservableCollection<> ) )]
        private static IDescriptor ProvideObservableCollection<T>( ContextKey context )
        {
            var desc = new IndexedCollectionDescriptor<System.Collections.ObjectModel.ObservableCollection<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.ObjectModel.ObservableCollection<T>(),
                ResizeMethod = ( coll, newSize ) =>
                {
                    coll.Clear();
                    for( int i = 0; i < newSize; i++ )
                        coll.Add( default );
                    return coll;
                },
                GetCountMethod = coll => coll.Count,
                GetElementMethod = ( coll, index ) => coll[index],
                SetElementMethod = ( coll, index, item ) => coll[index] = item
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( Queue<> ) )]
        private static IDescriptor ProvideQueue<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<Queue<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new Queue<T>( capacity ),
                AddMethod = ( coll, item ) => coll.Enqueue( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( SortedDictionary<,> ) )]
        private static IDescriptor ProvideSortedDictionary<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );
            var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );

            ContextKey keyCtx = selector?.Select( args1 ) ?? ContextKey.Default;
            ContextKey valCtx = selector?.Select( args2 ) ?? ContextKey.Default;
            ContextKey kvpCtx = ContextKey.Default;
            if( keyCtx != ContextKey.Default || valCtx != ContextKey.Default )
            {
                kvpCtx = ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
            }

            var desc = new EnumeratedCollectionDescriptor<SortedDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( kvpCtx ),
                Factory = capacity => new SortedDictionary<TKey, TValue>(),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( SortedList<,> ) )]
        private static IDescriptor ProvideSortedList<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );
            var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );

            ContextKey keyCtx = selector?.Select( args1 ) ?? ContextKey.Default;
            ContextKey valCtx = selector?.Select( args2 ) ?? ContextKey.Default;
            ContextKey kvpCtx = ContextKey.Default;
            if( keyCtx != ContextKey.Default || valCtx != ContextKey.Default )
            {
                kvpCtx = ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
            }

            var desc = new EnumeratedCollectionDescriptor<SortedList<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( kvpCtx ),
                Factory = capacity => new SortedList<TKey, TValue>( capacity ),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( SortedSet<> ) )]
        private static IDescriptor ProvideSortedSet<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<SortedSet<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new SortedSet<T>(),
                AddMethod = ( coll, item ) => coll.Add( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( Stack<> ) )]
        private static IDescriptor ProvideStack<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<Stack<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new Stack<T>( capacity ),
                AddMethod = ( coll, item ) => coll.Push( item ),
                GetEnumerable = coll => { var arr = coll.ToArray(); Array.Reverse( arr ); return arr; },
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

#warning TODO - interface-based providers for IEnumerable, ICollection, IList, IDictionary, etc. - need to be careful about the fact that these interfaces are implemented by many types.
        // needs to use the generic T and just assume e.g. an array when loading (array is an IList<T> and IEnumerable<T>, etc).

        // BitArray

#warning TODO - add a "reject" type and rejecting descriptor (e.g. span, intptr, etc, etc) which always result in an exception / null written / not read.
    }
}