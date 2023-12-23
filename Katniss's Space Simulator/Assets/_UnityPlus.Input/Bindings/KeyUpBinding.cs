using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being released (after being pressed).
    /// </summary>
    public sealed class KeyUpBinding : IInputBinding
    {
        public KeyCode Key { get; set; }

        public bool IsValid { get; private set; }

        bool _previousFrameWasPressed = true;
        bool _currentFrameIsReleased = true;

        public KeyUpBinding( KeyCode key )
        {
            this.Key = key;
        }

        public void Update( InputState currentState )
        {
            _currentFrameIsReleased = !currentState.CurrentHeldKeys.Contains( Key );

            this.IsValid = _previousFrameWasPressed && _currentFrameIsReleased;

            _previousFrameWasPressed = !_currentFrameIsReleased;
        }
    }
}