using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being pressed (after being not pressed).
    /// </summary>
    public class KeyDownBinding : InputBinding
    {
        public KeyCode Key { get; }

        bool _previousFrameWasReleased = true;
        bool _currentFrameIsPressed = false;

        public KeyDownBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override void Update( InputState currentState )
        {
            _previousFrameWasReleased = !_currentFrameIsPressed;
            _currentFrameIsPressed = currentState.CurrentHeldKeys.Contains( Key );
        }

        public override bool Check()
        {
            return _previousFrameWasReleased && _currentFrameIsPressed;
        }
    }
}