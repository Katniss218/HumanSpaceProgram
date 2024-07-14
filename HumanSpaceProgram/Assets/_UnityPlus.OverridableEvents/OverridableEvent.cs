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

        private void RecacheAndSortListeners()
        {
            _cachedListeners = _allListeners.Values
                .GetNonBlacklistedListeners()
                .SortDependencies()

                .Select( l => l.OnInvoke ).ToArray();
        }

        /// <summary>
        /// Tries to add a listener to the event.
        /// </summary>
        /// <returns>False if a listener with the specified ID is already present in the listener list.</returns>
        public bool TryAddListener( OverridableEventListener<T> listener )
        {
            if( _allListeners.TryAdd( listener.ID, listener ) )
            {
                _cachedListeners = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to remove a listener from the event.
        /// </summary>
        /// <returns>False if a listener with the specified ID isn't present in the listener list.</returns>
        public bool TryRemoveListener( string id )
        {
            if( _allListeners.Remove( id ) )
            {
                _cachedListeners = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invokes the event with the specified parameter value.
        /// </summary>
        public void Invoke( T obj )
        {
            if( _cachedListeners == null )
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