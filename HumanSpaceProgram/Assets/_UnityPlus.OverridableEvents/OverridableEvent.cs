using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.OverridableEvents
{
    /// <summary>
    /// Represents a strongly-typed event whoose listeners can block other listeners from being executed.
    /// </summary>
    public class OverridableEvent<T>
    {
        Dictionary<string, OverridableEventListener<T>> _allListeners = new Dictionary<string, OverridableEventListener<T>>();

        Action<T>[] _cachedListeners = null;
        bool _isStale = true;

        private void RecacheAndSortListeners()
        {
            _cachedListeners = _allListeners.Values
                .GetNonBlacklisted()
                .Cast<OverridableEventListener<T>>()
                .SortDependencies()
                .Cast<OverridableEventListener<T>>()

                .Select( l => l.OnInvoke ).ToArray();

            _isStale = false;
        }

        /// <summary>
        /// Tries to add a listener to the event.
        /// </summary>
        /// <returns>False if a listener with the specified ID is already present in the listener list.</returns>
        public bool TryAddListener( OverridableEventListener<T> listener )
        {
            if( _allListeners.TryAdd( listener.ID, listener ) )
            {
                _isStale = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to remove a listener from the event.
        /// </summary>
        /// <returns>False if a listener with the specified ID isn't present in the listener list.</returns>
        public bool TryRemoveListener( string listenerId )
        {
            if( _allListeners.Remove( listenerId ) )
            {
                _isStale = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invokes the event with the specified parameter value.
        /// </summary>
        public void Invoke( T obj )
        {
            if( _isStale )
            {
                RecacheAndSortListeners();
            }

            foreach( var listenerFunc in _cachedListeners )
            {
                try
                {
                    listenerFunc( obj );
                }
                catch( Exception ex )
                {
                    Debug.LogException( ex );
                }
            }
        }
    }
}