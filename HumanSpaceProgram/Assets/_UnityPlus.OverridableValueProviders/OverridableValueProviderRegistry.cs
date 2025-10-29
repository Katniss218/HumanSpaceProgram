using System.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.OverridableValueProviders
{
    public static class OverridableValueCombiners
    {
        private static void EnsureNotEmpty<T>( ReadOnlySpan<T> span )
        {
            if( span.Length == 0 )
                throw new InvalidOperationException( "Cannot combine zero elements." );
        }

        /// <summary>
        /// Returns the first element in the list.
        /// </summary>
        public static TResult First<TResult>( ReadOnlyMemory<TResult> mem )
        {
            var span = mem.Span;
            EnsureNotEmpty( span );
            return span[0];
        }

        /// <summary>
        /// Returns the last element in the list.
        /// </summary>
        public static TResult Last<TResult>( ReadOnlyMemory<TResult> mem )
        {
            var span = mem.Span;
            EnsureNotEmpty( span );
            return span[^1];
        }
    }

    public class OverridableValueProviderRegistry<T, TResult>
    {
        Dictionary<string, OverridableValueProvider<T, TResult>> _allProviders = new();

        OverridableValueProvider<T, TResult>[] _cachedProviders;
        bool _isStale = true;
        readonly Func<ReadOnlyMemory<TResult>, TResult> _combiner;

        public OverridableValueProviderRegistry( Func<ReadOnlyMemory<TResult>, TResult> combiner )
        {
            if( combiner == null )
                throw new ArgumentNullException( nameof( combiner ) );

            _combiner = combiner;
        }

        private void RecacheAndSortProvider()
        {
            _cachedProviders = _allProviders.Values
                .GetNonBlacklisted()
                .Cast<OverridableValueProvider<T, TResult>>()
                .SortDependencies()
                .Cast<OverridableValueProvider<T, TResult>>()
                .ToArray();

            _isStale = false;
        }

        public bool TryAddProvider( OverridableValueProvider<T, TResult> provider )
        {
            if( _allProviders.TryAdd( provider.ID, provider ) )
            {
                _isStale = true;
                return true;
            }

            return false;
        }

        public bool TryRemoveProvider( string providerId )
        {
            if( _allProviders.Remove( providerId ) )
            {
                _isStale = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to get the (possibly combined) value for input. Returns false if there are no providers.
        /// </summary>
        public bool TryGetValue( T input, out TResult value )
        {
            if( _isStale )
                RecacheAndSortProvider();

            if( _cachedProviders == null || _cachedProviders.Length == 0 )
            {
                value = default;
                return false;
            }

            var providers = _cachedProviders;
            int count = providers.Length;
            TResult[] rented = ArrayPool<TResult>.Shared.Rent( count );
            try
            {
                for( int i = 0; i < count; ++i )
                {
                    rented[i] = providers[i].GetValue( input );
                }

                value = _combiner( rented.AsMemory( 0, count ) );
                return true;
            }
            finally
            {
                // Clear the used portion to avoid retaining references (important for reference types).
                Array.Clear( rented, 0, count );
                ArrayPool<TResult>.Shared.Return( rented );
            }
        }

        /// <summary>
        /// Get value or return defaultValue if there are no providers.
        /// </summary>
        public TResult GetValueOrDefault( T input, TResult defaultValue )
        {
            if( _isStale )
                RecacheAndSortProvider();

            if( _cachedProviders == null || _cachedProviders.Length == 0 )
                return defaultValue;

            var providers = _cachedProviders;
            int count = providers.Length;
            TResult[] rented = ArrayPool<TResult>.Shared.Rent( count );
            try
            {
                for( int i = 0; i < count; ++i )
                {
                    rented[i] = providers[i].GetValue( input );
                }

                return _combiner( rented.AsMemory( 0, count ) );
            }
            finally
            {
                // Clear the used portion to avoid retaining references (important for reference types).
                Array.Clear( rented, 0, count );
                ArrayPool<TResult>.Shared.Return( rented );
            }
        }
    }
}