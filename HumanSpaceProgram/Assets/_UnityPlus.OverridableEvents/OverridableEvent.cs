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
        private struct Listener // cache struct.
        {
            public Action<T> Func;
            public string[] BlockList;
        }

        Dictionary<string, Listener> _allListeners = new Dictionary<string, Listener>();
        Action<T>[] _cachedListeners = null;

        private void RecacheNotBlockedListeners()
        {
            // Purpose:
            // - Figure out which listeners are not blocked, and cache them so this doesn't have to be done on every invoke.

            HashSet<string> blockList = new HashSet<string>();
            foreach( var listener in _allListeners.Values )
            {
                if( listener.BlockList == null )
                    continue;
                foreach( var block in listener.BlockList )
                {
                    blockList.Add( block );
                }
            }

            List<Action<T>> notBlockedListeners = new List<Action<T>>( _allListeners.Count ); // Limits resizes.
            foreach( var (listenerId, listener) in _allListeners )
            {
                if( !blockList.Contains( listenerId ) )
                {
                    notBlockedListeners.Add( listener.Func );
                }
            }

            _cachedListeners = notBlockedListeners.ToArray();
        }

        /// <summary>
        /// Tries to add a listener to the event.
        /// </summary>
        /// <returns>False if a listener with the specified ID is already present in the listener list.</returns>
        public bool TryAddListener( OverridableEventListener<T> listener )
        {
            if( _allListeners.ContainsKey( listener.id ) )
            {
                return false;
            }

            _allListeners.Add( listener.id, new Listener() { Func = listener.func, BlockList = listener.blacklist } );
            _cachedListeners = null;
            return true;
        }

        /// <summary>
        /// Tries to remove a listener from the event.
        /// </summary>
        /// <returns>False if a listener with the specified ID isn't present in the listener list.</returns>
        public bool TryRemoveListener( string id )
        {
            if( !_allListeners.ContainsKey( id ) )
            {
                return false;
            }

            _allListeners.Remove( id );
            _cachedListeners = null;
            return true;
        }

        /// <summary>
        /// Invokes the event with the specified parameter value.
        /// </summary>
        public void Invoke( T obj )
        {
            if( _cachedListeners == null )
            {
                RecacheNotBlockedListeners();
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