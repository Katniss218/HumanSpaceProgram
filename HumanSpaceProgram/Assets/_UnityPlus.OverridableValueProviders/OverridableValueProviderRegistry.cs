using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.OverridableValueProviders
{
    public abstract class OverridableValueProviderRegistry<T, TResult>
    {
        Dictionary<string, OverridableValueProvider<T, TResult>> _allProviders = new();

        OverridableValueProvider<T, TResult> _cachedProvider;
        bool _isStale = true;

        private void RecacheAndSortProvider()
        {
            _cachedProvider = _allProviders.Values
                .GetNonBlacklisted()
                .Cast<OverridableValueProvider<T, TResult>>()
                .SortDependencies()
                .Cast<OverridableValueProvider<T, TResult>>()

                .FirstOrDefault();

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

        public bool TryGetValue( T input, out TResult value )
        {
            if( _isStale )
                RecacheAndSortProvider();

            if( _cachedProvider == null )
            {
                value = default;
                return false;
            }

            value = _cachedProvider.GetValue( input );
            return true;
        }

        public TResult GetValueOrDefault( T input, TResult defaultValue )
        {
            if( _isStale )
                RecacheAndSortProvider();

            if( _cachedProvider == null )
                return defaultValue;

            return _cachedProvider.GetValue( input );
        }
    }
}