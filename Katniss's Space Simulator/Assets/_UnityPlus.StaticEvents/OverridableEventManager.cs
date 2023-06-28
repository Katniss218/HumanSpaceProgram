using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.StaticEvents
{
    /// <summary>
    /// Manages overridable game events.
    /// </summary>
    public class OverridableEventManager
    {
        Dictionary<string, OverridableEvent> _events = new Dictionary<string, OverridableEvent>();

        /// <summary>
        /// Tries to create an event with the given ID.
        /// </summary>
        /// <returns>True if the event was created, otherwise false (including if the event was added before).</returns>
        public bool TryCreate( string eventId )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.ContainsKey( eventId ) )
            {
                return false;
            }

            _events.Add( eventId, new OverridableEvent( eventId ) );
            return true;
        }

        /// <summary>
        /// Checks if the event with a given ID exists.
        /// </summary>
        public bool Exists( string eventId )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            return _events.ContainsKey( eventId );
        }

        /// <summary>
        /// Tries to adds the listener and returns
        /// </summary>
        /// <param name="eventId">The event ID to add the listener to.</param>
        /// <param name="listener">The listener to add.</param>
        /// <returns>False if the listener id was already added, or if the event doesn't exist. Otherwise true.</returns>
        public bool TryAddListener( string eventId, OverridableEventListener<Action<object>> listener )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.TryGetValue( eventId, out OverridableEvent @event ) )
            {
                return @event.TryAddListener( listener );
            }

            return false; // unknown event ID.
        }

        // You can't remove listeners.
        // This is because removing them would necessitate caching the entire invocation list, regardless of what is blocked,
        //   and then unblocking those that would no longer be blocked by the removed listener.

        /// <summary>
        /// Safely invokes each non-blocked listener of a specific event one-by-one.
        /// </summary>
        /// <param name="eventId">The event ID of the event to invoke.</param>
        /// <param name="obj">The object parameter to invoke the events with.</param>
        /// <returns>False if the event doesn't exist.</returns>
        public bool TryInvoke( string eventId, object obj = null )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.TryGetValue( eventId, out OverridableEvent @event ) )
            {
                return @event.TryInvoke( obj );
            }

            return false;
        }
    }
}