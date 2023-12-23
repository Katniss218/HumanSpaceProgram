using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being released after being pressed, restricting that the mouse must have moved between when the key was pressed and released.
    /// </summary>
    public class MouseDragBinding : InputBinding
    {
        public const float MaxMouseDelta = 4.0f;

        public KeyCode Key { get; }

        bool _previousFrameWasHeld = false;
        Vector2 _originMousePosition = Vector2.zero;

        public MouseDragBinding( KeyCode key )
        {
            this.Key = key;
        }

        bool UpdateAndCheck( InputState currentState )
        {
            if( currentState.CurrentHeldKeys.Contains( Key ) )
            {
                if( !_previousFrameWasHeld )
                {
                    _originMousePosition = currentState.MousePosition;
                    _previousFrameWasHeld = true; // now will be held.
                }
                return false;
            }
            else
            {
                if( _previousFrameWasHeld )
                {
                    Vector2 clickDelta = _originMousePosition - currentState.MousePosition;
                    _previousFrameWasHeld = false; // now won't be held.
                    return Mathf.Abs( clickDelta.x ) > MaxMouseDelta || Mathf.Abs( clickDelta.y ) > MaxMouseDelta;
                }

                // never pressed in the first place.
                return false;
            }
        }

        public override void Update( InputState currentState )
        {
            throw new NotImplementedException();
        }

        public override bool Check()
        {
            throw new NotImplementedException();
        }
    }
}