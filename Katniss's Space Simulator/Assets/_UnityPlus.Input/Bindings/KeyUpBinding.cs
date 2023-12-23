using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being released (after being pressed).
    /// </summary>
    public class KeyUpBinding : InputBinding
    {
        public KeyCode Key { get; }

        bool _previousFrameWasPressed = true;
        bool _currentFrameIsReleased = true;

        public KeyUpBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override void Update( InputState currentState )
        {
            _previousFrameWasPressed = !_currentFrameIsReleased;
            _currentFrameIsReleased = !currentState.CurrentHeldKeys.Contains( Key );
        }

        public override bool Check()
        {
            return _previousFrameWasPressed && _currentFrameIsReleased;
        }
    }
}