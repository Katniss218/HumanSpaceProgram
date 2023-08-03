using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.OverridableEvents
{
    /// <summary>
    /// Manages overridable game events.
    /// </summary>
    public class OverridableEventManager<T>
    {
        Dictionary<string, OverridableEvent<T>> _events = new Dictionary<string, OverridableEvent<T>>();

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

            _events.Add( eventId, new OverridableEvent<T>() );
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
        public bool TryAddListener( string eventId, OverridableEventListener<T> listener )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.TryGetValue( eventId, out OverridableEvent<T> @event ) )
            {
                @event.TryAddListener( listener );
                return true;
            }

            return false; // unknown event ID.
        }

        /// <summary>
        /// Safely invokes each non-blocked listener of a specific event one-by-one.
        /// </summary>
        /// <param name="eventId">The event ID of the event to invoke.</param>
        /// <param name="obj">The object parameter to invoke the events with.</param>
        /// <returns>False if the event doesn't exist.</returns>
        public bool TryInvoke( string eventId, T obj = default )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.TryGetValue( eventId, out OverridableEvent<T> @event ) )
            {
                @event.Invoke( obj );
                return true;
            }

            return false;
        }
    }
}