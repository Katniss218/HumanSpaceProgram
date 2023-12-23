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
        public KeyCode[] KeysOrdered { get; set; }

        public bool IsValid { get; private set; }

        int _keyToPress;
        bool _previousFrameWasReleased = false;

        public MultipleKeyDownBinding( params KeyCode[] keysOrdered )
        {
            this.KeysOrdered = keysOrdered;
        }

        public MultipleKeyDownBinding( IEnumerable<KeyCode> keysOrdered )
        {
            this.KeysOrdered = keysOrdered.ToArray();
        }

        public void Update( InputState currentState )
        {
            // Invalidate if any of the keys that were pressed was released.
            for( int i = 0; i < _keyToPress; i++ )
            {
                if( !currentState.CurrentHeldKeys.Contains( KeysOrdered[i] ) )
                {
                    _keyToPress = i;
                    IsValid = false;
                    return;
                }
            }

            bool currentFrameIsHeld = currentState.CurrentHeldKeys.Contains( KeysOrdered[KeysOrdered.Length - 1] );

            if( !IsValid )
            {
                // Allows pressing the keys *simultaneously*, but not backwards.
                while( _keyToPress < KeysOrdered.Length && currentState.CurrentHeldKeys.Contains( KeysOrdered[_keyToPress] ) )
                {
                    _keyToPress++;
                    if( _keyToPress == KeysOrdered.Length )
                    {
                        IsValid = true;
                        break;
                    }
                }
            }

            if( !_previousFrameWasReleased )
                IsValid = false;

            _previousFrameWasReleased = !currentFrameIsHeld;
        }
    }
}