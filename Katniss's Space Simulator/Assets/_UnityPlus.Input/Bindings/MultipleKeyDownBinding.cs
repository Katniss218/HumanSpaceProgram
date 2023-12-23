using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// A binding that binds against multiple keys being pressed in a specific order. <br/>
    /// The previous keys must be held down until the last key is pressed.
    /// </summary>
    public sealed class MultipleKeyDownBinding : IInputBinding
    {
        public KeyCode[] KeysOrdered { get; private set; }

        public bool IsValid { get; private set; }

        int _indexOfKeyToPressNow;
        bool _previousFrameWasReleased = false;

        public MultipleKeyDownBinding( params KeyCode[] keysOrdered )
        {
            if( keysOrdered.Length < 2 )
                throw new ArgumentException( $"There must be at least 2 keys. For a single key binding use the {nameof( KeyDownBinding )}.", nameof( KeysOrdered ) );

            this.KeysOrdered = keysOrdered;
        }

        public MultipleKeyDownBinding( IEnumerable<KeyCode> keysOrdered )
        {
            this.KeysOrdered = keysOrdered.ToArray();

            if( this.KeysOrdered.Length < 2 )
                throw new ArgumentException( $"There must be at least 2 keys. For a single key binding use the {nameof( KeyDownBinding )}.", nameof( KeysOrdered ) );
        }

        public void Update( InputState currentState )
        {
            // Invalidate if any of the keys that were pressed was released.
            for( int i = 0; i < _indexOfKeyToPressNow; i++ )
            {
                if( !currentState.CurrentHeldKeys.Contains( KeysOrdered[i] ) )
                {
                    _indexOfKeyToPressNow = i;
                    IsValid = false;
                    return;
                }
            }

            bool currentFrameIsHeld = currentState.CurrentHeldKeys.Contains( KeysOrdered[KeysOrdered.Length - 1] );

            if( !IsValid )
            {
                // Allows pressing the keys *simultaneously*, but not backwards.
                while( _indexOfKeyToPressNow < KeysOrdered.Length && currentState.CurrentHeldKeys.Contains( KeysOrdered[_indexOfKeyToPressNow] ) )
                {
                    _indexOfKeyToPressNow++;
                    if( _indexOfKeyToPressNow == KeysOrdered.Length )
                    {
                        IsValid = true;
                        break;
                    }
                }
            }

            if( !_previousFrameWasReleased ) // invalidate after the first valid frame, this is a key *down* binding, not key held.
                IsValid = false;

            _previousFrameWasReleased = !currentFrameIsHeld;
        }
    }
}