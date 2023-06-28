using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.StaticEvents
{
    /// <summary>
    /// Represents an event which listeners can block other listeners from being fired.
    /// </summary>
    public class OverridableEvent
    {
        string _id;

        Dictionary<string, Action<object>> _listeners = new Dictionary<string, Action<object>>();
        HashSet<string> _blacklist = new HashSet<string>();

        public OverridableEvent( string id )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ), $"The ID parameter can't be null." );
            }

            this._id = id;
        }

        public bool TryAddListener( OverridableEventListener<Action<object>> listener )
        {
            if( _listeners.ContainsKey( listener.id ) )
            {
                return false;
            }

            if( listener.blacklist != null )
            {
                // Allows a mod to block itself (!), but I don't think that's a problem.
                foreach( string blockedId in listener.blacklist )
                {
                    _blacklist.Add( blockedId );
                }

                // Remove listners that are on the new blacklist.
                foreach( string blockedId in listener.blacklist )
                {
                    if( _listeners.Remove( blockedId ) )
                    {
                        Debug.Log( $"{nameof( OverridableEvent )}: `{_id}`: Listener `{blockedId}` was blocked by `{listener.id}`." );
                    }
                }
            }

            if( _blacklist.Contains( listener.id ) )
            {
                Debug.Log( $"{nameof( OverridableEvent )}: `{_id}`: Listener `{listener.id}` is blocked." );
                return false;
            }

            _listeners.Add( listener.id, listener.func );
            return true;
        }

        public bool TryInvoke( object obj )
        {
            foreach( var listener in _listeners.Values )
            {
                try
                {
                    listener( obj );
                }
                catch( Exception ex )
                {
                    Debug.LogException( ex );
                }
            }
            return true;
        }
    }
}