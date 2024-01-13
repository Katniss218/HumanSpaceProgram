using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being released after being pressed, restricting that the mouse must have moved a certain distance between when the key was pressed and released.
    /// </summary>
    public sealed class MouseDragBinding : IInputBinding
    {
        public const float MaxMouseDelta = 4.0f;

        public KeyCode Key { get; set; }

        public bool IsValid { get; private set; }

        bool _previousFrameWasHeld = false;
        bool _deltaExceeded = false;

        Vector2 _startPosition = Vector2.zero;

        public MouseDragBinding( KeyCode key )
        {
            this.Key = key;
        }

        public void Update( InputState currentState )
        {
            Vector2 delta = _startPosition - currentState.MousePosition;

            if( Mathf.Abs( delta.x ) > MaxMouseDelta
             || Mathf.Abs( delta.y ) > MaxMouseDelta )
            {
                _deltaExceeded = true;
            }

            bool currentFrameIsHeld = currentState.CurrentHeldKeys.Contains( Key );
            if( currentFrameIsHeld )
            {
                if( !_previousFrameWasHeld ) // pressed - start click.
                {
                    _startPosition = currentState.MousePosition;
                    _deltaExceeded = false;
                }
            }

            this.IsValid = _previousFrameWasHeld && !currentFrameIsHeld && _deltaExceeded;

            _previousFrameWasHeld = currentFrameIsHeld;
        }
    }
}